function Test-FolderContainsAnyFilesWithExtension([string] $folder, [string] $extension ){
    $files = Get-ChildItem $folder -Filter *$extension -Recurse | ? { !$_.PSIsContainer }
    foreach ($file in $files) {
        if ($file-eq $null) {
            #Write-Output "ignore null ";
        }
        else {
            $str = "found " + $file.FullName
            Write-Output $str;
            return $true;
        }
    }
    Write-Output "no files found";
    return $false;
}

function Move-Directory ($source, $destination){
    if (Test-Path -literalpath $source ) {
        Write-Output "source folder $source exists ";
    }
    else {
        throw("source folder $source does not exist ?");
    }
    if (Test-Path -literalpath $destination -pathType container) {
        throw("$destination folder already exists ");
    }
    else {
    }
    Move-Item  -force -path $source -Destination $destination
    if (Test-Path $destination) {
        Write-Output "$destination folder exists ";
    }
    else {
        throw("destination folder $destination does not exist ?");
    }
}

function Remove-MaybeExistingItem ([string] $item) {    
    if (Test-Path $item) {
        try {
            Write-Output "$item exists so will remove for now";
            if (Remove-Item $item -Force -Recurse) {
            }
            else {
                Write-Output "had issues removing $item, so retest";
                Start-Sleep -s 5;
                if (Test-Path $item) {
                    Write-Output "exception $_";
                    throw "unable to remove $item";
                }
                else {
                    Write-Output "$item doesn't exist on retest so must be ok ";
                }
            }
        }
        catch {
            Write-Output "exception $_";
            throw "unable to remove $item";
        }
     }
    else {
        Write-Output "$item does not exist";
    }
}

function Test-Url([string]$url, [REF]$webresponse, [int]$timeout = 100000){
    $webresponse.Value = "UNTESTED";
    $request = [System.Net.WebRequest]::Create($url);
    $request.Timeout = $timeout
    Try {
        $response = $request.GetResponse();
        $webresponse.Value = $response.StatusCode;
    }
    Catch {
        $webresponse.Value = $_
    
    }
}

function Test-Key([string]$path, [string]$key){
    if(!(Test-Path $path)) { return $false }
    if ((Get-ItemProperty $path).$key -eq $null) { return $false }
    return $true
}

function Get-KeyValue([string]$path, [string]$key){
    if ((Get-ItemProperty $path).$key -eq $null) { return $false }
    else {
        return (Get-ItemProperty $path).$key
    }
}

function Install-WithMsiExe ($application){
    $executable = "msiexec.exe";
    if (test-path $application) {
        Write-Output "installing $application";
    }
    else {
        throw("cant find $application to install");
    }
    $arguments = '/i ' + $application + ' /qn';
    $fullcommand = $executable + " " + $arguments;
    & cmd.exe /c $fullcommand
    if ($LASTEXITCODE -ne 0) {
        throw("unable to install application $application as exitcode is $LASTEXITCODE");
    }
    else {
        Write-Output "installed $application";
    }
}

function Test-MsDeploy {
    $location = $env:Programfiles;
    $location2 = $location + "\IIS\Microsoft Web Deploy V2\msdeploy.exe"
    $location3 = $location + "\IIS\Microsoft Web Deploy V3\msdeploy.exe"
    if(Test-Key "HKLM:\SOFTWARE\Microsoft\IIS Extensions\MSDeploy\3" "InstallPath") {
        Write-Output "MSDeploy 3 Installed"
        return $true;
    }
    elseif (Test-Path $location2) {
        Write-Output "MSDeploy 2 Installed"
        return $true;
    }
    elseif (Test-Path $location3) {
        Write-Output "MSDeploy 3 Installed"
        return $true;
    }
    else {
        return $false;
        Write-Output "Required MSDeploy 2 or 3 doesn't appear to be Installed";
    }
}

function Test-WinRar {
    if(Test-Key "HKLM:\SOFTWARE\WinRAR" "exe64") {
        $winrarexe = Get-KeyValue "HKLM:\SOFTWARE\WinRAR" "exe64";
        Write-Output "winrar is $winrarexe";
        return $winrarexe;
    }
    elseif (Test-Path "C:\Program Files\WinRAR\WinRar.exe") {
        $winrarexe = "C:\Program Files\WinRAR\WinRar.exe";
        Write-Output "winrar is $winrarexe";
        return $winrarexe;
    }
    else {
        Write-Output "Required WinRAR 64 and it doesn't seem to exist";
        return $null;
    }
}

