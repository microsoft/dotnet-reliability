:: Need a build-from commandlet
:: navigate to S:
cd /d %CloudFileDrive%

git clone %SYSTEM_DEFAULTWORKINGDIRECTORY% %BUILD_BUILDNUMBER%

cd %BUILD_BUILDNUMBER%

call .\build.cmd
call .\build_test.cmd

