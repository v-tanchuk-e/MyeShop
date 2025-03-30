param(
    [string]$solutionPath
)

if (-not $solutionPath) {
    Write-Information "usage: .\dotnet_publish_web.ps1 -solutionPath ""absolute path to \eShopOnWeb"" -project ""./src/Web/Web.csproj"" -projectName ""Web"""
    Write-Error "project parameter is required"
    exit 1
}


$deployment = az deployment sub create `
--name depl00001 `
--template-file main.bicep `
--location westeurope `
--parameters aspSku=S1 createStagingSlot=false `
--query "properties.outputs" `
--output json

# # Parse the output
$deploymentOutput = $deployment | ConvertFrom-Json

# # Access specific output values
$rgWebAndApiName = $deploymentOutput.mainOutput.value.rgWebAndApiName
$rgWebName = $deploymentOutput.mainOutput.value.rgWebName
$publicApiServiceName = $deploymentOutput.mainOutput.value.publicApiServiceName
$webAppFailoverServiceName = $deploymentOutput.mainOutput.value.webAppFailoverServiceName
$webAppMainServiceName = $deploymentOutput.mainOutput.value.webAppMainServiceName
$webAppFailoverId = $deploymentOutput.mainOutput.value.webAppFailoverId
$webAppMainId = $deploymentOutput.mainOutput.value.webAppMainId
# # Use the output values
Write-Output "Resource Group for Web and API: $rgWebAndApiName"
Write-Output "Resource Group for Web: $rgWebName"
Write-Output "Public API Service Name: $publicApiServiceName"
Write-Output "Web App Failover Service Name: $webAppFailoverServiceName"
Write-Output "Web App Main Service Name: $webAppMainServiceName"
Write-Output "Web App Failover Id: $webAppFailoverId"
Write-Output "Web App Main Id: $webAppMainId"

$appZipArray = .\dotnet_publish_web.ps1 -solutionPath $solutionPath -project "./src/PublicApi/PublicApi.csproj" -projectName "PublicApi"
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet_publish_web.ps1 failed for PublicApi"
    exit $LASTEXITCODE
}
$appZip = $appZipArray[-1]  # Get the latest element
az webapp deploy --resource-group $rgWebAndApiName --name $publicApiServiceName --src-path $appZip --type zip

$webZipArray = .\dotnet_publish_web.ps1 -solutionPath $solutionPath -project "./src/Web/Web.csproj" -projectName "Web"
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet_publish_web.ps1 failed for PublicApi"
    exit $LASTEXITCODE
}
$webZip = $webZipArray[-1]  # Get the latest element
az webapp deploy --resource-group $rgWebAndApiName --name $webAppFailoverServiceName --src-path $webZip --type zip
az webapp deploy --resource-group $rgWebName --name $webAppMainServiceName --src-path $webZip --type zip

$suffix = (Get-Date).ToString("yyyyMMddHHmmssffff")

# do not create traffic manager profile
# exit 0

az deployment group create `
-g $rgWebName `
--name depl00002$suffix `
--template-file .\traficmanager.bicep `
--parameters webMainId=$webAppMainId webFailoverId=$webAppFailoverId trafficMngrProfileName="tmvivethere$suffix"