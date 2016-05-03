#!/usr/bin/env python

# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.

import os
import urllib
import zipfile
import site
import glob
import shutil
import json
import shutil

from os import path

from azure.servicebus   import ServiceBusService
from azure.storage      import CloudStorageAccount

from dumplingstatetable import DumplingStateContext
from dumpling_util import Logging
from dumpling_util import SafePathing

safePaths = SafePathing()
import nearby_config
_config = nearby_config.named('analysis_worker_config.json')

# I don't want this script doing anything until I can verify that all the required files are in place.
# Having these checks in place is luxurious!
# If any other files become required, add them to this list:
required_files = { 
            'triage.ini':       path.join(nearby_config.folder, 'triage.ini'),
            'analysis.py':      path.join(nearby_config.folder, 'analysis.py'),
            'sos_plugin':      str(_config.CONFIG_SOS_PLUGIN_PATH),
            'lldb_py':         str(_config.CONFIG_LLDB_PYTHON_PATH)
        }

safePaths.CheckTheseNamedPaths(required_files)
site.addsitedir(safePaths.pathof('lldb_py'))
import lldb

_bus_service = ServiceBusService(
    service_namespace= _config.SERVICE_BUS_NAMESPACE,
    shared_access_key_name= _config.SHARED_ACCESS_KEY_NAME,
    shared_access_key_value= _config.SERVICE_BUS_KEY)

##
## The Analysis Pipeline 
## The rest of these functions are 'steps in a pipeline'.  
##

# given a list of uris, produce a list of locations of core files.
def DownZip(dumpling_context):
    workset = []
    try:
        dumpling_context.SetState('downzipping')
        
        line = dumpling_context.data['DumpRelics_uri']
        
        # ignore white space lines
        if line.isspace():
            return workset           
        
        Logging.Verbose('DOWNLOADING ' + line)
        
        # intermediate zip file. This will just get overwritten each time.
        zipFile = 'file.zip'
        
        Logging.Event('StartDownload', 6)
        urllib.urlretrieve(line, filename = zipFile)
        Logging.Event('StartUnzip', 7)

        fh = open(zipFile, 'rb')

        z = zipfile.ZipFile(fh)

        locallist = [];
        
        # We want to know if we're not finding core files in the zip file.
        foundCore = False
        for name in z.namelist():
            outpath = '/'
            
            z.extract(name, outpath)

            basename = os.path.basename(name)
            dirname = os.path.dirname(name)

            if(basename == 'core' or basename.startswith('core.')):
                foundCore = True
                coredir = path.join(outpath, name)
                
                Logging.Verbose('adding path "%s" to workset because a core file named "%s" was found.'%(coredir, basename))
                workset.append(coredir)
            
            locallist.append(name)
        
        fh.close()
        
        if not foundCore: 
            Logging.Informative('NO CORE FILE IN ' + str(line))
            dumpling_context.SetState('no-core')
            return workset

                
        Logging.Verbose('downzip completed.')
    except Exception as details:
        Logging.Event('Failure', 11)
        dumpling_context.SetState('error')
        Logging.Failure('exception: %s'%(details) , 16, False)

    return workset

def Map(dumpling_context):
    analyzeList = []
    try:
        if(not dumpling_context.data['DumpRelics_uri'] or len(dumpling_context.data['DumpRelics_uri']) == 0):
            return dumpling_context.data['DumpRelics_uri']

        workset = DownZip(dumpling_context)
        if(len(workset) == 0):
            return analyzeList;
            
        for corePath in workset:
            spath = corePath.split('/')
            correlationId = spath[5]
            jobId = spath[7]

            Logging.Verbose('Correlation ID: ' + correlationId)
            Logging.Verbose('Job ID: ' + jobId)
            
            mypath = os.path.join(os.path.dirname(corePath), 'projectk*.exe')
            Logging.Verbose('test exe: ' + mypath)

            testname, ext = os.path.splitext(os.path.basename(glob.glob(mypath)[0])) # ew.
            Logging.Verbose(testname)        

            pathContext = (corePath, correlationId, jobId, testname) # meh, just gonna collect and pass on everything right now. I don't know exactly what we'll need.
            Logging.Verbose('PATH TUPLE: ' + str(pathContext))

            analyzeList.append(pathContext)        
    except Exception as details:
        Logging.Event('Failure', 11)
        dumpling_context.SetState('error')
        Logging.Failure('exception: %s'%(details) , 16, False)

    return analyzeList

# return a tuple that is the debugger, interpreter, target, and process
def StartDebugger(pathContext):
    relativeHostPath = '../core_root/corerun' # relative to the core file.
    corePath = pathContext[0]
    hostPath = os.path.join(os.path.dirname(corePath), relativeHostPath)

    debugger = lldb.SBDebugger.Create()
    debugger.SetAsync(False)

    interpreter = debugger.GetCommandInterpreter()
    
    Logging.Verbose('HOSTPATH: ' + str(hostPath))
    Logging.Verbose('COREPATH: ' + str(corePath))
    Logging.Verbose('SOS PLUGIN PATH: ' + safePaths.pathof('sos_plugin'))

    target = debugger.CreateTargetWithFileAndArch(hostPath, lldb.LLDB_ARCH_DEFAULT)
    
    if target:
      Logging.Verbose('target created.')
      process = target.LoadCore(corePath)

      if process:
        Logging.Verbose('core dump loaded.')
        loadPluginResult = lldb.SBCommandReturnObject() # we're going to store the results of our commands in this guy.
        Logging.Verbose('plugin load %s' % safePaths.pathof('sos_plugin'))

        interpreter.HandleCommand(str('plugin load %s' % safePaths.pathof('sos_plugin')), loadPluginResult)
        if loadPluginResult.Succeeded():
            Logging.Verbose('libsosplugin.so loaded.')
            debuggerTuple = (debugger, interpreter, target, process, pathContext)

            return debuggerTuple
        else:
            Logging.Failure('loading plugin failed with error: ' + loadPluginResult.GetError())
      else:
        Logging.Failure('could not load core dump.')
    else:
        Logging.Failure('no target.')

    return None
    
