$workingDirectory=$env:TEMP
$productArchitecture="TBD - passed in by VSTS likely."
$productConfiguration="chk/ret: TBD - passed in by VSTS likely."

$productDirectory="$workingDirectory/product/$productArchitecture/$productConfiguration"
$testsDirectory="$workingDirectory/tests/"

# filled in by GET-DropExe
$dropExe = ""; 

# NOT TESTED
#TODO: Naming
# We use https://1eswiki.com/wiki/VSTS_Drop to retrieve our artifacts from storage.
function Get-DropExe
{
    # Download the client from your VSTS account to TEMP/Drop.App/lib/net45/drop.exe
    $account = "devdiv" # Your VSTS account name is the first component of your custom visualstudio.com URL
    $sourceUrl = "https://$account.artifacts.visualstudio.com/DefaultCollection/_apis/drop/client/exe"
    $destinationZip = [System.IO.Path]::Combine($workingDirectory, "Drop.App.zip")
    $destinationDir = [System.IO.Path]::Combine($workingDirectory, "Drop.App")
    $dropExe = [System.IO.Path]::Combine($destinationDir, "lib", "net45", "drop.exe")
    $webClient = New-Object "System.Net.WebClient"
    $webClient.Downloadfile($sourceUrl, $destinationZip)
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::ExtractToDirectory($destinationZip, $destinationDir)
    Write-Host $dropExe
}

# NOT TESTED
function Get-ProductBinaries
{
    if(!(Test-Path $productDirectory))
    {
        mkdir $productDirectory
    }
    # use $dropexe to copy binaries to staging directory

    # TODO:
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
#retrieve the drop tool - we use this to pull the rest of our binaries
Get-DropExe
Get-ProductBinaries
Get-TestBinaries

# MS Build will now be executed by VSTS.


