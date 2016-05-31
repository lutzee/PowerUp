using System.Collections.Generic;
using System.IO;
using Id.PowershellExtensions.SubstitutedSettingFiles;
using NUnit.Framework;
using Tests.Helpers;

namespace Tests
{
    [TestFixture]
    public class SettingsSubstitutorTests
    {
        private string _templatesFolder;
        private string _outputFolder;
        private ISettingsSubstitutor _settingsSubstitutor;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _templatesFolder = GetTempFolderName();
            Directory.CreateDirectory(_templatesFolder);

            ResourceHelpers.SaveResourceToDisk(_templatesFolder, "Tests.ExampleTemplates.TemplateWithSubfile.txt");
            ResourceHelpers.SaveResourceToDisk(_templatesFolder, "Tests.ExampleTemplates.Subfile1.txt");
            ResourceHelpers.SaveResourceToDisk(_templatesFolder, "Tests.ExampleTemplates.Subfile2.txt");
        }

        [SetUp]
        public void SetUp()
        {
            _outputFolder = GetTempFolderName();
            _settingsSubstitutor = new SettingsSubstitutor();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_outputFolder))
            {
                Directory.Delete(_outputFolder, true);
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (Directory.Exists(_templatesFolder))
            {
                Directory.Delete(_templatesFolder, true);
            }
        }
        
        private static string GetTempFolderName()
        {
            return Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        [Test]
        public void Should_replace_subfiles_in_template()
        {
            //Arrange
            var settings = new Dictionary<string, string[]>
            {
                { "StandardSetting", new [] { "StandardSettingValue" } },
                { "SubFileFileName", new [] { "Tests.ExampleTemplates.Subfile2.txt" } },
                { "SecurityMode", new [] { "Transport" } }
            };

            //Act
            _settingsSubstitutor.CreateSubstitutedDirectory(_templatesFolder, _outputFolder, "test", settings);

            //Assert
            var fullEnvironmentFilename = Path.Combine(_outputFolder, "test", "Tests.ExampleTemplates.TemplateWithSubfile.txt");
            var subsituted = Id.PowershellExtensions.Helpers.GetFileWithEncoding(fullEnvironmentFilename);

            Assert.That(subsituted.Contents, Is.StringContaining("<!-- Secure config -->"));
            Assert.That(subsituted.Contents, Is.StringContaining("StandardSettingValue"));
            Assert.That(subsituted.Contents, Is.StringContaining("Transport"));
        }
    }
}
