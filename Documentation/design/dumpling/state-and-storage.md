#State Table
Azure Storage Table

Right before we begin receiving the dumpfile, we generate a dumpling-id. Upon generating the dumpling-id we create a row in the state table. The values within the state table update as dump moves through the pipeline. Upon creation the default state is 'uploading', and all of the uri values and the flight-messages are 'null'.

| owner | dumpling-id | timestamp | state | symbols-uri | dump-uri | results-uri | messages |
|---|---|---|---|---|---|---|---|
|a |b | c | d |e |f | g | h |
d: **possible states**
- uploading
- enqueued
- downzipping
- downloading symbols
- analyzing
- saving results
- building report
- done
- error

h: *messages*
- JSON array of "message_type","message" objects

#Storage

**`todo`**Blob storage for the dump zip. Container names will be the value of 'owner' in the state table for the corresponding `dumpling-id`.

**`stable`** Blob storage for the results json object. The container names will be the 'owner' in the state table for the corresponding `dumpling-id`. Then the first level of directory will be the `dumpling-id`. The `filename.zip` is preserved from upload.

`todo`*Symbols VSTS* will store our symbol files - before this can be brought up, we must begin indexing our symbols in VSTS. There is a preview program available that I want to leverage here.

