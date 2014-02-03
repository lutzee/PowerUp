using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Id.PowershellExtensions.UmbracoResources
{
    public class CourierWebServiceInitializer
    {
        private readonly string CourierWebServiceUrl;
        private readonly IPsCmdletLogger Logger;

        public CourierWebServiceInitializer(string courierWebServiceUrl, IPsCmdletLogger logger)
        {
            CourierWebServiceUrl = courierWebServiceUrl;
            Logger = logger;
        }

        public void WarmUpWebService()
        {
            try
            {
                var html = new WebClient().DownloadString(new Uri(CourierWebServiceUrl));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
