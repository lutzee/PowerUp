using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Id.PowershellExtensions.ParsedSettings;
using System.Linq;

namespace Id.PowershellExtensions {
    [Cmdlet(VerbsCommon.Get, "ParsedSettings", SupportsShouldProcess = true)]
    public class GetParsedSettings : PSCmdlet {
        private char _delimiter = '|';

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string FilePattern { get; set; }
        
        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Section { get; set; }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true)]
        public char Delimiter {
            get { return _delimiter; }
            set { _delimiter = value; }
        }

        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true)]
        public string Directory { get; set; }

        [Parameter(Position = 5, ValueFromPipelineByPropertyName = true)]
        public Hashtable OverrideSettings { get; set; }

        private IList<ISettingsReader> settingsReaders = new List<ISettingsReader>();
        private SettingsParser parser = new SettingsParser();

        public GetParsedSettings() {
            Section = null;
            FilePattern = null;
        }

        protected override void BeginProcessing() {
            try {
                string currentDirectory = String.IsNullOrEmpty(Directory) ? Environment.CurrentDirectory : Directory;

                if (ShouldProcess(Directory) && ShouldProcess(FilePattern) && ShouldProcess(Section))
                {
                    foreach (var file in System.IO.Directory.GetFiles(currentDirectory, FilePattern).OrderBy(Path.GetFileNameWithoutExtension))
                    {
                        settingsReaders.Add(new SettingsFileReader(file, currentDirectory));
                    }
                }

                // Add the override settings last so they are the highest priority
                settingsReaders.Add(new OverrideSettingsReader(OverrideSettings, Section));
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "ParsedSettings",
                        ErrorCategory.NotSpecified,
                        FilePattern)
                    );
            }
        }

        protected override void ProcessRecord() {
            try {
                if (settingsReaders.Count > 0) {
                    var settingsLines = new List<string>();

                    foreach(var reader in settingsReaders) {
                        settingsLines.AddRange(reader.ReadSettings());
                    }
                    var settings = parser.Parse(settingsLines, Section, Delimiter);

                    this.WriteObject(settings);
                }
            } catch (Exception e) {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "ParsedSettings",
                        ErrorCategory.NotSpecified,
                        this)
                    );
            }
        }

        protected override void EndProcessing() {
            if (settingsReaders.Count >0) {
                settingsReaders.Clear();
            }
        }
    }
}
