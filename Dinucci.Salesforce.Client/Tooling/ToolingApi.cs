using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Dinucci.Salesforce.Client.Auth;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Tooling
{
    public class ToolingApi : Api, IToolingApi
    {
        private readonly string _toolingServicePath;

        public ToolingApi(IAuthenticator authenticator, HttpClient httpClient, decimal apiVersion) :
            base(authenticator, httpClient)
        {
            if (apiVersion <= 0) throw new ArgumentOutOfRangeException(nameof(apiVersion));

            _toolingServicePath = $"services/data/v{apiVersion:F1}/tooling";
        }

        public Task<JObject> QueryAsync(string query)
        {
            return SendGetRequest($"query?q={query}");
        }

        public Task<JObject> ExecuteApexAsync(string apex)
        {
            if (string.IsNullOrWhiteSpace(apex))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(apex));

            return SendGetRequest($"executeAnonymous/?anonymousBody={HttpUtility.UrlEncode(apex)}");
        }

        private async Task<JObject> SendGetRequest(string urlSuffix)
        {
            var response =
                await SendRequestAsync(HttpMethod.Get, $"{_toolingServicePath}/{urlSuffix}", null)
                    .ConfigureAwait(false);

            return JObject.Parse(response);
        }
    }
}