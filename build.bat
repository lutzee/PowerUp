call .nuget\nuget install .nuget\packages.config -OutputDirectory .\packages
call .nuget\nuget.exe restore PowerUpPowershellExtensions\PowershellExtensions.sln
_powerup\build\nant\nant\bin\nant %*