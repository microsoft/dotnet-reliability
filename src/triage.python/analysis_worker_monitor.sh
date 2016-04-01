#!/bin/bash
echo "Starting reliability worker." >>./analysis_worker_log.log 2>&1 # pipe both std err and stdout to this file.
until ./analysis_worker.py >>./analysis_worker_log.log 2>&1; do 
  echo "Reliability worker crashed with non-zero exit code: $?. Respawning..." >> ./analysis_worker_log.log 2>&1
  sleep 1
done
