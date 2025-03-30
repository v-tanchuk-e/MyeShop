param(
    [string]$project,
    [string]$projectName,
    [string]$solutionPath
)

if (-not $solutionPath) {
    Write-Information "usage: .\dotnet_publish_web.ps1 -solutionPath ""C:\Users\vadym_tarasov\source\repos\eShopOnWeb"" -project ""./src/Web/Web.csproj"" -projectName ""Web"""
    Write-Error "solutionPath parameter is required"
    exit 1
}

if (-not $project) {
    Write-Information "usage: .\dotnet_publish_web.ps1 -solutionPath ""C:\Users\vadym_tarasov\source\repos\eShopOnWeb"" -project ""./src/Web/Web.csproj"" -projectName ""Web"""
    Write-Error "project parameter is required"
    exit 1
}

if (-not $projectName) {
    Write-Information "usage: .\dotnet_publish_web.ps1  -solutionPath ""C:\Users\vadym_tarasov\source\repos\eShopOnWeb"" -project ""./src/Web/Web.csproj"" -projectName ""Web"""
    Write-Error "projectName parameter is required"
    exit 1
}


$projectPath = Join-Path -Path $solutionPath -ChildPath $project

$publishFolder="$solutionPath\src\$projectName\bin\publish"
$suffix = (Get-Date).ToString("yyyyMMddHHmmssffff")
$zipFileLocation = "$publishFolder\$projectName-$suffix.zip"

Write-Information "Publishing project $project to $publishFolder"
if (Test-Path $publishFolder) {
    Write-Information "Removing existing publish folder $publishFolder"
    Remove-Item -Recurse -Force -Path $publishFolder
}

dotnet publish $projectPath -o $publishFolder -c Release
Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipFileLocation -Force

Write-Output $zipFileLocation
