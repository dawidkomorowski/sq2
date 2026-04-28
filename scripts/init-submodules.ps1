$env:GIT_LFS_SKIP_SMUDGE = 1 # Skip LFS for this init
git submodule update --init --recursive
Write-Host "Submodules initialized successfully."