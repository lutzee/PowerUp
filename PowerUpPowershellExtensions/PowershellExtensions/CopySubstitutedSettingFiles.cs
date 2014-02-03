using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using System.Collections;
using Id.PowershellExtensions.SubstitutedSettingFiles;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsCommon.Copy, "SubstitutedSettingFiles", SupportsShouldProcess = true)]
    public class CopySubstitutedSettingFiles : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string TemplatesDirectory { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string TargetDirectory { get; set; }
        
        [Parameter(Mandatory = true, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string DeploymentEnvironment { get; set; }

        [Parameter(Mandatory = true, Position = 4, ValueFromPipelineByPropertyName = true)]
        public Hashtable Settings { get; set; }

        public CopySubstitutedSettingFiles()
        {
            DeploymentEnvironment = null;
            TemplatesDirectory = null;
            Settings = new Hashtable();
        }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            try
            {
                var substitutor = new SettingsSubstitutor();

                var settingsDictionary = Settings.Keys.Cast<string>().ToDictionary(key => key, key => (string [])Settings[key]);
                substitutor.CreateSubstitutedDirectory(TemplatesDirectory, TargetDirectory, DeploymentEnvironment, settingsDictionary);
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "SubstitutedSettingFiles",
                        ErrorCategory.NotSpecified,
                        this
                        )
                    );
            }
        }

        protected override void EndProcessing()
        {           
        }
    }
}