function Test-DotNetv4 {
    if(Test-Key "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Client" "Install") { Write-Output "Net 4.0c Installed"
        return $True;
    }
    elseif(Test-Key "HKLM:\Software\Microsoft\NET Framework Setup\NDP\v4\Full" "Install") { Write-Output "Net 4.0c Installed"
        return $True;
    }
    else {
        Write-Output "Required .Net 4 not installed ";
        return $False;
    }
}

function Test-Silverlightv5-1 { 
    $displayname = "Microsoft Silverlight";
    [string]$displayVersion = "";
    if (Test-Key "HKLM:\Software\Microsoft\Silverlight" "Version") {
        Write-Output "Silverlight installed detected via HKLM";
        $slvversion = Get-KeyValue "HKLM:\Software\Microsoft\Silverlight" "Version";
    }
    elseif ($(Test-ApplicationIsInstalled $displayname ([REF]$displayVersion))) {
        $slvversion = $displayVersion;
        Write-Output "Silverlight $displayVersion installed detected via Test-ApplicationIsInstalled";      
    }
    
    if ($slvversion -lt "5.1") {
        Write-Output "Silverlight version $slvversion does not meet the requirements";
        return $false;
    }
    else {
        Write-Output "Silverlight version $slvversion appears to meet requirements";
        return $true;
    }
}

function Test-VCPlusPlus2010Redists {
    $displayname = "Visual C++ 2010";
    if (Test-ApplicationIsInstalled $displayname) {
        Write-Output "DisplayName: $displayname is installed";
        return $true;
    }
    else {
        Write-Output "DisplayName: $displayname is NOT installed";
        return $false;
    }
}

function Test-VCPlusPlus2012Redists {
    $displayname = "Visual C++ 2012";
    if (Test-ApplicationIsInstalled $displayname) {
        Write-Output "DisplayName: $displayname is installed";
        return $true;
    }
    else {
        Write-Output "DisplayName: $displayname is NOT installed";
        return $false;
    }
}

function Install-UsingInstaller([string]$pathToInstaller, [string]$commandParams) { 
    if (Test-Path $pathToInstaller) {
        Write-Output "$pathToInstaller exists so will try and install";             
        $status = (Start-Process -FilePath $pathToInstaller -ArgumentList $commandparams -Wait -Passthru).ExitCode;
        if ($status -ne 0)  {
            throw "An error occurred during invocation of $pathToInstaller";
        } else  {
            Write-Output "installation of $pathToInstaller appeared to be a success";           
        }   
    }
    else {
        throw "$pathToInstaller doesn't exist so can't try to install it";
    }
}

function Install-WinRar([string]$pathToInstaller) { 
    if (Test-Path $pathToInstaller) {
        Write-Output "$pathToInstaller exists so will try and install";
        $commandparams = "/S";
        # --- invoke the expression and check return code
        $status = (Start-Process -FilePath $pathToInstaller -ArgumentList $commandparams -Wait -Passthru).ExitCode;
        ## Dont check exit code of this as often throws spurious error codes, rely on the checkrar ...
    }
    else {
        Write-Output "$pathToInstaller doesn't exist so can't try to install it";
    }
}

function Install-DotNet([string]$pathToInstaller) {
    Install-UsingInstaller $pathToInstaller "/q /norestart";    
}

function Install-Silverlight([string]$pathToInstaller) {
    Install-UsingInstaller  $pathToInstaller "/q /doNotRequireDRMPrompt /ignorewarnings"    
}

function Invoke-ExternalCommand($expression) {
    $masterExpression = "& '$expression' /y"    
    Invoke-Expression $masterExpression
    if ($LASTEXITCODE -ne 0) {
         throw "An error occurred during invocation of $expression, please review logs"
    } else {
         Write-Output "Deployment Success";
    }
}

