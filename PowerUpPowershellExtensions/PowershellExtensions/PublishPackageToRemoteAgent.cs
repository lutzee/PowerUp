using System;
using System.Collections;
using System.Management.Automation;
using Id.PowershellExtensions.RemoteAgent;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsData.Publish, "PackageToRemoteAgent", SupportsShouldProcess = true)]
    public class PublishPackageToRemoteAgent : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public Hashtable Settings { get; set; }    

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            try
            {
                var publisher = new Publisher(Settings.ToDictionary(), new PsCmdletLogger(this));
                var output = publisher.PublishPackage();

                WriteObject(output);
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "PublishPackageToRemoteAgent",
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
