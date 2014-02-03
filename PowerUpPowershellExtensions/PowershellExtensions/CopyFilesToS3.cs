using System;
using System.Management.Automation;
using SystemWrapper.IO;
using Amazon.S3;
using Id.PowershellExtensions.S3;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsCommon.Copy, "FilesToS3", SupportsShouldProcess=true)]
    public class CopyFilesToS3 : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string AccessKey { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string Secret { get; set; }

        [Parameter(Mandatory = true, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string Bucket { get; set; }

        [Parameter(Mandatory = true, Position = 4, ValueFromPipelineByPropertyName = true)]
        public bool SetPublicRead { get; set; }

        [Parameter(Mandatory = true, Position = 5, ValueFromPipelineByPropertyName = true)]
        public string Folder { get; set; }

        [Parameter(Mandatory = true, Position = 6, ValueFromPipelineByPropertyName = true)]
        public bool Recurse { get; set; }

        private S3FileSystem s3FileSystem;

        protected override void BeginProcessing()
        {  }        

        protected override void ProcessRecord()
        {
            try
            {
                var logger = new PsCmdletLogger(this);
                s3FileSystem = new S3FileSystem(logger, AccessKey, Secret, new AmazonS3Config());
                s3FileSystem.UploadFiles(new DirectoryInfoWrap(Folder), Recurse, Bucket, SetPublicRead);
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "Copy-FilesToS3",
                        ErrorCategory.NotSpecified,
                        this
                        )
                    );
            }
        }

        protected override void EndProcessing()
        {
            s3FileSystem = null;
        }
    }
}
