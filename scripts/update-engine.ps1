# Update geisha engine submodule to latest commit on tracked branch
$env:GIT_LFS_SKIP_SMUDGE = 1 # Skip LFS for this update

Push-Location "$PSScriptRoot\.."
git submodule update --remote external/geisha
Pop-Location

Write-Host "Engine submodule updated."