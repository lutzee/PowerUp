function Get-KeepassSecret(
    [Parameter(Mandatory)][string]$VaultName,
    [Parameter(Mandatory)][string]$VaultPath,
    [Parameter(Mandatory)][string]$secretName
)
{
    # Register the vault if it isn't already
    Register-KeepassVault -VaultName $VaultName -VaultPath $VaultPath

    # Then unlock it so we can get the value
    $vaultPassword= ConvertTo-SecureString $env:KEEPASS_MASTER_PASSWORD -AsPlainText -Force

    Unlock-SecretVault -Password $vaultPassword -Name $vaultName

    $secret = Get-Secret -Name $secretName -Vault $vaultName
    # Always unregister the vault once used, as the vault points to a keepass file that 
    # might be in a different folder on the next deploy, we can't have the registration hanging around
    Unregister-SecretVault $vaultName
    return $secret
}

function New-KeepassSecret(
    [Parameter(Mandatory)][string]$VaultName,
    [Parameter(Mandatory)][string]$VaultPath,
    [Parameter(Mandatory)][string]$Name,
    [Parameter(Mandatory)][string]$Value
)
{
    # Register the vault if it isn't already
    Register-KeepassVault -VaultName $VaultName -VaultPath $VaultPath

    # Then unlock it so we can set the new value
    $vaultPassword= ConvertTo-SecureString $env:KEEPASS_MASTER_PASSWORD -AsPlainText -Force
    Unlock-SecretVault -Password $vaultPassword -Vault $vaultName

    Set-Secret -Name $name -Secret $Value -Vault $VaultName
    
    # Always unregister the vault once used, as the vault points to a keepass file that 
    # might be in a different folder on the next deploy, we can't have the registration hanging around
    Unregister-SecretVault $vaultName
}

function Register-KeepassVault (
    [Parameter(Mandatory)][string]$VaultName,
    [Parameter(Mandatory)][string]$VaultPath
)
{
    $path = [System.IO.Path]::GetFullPath($VaultPath)

    try 
    {
        Get-SecretVault -Name $VaultName
    } 
    catch 
    {
        Register-SecretVault -Name $VaultName -ModuleName SecretManagement.KeePass -VaultParameters @{
            Path = $path
            UseMasterPassword = $true
            #MasterPassword = $VaultPassword
        }
    }
}

Export-ModuleMember -function '*'