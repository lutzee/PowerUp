using Id.PowershellExtensions.UmbracoResources;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    [Category("Web")]
    public class CourierWebServiceInitializerTests
    {
        [Test]
        public void WhenCalledWithValidExternalUri_Succeeds()
        {
            var logger = new PsCmdletLoggerDouble();
            var initializer = new CourierWebServiceInitializer("http://www.w3schools.com/webservices/tempconvert.asmx", logger);

            initializer.WarmUpWebService();

            Assert.That(logger.ExceptionsLogged.Equals(0));
        }

        [Test]
        [Ignore]
        public void WhenCalledWithValidCourierUri_Succeeds()
        {
            var logger = new PsCmdletLoggerDouble();
            var initializer = new CourierWebServiceInitializer("http://courier.MILKBooksWebsite.eid.co.nz.local:8080/umbraco/plugins/courier/webservices/Repository.asmx", logger);
        
            initializer.WarmUpWebService();

            Assert.That(logger.ExceptionsLogged.Equals(0));
        }


        [Test]
        public void WhenCalledWithInvalidUri_FailsAndLogs()
        {
            var logger = new PsCmdletLoggerDouble();
            var initializer = new CourierWebServiceInitializer("Invalid Uri", logger);

            initializer.WarmUpWebService();

            Assert.That(logger.ExceptionsLogged.Equals(1));
        }
    }
}