#!/usr/bin/env python

# Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.


import argparse
import os
import zipfile
import string
import platform
import getpass 
import urllib    
import urllib2
import time
import json
import requests

class OutputController:
    s_squelch=False
    s_logPath='';

    @staticmethod
    def PrintVerbose(output):
        if not bool(OutputController.s_squelch):
            OutputController.Print(output)
    
    @staticmethod
    def Print(output):
        # always print out essential information.
        print output
        
        # sometimes amend our essential output to an existing log file.
        if(os.path.isfile(OutputController.s_logPath)):
            # Note: The file must exist.
            with open(OutputController.s_logPath, 'a') as log_file:
                log_file.write(output)

class DumplingService:
    _dumplingUri = 'http://dotnetrp.azurewebsites.net';

    @staticmethod
    def SayHelloAs(username):
        hello_url = DumplingService._dumplingUri + '/dumpling/test/hi/im/%s'%(username)
        response = requests.get(hello_url)

        return response.content

    @staticmethod
    def UploadZip(filepath, strUser, strDistro, strDisplayName):
        query = { 'displayname' : strDisplayName }
        upload_url = DumplingService._dumplingUri + '/dumpling/store/chunk/%s/%s/0/0/?%s'%(strUser, strDistro, urllib.urlencode(query))
        
        OutputController.PrintVerbose('Uploading core zip ' + args.zipfile  + ' to ' + upload_url)
        
        files = {'file': open(filepath, 'rb')}
    
        response = requests.post(upload_url, files = files)

        response.raise_for_status()

        idstr = response.content.strip('"')
        
        OutputController.PrintVerbose('dumpling upload succeeded. dumplingid: %s'%(idstr))
        OutputController.Print('%s/dumpling/download/%s'%(DumplingService._dumplingUri, idstr));

        return int(idstr)

    @staticmethod
    def UploadTriageInfo(dumplingid, dictData):
        upload_url = DumplingService._dumplingUri + '/dumpling/triageinfo/add/%s'%(dumplingid)
        
        triageinfo = json.dumps(dictData)

        response = requests.put(upload_url, data=dictData)

        response.raise_for_status()

        OutputController.PrintVerbose('dumplingid %s client triage information uploaded'%(dumplingid))

    @staticmethod
    def DownloadZip(dumplingId, zipPath):
        download_url = DumplingService._dumplingUri + '/dumpling/download/%s'%(dumplingId)
        
        download(download_url, zipPath)
    
    

def get_client_triage_data():
    triageProps = { }
    triageProps['CLIENT_ARCHITECTURE'] = platform.machine()
    triageProps['CLIENT_PROCESSOR'] = platform.processor()
    triageProps['CLIENT_NAME'] = platform.node()        
    triageProps['CLIENT_OS'] = platform.system()           
    triageProps['CLIENT_RELEASE'] = platform.release()     
    triageProps['CLIENT_VERSION'] = platform.version()
    if platform.system() == 'Linux':
        distroTuple = platform.linux_distribution()
        triageProps['CLIENT_DISTRO'] = distroTuple[0]
        triageProps['CLIENT_DISTRO_VER'] = distroTuple[1]
        triageProps['CLIENT_DISTRO_ID'] = distroTuple[2]
        
    return triageProps

