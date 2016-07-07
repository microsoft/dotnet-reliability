#
# Create Certificate
#

function Login-ToDumplingAzureRmAccount
{
    Login-AzureRmAccount
}

function Create-DumplingKV
{

;
}

function Register-DumplingWithAzureAD
{
    Login-AzureRmAccount

	$x509 = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2

	$x509.Import("E:\Secrets\dumpling.cer")

	$credValue = [System.Convert]::ToBase64String($x509.GetRawCertData())
	$startDate = [System.DateTime]::Parse("2016-07-05");
	$endDate = [System.DateTime]::Parse("2017-07-05");

	echo "$startDate and $endDate"

	$adapp = New-AzureRmADApplication -DisplayName "Dumpling" -HomePage "http://dotnetrp.azurewebsites.net/" -IdentifierUris "http://dotnetrp.azurewebsites.net/" -KeyValue $credValue -KeyType "AsymmetricX509Cert" -KeyUsage "Verify" -StartDate $startDate -EndDate $endDate

	$sp = New-AzureRmADServicePrincipal -ApplicationId $adapp.ApplicationId

	Set-AzureRmKeyVaultAccessPolicy -VaultName 'dumplingkv' -ServicePrincipalName $sp.ServicePrincipalName -PermissionsToKeys all -ResourceGroupName 'dumpling' 

	# get the thumbprint to use in your app settings
	$x509.Thumbprint
}