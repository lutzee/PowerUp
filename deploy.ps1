include .\_powerup\deploy\combos\PsakeCombos\StandardSettingsAndRemoteExec.ps1

task deploy {  
	DoNugetPackAndPush ${nuget.nuspec.name} ${nuget.manual.package.version}	
}

function DoNugetPackAndPush([string]$name, [string]$packageVersion) {
    $nugetExe = "$(get-location)\_packageupdate\Nuget.exe" 
    $version = ${package.build}
    if ((-not ${package.build}) -or (${package.build} -eq "Manual")) {
        $version = $packageVersion
    } else {
        $version = "1.0.${package.build}"
    }  
    $packageName = "$name." + $version + ".nupkg"
    
	if (${nuget.update.exe} -like "true") {
		& $nugetExe update -self	
	}
	
	Write-Host "Creating package $name version $version"    
	& $nugetExe pack "$(get-location)\$name.nuspec" -version "$version" -outputdirectory "$(get-location)" -NoPackageAnalysis -NonInteractive
	
	if (test-path "$(get-location)\$packageName") {
		Write-Host "Copying package to ${nuget.server.path}"
		Copy-Item "$(get-location)\$packageName" -destination ${nuget.server.path}    
	} else {
		throw "Failed to create package $packageName"
	}
}