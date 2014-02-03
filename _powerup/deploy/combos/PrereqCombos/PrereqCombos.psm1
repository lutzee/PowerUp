function Invoke-Combo-EstablishAspnetWebserverPrequisites($options)
{
	import-module -disablenamechecking powerupwindowsos
	import-module -disablenamechecking  powerupweb
	
	set-windowsfeature IIS-WebServer
	set-windowsfeature IIS-WebServerRole
	set-windowsfeature IIS-Windows-Authentication
	
	enable-aspnet	
	enable-aspnetisapi "${env:WINDIR}\Microsoft.NET\Framework\v4.0.30319\aspnet_isapi.dll"
	enable-aspnetisapi "${env:WINDIR}\Microsoft.NET\Framework64\v4.0.30319\aspnet_isapi.dll"
}
