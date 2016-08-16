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

:: The property FilterToTestTFM is temporarily required because of  https://github.com/dotnet/buildtools/commit/e9007c16b1832dbd0ea9669fa578b61900b7f724 
call msbuild test/genstress.proj /verbosity:%msbuildverbosity% /maxcpucount /p:BuildInParallel=true /p:CloudDropAccessToken=%1 /p:CloudResultsAccessToken=%2 /p:BuildCompleteConnection=%3 /p:HelixApiAccessKey=%4 /p:HelixApiEndPoint=%5 /p:FilterToTestTFM=netcoreapp1.0
set BUILDERRORLEVEL=%ERRORLEVEL%


echo.
:: Pull the build summary from the log file
findstr /ir /c:".*Warning(s)" /c:".*Error(s)" /c:"Time Elapsed.*" "%_buildlog%"
echo Build Exit Code = %BUILDERRORLEVEL%

exit /b %BUILDERRORLEVEL%
