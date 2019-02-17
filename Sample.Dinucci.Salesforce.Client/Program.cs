using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Auth;
using Dinucci.Salesforce.Client.Data;

namespace Sample.Dinucci.Salesforce.Client
{
    class Program
    {
        public const string ClientId =
            "3MVG9ZPHiJTk7yFyo2kgZvLTvpjobQskYGDyhEnON21Vz1BfOAbXSOBrvM395NJsBVhCgIck6IoESDreYY6Ah";

        public const string ClientSecret = "8204173310639812200";
        public const string Username = "";
        public const string Password = "";
        public const string AuthEndpoint = "https://test.salesforce.com/services/oauth2/token";

        static void Main(string[] args)
        {
            using (var httpClient = new HttpClient())
            {
                var authenticator = new Authenticator(ClientId, ClientSecret, Username, Password, AuthEndpoint,
                    TimeSpan.MaxValue, httpClient);

                var api = new DataApi(authenticator, httpClient, 44);

                var iterationTime = TimeSpan.FromSeconds(10);
                var ranFor = TimeSpan.Zero;
                while (true)
                {
                    RunQuery(api).GetAwaiter().GetResult();
                    Thread.Sleep(iterationTime);

                    ranFor = ranFor.Add(iterationTime);
                    Console.WriteLine($"Ran for {ranFor}");
                }
            }
        }

        static async Task RunQuery(IDataApi api)
        {
            try
            {
                var query = "SELECT Id, FirstName, LastName FROM Contact LIMIT 1";

                var result = await api.QueryAsync(query).ConfigureAwait(false);
                if (result.Records.Length != 1)
                    Console.Error.WriteLine($"**** UNEXPECTED LENGTH {result.Records.Length} *****");
                else
                    Console.WriteLine(result.Records[0].ToString());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Exception: {ex.Message} - {ex.StackTrace}");
            }
        }
    }
}