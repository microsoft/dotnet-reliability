# `dumpling` Service REST API
`dumpling` should be accessible from any device that the supported DotNet runtimes are supported on. In order to enable this we're creating a simple REST interface so that a zip can be submitted from any platform.

[Swagger View of API](http://dotnetrp.azurewebsites.net/swagger/ui/index)

A *`dumplingid`* is a unique identifier that is assigned to a zipped up dump at the time it is uploaded. It is typed as a 'string', but its contents are nothing more than a GUID. This is *the* unique identifier of a zip file.



