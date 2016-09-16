# Ideally, we should use the current logged in user but this appears to require domain joining. 

param([String]$DropPat, [String]$WorkingDirectory, [String]$FilterToTestTFM, [Switch]$DebuggingBinaries = $false)

$WorkingDirectory = [System.Environment]::ExpandEnvironmentVariables($WorkingDirectory)
if(!(Test-Path $WorkingDirectory))
{
    mkdir $WorkingDirectory
}

$TestDirectory = [System.IO.Path]::Combine($WorkingDirectory, "TestArchives")
if(!(Test-Path $TestDirectory))
{
    mkdir $TestDirectory
}

$ProductDirectory = [System.IO.Path]::Combine($WorkingDirectory, "ProductPackages")
if(!(Test-Path $ProductDirectory))
{
    mkdir $ProductDirectory
}

$FetchedDataDirectory = [System.IO.Path]::Combine($WorkingDirectory, "FetchedData")
if(!(Test-Path $FetchedDataDirectory))
{
    mkdir $FetchedDataDirectory
}

######################################################################
# This script is for automating CoreCLR Reliability Runs within VSTS #
######################################################################

$VSTSAccount = "devdiv";
$VSTSDefaultCollection = "https://devdiv.artifacts.visualstudio.com/DefaultCollection";

# We use https://1eswiki.com/wiki/VSTS_Drop to retrieve our builds.
# This works well core CoreCLR Binaries (Ret/Chk) and CoreFX Binaries (Ret only)
# Traditionally we do not see much benefit to build CoreFX binaries against a Chk configuration, so we do not.
function Get-DropExe
{
    $destinationZip = [System.IO.Path]::Combine($FetchedDataDirectory, "Drop.App.zip")
    $destinationDir = [System.IO.Path]::Combine($FetchedDataDirectory, "Drop.App")
    
    $DropExe = [System.IO.Path]::Combine($destinationDir, "lib", "net45", "drop.exe")
    
    # if we have it, then return that path. otherwise...go get it.
    if((Test-Path $DropExe))
    {
       $DropExe
       return;
    }
    
    Write-Verbose "retrieving drop exe archive and storing it at $destinationZip"

    Get-FileFromUrl "https://$VSTSAccount.artifacts.visualstudio.com/DefaultCollection/_apis/drop/client/exe" $destinationZip
    
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($destinationZip, $destinationDir)
    
    $DropExe
}

# preferably targetting git repositories
# on webexception, this will sleep for 10 seconds, and then try again up to 3 times.
function Get-StringFromUrl([String] $url)
{
    $retryCount = 3

    for($i = 0; $i -le $retryCount; $i++)
    {
        Try
        {
            Write-Output (New-Object "System.Net.WebClient").DownloadString($url)
            return
        }
        Catch [System.Net.WebException]
        {
            Start-Sleep -s 10
        }
        Catch
        {
            $ErrorMessage = $_.Exception.Message
            $FailedItem = $_.Exception.ItemName
            Write-Host "Exception: $ErrorMessage"
        }
    }
}

# on webexception, this will sleep for 10 seconds, and then try again up to 3 times.
function Get-FileFromUrl([String] $sourceUrl, [String] $destination)
{
    $retryCount = 3

    # if the folder for the file does not exist, then make it.
    $destinationDirectory = [System.IO.Path]::GetDirectoryName($destination)

    if(!(Test-Path -PathType Container -Path $destinationDirectory))
    {
        Write-Verbose "$destinationDirectory does not exist, creating it now."
        mkdir $destinationDirectory
    }
    else
    {
        Write-Verbose "$destinationDirectory existed already."
        if(Test-Path -PathType Leaf $destination)
        {
            Write-Verbose "removing $destination so that it can be updated."
            rm $destination
        }
    }

    Write-Verbose "from $sourceUrl to $destination"
    for($i = 0; $i -le $retryCount; $i++)
    {
        Try
        {
            (New-Object "System.Net.WebClient").Downloadfile($sourceUrl, $destination)
            return
        }
        Catch [System.Net.WebException]
        {
            Start-Sleep -s 10
        }
        Catch
        {
            $ErrorMessage = $_.Exception.Message
            $FailedItem = $_.Exception.ItemName
            Write-Host "Exception: $ErrorMessage"
        }
    }
}

# This is my best first-guess on the way to do this. Monikers have a nasty habit of changing, I am hoping that this
# can sustain the kinds of changes I have seen historically (namely, monikers always seem to END with the really relevant info)
function Convert-BuildMonikerToBuildVersion([string]$moniker)
{
    # a build moniker looks like: beta-24401-01
    # a build version looks like       24401.01

    # Take beta-24401-01
    # Convert to beta.24401.01
    $result = $moniker.Trim().Replace("-", ".")
    # Get the index of the dot in the parenthesis: beta.24401(.)01
    $separatorIndex = $result.LastIndexOf('.')
    # Get the index of the dot in the parenthesis: beta(.)24401.01
    $precedingIndex = $result.LastIndexOf('.', $separatorIndex - 1)

    # Take the substring: beta.(24401.01)
    $result = $result.Substring($precedingIndex + 1, $result.Length - $precedingIndex - 1);
    $result.Trim() # 24401.01
}

