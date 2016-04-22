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

from dumpling_util import Logging
from dumpling_util import SafePathing

safePaths = SafePathing()
# fun way of binding to a config object on disk.
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
# Oy, because of the need to call site.addsitedir(...) I have to bring the 'required_files' stuff to here.
safePaths.CheckTheseNamedPaths(required_files)
site.addsitedir(safePaths.pathof('lldb_py'))
import lldb

# initialize our Azure Services
_storage_account        = CloudStorageAccount(_config.STORAGE_ACCOUNT_NAME, _config.STORAGE_ACCOUNT_KEY)

_blob_service           = _storage_account.create_block_blob_service()

_bus_service = ServiceBusService(
    service_namespace= _config.SERVICE_BUS_NAMESPACE,
    shared_access_key_name= _config.SHARED_ACCESS_KEY_NAME,
    shared_access_key_value= _config.SERVICE_BUS_KEY)
    

def UpdateWorkerState(context, newState):
    Logging.Verbose('Setting worker state to "%s"' % newState)
    metadata = _blob_service.get_container_metadata(context)
    metadata['worker_state'] = newState;
    _blob_service.set_container_metadata(context, metadata)

##
## The Analysis Pipeline 
## The rest of these functions are 'steps in a pipeline'.  
##

# given a list of uris, produce a list of locations of core files.
def DownZip(uris, state):
    UpdateWorkerState(state, 'downzipping')
    workset = []
    for line in uris:
        # ignore white space lines
        if line.isspace():
            continue           
        
        Logging.Verbose('DOWNLOADING ' + line)
        
        # intermediate zip file. This will just get overwritten each time.
        zipFile = 'file.zip'
        
        Logging.Event('StartDownload')
        urllib.urlretrieve(line, filename = zipFile)

        Logging.Event('StartUnzip')
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
            
    Logging.Verbose('downzip completed.')
    return workset

def Obtain(uris, state):
    if(not uris or len(uris) == 0):
        return uris

    workset = DownZip(uris, state)
    analyzeList = []
    
    for corePath in workset:
        spath = corePath.split('/')
        correlationId = spath[5]
        jobId = spath[7]
        Logging.Verbose('Correlation ID: ' + correlationId)
        Logging.Verbose('Job ID: ' + jobId)
        
        mypath = os.path.join(os.path.dirname(corePath), 'projectk-*.exe')
        Logging.Verbose('test exe: ' + mypath)

        testname, ext = os.path.splitext(os.path.basename(glob.glob(mypath)[0])) # ew.
        Logging.Verbose(testname)        

        pathtuple = (corePath, correlationId, jobId, testname) # meh, just gonna collect and pass on everything right now. I don't know exactly what we'll need.
        Logging.Verbose('PATH TUPLE: ' + str(pathtuple))

        analyzeList.append(pathtuple)        
    
    return analyzeList

# return a tuple that is the debugger, interpreter, target, and process
def StartDebugger(pathTuple):
    relativeHostPath = '../core_root/corerun' # relative to the core file.
    corePath = pathTuple[0]
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
            debuggerTuple = (debugger, interpreter, target, process, pathTuple)

            return debuggerTuple
        else:
            Logging.Failure('loading plugin failed with error: ' + loadPluginResult.GetError())
      else:
        Logging.Failure('could not load core dump.')
    else:
        Logging.Failure('no target.')

    return None

# SOS is loaded before we get here.
def RunAnalysis(debuggerTuple):
    Logging.Verbose('In run_analysis.')
    debugger = debuggerTuple[0]
    interpreter = debuggerTuple[1]
    target = debuggerTuple[2]
    process = debuggerTuple[3]
    pathTuple = debuggerTuple[4]
    Logging.Verbose('Unwrapped tuples.')

    importScriptCommandResult  = lldb.SBCommandReturnObject()
    analyzeCommandResult       = lldb.SBCommandReturnObject()

    Logging.Verbose('Return objects prepared. Asking lldb to import %s' % safepaths.pathof('analysis.py'))
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

