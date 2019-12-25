$VerbosePreference="Continue"

# see https://github.com/meziantou/Meziantou.Analyzer/build

$version = $args[0]
$analyzerProjectPath = $args[1]
$vsixSourceManifestPath = $args[2]

Write-Host "Path to analyzer project: $analyzerProjectPath"
Write-Host "Path to vsix project manifest file: $vsixSourceManifestPath"

# Read version from csproj
$FullPath = $analyzerProjectPath
[xml]$content = Get-Content $FullPath
$version = "$content.Project.PropertyGroup.PackageVersion.$version"

Write-Host "Version: $version"

# Update NuGet package version
$FullPath = $analyzerProjectPath
[xml]$content = Get-Content $FullPath
$content.Project.PropertyGroup.PackageVersion = $version
$content.Save($FullPath)

# Update VSIX version
$FullPath = $vsixSourceManifestPath
[xml]$content = Get-Content $FullPath
$content.PackageManifest.Metadata.Identity.Version = $version
$content.Save($FullPath)