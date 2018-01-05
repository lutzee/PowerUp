@echo off

for /F "usebackq tokens=1,2 delims==" %%i in (`wmic os get LocalDateTime /VALUE 2^>NUL`) do if '.%%i.'=='.LocalDateTime.' set ldt=%%j
set ldt=%ldt:~0,4%-%ldt:~4,2%-%ldt:~6,2%_%ldt:~8,2%-%ldt:~10,2%-%ldt:~12,2%

echo Logging to .\log-%ldt%.txt

if not '%1'=='' goto RUN

:NOENVIRONMENT
	@echo on
	echo Deployment environment parameter is required
	echo e.g. deploy production	
	exit /B

:RUN
	call _powerup\deploy\core\ensure_prerequisites.bat
	
if not '%2'=='' goto RUNWITHTASK	
powershell -ExecutionPolicy Bypass -inputformat none -command ".\_powerup\deploy\core\deploy_with_psake.ps1 -buildFile .\deploy.ps1 -deploymentProfile %1 2>&1 | Tee-Object -file .\log-%ldt%.txt"

goto END

:RUNWITHTASK
powershell -ExecutionPolicy Bypass -inputformat none -command ".\_powerup\deploy\core\deploy_with_psake.ps1 -buildFile .\deploy.ps1 -deploymentProfile %1 -tasks %2 2>&1 | Tee-Object -file .\log-%ldt%.txt"

:END
exit /B %errorlevel%