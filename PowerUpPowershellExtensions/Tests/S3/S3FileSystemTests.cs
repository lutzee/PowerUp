using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SystemWrapper.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Id.PowershellExtensions.S3;
using Moq;
using NUnit.Framework;
using Tests.Helpers;

namespace Tests.S3
{
    [TestFixture]
    public class S3FileSystemTests
    {
        private const string BasePath = @"c:\tmp_localfs\";
        private Mock<AmazonS3> AmazonS3;
        private Mock<IDirectoryInfoWrap> DirectoryInfo;
        private S3FileSystem S3FileSystem;

        [SetUp]
        public void SetUp()
        {
            var file1 = new Mock<IFileInfoWrap>();
            file1.Setup(x => x.Name).Returns("File1.jpg");
            file1.Setup(x => x.FullName).Returns("c:\\files\\File1.jpg");

            var file2 = new Mock<IFileInfoWrap>();
            file2.Setup(x => x.Name).Returns("File2.jpg");
            file2.Setup(x => x.FullName).Returns("c:\\files\\File2.jpg");

            var file3 = new Mock<IFileInfoWrap>();
            file3.Setup(x => x.Name).Returns("File3.jpg");
            file3.Setup(x => x.FullName).Returns("c:\\files\\morefiles\\File3.jpg");

            var file4 = new Mock<IFileInfoWrap>();
            file4.Setup(x => x.Name).Returns("File4.jpg");
            file4.Setup(x => x.FullName).Returns("c:\\files\\morefiles\\File4.jpg");

            var file5 = new Mock<IFileInfoWrap>();
            file5.Setup(x => x.Name).Returns("File5.jpg");
            file5.Setup(x => x.FullName).Returns("c:\\files\\otherfiles\\File5.jpg");

            var moreFilesDirectory = new Mock<IDirectoryInfoWrap>();
            moreFilesDirectory.Setup(x => x.GetFiles()).Returns(() => new IFileInfoWrap[] { file3.Object, file4.Object });
            moreFilesDirectory.Setup(x => x.GetDirectories()).Returns(() => new IDirectoryInfoWrap[] { });
            moreFilesDirectory.Setup(x => x.Exists).Returns(() => true);
            moreFilesDirectory.Setup(x => x.Name).Returns(() => "morefiles");
            moreFilesDirectory.Setup(x => x.FullName).Returns(() => "c:\\files\\morefiles");

            var otherfilesDirectory = new Mock<IDirectoryInfoWrap>();
            otherfilesDirectory.Setup(x => x.GetFiles()).Returns(() => new IFileInfoWrap[] { file5.Object });
            otherfilesDirectory.Setup(x => x.GetDirectories()).Returns(() => new IDirectoryInfoWrap[] { });
            otherfilesDirectory.Setup(x => x.Exists).Returns(() => true);
            otherfilesDirectory.Setup(x => x.Name).Returns(() => "otherfiles");
            otherfilesDirectory.Setup(x => x.FullName).Returns(() => "c:\\files\\otherfiles");

            DirectoryInfo = new Mock<IDirectoryInfoWrap>();
            DirectoryInfo.Setup(x => x.GetFiles()).Returns(() => new IFileInfoWrap[] {file1.Object, file2.Object});
            DirectoryInfo.Setup(x => x.GetDirectories()).Returns(() => new IDirectoryInfoWrap[] { moreFilesDirectory.Object, otherfilesDirectory.Object });
            DirectoryInfo.Setup(x => x.Exists).Returns(() => true);
            DirectoryInfo.Setup(x => x.Name).Returns(() => "files");
            DirectoryInfo.Setup(x => x.FullName).Returns(() => "c:\\files");

            var notFoundException = new AmazonS3Exception("Key not found", HttpStatusCode.NotFound);

            AmazonS3 = new Mock<AmazonS3>();
            AmazonS3.Setup(x => x.PutObject(It.IsAny<PutObjectRequest>())).Returns(new PutObjectResponse()).Verifiable();
            AmazonS3.Setup(x => x.GetObjectMetadata(It.IsAny<GetObjectMetadataRequest>())).Throws(notFoundException);

            S3FileSystem = new S3FileSystem(AmazonS3.Object, GetFileStreamFromFile);
        }

        private IFileStreamWrap GetFileStreamFromFile(string fullFileName)
        {
            if (!File.Exists(Path.Combine(BasePath, "kitten.jpg")))
            {
                SaveResourceAsFile("Tests.Resources.kitten.jpg", "kitten.jpg");
            }
            return new FileStreamWrap(Path.Combine(BasePath, "kitten.jpg"), FileMode.Open);
        }

        private void SaveResourceAsFile(string resourceName, string filename)
        {
            var fullPath = Path.Combine(BasePath, filename);
            var resourceBytes = ResourceHelpers.GetFileBytesFromResource(GetType(), resourceName);
            var info = new FileInfoWrap(fullPath);

            if (!info.Directory.Exists)
                Directory.CreateDirectory(info.Directory.FullName);

            using (var fileStream = File.Create(fullPath))
            {
                fileStream.Write(resourceBytes, 0, resourceBytes.Length);
                fileStream.Close();
            }            
        }

        [TearDown]
        public void TearDown()
        {
            S3FileSystem = null;
            DirectoryInfo = null;
            AmazonS3 = null;
        }

        [Test]
        public void UploadFiles_Recursive_CallsPutObjectCorrectNumberOfTimes()
        {
            S3FileSystem.UploadFiles(DirectoryInfo.Object, true, "Files", true);
            AmazonS3.Verify(x => x.PutObject(It.IsAny<PutObjectRequest>()), Times.Exactly(5));
        }

        [Test]
        public void UploadFiles_NonRecursive_CallsPutObjectCorrectNumberOfTimes()
        {
            S3FileSystem.UploadFiles(DirectoryInfo.Object, false, "Files", true);
            AmazonS3.Verify(x => x.PutObject(It.IsAny<PutObjectRequest>()), Times.Exactly(2));
        }
    }
}
