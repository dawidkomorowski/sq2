$env:GIT_LFS_SKIP_SMUDGE = 1 # Skip LFS for this init

Push-Location "$PSScriptRoot\.."
git submodule update --init --recursive
Pop-Location

Write-Host "Submodules initialized successfully."