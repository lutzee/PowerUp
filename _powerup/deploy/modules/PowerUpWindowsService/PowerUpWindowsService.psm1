function Set-ServiceCredentials
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name'),
        [string] $Username = $(throw "Must provide a username"),
        [string] $Password
    )

    if (!($Username.Contains("\")))
    {
        $Username = "$env:COMPUTERNAME\$Username"
    }

    $service = gwmi win32_service -filter "name='$Name'"
    if ($service -ne $null)
    {
        $params = $service.psbase.getMethodParameters("Change");
        $params["StartName"] = $Username

        if($Password) {
            $params["StartPassword"] = $Password
        }

        $service.invokeMethod("Change", $params, $null) | out-null

        Write-Output "Credentials changed for service '$Name'"
    }
    else
    {
        throw "Could not find service $Name for which to change credentials"
    }
}

function Set-ServiceStartMode
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name'),
        [string] $Mode = $(throw 'Must provide a new start mode')
    )

    $service = gwmi win32_service -filter "name='$Name'"
    if ($service -ne $null)
    {
        $params = $service.psbase.getMethodParameters("Change");
        $params["StartMode"] = $Mode
        $service.invokeMethod("Change", $params, $null) | out-null

        Write-Output "Start mode change to '$Mode' for service '$Name'"
    }
    else
    {
        throw "Could not find service '$Name' for which to change start mode"
    }
}

function Set-ServiceFailureOptions
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name'),
        [int] $ResetDays,
        [string] $Action,
        [int] $DelayMinutes
    )

    $ResetSeconds = $($ResetDays*60*60*24)
    $DelayMilliseconds = $($DelayMinutes*1000*60)
    $Action = "restart"
    $Actions = "$Action/$DelayMilliseconds/$Action/$DelayMilliseconds/$Action/$DelayMilliseconds"

    Write-Output "Setting service failure options for service $Name to reset after $ResetDays days, and $Action after $DelayMinutes minutes"

    $output = & sc.exe failure $Name reset= $ResetSeconds actions= $Actions
}

function Get-SpecificService
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    return Get-Service | Where-Object {$_.Name -eq $Name}
}

function Stop-MaybeNonExistingService
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name'),
        [bool] $force = $false
    )

    $serviceExists = !((Get-Service | Where-Object {$_.Name -eq $Name}) -eq $null)

    if ($serviceExists) {
        Write-Output "Stopping service $Name"
        $ServiceNamePID  = Get-Service -Name $Name
        $ServicePID = (get-wmiobject win32_Service | Where { $_.Name -eq $ServiceNamePID.Name }).ProcessId

        if ($force) {
            Stop-Service $Name -Force
        } else {
            Stop-Service $Name
        }

        #Back-up "nuclear option" - attempt to kill the process.  This removes anything that might be locking the parent folder etc
        if ($ServicePID -gt 0) {
            Stop-Process $ServicePID -force -ErrorAction SilentlyContinue
        }
    }
    else
    {
        Write-Output "$Name Service is not installed, so cannot be stopped"
    }
}

function Start-MaybeNonExistingService
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    $serviceExists = !((Get-Service | Where-Object {$_.Name -eq $Name}) -eq $null)

    if ($serviceExists) {
        Write-Output "Starting service $Name"
        Start-Service $Name
    }
    else
    {
        Write-Output "$Name Service is not installed, so cannot be started"
    }
}

function Restart-MaybeNonExistingService {
    param(
        [string]$serviceName,
        [bool]$force = $false
        )

    Stop-MaybeNonExistingService $serviceName $force
    Start-MaybeNonExistingService $serviceName
}

function Remove-MaybeNonExistingService
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name'),
        [bool] $force = $false
    )

    $serviceExists = !((Get-Service | Where-Object {$_.Name -eq $Name}) -eq $null)

    if ($serviceExists) {
        Remove-Service $Name $force
    }
    else
    {
        Write-Output "$Name Service is not installed, so cannot be removed"
    }
}

function Remove-Service
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name'),
        [string] $InstallPath = $null,
        [string] $ExeFileName = $null,
        [bool] $UseInstallUtil = $false,
        [string] $InstallUtilCommandLine = $null,
        [string] $InstallUtilPath = $null,
        [bool] $ForceStop = $false
    )

    $binPath = "$InstallPath\$ExeFileName"

    $serviceExists = !((Get-Service | Where-Object {$_.Name -eq $Name}) -eq $null)

    if ($serviceExists) {
        Write-Output "Uninstalling $Name"

        Stop-MaybeNonExistingService $Name $ForceStop

        if ($UseInstallUtil) {
            $lastexitcode = 0
            $output = iex "& ""$InstallUtilPath"" /u ""$binPath"" $InstallUtilCommandLine"

            # in case of installutil failure, fall back on sc. This can also happen if we are replacing a service that was not installed with installutil with one that is....
            if ($lastexitcode -ne 0)
            {
                $output = & sc.exe delete "$Name"
            }
        }
        else {
            $lastexitcode = 0
            $output = & sc.exe delete "$Name"
        }

        if ($lastexitcode -ne 0)
        {
            write-error $output[0]
            throw "Unable to remove service $Name"
        }
    }
}

