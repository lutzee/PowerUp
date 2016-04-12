using System.Collections.Generic;
using Id.PowershellExtensions.RemoteAgent;
using NUnit.Framework;

namespace Tests.RemoteAgent
{
    [TestFixture]
    public class RemoteAgentFixture
    {
        [Test, Ignore("Needs http://localhost/RemoteAgentWeb/ to be running")]
        public void Should_post_settings_to_remote_agent()
        {
            var settings = new Dictionary<string, string>
            {
                { "RemoteAgentUrl", "http://localhost/RemoteAgentWeb/" },
                { "AccessKey", "AKIAJUSZ3BRBZK2XYDNA" },
                { "BucketName", "Transfer.Investigator.WynyardGroup" },
                { "DeploymentProfile", "local" },
                //{ "DeploymentScriptFilename", "deploy.bat" },
                { "Filename", "package_acanancyweb_30.zip" },
                { "SecretKey", "18XpZXzCHXGu1UxSMOnU9kIPksqVzSrhTG0shy/c" },
                { "SecurityKey", "7662E26F-7A40-4299-9193-F5BBA8067612" }
            };

            var pub = new Publisher(settings, null);
            var output = pub.PublishPackage();

            Assert.IsNotNull(output);
        }
    }
}
