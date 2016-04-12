using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Id.PowershellExtensions.RemoteAgent
{
    public class Publisher
    {
        private readonly Dictionary<string, string> _settings;
        private readonly IPsCmdletLogger _logger;

        public Publisher(Dictionary<string, string> settings, IPsCmdletLogger logger)
        {
            _settings = settings;
            _logger = logger ?? new TraceLogger();

            //Trust all SLL certs blindly, because ours could be self-signed
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }


        public string PublishPackage()
        {
            var expect100Continue = GetValueOrDefault(_settings, "Expect100Continue");
            if (expect100Continue.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
                ServicePointManager.Expect100Continue = false;

            var oneHour = TimeSpan.FromMinutes(60);
            var httpClient = GetHttpClient();
            httpClient.Timeout = oneHour;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger.Log("---- Publishing package remotely -----");

            try
            {
                var request = GetPublishRequestFromSettings();
                var remoteAgentUrl = _settings["RemoteAgentUrl"];

                _logger.Log("Contacting Remote Agent {0}", remoteAgentUrl);

                var pingResponseTask = httpClient.GetAsync(remoteAgentUrl);
                pingResponseTask.Wait();

                if (pingResponseTask.Result.StatusCode != HttpStatusCode.OK)
                {
                    var pingContentTask = pingResponseTask.Result.Content.ReadAsStringAsync();
                    pingContentTask.Wait();
                    var pingContent = pingContentTask.Result;

                    _logger.Log(pingContent);
                    throw new Exception(string.Format("Remote Agent is not listening: {0}", pingContent));
                }

                _logger.Log("Deploying via Remote Agent {0}", remoteAgentUrl);

                var publishResponseTask = httpClient.PostAsync(
                    remoteAgentUrl,
                    new StringContent(request.ToJsonString(), Encoding.UTF8, "application/json")
                );
                publishResponseTask.Wait(oneHour);

                var contentTask = publishResponseTask.Result.Content.ReadAsStringAsync();
                contentTask.Wait(oneHour);

                var content = contentTask.Result;

                if (publishResponseTask.Result.StatusCode != HttpStatusCode.OK)
                    throw new Exception(string.Format("Remote Agent publish failed: {0}", content));

                return content;

            }
            catch (Exception ex)
            {
                _logger.Log(ex);
                throw;
            }
            finally
            {
                httpClient.Dispose();
                httpClient = null;
            }
        }

        private PublishRequest GetPublishRequestFromSettings()
        {
            var request = new PublishRequest
            {
                AccessKey = GetValueOrDefault(_settings, "AccessKey"),
                BucketName = GetValueOrDefault(_settings, "BucketName"),
                DeploymentProfile = GetValueOrDefault(_settings, "DeploymentProfile"),
                DeploymentScriptFilename = GetValueOrDefault(_settings, "DeploymentScriptFilename", null),
                Filename = GetValueOrDefault(_settings, "Filename"),
                SecretKey = GetValueOrDefault(_settings, "SecretKey"),
                SecurityKey = GetValueOrDefault(_settings, "SecurityKey")
            };
            return request;
        }

        private HttpClient GetHttpClient()
        {
            var useDefaultProxy = GetValueOrDefault(_settings, "UseDefaultProxy");
            if (!useDefaultProxy.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
                return new HttpClient();

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = new WebProxy("http://proxy.cnw.co.nz:8080", false),
                UseProxy = true
            };

            var client = new HttpClient(httpClientHandler);
            var expect100Continue = GetValueOrDefault(_settings, "Expect100Continue");
            if (expect100Continue.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
                client.DefaultRequestHeaders.ExpectContinue = false;

            return client;
        }

        private static string GetValueOrDefault(IDictionary<string, string> values, string key, string @default = "")
        {
            return values.ContainsKey(key)
                ? values[key]
                : @default;
        }
    }
}
