#!/bin/sh

#download the dumpling client
wget https://dumpling.azurewebsites.net/api/client/dumpling.py

$HELIX_PYTHONPATH dumpling.py install --full

#Set the rlimit for coredumps
echo "executing ulimit -c unlimited"
ulimit -c unlimited


echo "executing ulimit -a"
ulimit -a

echo 0x3F > /proc/self/coredump_filter

#run the specified command line
echo "executing $*"
$*

export _EXITCODE=$?
echo command exited with ExitCode: $_EXITCODE

if [ $_EXITCODE != 0 ]
then
  #This is a temporary hack workaround for the fact that the process exits before the coredump file is completely written
  #We need to replace this with a more hardened way to guaruntee that we don't zip and upload before the coredump is available
  echo "command failed waiting for coredump" 
  sleep 3m
  
  #test if the core file was created.  
  #this condition makes the assumption that the file will be name either 'core' or 'core.*' and be in the current directory which is true all the distros tested so far
  #ideally this would be constrained more by setting /proc/sys/kernel/core_pattern to a specific file to look for
  #however we don't have root permissions from this script when executing in helix so this would have to depend on machine setup
  _corefile=$(ls $PWD | grep -E --max-count=1 '^core(\..*)?$')
  
  if [ -n '$_corefile' ]
  then
    echo "uploading core to dumpling service"
    
    echo "executing $HELIX_PYTHONPATH dumpling.py install --full upload --dumppath $_corefile --noprompt --triage full --displayname $STRESS_TESTID --incpaths $HELIX_WORKITEM_ROOT/execution --properties STRESS_BUILDID=$STRESS_BUILDID STRESS_TESTID=$STRESS_TESTID"
    $HELIX_PYTHONPATH dumpling.py install --full upload --dumppath $_corefile --noprompt --triage full --displayname $STRESS_TESTID --incpaths $HELIX_WORKITEM_ROOT/execution --properties STRESS_BUILDID=$STRESS_BUILDID STRESS_TESTID=$STRESS_TESTID
  else
    echo "no coredump file was found in $PWD"
  fi

  #the following code zips and uploads the entire execution directory to the helix results store
  #it is here as a backup source of dump file storage until we are satisfied that the uploading dumps to 
  #the dumpling service is solid and complete.  After that this can be removed as it is redundant
  echo "zipping work item data for coredump analysis"
  echo "executing  $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/zip_script.py -zipFile $HELIX_WORKITEM_ROOT/$STRESS_TESTID.zip $HELIX_WORKITEM_ROOT/execution"
  $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/zip_script.py -zipFile $HELIX_WORKITEM_ROOT/$STRESS_TESTID.zip $HELIX_WORKITEM_ROOT/execution

  echo "uploading coredump zip to $HELIX_RESULTS_CONTAINER_URI$STRESS_TESTID.zip analysis"
  echo "executing  $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/upload_result.py -result $HELIX_WORKITEM_ROOT/$STRESS_TESTID.zip -result_name $STRESS_TESTID.zip -upload_client_type Blob"
  $HELIX_PYTHONPATH $HELIX_SCRIPT_ROOT/upload_result.py -result $HELIX_WORKITEM_ROOT/$STRESS_TESTID.zip -result_name $STRESS_TESTID.zip -upload_client_type Blob
fi

exit $_EXITCODE