def pack(strCorePath, strZipPath, lstAddPaths):
    """creates a zip file containing core dump and all related images"""

    includedFiles = { }
    
    #add the core dump to the files to pack
    includedFiles[os.path.abspath(strCorePath)] = None

    if lstAddPaths is not None:
        for addPath in lstAddPaths:
            absPath = os.path.abspath(addPath)
            if os.path.isdir(absPath):
                for dirpath, dirnames, filenames in os.walk(absPath):
                    for name in filenames:
                        includedFiles[os.path.join(dirpath, name)] = None
            else:
                includedFiles[absPath] = None

    debuggerLoaded = False

    try:
        import lldb
        #load the coredump in lldb
        debugger = lldb.SBDebugger.Create()

        debugger.SetAsync(False)
        
        interpreter = debugger.GetCommandInterpreter()

        result = run_command('target create --no-dependents --arch x86_64 --core ' + strCorePath, interpreter)

        target = debugger.GetSelectedTarget()

        debuggerLoaded = True
    except:
        OutputController.PrintVerbose('Unable to load the core file in degbugger.  Loaded modules will not be included')

    #if the core image was loaded into the debugger iterate through the loaded modules and add them to the zip file
    if debuggerLoaded:
        #iterate through image list and pack the zip file
        for m in target.modules:
            includedFiles[m.file.fullpath] = None
            if m.file.basename == 'libcoreclr.so':
                includedFiles[os.path.join(m.file.dirname, 'libmscordaccore.so')] = None
                includedFiles[os.path.join(m.file.dirname, 'libsos.so')] = None
        
    try:
        import zlib
        compressionType = zipfile.ZIP_DEFLATED
    except:
        compressionType = zipfile.ZIP_STORED
        
    zip = zipfile.ZipFile(strZipPath, mode='w', compression=compressionType, allowZip64=True)

    for k in includedFiles.keys():
        if k != os.path.abspath(strZipPath):
            add_to_zip(k, zip)

    zip.close()

    OutputController.PrintVerbose('core dump related files written to: ' + strZipPath)

def add_to_zip(strPath, zipFile):
    if os.path.exists(strPath):
        OutputController.PrintVerbose('adding ' + str(strPath))
        zipFile.write(strPath)

def unpack(strZipPath, unpackdir):
    """unpacks zip restoring all files to their original paths"""
    with open(strZipPath, 'rb') as f:
        zip = zipfile.ZipFile(f)

        for path in zip.namelist():
            OutputController.PrintVerbose('extracting   /' + os.path.join(os.path.basename(strZipPath).replace('.zip', ''), path))
            zip.extract(path, unpackdir)
        zip.close()

    OutputController.PrintVerbose('\nall files extracted\n')

def download(url, zipPath):
    try:
        f = urllib2.urlopen(url)               
        OutputController.PrintVerbose('DOWNLOADING ' + str(url))
        with open(zipPath, 'wb') as localfile:
            localfile.write(f.read())
    except urllib2.HTTPError, e:
        OutputController.PrintVerbose('HTTP Error:' + str(e.code) + str(url))
    except urllib2.URLError, e:
        OutputController.PrintVerbose('URL Error:' + str(e.reason) + str(url))