def RunAnalysisScript(debuggerContext):
    Logging.Verbose('In run_analysis.')

    debugger = debuggerContext[0]
    interpreter = debuggerContext[1]
    target = debuggerContext[2]
    process = debuggerContext[3]
    pathTuple = debuggerContext[4]

    Logging.Verbose('Unwrapped tuples.')

    importScriptCommandResult  = lldb.SBCommandReturnObject()
    analyzeCommandResult       = lldb.SBCommandReturnObject()

    Logging.Verbose('Return objects prepared. Asking lldb to import %s' % safePaths.pathof('analysis.py'))

    interpreter.HandleCommand(str('command script import %s' % safePaths.pathof('analysis.py')), importScriptCommandResult)

    if importScriptCommandResult.Succeeded():
        Logging.Verbose('analysis.py imported. Running analyze.')

        interpreter.HandleCommand(str('analyze -i %s -o ./analysis.txt' % safePaths.pathof('triage.ini')), analyzeCommandResult)

        if analyzeCommandResult.Succeeded():
            Logging.Verbose('analysis command succeeded.')

            with open('./analysis.txt', 'r') as analysisFile:
                data = analysisFile.read().replace('\n', '')

            Logging.Verbose('ANALYSIS OUTPUT: ' + analyzeCommandResult.GetOutput())

            return data;
        else:
            Logging.Failure('analyze step failed with error: ' + analyzeCommandResult.GetError())
    else:
        Logging.Failure('import script failed with error: ' + importScriptCommandResult.GetError())

    return '<no results>'

def Reduce(testContexts, dumpling_context):
    dumpling_context.SetState('analyzing')
    
    try:
        Logging.Event('StartAnalyze', 8)

        if(not testContexts or len(testContexts) == 0):
            return testContexts

        for corePath, correlationId, jobid, testname in testContexts:
            Logging.Informative('executing analyzer with testname =' + testname)

            testContext = (corePath, correlationId, jobid, testname)
            debuggerContext = StartDebugger(testContext)

            Logging.Verbose('debugger context = ' + str(debuggerContext))
            result = RunAnalysisScript(debuggerContext)
            
            dumpling_context.SaveResult(result, testContext)
    except Exception as details:
        dumpling_context.SetState('error')
        Logging.Event('Failure', 11)
        Logging.Failure('exception: %s'%(details) , 16, False)

    
    Cleanup(debuggerContext, dumpling_context)

     
def Cleanup(debuggerContext, dumpling_context):
    # unpack our tuple
    debugger = debuggerContext[0]
    interpreter = debuggerContext[1]
    target = debuggerContext[2]
    process = debuggerContext[3]
    pathTuple = debuggerContext[4]

    # just delete the /home/DotNet folder for now.
    Logging.Verbose('cleaning up.')
    lldb.SBDebugger.Destroy(debugger)
    shutil.rmtree('/home/DotNetBot/') # This will likely not work in the future when we receive dumps from other people.

# return True when the message is handled successfully
def HandleMessage(msg):
    try:
    	# ensure our message is in ascii encoding.
    	decoded_msg = str(msg.body.decode('ascii'))  
    	Logging.Verbose('RECEIVED: ' + decoded_msg)
    
    	# deserialize the contents
    	message = json.loads(decoded_msg, encoding = 'ascii')

        dumpling_context = DumplingStateContext(message['owner'], message['dumpling_id'], message['dump_uri'])
    	# begin doing work
    	test_contexts = Map(dumpling_context)
        
        if(len(test_contexts) == 0):
            return False
        
    	analysis_results = Reduce(test_contexts, dumpling_context)
        
        dumpling_context.SetState('complete')
        return True
    except Exception as details:
    	Logging.Failure('exception: %s'%(details) , 16, False)
        Logging.Event('Failure', 11)

    return False

from time import time

if __name__ == '__main__':
    platform = _config.TARGET_OS
    
    if platform != 'ubuntu' and platform != 'centos':
        Logging.Failure('specify \'ubuntu\' or \'centos\' value for property TARGET_OS in analysis_config.json')
    else:
        Logging.Informative('listening for ' + platform + ' messages')
    
    Logging.Event('StartService', 5)
    while True:    
        try:
            # blocks until a message comes in.
            msg = _bus_service.peek_lock_subscription_message(_config.SERVICE_BUS_TOPIC, platform)

            work_start_time = time()

            if msg.body:
                if HandleMessage(msg):
                	Logging.Verbose('message handled. deleting it from the queue.')
                	msg.delete()
                	Logging.Event('Complete', 10)
		
            work_end_time = time()

            Logging.Verbose('Loop completed in ' + str(work_end_time - work_start_time) + ' seconds.')
        except Exception as details:
            # Log the Logging.Failure, but don't give up! 
            Logging.Failure('exception: %s'%(details) , 16, False)
            Logging.Event('Failure', 11)
