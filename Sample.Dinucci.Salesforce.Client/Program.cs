using System.Net.Http;
using Dinucci.Salesforce.Client.Auth;
using Dinucci.Salesforce.Client.Custom;
using Dinucci.Salesforce.Client.Data;
using Dinucci.Salesforce.Client.Tooling;

namespace Sample.Dinucci.Salesforce.Client
{
    public class Program
    {
        private const string ClientId =
            "3MVG9ZPHiJTk7yFyo2kgZvLTvpjobQskYGDyhEnON21Vz1BfOAbXSOBrvM395NJsBVhCgIck6IoESDreYY6Ah";

        private const string ClientSecret = "8204173310639812200";
        private const string Username = "";
        private const string Password = "";
        private const string AuthEndpoint = "https://test.salesforce.com/services/oauth2/token";

        public static void Main()
        {
            using (var httpClient = new HttpClient())
            {
                var authenticator =
                    new PasswordFlowAuthenticator(ClientId, ClientSecret, Username, Password, AuthEndpoint, httpClient);

                var dataApi = new DataApi(authenticator, httpClient, 44);
                var toolingApi = new ToolingApi(authenticator, httpClient, 44);
                var customApi = new CustomApi(authenticator, httpClient);

                new DataApiSample(dataApi).RunAsync().GetAwaiter().GetResult();
                new ToolingApiSample(toolingApi).RunAsync().GetAwaiter().GetResult();
                new CustomApiSample(customApi).RunAsync().GetAwaiter().GetResult();
            }
        }
    }
}