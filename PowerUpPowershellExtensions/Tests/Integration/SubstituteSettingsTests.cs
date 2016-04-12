using System.Collections;
using System.IO;
using System.Text;
using href.Utils;
using Id.PowershellExtensions;
using NUnit.Framework;

namespace Tests.Integration
{
    [TestFixture]
    public class SubstituteSettingsTests
    {
        private string file1 = @"Integration\_environments\Test\sample.txt";
        private string file2 = @"Integration\_environments\Test\subfolder\sample2.txt";
        private string file3 = @"Integration\_environments\Test\sample3.txt";

        [Test]
        public void WhenSubtituteSettingsCalled_SubstitutionsOccur()
        {
            CleanOutput();

            var hash = new Hashtable { { "Key A", new []{"Value A"} }, { "Key B", new []{"Value B" }} };

            var cmd = new CopySubstitutedSettingFiles()
                          {
                              DeploymentEnvironment = "Test",
                              TemplatesDirectory = @"Integration\_templates",
                              TargetDirectory = @"Integration\_environments",
                              Settings = hash
                          };

            var result = cmd.Invoke().GetEnumerator();

            result.MoveNext();

            var text1 = File.ReadAllText(file1);
            Assert.That(text1, Is.EqualTo( @"some sttuffff Value A"));

        }


        [Test]
        public void WhenSubtituteSettingsCalled_EscapedSubstitutionsRemain()
        {
            CleanOutput();

            var hash = new Hashtable { { "Key A", new[] { "Value A" } }, { "Key B", new []{"Value B" }}, { "verbatim", new[] { "not used" } } };

            var cmd = new CopySubstitutedSettingFiles()
            {
                DeploymentEnvironment = "Test",
                TemplatesDirectory = @"Integration\_templates",
                TargetDirectory = @"Integration\_environments",
                Settings = hash
            };

            var result = cmd.Invoke().GetEnumerator();

            result.MoveNext();

            var text1 = File.ReadAllText(file3);
            Assert.That(text1, Is.EqualTo(@"some sttuffff Value A ${verbatim} then some"));

        }
       

        [Test]
        public void WhenSubtituteSettingsCalled_SubstitutionsMaintainEncoding()
        {
            CleanOutput();

            var hash = new Hashtable { { "Key A", new[] { "Value A" } },  { "Key B", new[] {"Value B" }} };

            var cmd = new CopySubstitutedSettingFiles()
            {
                DeploymentEnvironment = "Test",
                TemplatesDirectory = @"Integration\_templates",
                TargetDirectory = @"Integration\_environments",
                Settings = hash
            };

            var result = cmd.Invoke().GetEnumerator();

            result.MoveNext();

            var encoding = EncodingTools.DetectInputCodepage(File.ReadAllBytes(file2));
            
            Assert.That(encoding, Is.EqualTo(Encoding.Unicode));

        }

        private void CleanOutput()
        {
            if (File.Exists(file1))
                File.Delete(file1);

            if (File.Exists(file2))
                File.Delete(file2);
        }
    }
}
