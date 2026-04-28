# Sync local submodule files to match the committed pointer - run after pulling changes from remote
$env:GIT_LFS_SKIP_SMUDGE = 1

Push-Location "$PSScriptRoot\.."
git submodule update external/geisha
Pop-Location

Write-Host "Submodules synced successfully."