function Get-Drop([string]$TargetDrop, [string]$LocalDirectory)
{
    Write-Verbose "retrieving drop $TargetDrop and storing it in $LocalDirectory"
    $Arguments = @('get', '--patAuth', $DropPat, '-s', $VSTSDefaultCollection, '-n', $TargetDrop, '-d', $LocalDirectory)
    & $DropExe $Arguments
}

# NOTE: At the moment we can only fetch RET builds using this approach. We are waiting for CHK builds to be pushed
# in to the drop.
function Get-ProductBinaries([string]$CoreCLRBuildMoniker,
                             [string]$CoreFXBuildMoniker)
{
    if(!(Test-Path $ProductDirectory))
    {
        mkdir $ProductDirectory
    }

    # Product binaries are laid out like this: 
    # dotnet/coreclr/master/{CoreCLRBuildMoniker}/packages
    # dotnet/corefx/master/{CoreFXBuildMoniker}/packages

    $CoreCLRDump = "$FetchedDataDirectory/CoreCLR"
    $CoreFXDump = "$FetchedDataDirectory/CoreFX"

    Write-Verbose "DROP EXE $DropExe"
    $LatestCoreCLRVersion = Convert-BuildMonikerToBuildVersion $CoreCLRBuildMoniker
    $LatestCoreFXVersion = Convert-BuildMonikerToBuildVersion $CoreFXBuildMoniker


    if($DebuggingBinaries) {
        Get-Drop "dotnet/coreclr/master/$LatestCoreCLRVersion/packages/checked" $CoreCLRDump
        Get-Drop "dotnet/corefx/master/$LatestCoreFXVersion/packages/debug" $CoreFXDump
    }
    else {
        Get-Drop "dotnet/coreclr/master/$LatestCoreCLRVersion/packages/release" $CoreCLRDump
        Get-Drop "dotnet/corefx/master/$LatestCoreFXVersion/packages/release" $CoreFXDump
    }

    # Copy from $CoreCLRDump/pkg to $ProductDirectory
    Write-Verbose "copying CoreCLR Packages to $ProductDirectory"
    Get-ChildItem -Path $CoreCLRDump/pkg -Recurse -ErrorAction SilentlyContinue -Filter *.nupkg | Copy-Item -Destination $ProductDirectory
    
    # Copy from $CoreFXDump/pkg to $ProductDirectory
    Write-Verbose "copying CoreFX Packages to $ProductDirectory"
    Get-ChildItem -Path $CoreFXDump/pkg -Recurse -ErrorAction SilentlyContinue -Filter *.nupkg | Copy-Item -Destination $ProductDirectory
}

# Since we are using CoreFX Test binaries we are using their moniker with corefx
function Get-TestBinaries([string]$CoreFXBuildMoniker)
{
    # We need test runtime info:
    # https://github.com/dotnet/corefx/blob/master/src/Common/test-runtime/project.json
    # Test binaries are laid out like this:
    # dotnet/corefx/master/{CoreFXBuildMoniker}/tests/anyos/anycpu/netcoreapp1.0
    $LatestCoreFXVersion = Convert-BuildMonikerToBuildVersion $CoreFXBuildMoniker

    Write-Verbose "retrieving test binaries with CoreFX Version: $LatestCoreFXVersion"
    # "dotnet/corefx/master/$LatestCoreFXVersion/tests/anyos/anycpu/netcoreapp1.0"
    # "dotnet/reliability/stress/prototype/test_binaries"
    Get-Drop "dotnet/corefx/master/$LatestCoreFXVersion/tests/anyos/anycpu/$FilterToTestTFM" $TestDirectory
    # Get-Drop "dotnet/corefx/master/24513.02/tests/anyos/anycpu/netcoreapp1.0" $TestDirectory
}

# Fetch CoreCLR/CoreFX Build Monikers: 
$CoreCLRBuildMoniker = Get-StringFromUrl "https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/coreclr/master/Latest.txt"
Write-Verbose "Using CoreCLR Version: $CoreCLRBuildMoniker"

$CoreFXBuildMoniker = Get-StringFromUrl "https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/corefx/master/Latest.txt"
Write-Verbose "Using CoreFX Version: $CoreFXBuildMoniker"

#retrieve the drop tool - we use this to pull the rest of our binaries
$DropExe = Get-DropExe
Write-Verbose "Drop.exe is located at: $DropExe"
Get-ProductBinaries $CoreCLRBuildMoniker $CoreFXBuildMoniker
Get-TestBinaries $CoreFXBuildMoniker
