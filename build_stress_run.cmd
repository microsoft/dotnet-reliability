:: TODO:
::    -- Error Handling
::    -- Limit number of builds we keep around. When we reach max: as we create a build, we should remove a build.
::    -- Change naming schema of directories.

cd /d %CloudFileDrive%

mkdir %BUILD_BUILDNUMBER%

robocopy %BUILD_SOURCESDIRECTORY% %CloudFileDrive%\%BUILD_BUILDNUMBER% /MIR

call %CloudFileDrive%\%BUILD_BUILDNUMBER%\build.cmd
