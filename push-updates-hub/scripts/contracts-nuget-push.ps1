param(
    [ValidateSet("Debug", "Release")]
    [string] $BuildConfiguration = "Debug",
    [bool] $OverwriteIfExistsAtFeed = $true
)

function Push-Package {
    Write-Host -Debug "Pushing package " -NoNewline; Write-Host -Debug $NugetPackageName -ForegroundColor Magenta -NoNewline; Write-Host -Debug " to " -NoNewline; Write-Host -Debug $FeedName -ForegroundColor Magenta -NoNewline; Write-Host -Debug " source."
    [string]$PushError = & dotnet nuget push $NugetPackageWithSymbolsAbsolutePath --source $FeedName
    return $PushError;
}

[string]$ProjectName = "PushUpdatesHub.IntegrationMessages"
[string]$ProjectFileNameWithExtension = "$ProjectName.csproj"
[string]$ProjectDirectoryAbsolutePath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath  "src" | Join-Path -ChildPath  "shared" | Join-Path -ChildPath $ProjectName -Resolve
[string]$ProjectFileAbsolutePath = Join-Path -Path $ProjectDirectoryAbsolutePath -ChildPath $ProjectFileNameWithExtension
[string]$NugetPackagesDirectoryAbsolutePath = Join-Path -Path $PSScriptRoot -ChildPath ".." | Join-Path -ChildPath ".." | Join-Path -ChildPath "nupkgs"

# Create packages' directory
if (Test-Path $NugetPackagesDirectoryAbsolutePath) {
    Write-Host -Debug "Removing already existing packages' directory " -NoNewline; Write-Host -Debug $NugetPackagesDirectoryAbsolutePath -ForegroundColor Green
    Remove-Item $NugetPackagesDirectoryAbsolutePath -Recurse -Force -ErrorAction Stop
}
Write-Host -Debug "Creating packages' directory " -NoNewline; Write-Host -Debug $NugetPackagesDirectoryAbsolutePath -ForegroundColor Green
New-Item -ItemType Directory -Path $NugetPackagesDirectoryAbsolutePath -ErrorAction Stop | Out-Null

# Pack package
Write-Host -Debug "Packing " -NoNewline; Write-Host -Debug $ProjectFileAbsolutePath -ForegroundColor Green -NoNewline; Write-Host -Debug " to " -NoNewline; Write-Host -Debug $NugetPackagesDirectoryAbsolutePath -ForegroundColor Green -NoNewline; Write-Host -Debug " using " -NoNewline; Write-Host -Debug $BuildConfiguration -ForegroundColor Magenta -NoNewline; Write-Host -Debug " configuration."
[string]$PackingError = & dotnet pack --configuration $BuildConfiguration --include-symbols --output $NugetPackagesDirectoryAbsolutePath --nologo --verbosity quiet $ProjectFileAbsolutePath
if ($null -ne $PackingError) {
    Write-Error "Failed to pack package. $PackingError" -ErrorAction Stop
}