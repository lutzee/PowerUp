using System;
using System.Linq;
using System.Management.Automation;
using Id.PowershellExtensions.ZipManipulation;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsData.Update, "ExistingZip", SupportsShouldProcess = true)]
    public class UpdateExistingZip : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string ExistingZipFullPath { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string[] FilesToAdd { get; set; }

        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string BaseDirectory { get; set; }

        [Parameter(Mandatory = false, Position = 4, ValueFromPipelineByPropertyName = true)]
        public string[] DirectoriesToAdd { get; set; }

        [Parameter(Mandatory = false, Position = 5, ValueFromPipelineByPropertyName = true)]
        public string[] FilesToRemove { get; set; }

        [Parameter(Mandatory = false, Position = 6, ValueFromPipelineByPropertyName = true)]
        public string[] DirectoriesToRemove { get; set; }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            try
            {
                var zipAugmentor = new ZipFileAugmentor(new PsCmdletLogger(this));

                var addArgs = (FilesToAdd ?? new string[0])
                    .Concat(DirectoriesToAdd ?? new string[0]);

                var removeArgs = (FilesToRemove ?? new string[0])
                    .Concat(DirectoriesToRemove ?? new string[0]);

                if (addArgs.Any())
                {
                    zipAugmentor.AugmentZip(ExistingZipFullPath, FilesToAdd, BaseDirectory, DirectoriesToAdd);
                }

                if (removeArgs.Any())
                {
                    zipAugmentor.DiminishZip(ExistingZipFullPath, FilesToRemove, DirectoriesToRemove);
                }
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "UpdateExistingZip",
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