# `dumpling` REST API
`dumpling` should be accessible from any device that the supported DotNet runtimes are supported on. In order to enable this we're creating a simple REST interface so that a zip can be submitted from any platform.


|state| api | parameters | returns | description |
|---|---|---|---|---|
|`todo` | upload-core-dump-zip | `owner-identifier`, `dump-data` | (*string*) `dumpling-id` | upload a zip file that contains a core dump, as well as the runtime artifacts responsible for the crash. |
|`todo` | get-status | `dumpling-id` | (*string*) `status` | after a dump has been uploaded, we enqueue it for analysis. To find out how far along in the process a dump submission is, you can query its state using this api. |
|`todo` | download-core-dump-zip | `dumpling-id` | (*binary*) `zip-file` | download a previously uploaded dump zip file. |

A *`dumpling-id`* is a unique identifier that is assigned to a zipped up core dump at the time it is uploaded. It is typed as a 'string', but its contents are nothing more than a GUID. This is *the* unique identifier of a zip file.