def Analyze(testList, state):
    UpdateWorkerState(state, 'analyzing')
    Logging.Event('StartAnalyze')

    if(not testList or len(testList) == 0):
        return testList

    for corePath, correlationId, jobid, testname in testList:
        Logging.Informative('running analyzer on ' + testname)
        pathTuple = (corePath, correlationId, jobid, testname)
        debuggerTuple = StartDebugger(pathTuple)

        Logging.Verbose('We have a tuple here: ' + str(debuggerTuple))
        result = RunAnalysis(debuggerTuple)
        
        Store(pathTuple, result, state)
    
    Cleanup(debuggerTuple, state)

# stores analysis results in Azure blob
def Store(pathTuple, result, state):
    Logging.Event('StartStore')
    
    #unpack our tuple
    path = pathTuple[0]
    correlationId = pathTuple[1]
    jobId = pathTuple[2]
    testName = pathTuple[3]
    
    # put stuff in to blob storage
    _blob_service.create_blob_from_text(state, os.path.join(state, correlationId, jobId, testName), result)
    Logging.Verbose("Getting container metadata.")

    metadata = _blob_service.get_container_metadata(state)
    if metadata.has_key('job_count_completed'):
        countCompleted = int(metadata['job_count_completed'])
        metadata['job_count_completed'] = str(countCompleted + 1)
    else:
        metadata['job_count_completed'] = '1'
    
    print str(metadata)
    _blob_service.set_container_metadata(state, metadata)
    metadata = _blob_service.get_container_metadata(state)
    Logging.Verbose(str(metadata))

def Cleanup(debuggerTuple, state):
    Logging.Event('StartCleanup')
    # unpack our tuple
    debugger = debuggerTuple[0]
    interpreter = debuggerTuple[1]
    target = debuggerTuple[2]
    process = debuggerTuple[3]
    pathTuple = debuggerTuple[4]

    # just delete the /home/DotNet folder for now.
    Logging.Verbose('cleaning up.')
    lldb.SBDebugger.Destroy(debugger)
    shutil.rmtree('/home/DotNetBot/') # This will likely not work in the future when we receive dumps from other people.


    
## TODO: Wrap HandleMessage and a method handler in to modules/objects?

def HandleMessage(msg):
    # ensure our message is in ascii encoding.
    decoded_msg = str(msg.body.decode('ascii'))  
    Logging.Verbose('RECEIVED: ' + decoded_msg)
    
    # deserialize the contents
    obj = json.loads(decoded_msg, encoding = 'ascii')

    # set up the pipeline state, we use properties of the message for this.
    state = obj['state']
    target_os = obj['target_os']
    download_uris = obj['result_payload_uris']

    # sanity checks
    Logging.Verbose('PARSED STATE: ' + state) 
    Logging.Verbose('RESULTS URIS: ' + str(download_uris))

    # begin doing work
    test_paths = Obtain(download_uris, state)
    analysis_results = Analyze(test_paths, state)

from time import time

if __name__ == '__main__':
    platform = _config.TARGET_OS
    
    if platform != 'ubuntu' and platform != 'centos':
        Logging.Failure('specify \'ubuntu\' or \'centos\' value for property TARGET_OS in config.json')
    else:
        Logging.Informative('listening for ' + platform + ' messages')

    Logging.Event('Start')
    in_queue = _bus_service.get_subscription('dopplertasktopic', platform)

    while True:    
        Logging.Verbose('msg count: ' + str(in_queue.message_count))
        try:
            # blocks until a message comes in.
            msg = _bus_service.peek_lock_subscription_message('dopplertasktopic', platform)

            work_start_time = time()
            if msg.body:
                result = HandleMessage(msg)
                Logging.Verbose('message handled. deleting it from the queue.')
                msg.delete()
                Logging.Event('Complete')

            work_end_time = time()
            Logging.Verbose('Loop completed in ' + str(work_end_time - work_start_time) + ' seconds.')
        except Exception as details:
            # Log the Logging.Failure, but don't give up! 
            Logging.Failure('exception: %s'%(details) , 16, False)
