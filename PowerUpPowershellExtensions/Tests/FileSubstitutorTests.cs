using System;
using System.Collections.Generic;
using System.Text;
using Id.PowershellExtensions;
using Id.PowershellExtensions.SubstitutedSettingFiles;
using Moq;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Tests
{
    [TestFixture]
    public class FileSubstitutorTests
    {
        [Test]
        public void SubstituteDoesNotChangeSourceFileWhenItIsNotXML()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string file = "Definitely not xml, this ${whatever} see!";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(file);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(file)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string> {{"Key.Name;XPath=true()", "Some new value"}});

        }

        [Test]
        public void SubstitutesWithStringReplacesValueInFlatTextFileWhilstPreservingFileEncoding()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            var encoding = Encoding.Unicode;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns("oo ${boo} too ${boo} coo");
            substitutor.Substitute(@"z:\some.file", new Dictionary<string, string> { { "boo", "bingo" } });
            writer.Verify(w => w.WriteText(@"z:\some.file", "oo bingo too bingo coo", encoding));
        }

        [Test]
        public void SubstituteWithXpathKeyReplacesAttributeValuesInUtf8()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <node dontReplace=\"first value\">value 1</node>\r\n" +
                                     "  <node dontReplace=\"second value\">value 2</node>\r\n" +
                                     "  <node replace=\"third value\">value 3</node>\r\n" +
                                     "</root>";

            const string xmlAfter = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                    "<root>\r\n" +
                                    "  <node dontReplace=\"first value\">value 1</node>\r\n" +
                                    "  <node dontReplace=\"second value\">value 2</node>\r\n" +
                                    "  <node replace=\"Some new value\">value 3</node>\r\n" +
                                    "</root>";

            Encoding encoding;

            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(xmlAfter)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string>
                                       {{"Key.Name;XPath=//node/@replace  ", "Some new value"}});
        }

        [Test]
        public void SubstituteWithXpathKeyReplacesAttributeValuesInUtf16()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <node dontReplace=\"first value\">value 1</node>\r\n" +
                                     "  <node dontReplace=\"second value\">value 2</node>\r\n" +
                                     "  <node replace=\"third value\">value 3</node>\r\n" +
                                     "</root>";

            const string xmlAfter = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n" +
                                    "<root>\r\n" +
                                    "  <node dontReplace=\"first value\">value 1</node>\r\n" +
                                    "  <node dontReplace=\"second value\">value 2</node>\r\n" +
                                    "  <node replace=\"Some new value\">value 3</node>\r\n" +
                                    "</root>";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(xmlAfter)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string> { { "Key.Name;XPath=//node/@replace  ", "Some new value" } });
        }


        [Test]
        public void SubstituteWithXpathKeyReplacesValuesWithAttribute()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <node replace=\"true\">value 1</node>\r\n" +
                                     "  <node replace=\"false\">value 2</node>\r\n" +
                                     "  <node replace=\"true\">value 3</node>\r\n" +
                                     "</root>";

            const string xmlAfter = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                    "<root>\r\n" +
                                    "  <node replace=\"true\">Some new value</node>\r\n" +
                                    "  <node replace=\"false\">value 2</node>\r\n" +
                                    "  <node replace=\"true\">Some new value</node>\r\n" +
                                    "</root>";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(xmlAfter)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string>
                                       {{"Key.Name;XPath=//node[@replace=\"true\"]", "Some new value"}});
        }

        [Test]
        public void SubstituteWithXpathKeysReplacesValuesByBasicXPath()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                     "  <nodeToReplace>value 2</nodeToReplace>\r\n" +
                                     "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                     "</root>";
            const string xmlAfter = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                    "<root>\r\n" +
                                    "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                    "  <nodeToReplace>Some new value</nodeToReplace>\r\n" +
                                    "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                    "</root>";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(xmlAfter)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string> {{"Key.Name;XPath=//nodeToReplace", "Some new value"}});
        }

        [Test]
        public void SubstituteWithXpathKeysReplacesXmlValuesByBasicXPath()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                     "  <nodeToReplace>value 2</nodeToReplace>\r\n" +
                                     "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                     "</root>";
            const string xmlAfter = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                    "<root>\r\n" +
                                    "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                    "  <nodeToReplace>\r\n    <someNewNode>someNewValue</someNewNode>\r\n  </nodeToReplace>\r\n" +
                                    "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                    "</root>";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(xmlAfter)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string> { { "Key.Name;XPath=//nodeToReplace", "<someNewNode>someNewValue</someNewNode>" } });
        }

        [Test]
        public void SubstituteWithXpathKeysReplacesEscapedXmlValuesByBasicXPath()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                     "  <nodeToReplace>value 2</nodeToReplace>\r\n" +
                                     "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                     "</root>";
            const string xmlAfter = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                    "<root>\r\n" +
                                    "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                    "  <nodeToReplace>&lt;!DOCTYPE AdManagerXML SYSTEM 'http://xml.accipiter.com/AdManager/2006/01/AdManager.dtd'&gt;</nodeToReplace>\r\n" +
                                    "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                    "</root>";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(xmlAfter)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string> { { "Key.Name;XPath=//nodeToReplace", "&lt;!DOCTYPE AdManagerXML SYSTEM 'http://xml.accipiter.com/AdManager/2006/01/AdManager.dtd'&gt;" } });
        }

        [Test]
        public void SubstituteWithXpathKeysReplacesValuesThatContainsAmpersandsByBasicXPath()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                     "  <nodeToReplace>value 2</nodeToReplace>\r\n" +
                                     "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                     "</root>";
            const string xmlAfter = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                    "<root>\r\n" +
                                    "  <nodeToLeave>value 1</nodeToLeave>\r\n" +
                                    "  <nodeToReplace>foo&amp;bar</nodeToReplace>\r\n" +
                                    "  <nodeToLeave>value 3</nodeToLeave>\r\n" +
                                    "</root>";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            writer.Setup(w => w.WriteText(@"z:\some.file", It.IsAny<string>(), It.IsAny<Encoding>())).Callback(
                (string fileOut, string xmlOut, Encoding encodingOut) => Assert.That(xmlOut, Is.EqualTo(xmlAfter)));
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string> { { "Key.Name;XPath=//nodeToReplace", "foo&bar" } });
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void SubstituteThrowsInvalidOperationExceptionIfXpathEvaluatesToNonEnumerable()
        {
            var reader = new Mock<ITextFileReader>();
            var writer = new Mock<ITextFileWriter>();
            var substitutor = new FileSubstitutor(reader.Object, writer.Object);

            const string xmlBefore = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                                     "<root>\r\n" +
                                     "  <node dontReplace=\"first value\">value 1</node>\r\n" +
                                     "  <node dontReplace=\"second value\">value 2</node>\r\n" +
                                     "  <node replace=\"third value\">value 3</node>\r\n" +
                                     "</root>";

            Encoding encoding;
            reader.Setup(r => r.ReadText(@"z:\some.file", out encoding)).Returns(xmlBefore);
            substitutor.Substitute(@"z:\some.file",
                                   new Dictionary<string, string> {{"Key.Name;XPath=true()", "Some new value"}});
        }
    }
}