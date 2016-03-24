using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Net;

namespace dumps
{
    class JobMetadataHelper
    {
        private const int CONFIG_RETRY_COUNT = 3;

        CloudBlobContainer _boundContainer = null;
        private string _blobId;

        public string WorkerState
        {
            get
            {
                _boundContainer.FetchAttributes(); // bad pattern?

                if (_boundContainer.Metadata.ContainsKey("worker_state"))
                {
                    return _boundContainer.Metadata["worker_state"];
                }

                return "NO-WORKER-STATE-VALUE";
            }
        }

        public string JobCount
        {
            get
            {
                _boundContainer.FetchAttributes(); // bad pattern?

                if (_boundContainer.Metadata.ContainsKey("job_count"))
                {
                    return _boundContainer.Metadata["job_count"];
                }

                return "NO-JOB-COUNT-VALUE";
            }
        }
        public string StartTime
        {
            get
            {
                _boundContainer.FetchAttributes();

                // stored it UTC ticks.
                if (_boundContainer.Metadata.ContainsKey("job_count"))
                {
                    return _boundContainer.Metadata["job_count"];
                }

                return "0";
            }
        }
        public string JobCountCompleted
        {
            get
            {
                _boundContainer.FetchAttributes();

                if (_boundContainer.Metadata.ContainsKey("job_count_completed"))
                {
                    return _boundContainer.Metadata["job_count_completed"];
                }

                return "NO-JOB-COUNT-COMPLETED-VALUE";
            }
        }
        public DateTimeOffset? LastUpdate
        {
            get
            {
                _boundContainer.FetchAttributes();

                return _boundContainer.Properties.LastModified;
            }
        }

        public CloudBlobContainer BoundContainer
        {
            get
            {
                _boundContainer.FetchAttributes();

                return _boundContainer;
            }
        }

        public JobMetadataHelper(BlobHelper helper, string containerName)
        {
            _boundContainer = helper.GetContainerAsync(containerName).Result;
            CommonConstructor();
        }

        public JobMetadataHelper(CloudBlobContainer containerToBindTo)
        {
            _boundContainer = containerToBindTo;
            CommonConstructor();
        }

        private void CommonConstructor()
        {
            _blobId = _boundContainer.Name;
        }

        // For readability.
        static KeyValuePair<string, string> METADATA(string key, string value)
        {
            return new KeyValuePair<string, string>(key, value);
        }

        public void IncrementJobCount(int delta)
        {
            SafeOptimisticMetadataSet<int>(IncrementJobCountInternal, delta);
        }

        public void SetStartTimeToNow()
        {
            SafeOptimisticMetadataSet<string>(SetStartTimeInternal, DateTimeOffset.Now.UtcTicks.ToString());
        }

        public void IncrementJobCompletedCount(int count = 1)
        {
            SafeOptimisticMetadataSet<int>(IncrementJobCountInternal, count);
        }

        #region PRIVATES
        /// <summary>
        /// Note that I am using an optimistic concurrency technique here with up to CONFIG_RETRY_COUNT retries. It is my 
        /// opinion that a majority of the time race conditions will succeed the first time through and that conflicts are super rare, hence
        /// why I would rather not even bother with pessimistic concurrency like taking a lease and releasing it, etc.
        /// </summary>
        /// <param name="delta"></param>
        private void SafeOptimisticMetadataSet<T>(Action<T> work, T data)
        {
            for (int i = 0; i < CONFIG_RETRY_COUNT; i++)
            {
                string eTag = _boundContainer.Properties.ETag;

                work(data);

                try
                {
                    _boundContainer.SetMetadata(accessCondition: AccessCondition.GenerateIfMatchCondition(eTag));
                    return;
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode != (int)HttpStatusCode.PreconditionFailed)
                    {
                        Console.WriteLine($"UNEXPECTED ERROR: Exception {ex}");
                    }
                }  // if the ETag is out of date because of a race, we'll just retry a few times, otherwise print the exception and also try again.
            }
        }
        private void SetStartTimeInternal(string value)
        {
            if (_boundContainer.Metadata.ContainsKey("start_time"))
            {
                _boundContainer.Metadata["start_time"] = value;
            }
            else
            {
                _boundContainer.Metadata.Add(METADATA("start_time", value));
            }
        }

        private void IncrementJobCountInternal(int delta)
        {
            // if a value is stored already, then we're going to retrieve this value, and then update it.
            if (_boundContainer.Metadata.ContainsKey("job_count"))
            {
                int jobCount = 0;
                var currentCount = _boundContainer.Metadata["job_count"];

                if (int.TryParse(currentCount, out jobCount))
                {
                    jobCount += delta;
                    _boundContainer.Metadata["job_count"] = jobCount.ToString();
                }
                else
                {
                    Console.WriteLine("UNEXPECTED ERROR: Job count is not a parseable metadata value. Someone may have tampered with this value.\r\nThis will likely affect tooling that depends on this metadata value.");
                    return;
                }
            }
            else // otherwise, just set it.
            {
                _boundContainer.Metadata.Add(METADATA("job_count", delta.ToString()));
            }
        }

        private void IncrementJobCountCompletedInternal(int delta)
        {
            // if a value is stored already, then we're going to retrieve this value, and then update it.
            if (_boundContainer.Metadata.ContainsKey("job_count_completed"))
            {
                int jobCount = 0;
                var currentCount = _boundContainer.Metadata["job_count_completed"];

                if (int.TryParse(currentCount, out jobCount))
                {
                    jobCount += delta;
                    _boundContainer.Metadata["job_count_completed"] = jobCount.ToString();
                }
                else
                {
                    Console.WriteLine("UNEXPECTED ERROR: Job count completed is not a parseable metadata value. Someone may have tampered with this value.\r\nThis will likely affect tooling that depends on this metadata value.");
                    return;
                }
            }
            else // otherwise, just set it.
            {
                _boundContainer.Metadata.Add(METADATA("job_count_completed", delta.ToString()));
            }
        }
        #endregion // PRIVATES
    }
}
