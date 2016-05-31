@echo off

call _powerup\deploy\core\ensure_prerequisites.bat
powershell -ExecutionPolicy Bypass -inputformat none -command ".\_powerup\deploy\core\deploy_with_psake.ps1 -buildFile .\deploy.ps1 -deploymentProfile default -tasks %2 validate  | Tee-Object -file .\validate-settings-log.txt"

:END
exit /B %errorlevel%
