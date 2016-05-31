function Add-FontsInDirectory([string]$path)
{
	$fontinstalldir = dir $path
	foreach($fontFile in $fontinstalldir) {
		Write-Output $fontFile.fullname
		$output = & "$PSScriptRoot\FontInstaller.exe" "$fontFile.fullname"
    }
}

export-modulemember -function Add-FontsInDirectory