function Set-Service
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name'),
        [string] $InstallPath = $(throw 'Must provide an install path'),
        [string] $ExeFileName = $(throw 'Must provide an exe file name'),
        [string] $DisplayName = $null,
        [string] $Description = $null,
        [string] $StartupType = "auto",
        [string] $Dependencies = $null,
        [bool] $UseInstallUtil = $false,
        [string] $InstallUtilCommandLine = $null,
        [string] $InstallUtilPath = $null
    )

    Remove-Service $Name $InstallPath $ExeFileName $UseInstallUtil $InstallUtilCommandLine $InstallUtilPath

    Write-Output "Installing service $Name"
    $binPath = "$InstallPath\$ExeFileName"

    if($UseInstallUtil) {
        $output = iex "& ""$InstallUtilPath"" ""$binPath"" $InstallUtilCommandLine"
    }
    else {
        if ($DisplayName) {
            $output = & sc.exe create "$Name" binPath= "$binPath" DisplayName= "$DisplayName"
        } else {
            $output = & sc.exe create "$Name" binPath= "$binPath"
        }
    }

    $lastexitcode = 0;
    $output = &sc.exe config "$Name" start= $StartupType

    if ($Description) {
        $output = & sc.exe description "$Name" "$Description"

        if ($lastexitcode -ne 0)
        {
            write-error $output[0]
            throw "Unable to set description for service $Name"
        }
    }

    if ($Dependencies) {
        $output = & sc.exe config "$Name" depend= "$Dependencies"

        if ($lastexitcode -ne 0)
        {
            write-error $output[0]
            throw "Unable to set dependencies for service $Name"
        }
    }
}

function Set-ServiceDependencies([string]$Name, [string]$Dependencies)
{
    $output = & sc.exe config "$Name" depend= "$Dependencies"
}

function Is-ServiceDisabled
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    if (Does-ServiceExist "$Name") {

        $StartMode = Get-ServiceStartMode($Name);

        if ($StartMode -eq "Disabled")
        {
            Write-Output ("Service $Name is disabled");
            return $True;
        }
        else
        {
            Write-Output ("Service $Name is not disabled its $StartMode");
            return $False;
        }

    }
    else {
        Write-Output ("Service $Name is not disabled as it doesn't exist");

    }
}

function Get-ServiceStartMode
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    $myfilter = "name='"+$Name+"'"
    $status = gwmi win32_service -filter $myfilter -computer "."

    $StartMode = $status.StartMode;

    return $StartMode;

}

function Get-ServiceStatus
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    $SStatus = Get-Service -Name $Name

    $ServiceStatus = $SStatus.Status
    return $ServiceStatus;

}


function Is-ServiceRunning {
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    if (Does-ServiceExist "$Name") {
        if (Get-ServiceStatus "$Name" -eq "Running") {
            return $True
        }
        else {
            return $False
        }

    }
    else {
        Write-Output "Service cannot be running as it doesnt exist"
        return $False;
    }


}

function Is-ServiceStopped {
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    if (Does-ServiceExist "$Name") {
        if (Get-ServiceStatus "$Name" -eq "Stopped") {
            return $True
        }
        else {
            return $False
        }
    }
    else {
        Write-Output "Service cannot be stopped as it doesnt exist"
        return $False;
    }

}

function Is-ServiceStoppedOrNonExistent {
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    if (Does-ServiceExist "$Name") {
        if (Is-ServiceStopped "$Name") {
            return $True
        }
        else {
            return $False
        }
    }
    else {
        Write-Output "Service $Name doesn't exist"
        return $True;
    }

}


function Is-ServiceDisabled
{
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    if (Does-ServiceExist "$Name") {

        $StartMode = Get-ServiceStartMode($Name);

        if ($StartMode -eq "Disabled")
        {
            Write-Output ("Service $Name is disabled");
            return $True;
        }
        else
        {
            Write-Output ("Service $Name is not disabled its $StartMode");
            return $False;
        }

    }
    else {
        Write-Output ("Service $Name is not disabled as it doesn't exist");

    }
}

function Does-ServiceExist {
    param
    (
        [string] $Name = $(throw 'Must provide a service name')
    )

    $serviceExists = !((Get-Service | Where-Object {$_.Name -eq $Name}) -eq $null)

    if ($serviceExists) {
        return $True
    }
    else {
        return $False
    }
}

function Start-Service-With-Retry {
   param(
        [string]$serviceName,
        [int]$attempts = 2
        )
        
    $currentRetry = 0;
    while ($True) {
        try
        {
            $currentRetry++
            Start-Service $serviceName
            break
        }
        catch
        {
            if ($currentRetry -gt $attempts) {
                throw "Could not start service $serviceName"
            }
        }
    }
}

Export-ModuleMember -Function '*'
