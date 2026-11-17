# Fails when CITATION.cff and CHANGELOG.md do not match the release version, so Zenodo never archives inconsistent metadata.
param(
    [Parameter(Mandatory)][string]$Version,
    [string]$NotesPath
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$errors = @()

$citation = Get-Content (Join-Path $root 'CITATION.cff') -Raw
$citationVersion = [regex]::Match($citation, '(?m)^version:\s*"?([^"\r\n]+?)"?\s*$').Groups[1].Value
$citationDate = [regex]::Match($citation, '(?m)^date-released:\s*"?([^"\r\n]+?)"?\s*$').Groups[1].Value
if ($citationVersion -ne $Version) {
    $errors += "CITATION.cff version is '$citationVersion', expected '$Version'."
}

$changelog = Get-Content (Join-Path $root 'CHANGELOG.md') -Raw
$escaped = [regex]::Escape($Version)
$section = [regex]::Match($changelog, "(?ms)^## \[$escaped\] - (\d{4}-\d{2}-\d{2})[^\r\n]*\r?\n(.*?)(?=^---|^## |\z)")
if (-not $section.Success) {
    $errors += "CHANGELOG.md has no '## [$Version] - YYYY-MM-DD' section."
}
elseif ($section.Groups[1].Value -ne $citationDate) {
    $errors += "CHANGELOG.md date is $($section.Groups[1].Value) but CITATION.cff date-released is '$citationDate'."
}

if ($errors.Count -gt 0) {
    foreach ($e in $errors) { Write-Host "::error::$e" }
    exit 1
}

if ($NotesPath) {
    Set-Content -Path $NotesPath -Value $section.Groups[2].Value.Trim() -Encoding utf8
}

Write-Host "Release metadata is consistent for $Version."
