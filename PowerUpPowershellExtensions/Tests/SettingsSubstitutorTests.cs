using System;
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
            ResourceHelpers.SaveResourceToDisk(_templatesFolder, "Tests.ExampleTemplates.Subfile.environment1.template");
            ResourceHelpers.SaveResourceToDisk(_templatesFolder, "Tests.ExampleTemplates.Subfile.environment2.template");
            ResourceHelpers.SaveResourceToDisk(_templatesFolder, "Tests.ExampleTemplates.Subfile.test.template");
        }

        [SetUp]
        public void SetUp()
        {
            _outputFolder = GetTempFolderName();
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
        [TestCase("environment1", "environment1")]
        [TestCase("environment2", "environment2")]
        [TestCase("environment1", "test")]
        public void Should_replace_subfiles_in_template(string environment, string setting)
        {
            _settingsSubstitutor = new SettingsSubstitutor(environment);

            //Arrange
            var settings = new Dictionary<string, string[]>
            {
                { "StandardSetting1", new [] { "AAA" } },
                { "StandardSetting2", new [] { "BBB" } },
                { "SubFileFileName", new [] { "Tests.ExampleTemplates.Subfile." + setting +  ".template" } }
            };

            // Only add the 3rd setting when testing environment2 - this demonstrates that missing settings don't matter
            // when doing sub-template substitutions as only the sub-templates for the current environment are handled.
            if (environment.Equals("environment2", StringComparison.OrdinalIgnoreCase))
            {
                settings.Add("StandardSetting3", new [] { "CCC" });
            }

            //Act
            _settingsSubstitutor.CreateSubstitutedDirectory(_templatesFolder, _outputFolder, settings);

            //Assert
            var fullEnvironmentFilename = Path.Combine(_outputFolder, environment, "Tests.ExampleTemplates.TemplateWithSubfile.txt");
            var subsituted = Id.PowershellExtensions.Helpers.GetFileWithEncoding(fullEnvironmentFilename);

            Assert.That(subsituted.Contents, Is.StringContaining("StandardSetting2"));
            Assert.That(subsituted.Contents, Is.StringContaining("BBB"));

            var outputDir = Path.Combine(_outputFolder, environment);
            var templates = Directory.GetFiles(outputDir, "*.template");

            Assert.That(templates.Length, Is.EqualTo(0));
        }
    }
}
