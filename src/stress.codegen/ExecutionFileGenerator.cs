// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using stress.execution;

namespace stress.codegen
{
    public class ExecutionFileGeneratorLinux : ISourceFileGenerator
    {
        private const string STRESS_SCRIPT_NAME = "stress.sh";
        /*
         * scriptName - name of the script to be generated, this should include the path
         * testName - the name of the test executable
         * arguments - arguments to be passed to the test executable
         * envVars - dictionary of environment variables and their values
         * host (optional) - needed for hosted runtimes
         */
        public void GenerateSourceFile(LoadTestInfo loadTestInfo)
        {
            string shellScriptPath = Path.Combine(loadTestInfo.SourceDirectory, STRESS_SCRIPT_NAME);

            using (TextWriter stressScript = new StreamWriter(shellScriptPath, false))
            {
                // set the line ending for shell scripts
                stressScript.NewLine = "\n";

                stressScript.WriteLine("#! /bin/sh");
                stressScript.WriteLine();
                stressScript.WriteLine();

                stressScript.WriteLine("# stress script for {0}", loadTestInfo.TestName);
                stressScript.WriteLine();
                stressScript.WriteLine();

                stressScript.WriteLine("# environment section");
                // first take care of the environment variables
                foreach (KeyValuePair<string, string> kvp in loadTestInfo.EnvironmentVariables)
                {
                    stressScript.WriteLine("export {0}={1}", kvp.Key, kvp.Value);
                }
                stressScript.WriteLine();
                stressScript.WriteLine();

                // The default limit for coredumps on Linux and Mac is 0 and needs to be reset to allow core dumps to be created
                stressScript.WriteLine("# The default limit for coredumps on Linux and Mac is 0 and this needs to be reset to allow core dumps to be created");
                stressScript.WriteLine("echo calling [ulimit -c unlimited]");
                stressScript.WriteLine("ulimit -c unlimited");
                
                // report the current limits (in theory this should get into the test log)
                stressScript.WriteLine("echo calling [ulimit -a]");
                stressScript.WriteLine("ulimit -a");
                
                //update the coredump collection filter 
                stressScript.WriteLine("echo 0x3F > /proc/self/coredump_filter");
                stressScript.WriteLine();
                stressScript.WriteLine();

                // Prepare the test execution line
                string testCommandLine = loadTestInfo.TestName + ".exe";

                // If there is a host then prepend it to the test command line
                if (!String.IsNullOrEmpty(loadTestInfo.SuiteConfig.Host))
                {
                    testCommandLine = loadTestInfo.SuiteConfig.Host + " " + testCommandLine;
                    // If the command line isn't a full path or ./ for current directory then add it to ensure we're using the host in the current directory
                    if ((!loadTestInfo.SuiteConfig.Host.StartsWith("/")) && (!loadTestInfo.SuiteConfig.Host.StartsWith("./")))
                    {
                        testCommandLine = "./" + testCommandLine;
                    }
                }
                stressScript.WriteLine("# test execution");

                stressScript.WriteLine("echo calling [{0}]", testCommandLine);
                stressScript.WriteLine(testCommandLine);
                // Save off the exit code
                stressScript.WriteLine("export _EXITCODE=$?");

                stressScript.WriteLine("echo test exited with ExitCode: $_EXITCODE");

                // Check the return code
                stressScript.WriteLine("if [ $_EXITCODE != 0 ]");

                stressScript.WriteLine("then");

                //This is a temporary hack workaround for the fact that the process exits before the coredump file is completely written
                //We need to replace this with a more hardened way to guaruntee that we don't zip and upload before the coredump is available
                stressScript.WriteLine("  echo Work item failed waiting for coredump...");
                stressScript.WriteLine("  sleep 2m");

                //test if the core file was created.  
                //this condition makes the assumption that the file will be name either 'core' or 'core.*' which is true all the distros tested so far
                //ideally this would be constrained more by setting /proc/sys/kernel/core_pattern to a specific file to look for
                //however we don't have root permissions from this script when executing in helix so this would have to depend on machine setup
                stressScript.WriteLine(@"  _corefile=$(ls $HELIX_WORKITEM_ROOT/execution | grep -E --max-count=1 '^core(\..*)?$')");
                stressScript.WriteLine("  if [ -n '$_corefile' ]");
                stressScript.WriteLine("  then");

                //if the file core file was produced upload it to the dumpling service 
                stressScript.WriteLine("    echo zipping and uploading core to dumpling service");

                stressScript.WriteLine($"    echo EXEC:  $HELIX_PYTHONPATH ./dumpling.py upload --corefile $HELIX_WORKITEM_ROOT/execution/$_corefile --zipfile $HELIX_WORKITEM_ROOT/{loadTestInfo.TestName}.zip --addpaths $HELIX_WORKITEM_ROOT/execution");

                stressScript.WriteLine($"    $HELIX_PYTHONPATH ./dumpling.py upload --corefile $_corefile --zipfile $HELIX_WORKITEM_ROOT/{loadTestInfo.TestName}.zip --addpaths $HELIX_WORKITEM_ROOT/execution");

                stressScript.WriteLine("  else");

                //if the core file was not 
                stressScript.WriteLine("    echo no coredump file '$HELIX_WORKITEM_ROOT/execution/core' was found");

                stressScript.WriteLine("  fi");

                //the following code zips and uploads the entire execution directory to the helix results store
                //it is here as a backup source of dump file storage until we are satisfied that the uploading dumps to 
                //the dumpling service is solid and complete.  After that this can be removed as it is redundant
                stressScript.WriteLine("  echo zipping work item data for coredump analysis");
                stressScript.WriteLine($"  echo EXEC:  $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/zip_script.py $HELIX_WORKITEM_ROOT/{loadTestInfo.TestName}.zip $HELIX_WORKITEM_ROOT/execution");
                stressScript.WriteLine($"  $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/zip_script.py -zipFile $HELIX_WORKITEM_ROOT/{loadTestInfo.TestName}.zip $HELIX_WORKITEM_ROOT/execution");
                stressScript.WriteLine($"  echo uploading coredump zip to $HELIX_RESULTS_CONTAINER_URI{loadTestInfo.TestName}.zip analysis");
                stressScript.WriteLine($"  echo EXEC: $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/upload_result.py -result $HELIX_WORKITEM_ROOT/{loadTestInfo.TestName}.zip -result_name {loadTestInfo.TestName}.zip -upload_client_type Blob");
                stressScript.WriteLine($"  $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/upload_result.py -result $HELIX_WORKITEM_ROOT/{loadTestInfo.TestName}.zip -result_name {loadTestInfo.TestName}.zip -upload_client_type Blob");

                stressScript.WriteLine("fi");

                // exit the script with the return code
                stressScript.WriteLine("exit $_EXITCODE");
            }

            // Add the shell script to the source files
            loadTestInfo.SourceFiles.Add(new SourceFileInfo(STRESS_SCRIPT_NAME, SourceFileAction.Binplace));

            //this script also depends on dumpling.py so add this to the source files
            loadTestInfo.SourceFiles.Add(new SourceFileInfo(@"$([MSBuild]::GetDirectoryNameOfFileAbove($(MSBuildThisFileDirectory), Build.cmd))\src\triage.python\dumpling.py", SourceFileAction.Binplace));
        }
        
    }