function Test-ApplicationIsInstalled($displayname, [REF]$displayVersion) {  
    if (!([Diagnostics.Process]::GetCurrentProcess().Path -match '\\syswow64\\'))
    {
      $unistallPath = "\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\"
      $unistallWow6432Path = "\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\"
        $colItems  =  @(
        if (Test-Path "HKLM:$unistallWow6432Path" ) { Get-ChildItem "HKLM:$unistallWow6432Path"}
        if (Test-Path "HKLM:$unistallPath" ) { Get-ChildItem "HKLM:$unistallPath" }
        if (Test-Path "HKCU:$unistallWow6432Path") { Get-ChildItem "HKCU:$unistallWow6432Path"}
        if (Test-Path "HKCU:$unistallPath" ) { Get-ChildItem "HKCU:$unistallPath" }
      )
      foreach ($objItem in $colItems) {
        $obj2 = Get-ItemProperty $objItem.PSPath
         if ($obj2.DisplayName) {
            if ($obj2.DisplayName.Contains($displayname)) {
                if ($displayVersion) {
                    $displayVersion.Value = $obj2.DisplayVersion;
                }
                $mymessage = "DisplayName:" + $obj2.DisplayName + " : Is Installed";
                Write-Output $mymessage;
                return $true;
            };
        };
      } Where-Object {
        $objItem.DisplayName -and !$objItem.SystemComponent -and !$objItem.ReleaseType -and !$objItem.ParentKeyName -and ($objItem.UninstallString -or $objItem.NoRemove)
      };
    }
    else
    {
        throw("You are running 32-bit Powershell on 64-bit system. Please run 64-bit Powershell instead");
    }
    
    $mymessage = "DisplayName:" + $displayname + " : Is not Installed";
    return $false;
}

function ConvertTo-Bool($boolString) {
    return [System.Convert]::ToBoolean($boolString)
}

function ConvertTo-InstallerDriveFormat([string]$string = $(throw "Argument required")) {
    if ($string.length -eq 1) {
        $string = $string + ":\";
    }
    elseif ($string.length -eq 2) {
        $string = $string + "\";
    }
    return $string;
}

function Set-HttpUrlAcl
{
    param
    (
        [string] $url = $(throw 'Must provide a url'),
        [string] $user = $(throw 'Must provide a user'),
        [bool] $deleteExisting = $false
    ) 
    
    Write-Output "Setting http urlacl for url [$url], user [$user]"
    $comspec = $env:ComSpec
    $netshExe = (Split-Path $comspec -parent) + "\netsh.exe"    
    
    if ($deleteExisting)
    {
        $output = & $netshExe http delete urlacl $url
        Write-Output $output
    }
    
    $output = & $netshExe http add urlacl url=$url user=$user
    Write-Output $output
    if ($LASTEXITCODE -ne 0)
    {
         throw "An error occurred setting urlacl"
    }
}

function Write-XmlValues($filename, $values)
{
    import-module -disablenamechecking PowerUpXml   
    
    if(Test-Path $filename){
        Write-Output "Updating $filename" 10;
    }
    else {
        throw "file $filename does not exist and required in Write-XmlValues";
    }

    foreach($value in $values)
    {       
        #Log "Updating $filename with $value[0] and $value[1]" 10;
        
        #Write-Output "$value[0] $value[1] $value[2]"
        
        try {
            Write-XMLValue -filename $filename -xpath $value[0] -element  $value[1] -filenameout $filename -value $value[2] | out-null
            }
        catch {## Hack for now 
            Start-Sleep -m 100; 
            Write-XMLValue -filename $filename -xpath $value[0] -element  $value[1] -filenameout $filename -value $value[2] | out-null
        }

        Start-Sleep -m 100;
    }
}

function Remove-XmlNodes($filename, $nodes)
{
    foreach($node in $nodes)
    {
        try {
            Remove-XMLNode -filename $filename -xpath $node -filenameout $filename | out-null
        }
        catch { ## Hack for now 
            Remove-XMLNode -filename $filename -xpath $node -filenameout $filename | out-null  
        }
        
        Start-Sleep -m 100;
    }
}

function PrintHashtable {
    param ($hash)   
    $hash.GetEnumerator() | Sort-Object -property Key | Format-Table -property "Key","Value"
}

function Write-ColoredOutput {
    param(
        [string] $message,
        [System.ConsoleColor] $foregroundcolor
    )
   
    if (($Host.UI -ne $null) -and ($Host.UI.RawUI -ne $null) -and ($Host.UI.RawUI.ForegroundColor -ne $null)) {
        $previousColor = $Host.UI.RawUI.ForegroundColor
        $Host.UI.RawUI.ForegroundColor = $foregroundcolor
    }
    
    $message

    if ($previousColor -ne $null) {
        $Host.UI.RawUI.ForegroundColor = $previousColor
    }
}

Export-ModuleMember -Function '*'