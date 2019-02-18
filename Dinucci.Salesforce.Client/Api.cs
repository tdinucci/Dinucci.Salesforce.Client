using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client
{
    public class Api : IApi
    {
        protected class ConnectionInfo
        {
            public string AccessToken { get; }
            public string Url { get; internal set; }

            public ConnectionInfo(string accessToken, string url)
            {
                if (string.IsNullOrWhiteSpace(accessToken))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));
                if (string.IsNullOrWhiteSpace(url))
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(url));

                AccessToken = accessToken;
                Url = url;
            }
        }

        private const string MediaType = "application/json";

        public IAuthenticator Authenticator { get; }
        protected HttpClient HttpClient { get; }

        protected Api(IAuthenticator authenticator, HttpClient httpClient)
        {
            Authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        protected virtual ConnectionInfo GetConnectionInfo(string urlSuffix)
        {
            var authInfo = Authenticator.GetAuthInfo();
            return new ConnectionInfo(authInfo.Token, $"{authInfo.Url.TrimEnd('/')}/{urlSuffix.TrimStart('/')}");
        }

        private ConnectionInfo GetConnectionInfo(string urlSuffix, IDictionary<string, string> parameters)
        {
            var connectionInfo = GetConnectionInfo(urlSuffix);

            var queryString = string.Empty;
            if (parameters != null && parameters.Any())
                queryString = "?" + parameters.Select(kv => $"{kv.Key}={kv.Value}").Aggregate((c, n) => $"{c}&{n}");

            connectionInfo.Url = connectionInfo.Url + queryString;
            return connectionInfo;
        }

        protected virtual Task<string> SendRequestAsync(HttpMethod method, string servicePath,
            IDictionary<string, string> parameters = null)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (string.IsNullOrWhiteSpace(servicePath))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(servicePath));

            var connectionInfo = GetConnectionInfo(servicePath, parameters);
            var request = new HttpRequestMessage(method, connectionInfo.Url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connectionInfo.AccessToken);

            return GetResponseAsync(request);
        }

        protected virtual Task<string> SendRequestWithBodyAsync(HttpMethod method, string servicePath, JObject jObject)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));
            if (string.IsNullOrWhiteSpace(servicePath))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(servicePath));

            var connectionInfo = GetConnectionInfo(servicePath);
            var request = new HttpRequestMessage(method, connectionInfo.Url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connectionInfo.AccessToken);
            request.Content = new StringContent(jObject.ToString(Formatting.None), Encoding.UTF8, MediaType);

            return GetResponseAsync(request);
        }

        protected virtual Task<string> GetResponseAsync(HttpRequestMessage request)
        {
            return GetResponseAsync(request, false);
        }

        private async Task<string> GetResponseAsync(HttpRequestMessage request, bool isRepeat)
        {
            try
            {
                using (var response = await HttpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.Unauthorized && !isRepeat)
                        {
                            var authInfo = Authenticator.Authenticate();

                            var repeatRequest = new HttpRequestMessage(request.Method, request.RequestUri)
                            {
                                Content = request.Content
                            };
                            repeatRequest.Headers.Authorization =
                                new AuthenticationHeaderValue("Bearer", authInfo.Token);

                            return await GetResponseAsync(repeatRequest, true).ConfigureAwait(false);
                        }

                        throw new SalesforceException(
                            $"{request.Method} failed. HTTP {(int) response.StatusCode} - {response.StatusCode} - " +
                            $"{responseContent}", responseContent);
                    }

                    return responseContent.Trim('"');
                }
            }
            finally
            {
                request?.Dispose();
            }
        }
    }
}