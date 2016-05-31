using System.Collections.Generic;
using System.IO;
using Id.PowershellExtensions;
using Id.PowershellExtensions.ZipManipulation;
using Ionic.Zip;
using NUnit.Framework;
using Tests.Helpers;

namespace Tests
{
    [TestFixture]
    public class ZipFileAugmentorTests
    {
        private string _zipFileName;
        private IZipFileAugmentor _component;
        private HashSet<string> _filesCreated;

        [SetUp]
        public void SetUp()
        {
            _filesCreated = new HashSet<string>();
            _component = new ZipFileAugmentor(new TraceLogger());
            _zipFileName = GetTempFileName();
            ResourceHelpers.SaveResourceToDiskAsFilename(_zipFileName, "Tests.Resources.Archive.zip");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var fileName in _filesCreated)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        private string GetTempFileName(string subfolder = null)
        {
            string filename;

            if (string.IsNullOrWhiteSpace(subfolder))
            {
                filename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            }
            else
            {
                filename = Path.Combine(Path.GetTempPath(), subfolder, Path.GetRandomFileName());
                var folder = Path.GetDirectoryName(filename);

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            _filesCreated.Add(filename);

            return filename;
        }

        private string GetPopulatedTempFileName(string subfolder = null)
        {
            var filename = GetTempFileName(subfolder);

            File.WriteAllText(filename, "Content");

            return filename;
        }

        [Test]
        public void Should_add_file_to_existing_zip()
        {
            //Arrange
            var tempFile = GetPopulatedTempFileName();

            //Act
            _component.AugmentZip(_zipFileName, new[] { tempFile }, null, new string[0]);

            //Assert
            using (var zip = ZipFile.Read(_zipFileName))
            {
                Assert.IsTrue(zip.ContainsEntry(Path.GetFileName(tempFile)));
            }
        }

        [Test]
        public void Should_add_folder_to_existing_zip()
        {
            //Arrange
            var tempFile = GetPopulatedTempFileName("subfolder");
            var subfolder = Path.GetDirectoryName(tempFile);
            var baseFolder = Path.GetTempPath();
            var expected = tempFile.Replace(baseFolder, "");

            //Act
            _component.AugmentZip(_zipFileName, new string[0], baseFolder, new[] { subfolder });

            //Assert
            using (var zip = ZipFile.Read(_zipFileName))
            {
                Assert.IsTrue(zip.ContainsEntry(expected));
            }
        }
    }
}