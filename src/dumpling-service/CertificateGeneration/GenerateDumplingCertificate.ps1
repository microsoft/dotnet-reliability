# To create a certificate, navigate to our internal site //ssladmin/
$globalVar_resourceGroupName = "dumpling_rg"
$globalVar_USLocation = "West US"
$globalVar_keyVaultName = "dumplingVault"
$globalVar_dumplingDisplayName = "dotnetdumpling"

# note that if the resources exist, the user that runs this function will be prompted so there is nothing to worry about
# with regards to running this function multiple times.
function Create-DumplingKeyVault
{
	#login to Azure
    Login-AzureRmAccount

	#create the dumpling resource group if it doesn't exist.
	New-AzureRmResourceGroup –Name $globalVar_resourceGroupName –Location $globalVar_USLocation
	
	# now that we certainly have our resource group, we will create the key vault within.
	New-AzureRmKeyVault -VaultName $globalVar_keyVaultName -ResourceGroupName $globalVar_resourceGroupName -Location $globalVar_USLocation
}

# To create a securePassword, use this:
#$securepfxpwd = ConvertTo-SecureString –String 'password-to-cert-goes-here' –AsPlainText –Force
function Add-CertificateToKeyVault([string] $secretKey, [SecureString] $securePassword)
{
	# the destination keyword here can be either HSM (Hardware Security Module) or Software (software-protected key)
	Add-AzureKeyVaultKey -VaultName $globalVar_keyVaultName -Name $secretKey -KeyFilePath $pathToPfx -KeyFilePassword $securePassword -Destination Software
}

# TODO: Load in real certificate with legitimate start date and end date.
function Register-DumplingWithAzureAD
{
	#login to Azure
    Login-AzureRmAccount

	$x509 = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2

	$x509.Import("E:\Secrets\dotnetrp.azurewebsites.net.cer")

	$credValue = [System.Convert]::ToBase64String($x509.GetRawCertData())
	$startDate = [System.DateTime]::Parse("2016-07-08"); # use the day you authored the private cert, not the day the cert was created.
	$endDate = [System.DateTime]::Parse("2018-04-07");

	echo "$startDate and $endDate"

	$application = (Get-AzureRMAdApplication -DisplayNameStartWith $globalVar_dumplingDisplayName)
	
	if(!$application)
	{
		echo "Creating new AzureRM AD Application named $globalVar_dumplingDisplayName"
		$application = New-AzureRmADApplication -DisplayName $globalVar_dumplingDisplayName -HomePage "http://dotnetrp.azurewebsites.net/" -IdentifierUris "http://dotnetrp.azurewebsites.net/" -KeyValue $credValue  -KeyUsage "Verify" -StartDate $startDate -EndDate $endDate -KeyType "AsymmetricX509Cert"
	}

	$sp = Get-AzureRmADServicePrincipal -ServicePrincipalName $application.ApplicationId

	if(!$sp)
	{
		$sp = New-AzureRmADServicePrincipal -ApplicationId $application.ApplicationId
	}

	# this sets the policy on the key
	# the policy we're using dictates that the dumpling web service (when we see the certificate) gets full access to the secrets. The goal
	# is to keep this centered on just the web service.
	Set-AzureRmKeyVaultAccessPolicy -VaultName $globalVar_keyVaultName -ServicePrincipalName $application.ApplicationId -PermissionsToKeys all -ResourceGroupName $globalVar_resourceGroupName
	
	# get the thumbprint to use in your app settings
	$x509.Thumbprint
}

function Deploy-StorageAccountConnectionString
{
    Login-AzureRmAccount

    $keyName = "dumplingstorage"
    $expires = (Get-Date).AddYears(2).ToUniversalTime() 
    $notbefore = (Get-Date).ToUniversalTime() 

    $securepwd = ConvertTo-SecureString –String "" –AsPlainText –Force
    
    Set-AzureKeyVaultSecret -VaultName $globalVar_keyVaultName -Name $keyName -SecretValue $securepwd -Expires $expires -NotBefore $notbefore
}

function Deploy-EventHubConnectionString
{
    Login-AzureRmAccount

    $keyName = "dumplingeventhub"
    $expires = (Get-Date).AddYears(2).ToUniversalTime() 
    $notbefore = (Get-Date).ToUniversalTime() 

    $securepwd = ConvertTo-SecureString –String "" –AsPlainText –Force
    
    Set-AzureKeyVaultSecret -VaultName $globalVar_keyVaultName -Name $keyName -SecretValue $securepwd -Expires $expires -NotBefore $notbefore
}

function Deploy-ServiceBusConnectionString
{
    Login-AzureRmAccount

    $keyName = "dumplingservicebus"
    $expires = (Get-Date).AddYears(2).ToUniversalTime() 
    $notbefore = (Get-Date).ToUniversalTime() 

    $securepwd = ConvertTo-SecureString –String "" –AsPlainText –Force
    
    Set-AzureKeyVaultSecret -VaultName $globalVar_keyVaultName -Name $keyName -SecretValue $securepwd -Expires $expires -NotBefore $notbefore
}

