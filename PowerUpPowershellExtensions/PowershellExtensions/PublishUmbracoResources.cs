using System;
using System.Management.Automation;

using Id.PowershellExtensions.UmbracoResources;
using Umbraco.Courier.RepositoryProviders;

namespace Id.PowershellExtensions
{
    [Cmdlet(VerbsData.Publish, "UmbracoResources", SupportsShouldProcess = true)]
    public class PublishUmbracoResources : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
        public string RevisionDirectory { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true)]
        public string TargetUmbracoUrl { get; set; }
        
        [Parameter(Mandatory = true, Position = 3, ValueFromPipelineByPropertyName = true)]
        public string UmbracoUsername { get; set; }

        [Parameter(Mandatory = true, Position = 4, ValueFromPipelineByPropertyName = true)]
        public string UmbracoPassword { get; set; }

        [Parameter(Mandatory = true, Position = 5, ValueFromPipelineByPropertyName = true)]
        public string UmbracoPasswordEncoding { get; set; }

        [Parameter(Mandatory = false, Position = 6, ValueFromPipelineByPropertyName = true)]
        public string PluginFolder { get; set; }

        protected override void ProcessRecord()
        {
            try
            {
                var courierWebserviceRepositoryProvider = new CourierWebserviceRepositoryProvider
                                                              {
                                                                  Url = TargetUmbracoUrl,
                                                                  Login = UmbracoUsername,
                                                                  Password = UmbracoPassword,
                                                                  PasswordEncoding = UmbracoPasswordEncoding,
                                                                  UserId = -1,
                                                                  Name = "webservicerepository"
                                                              };

                var resourcePublisher = new ResourcePublisher(courierWebserviceRepositoryProvider, new PsCmdletLogger(this), PluginFolder);
                resourcePublisher.Publish(RevisionDirectory);
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
    }
}
