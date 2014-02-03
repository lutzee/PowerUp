using System.Collections;
using System.Linq;
using Id.PowershellExtensions.ParsedSettings;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class OverrideSettingsReaderTests
    {
        [Test]
        public void Reader_ReadSettings_EmptyTable_ReturnsZeroLines()
        {
            var reader = new OverrideSettingsReader(new Hashtable(), "TestSection");
            Assert.AreEqual(0, reader.ReadSettings().Count());
        }

        [Test]
        public void Reader_ReadSettings_NullTable_ReturnsZeroLines()
        {
            var reader = new OverrideSettingsReader(null, "TestSection");
            Assert.AreEqual(0, reader.ReadSettings().Count());
        }

        [Test]
        public void Reader_ReadSettings_NonEmptyTable_ReturnsCorrectlyFormattedLines()
        {
            var settings = new Hashtable
                {
                    {"key1", "value1"},
                    {"key2", "value2"},
                    {"key3", "value3"},
                };
            var reader = new OverrideSettingsReader(settings, "TestSection");

            var readLines = reader.ReadSettings().ToList();

            Assert.AreEqual(4, readLines.Count);
            Assert.AreEqual("TestSection", readLines[0]);
            Assert.Contains("\tkey1\tvalue1", readLines);
            Assert.Contains("\tkey2\tvalue2", readLines);
            Assert.Contains("\tkey3\tvalue3", readLines);
        }
    }
}