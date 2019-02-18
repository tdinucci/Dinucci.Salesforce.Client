using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Auth;
using Dinucci.Salesforce.Client.Data;
using Dinucci.Salesforce.Client.Tooling;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Test.Dinucci.Salesforce.Client.Tooling
{
    public class ToolingApiTest : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IDataApi _dataApi;
        private readonly IToolingApi _toolingApi;
        private readonly IAuthenticator _authenticator;

        public ToolingApiTest()
        {
            _httpClient = new HttpClient();

            _authenticator = new PasswordFlowAuthenticator(SalesforceConfig.ClientId, SalesforceConfig.ClientSecret,
                SalesforceConfig.Username, SalesforceConfig.Password, SalesforceConfig.AuthEndpoint, _httpClient);

            _dataApi = new DataApi(_authenticator, _httpClient, 44);
            _toolingApi = new ToolingApi(_authenticator, _httpClient, 44);
        }

        [Fact]
        public void Authenticator()
        {
            Assert.Equal(_authenticator, _toolingApi.Authenticator);
        }

        [Fact]
        public async Task Query()
        {
            var query = @"
SELECT 
  Id, DataType, DeveloperName, Metadata 
 FROM 
  FieldDefinition
 WHERE 
  EntityDefinition.QualifiedApiName = 'Account' 
  AND DurableId ='Account.Industry'";

            var result = await _toolingApi.QueryAsync(query).ConfigureAwait(false);
            Assert.Equal("FieldDefinition", result["entityTypeName"].Value<string>());
        }

        [Fact]
        public async Task ExecuteAnonymous()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var postalCode = DateTime.Now.Millisecond.ToString();
            var contact = new JObject
            {
                {"Salutation", "Mr"},
                {"FirstName", firstName},
                {"LastName", lastName},
                {"MailingPostalCode", postalCode},
                {"MailingCountry", "United Kingdom"}
            };

            var id = await _dataApi.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            var apex = $@"
Contact contact = [SELECT FirstName, LastName FROM Contact WHERE Id = '{id}'];
contact.FirstName = '';
contact.LastName = '';

for(Integer i=0;i<5;i++) {{
    contact.FirstName += String.valueOf(i);
    contact.LastName += String.valueOf(i*2);
}}

update contact;".Trim();

            await _toolingApi.ExecuteApexAsync(apex).ConfigureAwait(false);

            var query = $"SELECT Salutation, FirstName, LastName, MailingCountry FROM Contact WHERE Id = '{id}'";

            var queryResult = await _dataApi.QueryAsync(query).ConfigureAwait(false);
            Assert.True(queryResult.Done);
            Assert.Equal(1, queryResult.TotalSize);
            Assert.Equal(queryResult.TotalSize, queryResult.Records.Length);

            contact = queryResult.Records[0];
            Assert.Equal("Mr", contact["Salutation"].Value<string>());
            Assert.Equal("01234", contact["FirstName"].Value<string>());
            Assert.Equal("02468", contact["LastName"].Value<string>());
            Assert.Equal("United Kingdom", contact["MailingCountry"].Value<string>());
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}