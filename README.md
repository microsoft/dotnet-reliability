## the `dumpling` service
###http://aka.ms/dumpling

# dotnet-reliability
Contains the tooling and infrastructure used for .NET stress testing and reliability investigation.  More specifically this includes tools for, but not limited to :

- authoring component specific stress and load tests 
- generating stress tests from existing framework and runtime tests  
- unified dump collection, storage, and bucketing across all .NET supported platforms
- investigating and diagnosis of reliability failures 
 
Steps to Build:
build.cmd - builds stress tooling
build_test.cmd should run
```
msbuild test\genstress.proj /p:CoreFxCloudDropAccessToken="" /p:CoreClrCloudDropAccessToken="" /p:HelixApiAccessKey="" /p:BuildPAT="" /p:OperatingSystem="Ubuntu14.04" /flp:v=diag
```