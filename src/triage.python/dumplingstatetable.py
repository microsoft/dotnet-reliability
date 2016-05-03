import os

from datetime import date
from datetime import timedelta

from azure.storage      import CloudStorageAccount, AccessPolicy, AccountPermissions
from dumpling_util      import Logging

import nearby_config
_config = nearby_config.named('analysis_worker_config.json')

s_storage_account        = CloudStorageAccount(_config.STORAGE_ACCOUNT_NAME, _config.STORAGE_ACCOUNT_KEY)
s_blob_service           = s_storage_account.create_block_blob_service()
s_tableService 	         = s_storage_account.create_table_service();

class DumplingStateContext:    
    def Update(self):
        Logging.Verbose('Setting dumpling state to "%s"' % self.data['State'])
        s_tableService.insert_or_replace_entity(_config.STORAGE_ACCOUNT_STATE_TABLE_NAME, self.data);

    def SetState(self, newState):
        self.data['State'] = str(newState)
        self.Update()
 
    def SaveResult(self, result, testContext):
        Logging.Event('SaveResults', 9)
        
        # unpack our tuple
        path = testContext[0]
        correlationId = testContext[1]
        jobId = testContext[2]
        testName = testContext[3]
        
        containerName = self.data['PartitionKey']
        blobName = os.path.join(self.data['PartitionKey'], correlationId, jobId, testName)

        # put results in to blob storage
        s_blob_service.create_blob_from_text(containerName, blobName, result)

        blob_sas = s_blob_service.generate_blob_shared_access_signature(
            container_name=containerName,
            blob_name=blobName,
            expiry=(date.today() + timedelta(days = 365)).isoformat(), # expire in one year
            permission=AccountPermissions.READ)

        # update the dumpling state table with the results uri
        self.data['Results_uri'] = s_blob_service.make_blob_url(containerName, blobName, sas_token = blob_sas)
        self.Update()
        
    def __init__(self, owner, dumpling_id, download_uri):
    
	    # sanity
        Logging.Verbose('OWNER: ' + str(owner)) 
        Logging.Verbose('DUMPLING ID: ' + str(dumpling_id))
        Logging.Verbose('DUMP URI: ' + str(download_uri))
        
        # we manage state in a dictionary to fit in to the azure storage apis.
        self.data = {} ;
        
        self.data['PartitionKey'] = str(owner)
        self.data['RowKey'] = str(dumpling_id)
        self.data['DumpRelics_uri'] = str(download_uri)
        self.data['Results_uri']= 'null'

