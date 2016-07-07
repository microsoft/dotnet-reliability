# To create a certificate, navigate to our internal site //ssladmin/
$globalVar_resourceGroupName = "dumpling"
$globalVar_USLocation = "West US"
$globalVar_keyVaultName = "dumplingVault"
$globalVar_tags = "dumpling"
$globalVar_dumplingClientId = ""
$globalVar_dumplingDisplayName = "Dumpling"

# note that if the resources exist, the user that runs this function will be prompted so there is nothing to worry about
# with regards to running this function multiple times.
function Create-DumplingKeyVault
{
	#login to Azure
    Login-AzureRmAccount

	#create the dumpling resource group if it doesn't exist.
	New-AzureRmResourceGroup –Name $globalVar_resourceGroupName –Location $globalVar_USLocation -Tag $globalVar_tags
	
	# now that we certainly have our resource group, we will create the key vault within.
	New-AzureRmKeyVault -VaultName $globalVar_keyVaultName -ResourceGroupName 'dumpling' -Location $globalVar_USLocation -Tag $globalVar_tags
}

function Add-KeyToKeyVault
{
	$key = Add-AzureKeyVaultKey -VaultName $globalVar_keyVaultName -Name 'ContosoFirstKey' -Destination 'Software'
}

# To create a securePassword, use this:
#$securepfxpwd = ConvertTo-SecureString –String 'password-to-cert-goes-here' –AsPlainText –Force
function Add-CertificateToKeyVault([string]$pathToPfx, [SecureString]$securePassword)
{
	# the destination keyword here can be either HSM (Hardware Security Module) or Software (software-protected key)

	$key = Add-AzureKeyVaultKey -VaultName $globalVar_keyVaultName -Name 'ContosoFirstKey' -KeyFilePath $pathToPfx -KeyFilePassword $securePassword
}

function Register-DumplingWithAzureAD
{
	#login to Azure
    Login-AzureRmAccount

	$x509 = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2

	$x509.Import("E:\Secrets\dumpling.cer")

	$credValue = [System.Convert]::ToBase64String($x509.GetRawCertData())
	$startDate = [System.DateTime]::Parse("2016-07-05");
	$endDate = [System.DateTime]::Parse("2017-07-05");

	echo "$startDate and $endDate"

	$adapp = New-AzureRmADApplication -DisplayName $globalVar_dumplingDisplayName -HomePage "http://dotnetrp.azurewebsites.net/" -IdentifierUris "http://dotnetrp.azurewebsites.net/" -KeyValue $credValue  -KeyUsage "Verify" -StartDate $startDate -EndDate $endDate -KeyType "AsymmetricX509Cert"

	$sp = New-AzureRmADServicePrincipal -ApplicationId $adapp.ApplicationId

	# this sets the policy on the key
	# the policy we're using dictates that the dumpling web service (when we see the certificate) gets full access to the secrets. The goal
	# is to keep this centered on just the web service.
	Set-AzureRmKeyVaultAccessPolicy -VaultName $globalVar_keyVaultName -ServicePrincipalName $globalVar_dumplingClientId -PermissionsToKeys all -ResourceGroupName $globalVar_resourceGroupName
	
	# get the thumbprint to use in your app settings
	$x509.Thumbprint
}