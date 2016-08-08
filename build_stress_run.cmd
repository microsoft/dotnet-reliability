:: Need a build-from commandlet
:: navigate to our directory

:: TODO:
::    -- Error Handling
::    -- Limit number of builds we keep around. When we reach max: as we create a build, we should remove a build.
::    -- Change naming schema of directories.

cd /d %CloudFileDrive%

mkdir %BUILD_BUILDNUMBER%

set GeneratedRootPath=%CloudFileDrive%\%BUILD_BUILDNUMBER%\test\
set BaseIntermediateOutputPath=%CloudFileDrive%\%BUILD_BUILDNUMBER%

