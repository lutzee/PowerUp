using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Id.PowershellExtensions.ParsedSettings;
using NUnit.Framework;
using System.IO;
using Id.PowershellExtensions;

namespace Tests
{
    [TestFixture]
    public class SettingsParserTests
    {
        SettingsFileReader _basicSettings;
        SettingsFileReader _advancedSettings;
        SettingsFileReader _advancedXmlSettings;
        SettingsFileReader _invalidSettings;
        SettingsFileReader _tgSettings;
        SettingsFileReader _visaSettings;
        SettingsFileReader _serverSettings;
        SettingsFileReader _multipleSettings;
        SettingsFileReader _inheritanceSettings;
        SettingsFileReader _inheritanceXmlSettings;
        SettingsFileReader _repeatSectionSettings;

        [SetUp]
        public void SetUp()
        {
            _basicSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.Settings.txt"));
            _advancedSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.AdvancedSettings.txt"));
            _advancedXmlSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.AdvancedSettings.xml"));
            _multipleSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.MultipleSettings.txt"));
            _invalidSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.InvalidSettings.txt"));
            _tgSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.TGSettings.txt"));
            _visaSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.VisaSettings.txt"));
            _serverSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.Servers.txt"));
            _inheritanceSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.SettingsWithInheritance.txt"));
            _inheritanceXmlSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.SettingsWithInheritance.xml"));
            _repeatSectionSettings = new SettingsFileReader(Helpers.ResourceHelpers.GetStreamFromResource("Tests.ExampleSettingsFiles.RepeatSectionSettings.txt"));
        }

        [TearDown]
        public void TearDown()
        {
            _basicSettings = null;
            _advancedSettings = null;
            _invalidSettings = null;
            _tgSettings = null;
            _visaSettings = null;
            _serverSettings = null;
            _multipleSettings = null;
            _inheritanceSettings = null;
        }


        [Test]
        public void SettingsParser_Parse_BasicSettings_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_basicSettings.ReadSettings(), "Dev", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(3, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("other", settings.Keys.ElementAt(2));
            Assert.AreEqual("5", settings["Wotsit"][0]);
            Assert.AreEqual("3", settings["Thing"][0]);
            Assert.AreEqual("4", settings["other"][0]);
        }


        [Test]
        public void SettingsParser_Parse_AdvancedSettings_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_advancedSettings.ReadSettings(), "Dev", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(3, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("Other", settings.Keys.ElementAt(2));
            Assert.AreEqual("3 4 5", settings["Wotsit"][0]);
            Assert.AreEqual("3 4", settings["Thing"][0]);
            Assert.AreEqual("4", settings["Other"][0]);
        }

        [Test]
        public void SettingsParser_Parse_RepearSettings_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_repeatSectionSettings.ReadSettings(), "Live", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(4, settings.Keys.Count);
           
        }

        [Test]
        public void SettingsParser_Parse_XmlAdvancedSettings_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_advancedXmlSettings.ReadSettings(), "Dev", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(3, settings.Keys.Count);
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
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_tgSettings.ReadSettings(), "DEV", '|');

            Assert.IsNotNull(settings);
        }

        [Test]
        public void SettingsParser_Parse_VisaSettings_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_visaSettings.ReadSettings(), "Test", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(9, settings.Keys.Count);
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
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_multipleSettings.ReadSettings(), "Live", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(4, settings.Keys.Count);
            Assert.AreEqual("Other", settings.Keys.ElementAt(1));
            Assert.AreEqual("2", settings["Other"][0]);
            Assert.AreEqual("3", settings["Other"][1]);
            Assert.AreEqual("2|3", settings["Quoted"][0]);
            Assert.AreEqual("", settings["Nothing"][0]);
        }

        [Test]
        [ExpectedException(typeof(Exception), "Circular dependency detected")]
        public void SettingsParser_Parse_InvalidSettings_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_invalidSettings.ReadSettings(), "Dev", '|');
        }

        [Test]
        public void SettingsParser_Parse_Servers_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_serverSettings.ReadSettings(), "icevm069", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(5, settings.Keys.Count);
            Assert.AreEqual(@"icevm069", settings["server.name"][0]);
            Assert.AreEqual(@"d", settings["local.root.drive.letter"][0]);
            Assert.AreEqual(@"_releasetemp", settings["deployment.working.folder"][0]);
            Assert.AreEqual(@"d:\_releasetemp", settings["local.temp.working.folder"][0]);
            Assert.AreEqual(@"\\icevm069\_releasetemp", settings["remote.temp.working.folder"][0]);
        }

        [Test]
        public void SettingsParser_Parse_SettingsWithInheritanceAt2Levels_ReturnsExpectedResult()
        {
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_inheritanceSettings.ReadSettings(), "Live", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(3, settings.Keys.Count);
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
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_inheritanceSettings.ReadSettings(), "Prod", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(3, settings.Keys.Count);
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
            SettingsParser sp = new SettingsParser();
            var settings = sp.Parse(_inheritanceXmlSettings.ReadSettings(), "Prod", '|');

            Assert.IsNotNull(settings);
            Assert.AreEqual(3, settings.Keys.Count);
            Assert.AreEqual("Wotsit", settings.Keys.ElementAt(0));
            Assert.AreEqual("Thing", settings.Keys.ElementAt(1));
            Assert.AreEqual("Other", settings.Keys.ElementAt(2));
            Assert.AreEqual("10", settings["Wotsit"][0]);
            Assert.AreEqual("30", settings["Thing"][0]);
            Assert.AreEqual("200", settings["Other"][0]);
        }

        [Test]
        public void URI()
        {
            var uri = new Uri("http://cms.milkbooks.com/umbraco/umbraco.aspx?sdsd=12");

            Assert.AreEqual("cms.milkbooks.com", uri.Host);
        }
    }
}
