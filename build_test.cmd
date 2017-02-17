@echo off
setlocal

:: Note: We've disabled node reuse because it causes file locking issues.
::       The issue is that we extend the build with our own targets which
::       means that that rebuilding cannot successfully delete the task
::       assembly. 

if not defined VisualStudioVersion (
    if defined VS140COMNTOOLS (
        call "%VS140COMNTOOLS%\VsDevCmd.bat"
        goto :EnvSet
    )

    if defined VS120COMNTOOLS (
        call "%VS120COMNTOOLS%\VsDevCmd.bat"
        goto :EnvSet
    )

    echo Error: build.cmd requires Visual Studio 2013 or 2015.  
    echo        Please see https://github.com/dotnet/corefx/blob/master/Documentation/developer-guide.md for build instructions.
    exit /b 1
)

:EnvSet
call msbuild test/genstress.proj /maxcpucount %*
set BUILDERRORLEVEL=%ERRORLEVEL%

exit /b %BUILDERRORLEVEL%