def run_command(strCmd, interpreter):
    strOut = ""
    result = lldb.SBCommandReturnObject()
    interpreter.HandleCommand(strCmd, result)
    if result.Succeeded():
        strOut = result.GetOutput()
        OutputController.PrintVerbose(result.GetOutput())
    else:
        OutputController.PrintVerbose("ERROR: Command FAILED: '" + strCmd + "'")
        OutputController.PrintVerbose(result.GetOutput())
    return strOut

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='dumpling client for managing core files and interacting with the dumpling service')
    
    parser.add_argument('command',
                      choices = [ 'wrap', 'unwrap', 'upload', 'download', 'update' ],
                      help='The dumpling command to be run')

    parser.add_argument('--zipfile', '-z', 
                      type=str,
                      help='path to the core dump zip file')
                                                            
    parser.add_argument('--corefile', '-c', 
                      type=str,
                      help='path to the core dump file')

    parser.add_argument('--unpackdir', '-d', 
                      type=str,
                      help='path to unpack the core dump zip file')

    parser.add_argument('--user', type=str, help='username to pass to the dumpling service')

    parser.add_argument('--distro',
                      choices = ['redhat', 'centos', 'ubuntu', 'windows' ], 
                      help = 'specifies the distro of the dump file to be uploaded.  Note, this should only be used to override when uploading from a different machine then the dump was collected on.')

    parser.add_argument('--suppresstriage', help='supresses client side triage information from being uploadeded with the dump')

    parser.add_argument('--displayname',
                      type=str,
                      help='the name to be displayed in reports for the uploaded dump')

    parser.add_argument('--url', '-u', type=str, help='url of dumpling dump to download and unwrap')

    parser.add_argument('--dumpid', '-i', type=int, help='the id of the dumpling dump to download and unwrap')

    parser.add_argument('--triagefile', type=str, help='path to the file containing json triage data')

    parser.add_argument('--addpaths', nargs='*', type=str, help='path to additional files to be included in the packaged coredump')

    parser.add_argument('--squelch', default=False, action='store_true', help='Indicates that we should only print essential information. This is used by Microsoft CI automation.')
    
    parser.add_argument('--logpath', type=str, help='specify the path to an EXISTING log file where you want essential output to be routed to.')

    args = parser.parse_args()

    OutputController.s_squelch=bool(args.squelch)
    OutputController.s_logPath=str(args.amendEssentialToLogpath);

    if args.command == 'wrap':
        pack(args.corefile, args.zipfile, args.addpaths)
    elif args.command == 'upload':
        if args.user == None:
            args.user = getpass.getuser()
        args.user = args.user.lower()
        if args.distro == None:
            if platform.system().lower() == 'linux':
                args.distro = platform.dist()[0].lower()
            else:
                args.distro = 'win'
        if args.zipfile == None:
            args.zipfile = os.path.join(os.getcwd(), '%s.%.7f.zip'%(args.user, time.time()))
        if args.corefile != None:
            pack(args.corefile, args.zipfile, args.addpaths)
        if args.displayname == None:
            filename = os.path.basename(os.path.abspath(args.zipfile))
            args.displayname = os.path.splitext(filename)[0]
        dumplingid = DumplingService.UploadZip(os.path.abspath(args.zipfile), args.user, args.distro, args.displayname)
        if not args.suppresstriage:
            DumplingService.UploadTriageInfo(dumplingid, get_client_triage_data())
    elif args.command == 'unwrap':
        if args.unpackdir == None:
            args.unpackdir = os.path.join(os.getcwd(), os.path.basename(args.zipfile).replace('.zip', '')) + os.path.sep
        OutputController.PrintVerbose('unpacking core dump zip to: ' + str(args.unpackdir))
        unpack(args.zipfile, args.unpackdir)
    elif args.command == 'download':
        if args.unpackdir == None:                                                    
            if args.dumpid is not None:
                args.unpackdir = os.path.join(os.getcwd(), 'dumpling.%s'%args.dumpid)
            else:
                args.unpackdir = os.path.join(os.getcwd(), 'dumpling.%.7f'%time.time())
        args.unpackdir = args.unpackdir.replace('.', '_')
        if args.zipfile == None:
            args.zipfile = args.unpackdir + '.zip'
        if args.dumpid is not None:
            DumplingService.DownloadZip(args.dumpid, args.zipfile)
        elif args.url is not None:
            download(args.url, args.zipfile)
        else:
            parser.print_help()
            OutputController.Print('either dumpid or url must be specified for the download command')
        if ~os.path.isdir(args.unpackdir):
            os.mkdir(args.unpackdir)
        unpack(args.zipfile, args.unpackdir)
        os.remove(args.zipfile)
    elif args.command == 'update':
        if args.dumpid == None or args.triagefile == None:
            OutputController.Print('--dumpid and --triagefile are required arguments to the update command')
        if not os.path.exists(args.triagefile):
            OutputController.Print('FILE NOT FOUND: \'%s\''%(args.triagefile))
        with open(args.triagefile, 'rb') as tfile:
            triagedata = json.load(tfile)
        DumplingService.UploadTriageInfo(args.dumpid, triagedata)     
            
            
        


  
