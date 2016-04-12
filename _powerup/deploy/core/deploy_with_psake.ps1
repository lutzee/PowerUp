param([string]$deployFile = ".\deploy.ps1", [string]$deploymentProfile, $tasks="default", [hashtable] $parameters = @{})

try {
    $ErrorActionPreference='Stop'
    $ExitCode = 1
    $LastExitCode = 0
    Write-Output "Deploying package using profile $deploymentProfile"
    Write-Output "Deployment being run under account $env:username"
    Write-Output "Importing modules required by PowerUp"

    $currentPath = Get-Location
    $env:PSModulePath = $env:PSModulePath + ";$currentPath\_powerup\deploy\core\" + ";$currentPath\_powerup\deploy\modules\" + ";$currentPath\_powerup\deploy\combos\"

    import-module psake.psm1
    $msgs.build_success = 'Deployment succeeded'

    Write-Output "Calling psake with deployment file $deployFile "
    $psake.use_exit_on_error = $true
    $psakeParameters = @{"deployment.profile"=$deploymentProfile; "deployment.parameters"=$parameters}
    invoke-psake $deployFile $tasks -parameters $psakeParameters

    if (-not $psake.build_success) {
        $foregroundColour = $host.ui.RawUI.ForegroundColor
        $host.ui.RawUI.ForegroundColor = "Red"
        Write-Output "Build Failed!"
        $host.ui.RawUI.ForegroundColor = $foregroundColour
        $ExitCode = 1
    } else {
        $ExitCode = $LastExitCode
    }
} finally {
    Write-Output "Exiting with exit code: $ExitCode"
    try {
        remove-module psake
    }
    catch{}

    exit $ExitCode
}
