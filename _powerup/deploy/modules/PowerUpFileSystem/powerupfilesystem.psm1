
function Ensure-Directory([string]$directory)
{
	if (!(Test-Path $directory -PathType Container))
	{
		Write-Host "Creating folder $directory"
		New-Item $directory -type directory | out-null
	}
}

function ReplaceDirectory([string]$sourceDirectory, [string]$destinationDirectory)
{
	if (Test-Path $destinationDirectory -PathType Container)
	{
		Write-Host "Removing folder"
		Remove-Item $destinationDirectory -recurse -force
	}
	Write-Host "Copying files"
	Copy-Item $sourceDirectory\ -destination $destinationDirectory\ -container:$false -recurse -force
}

function copy-directory([string]$sourceDirectory, [string]$destinationDirectory, $onlyNewer)
{
	Write-Host "Copying newer files from $sourceDirectory to $destinationDirectory"
		
	if($onlyNewer)
	{
		$output = & "$PSScriptRoot\robocopy.exe" $sourceDirectory $destinationDirectory /E /np /njh /nfl /ns /nc /xo
	}
	else
	{	
		$output = & "$PSScriptRoot\robocopy.exe" $sourceDirectory $destinationDirectory /E /np /njh /nfl /ns /nc
	}	
	
	if ($lastexitcode -lt 8)
	{
		cmd /c #reset the lasterrorcode strangely set by robocopy to be non-0
	}
	else
	{
		throw "Robocopy failed to mirror to $destinationDirectory. Exited with exit code $lastexitcode"
	}	
}

function Copy-MirroredDirectory([string]$sourceDirectory, [string]$destinationDirectory, $excludedPaths)
{
	Write-Host "Mirroring $sourceDirectory to $destinationDirectory"
	
	if($excludedPaths)
	{
		$dirs = $excludedPaths -join " "
		$output = & "$PSScriptRoot\robocopy.exe" $sourceDirectory $destinationDirectory /E /np /njh /nfl /ns /nc /mir /XD $dirs  
	}
	else
	{
		$output = & "$PSScriptRoot\robocopy.exe" $sourceDirectory $destinationDirectory  /E /np /njh /nfl /ns /nc /mir 
	}
	
	if ($lastexitcode -lt 8)
	{
		cmd /c #reset the lasterrorcode strangely set by robocopy to be non-0
	}
	else
	{
		throw "Robocopy failed to mirror to $destinationDirectory. Exited with exit code $lastexitcode"
	}
}

function New-Shortcut ( [string]$targetPath, [string]$fullShortcutPath ){
	Write-Host "Creating shortcut $fullShortcutPath targetting path $targetPath"
	
	$WshShell = New-Object -comObject WScript.Shell
	$Shortcut = $WshShell.CreateShortcut($fullShortcutPath)
	$Shortcut.TargetPath = $targetPath
	$Shortcut.Save()
}

function New-DesktopShortcut ( [string]$targetPath , [string]$shortcutName ){
	New-Shortcut $targetPath "$env:USERPROFILE\Desktop\$shortcutName"
}

function Write-FileToConsole([string]$fileName)
{	
	$line=""

	if ([System.IO.File]::Exists($fileName))
	{
		$streamReader=new-object System.IO.StreamReader($fileName)
		$line=$streamReader.ReadLine()
		while ($line -ne $null)
		{
			write-host $line
			$line=$streamReader.ReadLine()
		}
		$streamReader.close()		
	}
	else
	{
	   write-host "Source file ($fileName) does not exist." 
	}
}

Set-Alias RobocopyDirectory Copy-Directory 

Export-ModuleMember -function copy-directory, New-Shortcut, New-DesktopShortcut, Write-FileToConsole, Ensure-Directory, Copy-MirroredDirectory, Copy-Directory -alias  RobocopyDirectory 
