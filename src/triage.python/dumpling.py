                                                                                                   # Licensed to the .NET Foundation under one or more agreements.
# The .NET Foundation licenses this file to you under the MIT license.
# See the LICENSE file in the project root for more information.
#!/usr/bin/env python

import argparse
import os
import zipfile
import string
import requests
import platform
import getpass     
import urllib
import time

class DumplingService:
    _dumplingUri = 'http://dotnetrp.azurewebsites.net';

    @staticmethod
    def SayHelloAs(username):
        hello_url = DumplingService._dumplingUri + '/dumpling/test/hi/im/%s'%(username)
        response = requests.get(hello_url)

        return response.content

    @staticmethod
    def UploadZip(filepath):
        upload_url = DumplingService._dumplingUri + '/dumpling/store/chunk/%s/%s/0/0'%(getpass.getuser(), platform.dist()[0].lower());
        
        print 'Uploading core zip ' + args.zipfile  + ' to ' + upload_url
        
        files = {'file': open(filepath, 'rb')}
    
        response = requests.post(upload_url, files = files)

        return response.content

def pack(strCorePath, strZipPath, lstAddPaths):
    """creates a zip file containing core dump and all related images"""

    includedFiles = { }
    
    #add the core dump to the files to pack
    includedFiles[os.path.abspath(strCorePath)] = None


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
        print 'Unable to load the core file in degbugger.  Loaded modules will not be included'

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

    print 'core dump related files written to: ' + strZipPath 

def add_to_zip(strPath, zipFile):
    if os.path.exists(strPath):
        print 'adding ' + strPath
        zipFile.write(strPath)

def unpack(strZipPath, unpackdir):
    """unpacks zip restoring all files to their original paths"""
    with open(strZipPath, 'rb') as f:
        zip = zipfile.ZipFile(f)

        for path in zip.namelist():
            print 'extracting   /' + os.path.join(os.path.basename(strZipPath).replace('.zip', ''), path)
            zip.extract(path, unpackdir)
        zip.close()
    print '\nall files extracted\n'

def download(url, zipPath):
    print("DOWNLOADING " + zipPath + " FROM " + url)

    urllib.urlretrieve(url, filename = zipPath)

def run_command(strCmd, interpreter):
    strOut = ""
    result = lldb.SBCommandReturnObject()
    interpreter.HandleCommand(strCmd, result)
    if result.Succeeded():
        #print "INFO: Command SUCCEEDED: '" + strCmd + "'"
        strOut = result.GetOutput()
        print result.GetOutput()
        #print strOut
    else:
        print "ERROR: Command FAILED: '" + strCmd + "'"
        print result.GetOutput()
    return strOut

if __name__ == '__main__':
    parser = argparse.ArgumentParser(description='dumpling client for managing core files and interacting with the dumpling service')
    parser.add_argument('command',
                      choices = [ 'wrap', 'unwrap', 'upload', 'download' ],
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
    parser.add_argument('--url', '-u', type=str, help='url of dumpling dump to download and unwrap')
    parser.add_argument('--addpaths', nargs='*', type=str, help='path to additional files to be included in the packaged coredump')
    args = parser.parse_args()

    if args.command == 'wrap':
        pack(args.corefile, args.zipfile, args.addpaths)
    elif args.command == 'upload':
        if args.corefile != None:
            pack(args.corefile, args.zipfile, args.addpaths)
        print DumplingService.UploadZip(os.path.abspath(args.zipfile));
    elif args.command == 'unwrap':
        if args.unpackdir == None:
            args.unpackdir = os.path.join(os.getcwd(), os.path.basename(args.zipfile).replace('.zip', '')) + os.path.sep
        print 'unpacking core dump zip to: ' + args.unpackdir
        unpack(args.zipfile, args.unpackdir)
    elif args.command == 'download':
        if args.unpackdir == None:
            args.unpackdir = os.path.join(os.getcwd(), 'dumpling.%.7f'%time.time())
        if args.zipfile == None:
            args.zipfile = args.unpackdir + '.zip'
        download(args.url, args.zipfile)
        if ~os.path.isdir(args.unpackdir):
            os.mkdir(args.unpackdir)
        unpack(args.zipfile, args.unpackdir)
        os.remove(args.zipfile)
            
        


  