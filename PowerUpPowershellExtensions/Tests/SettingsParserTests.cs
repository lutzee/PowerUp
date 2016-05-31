using System;
using System.Collections.Generic;
using System.Linq;
using Id.PowershellExtensions.ParsedSettings;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SettingsParserTests
    {
        private const string Basic = "Tests.ExampleSettingsFiles.Settings.txt";
        private const string AdvancedSettings = "Tests.ExampleSettingsFiles.AdvancedSettings.txt";
        private const string AdvancedSettingsWhitespaceDelimited = "Tests.ExampleSettingsFiles.AdvancedSettingsSpaceDelimited.txt";
        private const string AdvancedSettingsXml = "Tests.ExampleSettingsFiles.AdvancedSettings.xml";
        private const string MultipleSettings = "Tests.ExampleSettingsFiles.MultipleSettings.txt";
        private const string InvalidSettings = "Tests.ExampleSettingsFiles.InvalidSettings.txt";
        private const string TGSettings = "Tests.ExampleSettingsFiles.TGSettings.txt";
        private const string VisaSettings = "Tests.ExampleSettingsFiles.VisaSettings.txt";
        private const string Servers = "Tests.ExampleSettingsFiles.Servers.txt";
        private const string SettingsWithInheritance = "Tests.ExampleSettingsFiles.SettingsWithInheritance.txt";
        private const string SettingsWithInheritanceAndRepetitionWarnings = "Tests.ExampleSettingsFiles.SettingsWithInheritanceAndRepetitionWarnings.txt";
        private const string SettingsWithInheritanceAndRevertedValueWarnings = "Tests.ExampleSettingsFiles.SettingsWithInheritanceAndRevertedValueWarnings.txt";
        private const string SettingsWithInheritanceSeparateFile = "Tests.ExampleSettingsFiles.SettingsWithInheritanceSeparateFile.txt";
        private const string SettingsWithInheritanceXml = "Tests.ExampleSettingsFiles.SettingsWithInheritance.xml";
        private const string RepeatSectionSettings = "Tests.ExampleSettingsFiles.RepeatSectionSettings.txt";
        private const string SettingsWithReservedKey = "Tests.ExampleSettingsFiles.SettingsWithReservedKey.txt";

        private static SettingsFileReader GetSettingsFileReader(string resourceName)
        {
            return new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource(resourceName));
        }

        [Test]
        public void SettingsParser_Parse_BasicSettings_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(Basic);
            var settings = sp.Parse(reader.ReadSettings(), "Dev", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(4, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("other", settings.Keys.ElementAt(2));
            Assert.AreEqual("5", settings["Wotsit"][0]);
            Assert.AreEqual("3", settings["Thing"][0]);
            Assert.AreEqual("4", settings["other"][0]);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Could not find section \"AAAA\"")]
        public void SettingsParser_Parse_with_invalid_section_throws_exception()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(Basic);
            sp.Parse(reader.ReadSettings(), "AAAA", '|');
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Could not find section \"AAAA\"")]
        public void SettingsParser_Parse_settings_with_inheritance_with_invalid_section_throws_exception()
        {
            var sp = new SettingsParser {AppendReservedSettings = true};
            var reader1 = GetSettingsFileReader(SettingsWithInheritance);
            var reader2 = GetSettingsFileReader(SettingsWithInheritanceSeparateFile);
            var lines = reader1.ReadSettings().Concat(reader2.ReadSettings());
            var overrides = new[]
            {
                "AAAA",
                "\tpackage.name	",
                "\tpackage.build	1",
                "\tpackage.date	20150709-0948"
            };

            sp.Parse(lines, overrides, "AAAA", '|');
        }

        [Test]
        public void SettingsParser_Parse_settings_with_inheritance_with_valid_section_parses_settings()
        {
            var sp = new SettingsParser { AppendReservedSettings = true };
            var reader1 = GetSettingsFileReader(SettingsWithInheritance);
            var reader2 = GetSettingsFileReader(SettingsWithInheritanceSeparateFile);
            var lines = reader1.ReadSettings().Concat(reader2.ReadSettings());
            var overrides = new[]
            {
                "Dev",
                "\tpackage.name	",
                "\tpackage.build	1",
                "\tpackage.date	20150709-0948"
            };

            sp.Parse(lines, overrides, "Dev", '|');
        }

        [Test]
        [TestCase(AdvancedSettings)]
        [TestCase(AdvancedSettingsWhitespaceDelimited)]
        public void SettingsParser_Parse_AdvancedSettings_ReturnsExpectedResult(string settingsFileName)
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(settingsFileName);
            var settings = sp.Parse(reader.ReadSettings(), new String[0], "Dev", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(5, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("Other", settings.Keys.ElementAt(2));
            Assert.AreEqual("3  4 5", settings["Wotsit"][0]);
            Assert.AreEqual("3  4", settings["Thing"][0]);
            Assert.AreEqual("4", settings["Other"][0]);
        }

        [Test]
        public void SettingsParser_Parse_RepeatSettings_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(RepeatSectionSettings);
            var settings = sp.Parse(reader.ReadSettings(), "Live", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(5, settings.Keys.Count);
        }

        [Test]
        public void SettingsParser_Parse_XmlAdvancedSettings_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(AdvancedSettingsXml);
            var settings = sp.Parse(reader.ReadSettings(), "Dev", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(4, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("Other", settings.Keys.ElementAt(2));
            Assert.AreEqual("3 4 5", settings["Wotsit"][0]);
            Assert.AreEqual("3 4", settings["Thing"][0]);
            Assert.AreEqual("4", settings["Other"][0]);
        }

        [Test]
        public void SettingsParser_Parse_TGSettings_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(TGSettings);
            var settings = sp.Parse(reader.ReadSettings(), "DEV", '|');

            Assert.IsNotNull(settings);
        }

        [Test]
        public void SettingsParser_Parse_VisaSettings_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(VisaSettings);
            var settings = sp.Parse(reader.ReadSettings(), "Test", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(10, settings.Keys.Count);
            Assert.AreEqual(@"VisaDebitMicroSiteAU", settings["ProjectName"][0]);
            Assert.AreEqual(@"\\reliant", settings["DeployServer"][0]);
            Assert.AreEqual(@"e:\temp", settings["DeploymentPath"][0]);
            Assert.AreEqual(@"\\reliant\e$\releasetemp", settings["RemoteReleaseWorkingFolder"][0]);
            Assert.AreEqual(@"e:\releasetemp", settings["LocalReleaseWorkingFolder"][0]);
            Assert.AreEqual(@"VisaDebitMicroSiteAUadmin", settings["AdminSiteFolder"][0]);
            Assert.AreEqual(@"VisaDebitMicroSiteAUadmin.dev.work", settings["AdminSiteUrl"][0]);
            Assert.AreEqual(@"VisaDebitMicroSiteAUweb", settings["WebSiteFolder"][0]);
            Assert.AreEqual(@"VisaDebitMicroSiteAU.dev.work", settings["WebSiteUrl"][0]);
        }

        [Test]
        public void SettingsParser_Parse_DelimitedReturnsMultipleValues()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(MultipleSettings);
            var settings = sp.Parse(reader.ReadSettings(), "Live", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(5, settings.Keys.Count);
            Assert.AreEqual("Other", settings.Keys.ElementAt(1));
            Assert.AreEqual("2", settings["Other"][0]);
            Assert.AreEqual("3", settings["Other"][1]);
            Assert.AreEqual("2|3", settings["Quoted"][0]);
            Assert.AreEqual("", settings["Nothing"][0]);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Circular dependency detected")]
        public void SettingsParser_Parse_InvalidSettings_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(InvalidSettings);
            var settings = sp.Parse(reader.ReadSettings(), "Dev", '|');
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Reserved key \"environment.profile\" found")]
        public void SettingsParser_Parse_SettingsWithReservedKey_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(SettingsWithReservedKey);
            sp.Parse(reader.ReadSettings(), "Dev", '|');
        }

        [Test]
        public void SettingsParser_Parse_Servers_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(Servers);
            var settings = sp.Parse(reader.ReadSettings(), "icevm069", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(6, settings.Keys.Count);
            Assert.AreEqual(@"icevm069", settings["server.name"][0]);
            Assert.AreEqual(@"d", settings["local.root.drive.letter"][0]);
            Assert.AreEqual(@"_releasetemp", settings["deployment.working.folder"][0]);
            Assert.AreEqual(@"d:\_releasetemp", settings["local.temp.working.folder"][0]);
            Assert.AreEqual(@"\\icevm069\_releasetemp", settings["remote.temp.working.folder"][0]);
        }

        [Test]
        [TestCase("dev")]
        [TestCase("live")]
        public void SettingsParser_Parse_AdvancedSettings_for_environment_profile_ReturnsExpectedResult(string section)
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(AdvancedSettings);
            var settings = sp.Parse(reader.ReadSettings(), section, '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(section, settings["environment.profile"][0]);
            Assert.AreEqual(section, settings["Profile"][0]);
        }

        [Test]
        [TestCase("dev", true)]
        [TestCase("dev", false)]
        [TestCase("live", true)]
        [TestCase("live", false)]
        public void SettingsParser_Parse_includes_environment_profile(string section, bool include)
        {
            var sp = new SettingsParser
            {
                AppendReservedSettings = include
            };
            var reader = GetSettingsFileReader(Basic);
            var settings = sp.Parse(reader.ReadSettings(), section, '|');

            Assert.IsNotNull(settings);

            if (include)
            {
                Assert.AreEqual(section, settings["environment.profile"][0]);
            }
            else
            {
                Assert.IsFalse(settings.ContainsKey("environment.profile"));
            }
        }

        [Test]
        public void SettingsParser_Parse_SettingsWithInheritanceAt2Levels_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(SettingsWithInheritance);
            var settings = sp.Parse(reader.ReadSettings(), "Live", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(4, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("Other", settings.Keys.ElementAt(2));
            Assert.AreEqual("10", settings["Wotsit"][0]);
            Assert.AreEqual("30", settings["Thing"][0]);
            Assert.AreEqual("2", settings["Other"][0]);
        }

        [Test]
        public void SettingsParser_Parse_SettingsWithInheritanceAt3Levels_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(SettingsWithInheritance);
            var settings = sp.Parse(reader.ReadSettings(), "Prod", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(4, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("Other", settings.Keys.ElementAt(2));
            Assert.AreEqual("10", settings["Wotsit"][0]);
            Assert.AreEqual("30", settings["Thing"][0]);
            Assert.AreEqual("200", settings["Other"][0]);
        }

        [Test]
        public void SettingsParser_Parse_SettingsXmlWithInheritanceAt3Levels_ReturnsExpectedResult()
        {
            var sp = new SettingsParser();
            var reader = GetSettingsFileReader(SettingsWithInheritanceXml);
            var settings = sp.Parse(reader.ReadSettings(), "Prod", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(4, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("Other", settings.Keys.ElementAt(2));
            Assert.AreEqual("10", settings["Wotsit"][0]);
            Assert.AreEqual("30", settings["Thing"][0]);
            Assert.AreEqual("200", settings["Other"][0]);
        }

        [Test]
        [TestCase("Section2a", 1)]
        [TestCase("Section2b", 4)]
        [TestCase("Section3", 1)]
        public void SettingsParser_Parse_Should_Raise_Repetition_Warnings(
            string section,
            int expectedWarnings
        )
        {
            var warnings = new HashSet<string>();
            var sp = new SettingsParser
            {
                WriteWarning = s => warnings.Add(s)
            };

            var reader = GetSettingsFileReader(SettingsWithInheritanceAndRepetitionWarnings);
            var settings = sp.Parse(reader.ReadSettings(), section, '|');

            Assert.IsNotNull(settings);
            CollectionAssert.IsNotEmpty(warnings);
            Assert.AreEqual(expectedWarnings, warnings.Count);
        }

        [Test]
        public void SettingsParser_Validate_Should_Raise_Repetition_Warnings()
        {
            var warnings = new HashSet<string>();
            var sp = new SettingsParser
            {
                WriteWarning = s => warnings.Add(s)
            };

            var reader = GetSettingsFileReader(SettingsWithInheritanceAndRepetitionWarnings);
            sp.Validate(reader.ReadSettings());

            CollectionAssert.IsNotEmpty(warnings);
            Assert.AreEqual(7, warnings.Count);
        }

        [Test]
        [TestCase("Section2", 1)]
        [TestCase("Section3", 2)]
        public void SettingsParser_Parse_Should_Raise_RevertedValue_Warnings(
            string section,
            int expectedWarnings
        )
        {
            var warnings = new HashSet<string>();
            var sp = new SettingsParser
            {
                WriteWarning = s => warnings.Add(s)
            };

            var reader = GetSettingsFileReader(SettingsWithInheritanceAndRevertedValueWarnings);
            var settings = sp.Parse(reader.ReadSettings(), section, '|');

            Assert.IsNotNull(settings);
            CollectionAssert.IsNotEmpty(warnings);
            Assert.AreEqual(expectedWarnings, warnings.Count);
        }

        [Test]
        public void SettingsParser_Validate_Should_Raise_RevertedValue_Warnings()
        {
            var warnings = new HashSet<string>();
            var sp = new SettingsParser
            {
                WriteWarning = s => warnings.Add(s)
            };

            var reader = GetSettingsFileReader(SettingsWithInheritanceAndRevertedValueWarnings);
            sp.Validate(reader.ReadSettings());

            CollectionAssert.IsNotEmpty(warnings);
            Assert.AreEqual(2, warnings.Count);
        }
    }
}
