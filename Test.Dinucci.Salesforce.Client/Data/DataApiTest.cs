using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Auth;
using Dinucci.Salesforce.Client.Data;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Test.Dinucci.Salesforce.Client.Data
{
    public class DataApiTest : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IDataApi _api;
        private readonly IAuthenticator _authenticator;

        public DataApiTest()
        {
            _httpClient = new HttpClient();

            _authenticator = new PasswordFlowAuthenticator(SalesforceConfig.ClientId, SalesforceConfig.ClientSecret,
                SalesforceConfig.Username, SalesforceConfig.Password, SalesforceConfig.AuthEndpoint, _httpClient);

            _api = new DataApi(_authenticator, _httpClient, 44);
        }

        [Fact]
        public void Authenticator()
        {
            Assert.Equal(_authenticator, _api.Authenticator);
        }
        
        [Fact]
        public async Task DescribeGood()
        {   
            var result = await _api.DescribeAsync("Contact").ConfigureAwait(false);
            Assert.True(result.ContainsKey("actionOverrides"));
            Assert.True(result.ContainsKey("childRelationships"));
        }

        [Fact]
        public async Task DescribeInvalidType()
        {
            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.DescribeAsync("xContact"))
                .ConfigureAwait(false);

            Assert.Equal("==== Level 1 ====\nThe requested resource does not exist", ex.Message.Trim());
        }

        [Fact]
        public async Task QueryGood()
        {
            var query = "SELECT Id, FirstName, LastName FROM Contact LIMIT 10";
            var result = await _api.QueryAsync(query).ConfigureAwait(false);

            Assert.True(result.Done);
            Assert.True(result.TotalSize == 10);
            Assert.Equal(result.TotalSize, result.Records.Length);
            Assert.Null(result.NextRecordsUrl);
            EnsureValidRecords(result.Records, new[] {"Id", "FirstName", "LastName"});
        }

        [Fact]
        public async Task QueryGoodWithNext()
        {
            const int limit = 3900;
            const int maxResultLength = 2000;

            var query = $"SELECT Id, FirstName, LastName FROM Contact LIMIT {limit}";
            var result = await _api.QueryAsync(query).ConfigureAwait(false);

            Assert.False(result.Done);
            Assert.Equal(limit, result.TotalSize);
            Assert.True(result.Records.Length == maxResultLength);
            Assert.False(string.IsNullOrWhiteSpace(result.NextRecordsUrl));
            EnsureValidRecords(result.Records, new[] {"Id", "FirstName", "LastName"});
            
            result = await _api.GetNextAsync(result.NextRecordsUrl).ConfigureAwait(false);

            Assert.True(result.Done);
            Assert.Equal(limit, result.TotalSize);
            Assert.True(result.Records.Length == limit - maxResultLength);
            Assert.True(string.IsNullOrWhiteSpace(result.NextRecordsUrl));
            EnsureValidRecords(result.Records, new[] {"Id", "FirstName", "LastName"});
        }

        [Fact]
        public async Task QueryGoodWithNoResults()
        {
            var query = "SELECT Id, FirstName, LastName FROM Contact WHERE FirstName = 'nkjuius565367sf-fdlk'";
            var result = await _api.QueryAsync(query).ConfigureAwait(false);

            Assert.True(result.Done);
            Assert.True(result.TotalSize == 0);
            Assert.Equal(result.TotalSize, result.Records.Length);
            Assert.Null(result.NextRecordsUrl);
        }

        [Fact]
        public async Task QueryInvalid()
        {
            var query = "SELECT Id, NonField FROM Contact";
            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.QueryAsync(query)).ConfigureAwait(false);

            Assert.StartsWith("==== Level 1 ====\nSELECT Id, NonField FROM Contact", ex.Message);
        }

        [Fact]
        public async Task SelectAllFields()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            var jobj = await _api.SelectAllFieldsAsync("Contact", id).ConfigureAwait(false);

            Assert.Equal(firstName, jobj["FirstName"].Value<string>());
            Assert.Equal(lastName, jobj["LastName"].Value<string>());
            Assert.NotNull(jobj["RecordTypeId"].Value<string>());
            Assert.NotNull(jobj["CreatedDate"].Value<string>());
            Assert.NotNull(jobj["PhotoUrl"].Value<string>());
            Assert.NotNull(jobj["Disclaimer_Text__c"].Value<string>());
        }

        [Fact]
        public async Task CreateGood()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            var query = "SELECT Salutation, FirstName, LastName, MailingPostalCode, MailingCountry " +
                        $"FROM Contact WHERE Id = '{id}'";

            var queryResult = await _api.QueryAsync(query).ConfigureAwait(false);
            Assert.True(queryResult.Done);
            Assert.Equal(1, queryResult.TotalSize);
            Assert.Equal(queryResult.TotalSize, queryResult.Records.Length);

            contact = queryResult.Records[0];
            Assert.Equal("Mr", contact["Salutation"].Value<string>());
            Assert.Equal(firstName, contact["FirstName"].Value<string>());
            Assert.Equal(lastName, contact["LastName"].Value<string>());
            Assert.Equal(postalCode, contact["MailingPostalCode"].Value<string>());
            Assert.Equal("United Kingdom", contact["MailingCountry"].Value<string>());
        }

        [Fact]
        public async Task CreateMissingRequiredFields()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var contact = new JObject
            {
                {"FirstName", firstName},
                {"LastName", lastName}
            };

            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.CreateAsync("Contact", contact))
                .ConfigureAwait(false);
            
            Assert.Equal(@"==== Level 1 ====
Salutation is Mandatory.
==== Level 2 ====
Mailing country and mailing postcode are mandatory", ex.Message.Trim());
        }
        
        [Fact]
        public async Task CreateInvalidFieldName()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var postalCode = DateTime.Now.Millisecond.ToString();
            var contact = new JObject
            {
                {"xSalutation", "Mr"},
                {"FirstName", firstName},
                {"LastName", lastName},
                {"MailingPostalCode", postalCode},
                {"MailingCountry", "United Kingdom"}
            };

            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.CreateAsync("Contact", contact))
                .ConfigureAwait(false);
            
            Assert.Equal(@"==== Level 1 ====
No such column 'xSalutation' on sobject of type Contact", ex.Message.Trim());
        }
        
        [Fact]
        public async Task CreateInvalidType()
        {
            var firstName = Guid.NewGuid().ToString();
            var lastName = Guid.NewGuid().ToString();
            var postalCode = DateTime.Now.Millisecond.ToString();
            var contact = new JObject
            {
                {"xSalutation", "Mr"},
                {"FirstName", firstName},
                {"LastName", lastName},
                {"MailingPostalCode", postalCode},
                {"MailingCountry", "United Kingdom"}
            };

            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.CreateAsync("xContact", contact))
                .ConfigureAwait(false);
            
            Assert.Equal("==== Level 1 ====\nThe requested resource does not exist", ex.Message.Trim());
        }

        [Fact]
        public async Task UpdateGood()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            var updatedFirstName = Guid.NewGuid().ToString();
            var updatedLastName = Guid.NewGuid().ToString();
            var update = new JObject
            {
                {"FirstName", updatedFirstName},
                {"LastName", updatedLastName}
            };

            await _api.UpdateAsync("Contact", id, update).ConfigureAwait(false);

            var query = "SELECT Salutation, FirstName, LastName, MailingPostalCode, MailingCountry " +
                        $"FROM Contact WHERE Id = '{id}'";

            var queryResult = await _api.QueryAsync(query).ConfigureAwait(false);
            Assert.True(queryResult.Done);
            Assert.Equal(1, queryResult.TotalSize);
            Assert.Equal(queryResult.TotalSize, queryResult.Records.Length);

            contact = queryResult.Records[0];
            Assert.Equal("Mr", contact["Salutation"].Value<string>());
            Assert.Equal(updatedFirstName, contact["FirstName"].Value<string>());
            Assert.Equal(updatedLastName, contact["LastName"].Value<string>());
            Assert.Equal(postalCode, contact["MailingPostalCode"].Value<string>());
            Assert.Equal("United Kingdom", contact["MailingCountry"].Value<string>());
        }

        [Fact]
        public async Task UpdateInvalidFieldName()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            var updatedFirstName = Guid.NewGuid().ToString();
            var updatedLastName = Guid.NewGuid().ToString();
            var update = new JObject
            {
                {"xFirstName", updatedFirstName},
                {"LastName", updatedLastName}
            };

            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.UpdateAsync("Contact", id, update))
                .ConfigureAwait(false);

            Assert.Equal("==== Level 1 ====\nNo such column 'xFirstName' on sobject of type Contact", ex.Message.Trim());
        }
        
        [Fact]
        public async Task UpdateNotFound()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            id = "x" + id.Substring(1); 
            
            var updatedFirstName = Guid.NewGuid().ToString();
            var updatedLastName = Guid.NewGuid().ToString();
            var update = new JObject
            {
                {"FirstName", updatedFirstName},
                {"LastName", updatedLastName}
            };

            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.UpdateAsync("Contact", id, update))
                .ConfigureAwait(false);

            Assert.Equal("==== Level 1 ====\nThe requested resource does not exist", ex.Message.Trim());
        }

        [Fact]
        public async Task UpdateInvalidType()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            var updatedFirstName = Guid.NewGuid().ToString();
            var updatedLastName = Guid.NewGuid().ToString();
            var update = new JObject
            {
                {"FirstName", updatedFirstName},
                {"LastName", updatedLastName}
            };

            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.UpdateAsync("xContact", id, update))
                .ConfigureAwait(false);

            Assert.Equal("==== Level 1 ====\nThe requested resource does not exist", ex.Message.Trim());
        }

        [Fact]
        public async Task DeleteGood()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            await _api.DeleteAsync("Contact", id).ConfigureAwait(false);

            var queryResult = await _api.QueryAsync($"SELECT Id FROM Contact WHERE Id = '{id}'")
                .ConfigureAwait(false);

            Assert.Equal(0, queryResult.TotalSize);
        }

        [Fact]
        public async Task DeleteInvalidId()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            id = "x" + id.Substring(1);
            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.DeleteAsync("Contact", id))
                .ConfigureAwait(false);

            Assert.Equal("==== Level 1 ====\nThe requested resource does not exist", ex.Message.Trim());
        }
        
        [Fact]
        public async Task DeleteInvalidType()
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

            var id = await _api.CreateAsync("Contact", contact).ConfigureAwait(false);
            Assert.False(string.IsNullOrWhiteSpace(id));

            var ex = await Assert.ThrowsAsync<SalesforceDataException>(() => _api.DeleteAsync("xContact", id))
                .ConfigureAwait(false);

            Assert.Equal("==== Level 1 ====\nThe requested resource does not exist", ex.Message.Trim());
        }

        private void EnsureValidRecords(IEnumerable<JObject> records, IEnumerable<string> expectedFields)
        {
            foreach (var record in records)
            {
                foreach (var expectedField in expectedFields)
                    Assert.True(record.ContainsKey(expectedField));
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}