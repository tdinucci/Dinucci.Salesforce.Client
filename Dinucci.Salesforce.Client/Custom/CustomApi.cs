using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Auth;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Custom
{
    public class CustomApi : Api, ICustomApi
    {
        public CustomApi(IAuthenticator authenticator, HttpClient httpClient) : base(authenticator, httpClient)
        {
        }

        protected override ConnectionInfo GetConnectionInfo(string urlSuffix)
        {
            return base.GetConnectionInfo($"services/apexrest/{urlSuffix}");
        }

        public Task<string> GetAsync(string servicePath, IDictionary<string, string> parameters = null)
        {
            return SendRequestAsync(HttpMethod.Get, servicePath, parameters);
        }

        public Task<string> PostAsync(string servicePath, JObject jObject)
        {
            return SendRequestWithBodyAsync(HttpMethod.Post, servicePath, jObject);
        }

        public Task<string> PatchAsync(string servicePath, JObject jObject)
        {
            return SendRequestWithBodyAsync(new HttpMethod("PATCH"), servicePath, jObject);
        }

        public Task<string> PutAsync(string servicePath, JObject jObject)
        {
            return SendRequestWithBodyAsync(HttpMethod.Put, servicePath, jObject);
        }

        public Task<string> DeleteAsync(string servicePath, IDictionary<string, string> parameters = null)
        {
            return SendRequestAsync(HttpMethod.Delete, servicePath, parameters);
        }
    }
}