using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using Id.PowershellExtensions.ParsedSettings;
using System.Linq;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsCommon.Show, "SettingsWarnings", SupportsShouldProcess = true)]
    public class ShowSettingsWarnings : PSCmdlet
    {
        private char _delimiter = '|';

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string FilePattern { get; set; }

        [Parameter(Position = 2, ValueFromPipelineByPropertyName = true)]
        public char Delimiter
        {
            get { return _delimiter; }
            set { _delimiter = value; }
        }

        [Parameter(Position = 3, ValueFromPipelineByPropertyName = true)]
        public string Directory { get; set; }

        [Parameter(Position = 4, ValueFromPipelineByPropertyName = true)]
        public bool AppendReservedSettings { get; set; }


        private readonly IList<ISettingsReader> _settingsReaders = new List<ISettingsReader>();
        private readonly SettingsParser _parser = new SettingsParser();

        public ShowSettingsWarnings()
        {
            FilePattern = null;
        }

        protected override void BeginProcessing()
        {
            try
            {
                _parser.AppendReservedSettings = AppendReservedSettings;
                _parser.WriteWarning = this.WriteWarning;

                var currentDirectory = String.IsNullOrEmpty(Directory) ? Environment.CurrentDirectory : Directory;

                if (ShouldProcess(Directory) && ShouldProcess(FilePattern))
                {
                    foreach (var file in System.IO.Directory.GetFiles(currentDirectory, FilePattern)
                        .OrderBy(Path.GetFileNameWithoutExtension))
                    {
                        _settingsReaders.Add(new SettingsFileReader(file, currentDirectory));
                    }
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "SettingsWarnings",
                        ErrorCategory.NotSpecified,
                        FilePattern)
                    );
            }
        }

        protected override void ProcessRecord()
        {
            try
            {
                if (_settingsReaders.Count > 0)
                {
                    var settingsLines = new List<string>();

                    foreach (var reader in _settingsReaders)
                    {
                        settingsLines.AddRange(reader.ReadSettings());
                    }
                    
                    _parser.Validate(settingsLines);
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "SettingsWarnings",
                        ErrorCategory.NotSpecified,
                        this)
                    );
            }
        }

        protected override void EndProcessing()
        {
            if (_settingsReaders.Count > 0)
            {
                _settingsReaders.Clear();
            }
        }
    }
}
