##########################################################
# This script is for automating CoreCLR Reliability Runs #
##########################################################

$VSTSAccount = "devdiv";
$VSTSDefaultCollection = "https://$account.artifacts.visualstudio.com/DefaultCollection";

# Use VSTS to set environment variable for the product (LocalPackageSource)
# Use VSTS to set UnitTestDirectory
# Use VSTS to set Product Architecture
# Use VSTS to set Product Configuration

$workingDirectory=$env:TEMP

$FetchedDataDirectory="$workingDirectory/FetchedData/"
$ProductPackagesDirectory="$workingDirectory/ProductPackages/"
$TestDirectory="$workingDirectory/TestPackages/"

# filled in by GET-DropExe
$DropExe = ""; 

# TODO: Relearn PowerShell Naming conventions - then apply it to this.
# We use https://1eswiki.com/wiki/VSTS_Drop to retrieve our builds.
# This works well core CoreCLR Binaries (Ret/Chk) and CoreFX Binaries (Ret only)
# Traditionally we do not see much benefit to build CoreFX binaries against a Chk configuration, so we do not.
function Get-DropExe
{
    $destinationZip = [System.IO.Path]::Combine($workingDirectory, "Drop.App.zip")
    $destinationDir = [System.IO.Path]::Combine($workingDirectory, "Drop.App")
    $DropExe = [System.IO.Path]::Combine($destinationDir, "lib", "net45", "drop.exe")

    if((Test-Path $DropExe))
    {
       $DropExe
       return;
    }
    
    # Download the client from your VSTS account to TEMP/Drop.App/lib/net45/drop.exe
    $sourceUrl = "https://$VSTSAccount.artifacts.visualstudio.com/DefaultCollection/_apis/drop/client/exe"

    $webClient = New-Object "System.Net.WebClient"
    $webClient.Downloadfile($sourceUrl, $destinationZip)
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    [System.IO.Compression.ZipFile]::ExtractToDirectory($destinationZip, $destinationDir)
    
    $DropExe
}

function Get-BuildMoniker([string]$url)
{
    Write-Output (New-Object "System.Net.WebClient").DownloadString($url)
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

# NOTE: At the moment we can only fetch RET builds using this approach. We are waiting for CHK builds to be pushed
# in to the drop.
function Get-ProductBinaries([string]$CoreCLRBuildMoniker,
                                [string]$CoreFXBuildMoniker)
{
    if(!(Test-Path $ProductPackagesDirectory))
    {
        mkdir $ProductPackagesDirectory
    }

    # Product binaries are laid out like this: 
    # dotnet/coreclr/master/{CoreCLRBuildMoniker}/packages
    # dotnet/corefx/master/{CoreFXBuildMoniker}/packages

    $CoreCLRDump = "$FetchedDataDirectory/CoreCLR"
    $CoreFXDump = "$FetchedDataDirectory/CoreFX"

    echo "DROP EXE $DropExe"
    $LatestCoreCLRVersion = Convert-BuildMonikerToBuildVersion $CoreCLRBuildMoniker
    echo "Converting Moniker To Version: $CoreCLRBuildMoniker => $LatestCoreCLRVersion"

    $LatestCoreFXVersion = Convert-BuildMonikerToBuildVersion $CoreFXBuildMoniker
    echo "Converting Moniker To Version: $CoreFXBuildMoniker => $LatestCoreFXVersion"

    echo "Attempting to download latest CoreCLR : dotnet/coreclr/master/$LatestCoreCLRVersion/packages"
    $CoreCLRDropArguments = @('get', '-s', $VSTSDefaultCollection, '-n', "dotnet/coreclr/master/$LatestCoreCLRVersion/packages", '-d', $CoreCLRDump)
    & $DropExe $CoreCLRDropArguments

    echo "Attempting to download latest CoreFX : dotnet/coreclr/master/$LatestCoreFXVersion/packages"
    $CoreFXArguments = @('get', '-s', $VSTSDefaultCollection, '-n', "dotnet/corefx/master/$LatestCoreFXVersion/packages", '-d', $CoreFXDump)
    &$DropExe $CoreFXArguments

    # TODO:
    # Copy from $CoreCLRDump/pkg to $ProductPackagesDirectory
    # Copy from $CoreFXDump/pkg to $ProductPackagesDirectory
}



function Get-TestBinaries
{
    if(!(Test-Path $testsDirectory))
    {
        mkdir $testsDirectory
    }

# use $dropexe to copy binaries to staging directory
 #TODO:
}


# This script is executed by VSTS.

# Fetch CoreCLR/CoreFX Build Monikers: 
$CoreCLRBuildMoniker = Get-BuildMoniker "https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/coreclr/master/Latest.txt"
echo "Using CoreCLR Version: $CoreCLRBuildMoniker"

$CoreFXBuildMoniker = Get-BuildMoniker "https://raw.githubusercontent.com/dotnet/versions/master/build-info/dotnet/corefx/master/Latest.txt"
echo "Using CoreFX Version: $CoreFXBuildMoniker"

#retrieve the drop tool - we use this to pull the rest of our binaries
$DropExe = Get-DropExe
echo "Using $DropExe"
Get-ProductBinaries $CoreCLRBuildMoniker $CoreFXBuildMoniker

#Get-TestBinaries

# MS Build will then be executed by VSTS.


