namespace Id.PowershellExtensions.RemoteAgent
{
    public class PublishRequest
    {
        public string SecurityKey { get; set; }
        public string BucketName { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string Filename { get; set; }
        public string DeploymentProfile { get; set; }
        public string DeploymentScriptFilename { get; set; }

        //Manual JSON serialisation since this class is so trivial
        //Prevents reliance on 3rd party library for this one method
        public string ToJsonString()
        {
            return string.Format(@"{{
    ""SecurityKey"": ""{0}"",
    ""BucketName"": ""{1}"",
    ""AccessKey"": ""{2}"",
    ""SecretKey"": ""{3}"",
    ""Filename"": ""{4}"",
    ""DeploymentProfile"": ""{5}"",
    ""DeploymentScriptFilename"": ""{6}""
}}",
          SecurityKey,
          BucketName,
          AccessKey,
          SecretKey,
          Filename,
          DeploymentProfile,
          DeploymentScriptFilename);
        }
    }
}