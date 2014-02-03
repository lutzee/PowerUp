using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SystemWrapper.IO;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Id.PowershellExtensions.S3
{
    public class S3FileSystem
    {
        protected readonly IPsCmdletLogger Logger;
        protected readonly AmazonS3 S3Client;
        protected readonly TransferUtility TransferUtility;
        protected readonly Func<string, IFileStreamWrap> FileLoader;

        public S3FileSystem(IPsCmdletLogger logger, string accessKey, string secret, AmazonS3Config config)
        {
            Logger = logger ?? new TraceLogger();
            S3Client = new AmazonS3Client(accessKey, secret, config);
            TransferUtility = new TransferUtility(S3Client);
            FileLoader = (fileFullName) => new FileWrap().Open(fileFullName, FileMode.Open, FileAccess.ReadWrite);
        }

        //For testing - allows mocking of files and s3
        public S3FileSystem(AmazonS3 s3Client, Func<string, IFileStreamWrap> fileLoader)
        {
            Logger = new TraceLogger();
            S3Client = s3Client;
            TransferUtility = new TransferUtility(s3Client);
            FileLoader = fileLoader;
        }

        public void UploadFiles(IDirectoryInfoWrap directoryInfo, bool recurse, string bucketName, bool setPublicRead)
        {
            var rootFolder = directoryInfo.FullName;
            var files = GetFilesInDirectory(directoryInfo, recurse);

            foreach (var file in files)
            {
                var key = NormalisePathForKey(file.FullName.Replace(rootFolder, string.Empty));

                using (var fileStream = FileLoader(file.FullName))
                {
                    Upload(fileStream, bucketName, key, setPublicRead);
                    fileStream.Close();
                }
            }
        }

        private string NormalisePathForKey(string path)
        {
            return path.Replace("\\", "/").TrimStart(new[] { '/' }).ToLowerInvariant();
        }

        private IEnumerable<IFileInfoWrap> GetFilesInDirectory(IDirectoryInfoWrap source, bool recurse)
        {
            if (!source.Exists)
            {
                return Enumerable.Empty<IFileInfoWrap>();
            }            

            var files = source.GetFiles().ToList();

            if (recurse)
            {
                IDirectoryInfoWrap[] dirs = source.GetDirectories();
                foreach (IDirectoryInfoWrap dir in dirs)
                {
                    files.AddRange(GetFilesInDirectory(dir, true));
                }
            }

            return files;
        }

        private void Upload(IFileStreamWrap stream, string bucketName, string key, bool setPublicAccess)
        {
            try
            {
                key = key.ToLowerInvariant();

                var existsResponse = FileExists(bucketName, key);
                if (existsResponse != null && existsResponse.ContentLength == stream.Length)
                {
                    Logger.Log(string.Format("Skipping {0} because it already exists in {1}", key, bucketName));
                    return;
                }

                var uploadRequest = new TransferUtilityUploadRequest();
                uploadRequest.WithInputStream(stream.StreamInstance);
                uploadRequest.WithBucketName(bucketName);
                Logger.Log(String.Format("Bucket {0}", bucketName));
                if (setPublicAccess)
                    uploadRequest.CannedACL = S3CannedACL.PublicRead;
                uploadRequest.Key = key;
                Logger.Log(String.Format("Key {0}", key));
                uploadRequest.WithTimeout(14400000); // 4 Hours

                var lastKnownPercentage = 0;
                uploadRequest.UploadProgressEvent += (s, e) =>
                {
                    if (e.PercentDone <= lastKnownPercentage)
                        return;

                    Logger.Log(String.Format("UploadProgress:{0} :{1}%", key, e.PercentDone));
                    lastKnownPercentage = e.PercentDone;
                };
                
                TransferUtility.Upload(uploadRequest);                
            }
            catch (Exception exception)
            {
                Logger.Log("Error uploading to s3");
                Logger.Log(exception.Message);
                throw;
            }
        }

        protected GetObjectMetadataResponse FileExists(string bucketName, string key)
        {
            var location = String.Format("{0}/{1}", bucketName, key);
            Logger.Log(String.Format("Checking if file Exists on S3: {0}", location));
            try
            {
                return S3Client.GetObjectMetadata(new GetObjectMetadataRequest()
                   .WithBucketName(bucketName)
                   .WithKey(key.ToLowerInvariant()));
            }
            catch (AmazonS3Exception ex)
            {
                Logger.Log(String.Format("File Does Not Exist:{0}", location));
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                //status wasn't not found, so throw the exception
                throw;
            }
        }
    }
}
