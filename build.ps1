$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

Write-Host 'Building GetActiveRitualsFixPatch (Release)...' -ForegroundColor Cyan
dotnet build (Join-Path $root 'GetActiveRitualsFixPatch.csproj') -c Release

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ''
Write-Host "Output: $(Join-Path $root 'out\GetActiveRitualsFixPatch.dll')" -ForegroundColor Green
