
$toolsdir="C:\work\stress_runs\tools"

# TODO: Retrieve tests appropriately. At the moment they're just placed on the developer box and fixed there.
#$testdir = "C:\temp\workdir\test"
$productdir = "E:\stress_work\working_directories\product";
$chkwrkdir="E:\stress_work\working_directories\chk"
$retwrkdir="E:\stress_work\working_directories\ret"

#Set environment variables for Visual Studio Command Prompt
pushd "$env:VS140COMNTOOLS"
cmd /c "vsvars32.bat&set" |
foreach {
  if ($_ -match "=") {
    $v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
  }
}
popd
write-host "`nVisual Studio 2015 Command Prompt variables set." -ForegroundColor Yellow

## NOT TESTED
function Get-ProductBinaries
{
    # Copy the build binaries to some staging location.
    start-process $toolsdir\getbuild.bat \\cpvsbuild\drops\dev14\ProjectK\raw\24306.00 $productdir
}

## TESTED - WORKS WELL
function Remove-Packages([string]$wrkdir)
{
    pushd

    if(-not (Test-Path $wrkdir))
    {
        popd # if cd is going to fail because it doesnt exist, it's better to just get out of here now before rmdir gets a chance to do anything.
    }

    cd $wrkdir

    # reset packages
    rmdir packages -Recurse

    popd
}

## TESTED - WORKS NICELY
function Create-StressRepository([string]$wrkdir)
{
    echo "spawning git repository at $wrkdir"

    if(-not (Test-Path $wrkdir))
    {
        echo "creating directory $wrkdir"
        mkdir $wrkdir
    }

    pushd $wrkdir

    #clone the repo and proceed to it.
    git clone http://www.github.com/Microsoft/dotnet-reliability
    cd dotnet-reliability

    # build stress tools
    .\build.cmd /t:rebuild

    popd
}

# TESTED - WORK WELL
function Start-StressChk
{
    echo "initializing working directory $chkwrkdir"
    Create-StressRepository $chkwrkdir

    Remove-Packages $chkwrkdir
    
    [System.IO.Path]::Combine($chkwrkdir, "dotnet-reliability", "test") | pushd

    msbuild genstress.proj @$toolsdir\buildcentoschk.rsp /m:4
    msbuild genstress.proj @$toolsdir\buildrhelchk.rsp /m:4
    msbuild genstress.proj @$toolsdir\buildubuntuchk.rsp /m:4
}

# TESTED - WORK WELL
function Start-StressRet
{
    echo "initializing working directory $retwrkdir"
    Create-StressRepository $retwrkdir

    Remove-Packages $retwrkdir
    
    [System.IO.Path]::Combine($retwrkdir, "dotnet-reliability", "test") | pushd

    msbuild genstress.proj @$toolsdir\buildcentosret.rsp /m:4
    msbuild genstress.proj @$toolsdir\buildrhelret.rsp /m:4
    msbuild genstress.proj @$toolsdir\buildubunturet.rsp /m:4
}
