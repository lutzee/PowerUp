using Id.PowershellExtensions.UmbracoResources;
using NUnit.Framework;
using Tests.Doubles;

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
            var initializer = new CourierWebServiceInitializer("http://www.w3schools.com/xml/tempconvert.asmx", logger);

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