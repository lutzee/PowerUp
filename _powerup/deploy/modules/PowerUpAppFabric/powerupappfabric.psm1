function Install-AppFabric($isServer2008R2)
{
	#See http://msdn.microsoft.com/en-us/library/ff637714.aspx
	if ([System.Convert]::ToBoolean($isServer2008R2))
	{
		Write-Output "Installing AppFabric for Server 2008 R2 x64"
		$output = & "$PSScriptRoot\WindowsServerAppFabricSetup_x64_6.1.exe" /i HostingServices
	}
	else
	{
		Write-Output "Installing AppFabric for Server 2008 x64"
		$output = & "$PSScriptRoot\WindowsServerAppFabricSetup_x64_6.0.exe" /i HostingServices
	}
	Write-Output "AppFabric Installed"
}

Export-ModuleMember -function Install-AppFabric