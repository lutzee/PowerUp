using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;
using System.IO;
using System.Collections;
using Id.PowershellExtensions.SubstitutedSettingFiles;
using Id.PowershellExtensions.UmbracoResources;
using Umbraco.Courier.RepositoryProviders;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsData.Initialize, "CourierWebService", SupportsShouldProcess = true)]
    public class InitializeCourierWebService : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string CourierWebServiceUrl { get; set; }       

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            try
            {
                var initializer = new CourierWebServiceInitializer(CourierWebServiceUrl, new PsCmdletLogger(this));
                initializer.WarmUpWebService();
            }
            catch (Exception e)
            {
                ThrowTerminatingError(
                    new ErrorRecord(
                        e,
                        "UmbracoResources",
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
