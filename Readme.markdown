## Introduction

PowerUp is a build and deployment framework, written on top of Powershell and Psake.  

PowerUp prefers to be simple, low obligation and assumes very little. 
There is nothing to be installed, with the only dependency being Powershell. 

It is designed for people that rightly think deployments really shouldn't be complicated. 

The philosophy of PowerUp is based on the concept of deployment through "unremarkable" zipped packages of files. 
Rooted in the xcopy deployment mindset, it simply adds the plumbing required to make one package deployable in a number of different environments.
It also bundles convenient  tools to enable the configuration of Windows servers (ie, create websites etc).

# Status

PowerUp is regularly used by Wynyard Group, Affinity ID, BBC Worldwide and Universal Music to release projects through to production. 
This includes file deployment, website creation (with SSL), IIS managment, windows features, App Fabric, Windows Services, scheduled tasks, MSMQ, Amazon Web Services, database migrations, fonts and Umbraco Courier revision publications. Using InstallShield as a wrapper is also known to work (contact for details, if interested!)

Andrew, the main contributor, works at Wynyard Group.

# Getting started

PowerUp is available as a NuGet package at https://www.nuget.org/packages/PowerUp

Although other modes are supported, the easiest way to start is to add PowerUp as a package to your solution.
Once this is done, you need to add a few other files to the root folder of your solution root. These consist of files that script your build and others that script and set up configuration for your deployments.

Please look at https://github.com/PowerUpDeploy/PowerUpSamples for a very simple example of this.

# Disclaimer of Background Influences

PowerUp is influenced by a number of existing tools, including proprietary ones.
In particular, many ideas are similar to the Nant based build system used by BBC Worldwide.

The aspects where this influence shows are, in particular:  
- The idea of substituting values from a plain text settings file into template files.  
- The use of psexec to execute remote scripts, and the use of "cmd.js" (originally described here http://forum.sysinternals.com/psexec-the-pipe-has-been-ended_topic10825.html) to control standard output.  

The intention is that these are fair-use adoptions of ideas.

## Alternatives

### Bounce
https://github.com/refractalize/bounce. 

The main difference is that Bounce is C# based. We decided on Powershell for its unparalleled breath of support in Windows, flexibility across languages, and to provide a very low barrier for entry. Bounce has other strengths - more maturity, clearer semantics and testability. There is future potential for the use of Bounce within PowerUp. 

### UppercuT
http://code.google.com/p/uppercut/

Largely a build and test running framework, not a deployment one.
In theory, UppercuT could be used as an alternative to straight Nant to create PowerUp packages.
It does, however, build a package per environment which goes against the environment neutrality built into PowerUp packages.

### Pstrami
https://github.com/jhicks/pstrami

Has an attractively closer similarity to capistrano in terms of the script syntax, but has less functionality overall.

### Carbon

http://get-carbon.org/

A solid collection of cmdlets, some of which have been incorporated
