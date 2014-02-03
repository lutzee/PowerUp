using System.Collections;
using System.Collections.Generic;
using System.IO;
using Id.PowershellExtensions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Tests.Integration
{
    [TestFixture]
    [Ignore("Can only run in very specific environments")]
    public class PublishUmbracoResourcesTests
    {

        [Test] 
        public void WhenPublishCalledForRealUrl_ThenPublishingOccurs()
        {
            Publish();
        }

        private static void Publish()
        {
            var cmd = new PublishUmbracoResources()
                          {
                              RevisionDirectory = @"ExampleUmbracoRevisions\V1",
                              UmbracoPassword = "password",
                              UmbracoUsername = "admin",
                              TargetUmbracoUrl = "http://couriertest2.eid.co.nz",
                              UmbracoPasswordEncoding = "Hashed",
                              PluginFolder = @".\UmbracoResources\plugins"
                          };

            var result = cmd.Invoke().GetEnumerator();

            result.MoveNext();
        }

        [Test]
        public void WhenPublishCalledForRealUrlTwice_ThenPublishingOccurs()
        {
            Publish();
            Publish();
        }
    }
}
