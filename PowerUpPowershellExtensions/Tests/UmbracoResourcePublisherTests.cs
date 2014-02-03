using System;
using Id.PowershellExtensions.UmbracoResources;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Umbraco.Courier.RepositoryProviders;

namespace Tests
{
    [TestFixture]
    public class UmbracoResourcePublisherTests
    {
        [Test]
        public void WhenRevisionPublished_ThenProviderCalledWithExpectedNumberOfResources()
        {
            var repositoryProviderDouble = new RepositoryProviderDouble();
            var publisher = new ResourcePublisher(repositoryProviderDouble, new PsCmdletLoggerDouble(), @".\UmbracoResources\plugins");

            // fake instatiation in order to load the repository provider into the app domain
            var provider = new CourierWebserviceRepositoryProvider();

            publisher.Publish(@"..\..\ExampleUmbracoRevisions\V1");

            Assert.That(repositoryProviderDouble.ResourcesSeen, Is.EqualTo(113));
        }
    }
}