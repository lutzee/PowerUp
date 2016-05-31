using System.Linq;
using Id.PowershellExtensions.ParsedSettings;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SettingsFileReaderTests
    {
        [Test]
        public void SettingsFileReader_ReadSettings_FromSettingsFile_ReturnsExpectedResult()
        {
            var reader = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.Settings.txt"));
            Assert.AreEqual(9, reader.ReadSettings().Count());
        }

        [Test]
        public void SettingsFileReader_ReadSettings_FromXmlSettingsFile_ReturnsExpectedResult()
        {
            var reader = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.Settings.xml"));
            Assert.AreEqual(6, reader.ReadSettings().Count());
        }
    }
}
