Set-StrictMode -Version 2
$ErrorActionPreference = "Stop"

[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.Smo")
[System.Reflection.Assembly]::LoadWithPartialName("Microsoft.SqlServer.ConnectionInfo")

function Create-ConnectionStringFromMigrationSettings($settings) {
  $connString = "Server=$($settings.Server);Database=$($settings.Database)"  
  if (!($($settings.MigrationUser))) {
	$connString = $connString + ";Integrated Security=True;"
  } else  {
	$connString = $connString + ";User Id=$($settings.MigrationUser);Password=$($settings.MigrationUserPassword);"
  }
    
  return $connString
} 

function Get-SqlServer (    
    [Parameter(Mandatory=$true)] [string] $connectionString
) {	
	$connection = New-Object System.Data.SqlClient.SqlConnection ($connectionString)
	$serverConnection = New-Object Microsoft.SqlServer.Management.Common.ServerConnection ($connection)
	$server = New-Object Microsoft.SqlServer.Management.Smo.Server ($serverConnection)
	
	return $server
}

function Get-SqlServerDatabases (
	[Parameter(Mandatory=$true)] [string] $connectionString
) {
	$server = Get-SqlServer $connectionString
	return $server.Databases
}
