# Discard any accidental local modifications inside submodule directories
Push-Location "$PSScriptRoot\..\external\geisha"
git checkout -- .
Pop-Location

Write-Host "Submodules cleaned successfully."