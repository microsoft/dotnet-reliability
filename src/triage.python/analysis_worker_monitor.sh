#!/bin/bash

# This is our current directory.
DIR="$( cd "$( dirname "$0")" && pwd)"

echo ${DIR}
echo "Starting reliability worker in directory: ${DIR}" >>${DIR}/analysis_worker_log.log 2>&1 # pipe both std err and stdout to this file.
until ${DIR}/analysis_worker.py >> ${DIR}/analysis_worker_log.log 2>&1; do 
  echo "Reliability worker crashed with non-zero exit code: $?. Respawning..." >> ${DIR}/analysis_worker_log.log 2>&1
  sleep 1
done
