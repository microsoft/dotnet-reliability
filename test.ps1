Param(
  [string]$computerName
)

$isSame = $computerName -eq "hello==world!"

Write-Host "hello==world! == $computerName - $isSame"