    public class ExecutionFileGeneratorWindows : ISourceFileGenerator
    {
        private const string STRESS_SCRIPT_NAME = "stress.bat";
        public void GenerateSourceFile(LoadTestInfo loadTestInfo)// (string scriptName, string testName, Dictionary<string, string> envVars, string host = null)
        {
            string batchScriptPath = Path.Combine(loadTestInfo.SourceDirectory, STRESS_SCRIPT_NAME);

            using (TextWriter stressScript = new StreamWriter(batchScriptPath, false))
            {
                stressScript.WriteLine("@echo off");
                stressScript.WriteLine("REM stress script for " + loadTestInfo.TestName);
                stressScript.WriteLine();
                stressScript.WriteLine();

                stressScript.WriteLine("REM environment section");
                // first take care of the environment variables
                foreach (KeyValuePair<string, string> kvp in loadTestInfo.EnvironmentVariables)
                {
                    stressScript.WriteLine("set {0}={1}", kvp.Key, kvp.Value);
                }
                stressScript.WriteLine();
                stressScript.WriteLine();

                // Prepare the test execution line
                string testCommandLine = loadTestInfo.TestName + ".exe";

                // If there is a host then prepend it to the test command line
                if (!String.IsNullOrEmpty(loadTestInfo.SuiteConfig.Host))
                {
                    testCommandLine = loadTestInfo.SuiteConfig.Host + " " + testCommandLine;
                }
                stressScript.WriteLine("REM test execution");
                stressScript.WriteLine("echo calling [{0}]", testCommandLine);
                stressScript.WriteLine(testCommandLine);
                // Save off the exit code
                stressScript.WriteLine("set _EXITCODE=%ERRORLEVEL%");
                stressScript.WriteLine("echo test exited with ExitCode: %_EXITCODE%");
                stressScript.WriteLine();
                stressScript.WriteLine();

                //                // Check the return code
                //                stressScript.WriteLine("if %_EXITCODE% EQU 0 goto :REPORT_PASS");
                //                stressScript.WriteLine("REM error processing");
                //                stressScript.WriteLine("echo JRS - Test Failed. Report the failure, call to do the initial dump analysis, zip up the directory and return that along with an event");
                //                stressScript.WriteLine("goto :END");
                //                stressScript.WriteLine();
                //                stressScript.WriteLine();
                //
                //                stressScript.WriteLine(":REPORT_PASS");
                //                stressScript.WriteLine("echo JRS - Test Passed. Report the pass.");
                //                stressScript.WriteLine();
                //                stressScript.WriteLine();

                // exit the script with the exit code from the process
                stressScript.WriteLine(":END");
                stressScript.WriteLine("exit /b %_EXITCODE%");
            }

            // Add the batch script to the source files
            loadTestInfo.SourceFiles.Add(new SourceFileInfo(STRESS_SCRIPT_NAME, SourceFileAction.Binplace));
        }
    }
}