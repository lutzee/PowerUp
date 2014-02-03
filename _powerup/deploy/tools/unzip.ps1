$currentPath = Get-Location

copy-item _powerup\deploy\tools\unzip.exe $currentPath\unzipexe.exe

$zipFiles = Get-ChildItem $currentPath | where { $_.extension -eq ".zip" }
$zipFiles | % { .\unzipexe.exe -o -q $_ }

$zipFiles | Remove-Item
remove-item $currentPath\unzipexe.exe
