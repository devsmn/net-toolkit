param(
    [Parameter(Mandatory = $true)]
    [string]$OutputFolder,

    [Parameter(Mandatory = $true)]
    [string]$ApiKey,

    [string]$Source = "https://api.nuget.org/v3/index.json",

    [switch]$SkipSymbols
)

Write-Host "Searching for .nupkg files in: $OutputFolder"

$packages = Get-ChildItem -Path $OutputFolder -Filter "*.nupkg" -Recurse | Where-Object {
    if ($SkipSymbols) { $_.Name -notlike "*.snupkg" } else { $_.Name -notlike "*.symbols.nupkg" }
}

if ($packages.Count -eq 0) {
    Write-Host "No .nupkg files found in $OutputFolder"
    exit 1
}

foreach ($pkg in $packages) {
    Write-Host "Publishing $($pkg.Name)..."

    try {
        dotnet nuget push $pkg.FullName `
            --api-key $ApiKey `
            --source $Source `
            --skip-duplicate `
            --force-english-output `

        Write-Host "Successfully published $($pkg.Name)"
    }
    catch {
        Write-Host "Failed to publish $($pkg.Name): $_"
    }
}

Write-Host "All done."
