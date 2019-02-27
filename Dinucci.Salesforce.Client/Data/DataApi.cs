using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Auth;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Data
{
    public class DataApi : Api, IDataApi
    {
        private readonly string _dataServicePath;

        public DataApi(IAuthenticator authenticator, HttpClient httpClient, decimal apiVersion) :
            base(authenticator, httpClient)
        {
            if (apiVersion <= 0) throw new ArgumentOutOfRangeException(nameof(apiVersion));

            _dataServicePath = $"services/data/v{apiVersion:F1}";
        }

        protected override ConnectionInfo GetConnectionInfo(string urlSuffix)
        {
            return base.GetConnectionInfo($"{_dataServicePath}/{urlSuffix}");
        }

        protected override async Task<string> SendRequestAsync(HttpMethod method, string servicePath,
            IDictionary<string, string> parameters)
        {
            try
            {
                return await base.SendRequestAsync(method, servicePath, parameters)
                    .ConfigureAwait(false);
            }
            catch (SalesforceException ex) when (!string.IsNullOrWhiteSpace(ex.SalesforceResponse))
            {
                throw SalesforceDataException.Parse(ex.SalesforceResponse);
            }
        }

        protected override async Task<string> SendRequestWithBodyAsync(HttpMethod method, string servicePath,
            JObject jObject)
        {
            try
            {
                return await base.SendRequestWithBodyAsync(method, servicePath, jObject).ConfigureAwait(false);
            }
            catch (SalesforceException ex) when (!string.IsNullOrWhiteSpace(ex.SalesforceResponse))
            {
                throw SalesforceDataException.Parse(ex.SalesforceResponse);
            }
        }

        public async Task<JObject> DescribeAsync(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(typeName));

            var response =
                await SendRequestAsync(HttpMethod.Get, $"sobjects/{typeName}/describe", null)
                    .ConfigureAwait(false);

            return JObject.Parse(response);
        }

        public async Task<ReadResult<JObject>> QueryAsync(string soql)
        {
            if (string.IsNullOrWhiteSpace(soql))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(soql));

            var response = await SendRequestAsync(HttpMethod.Get, $"query?q={soql}", null)
                .ConfigureAwait(false);

            return ReadResult<JObject>.Parse(JObject.Parse(response));
        }

        public async Task<JObject> QuerySingleAsync(string soql)
        {
            var result = await QueryAsync(soql).ConfigureAwait(false);
            if (!result.Done)
                throw new InvalidOperationException($"Query failed: {soql}");

            if (result.Records.Length != 1)
            {
                throw new InvalidOperationException(
                    $"Expected to receive 1 record but received {result.Records.Length} with query {soql}");
            }

            return result.Records[0];
        }

        public async Task<ReadResult<JObject>> GetNextAsync(string nextRecordsUrlSuffix)
        {
            if (string.IsNullOrWhiteSpace(nextRecordsUrlSuffix))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(nextRecordsUrlSuffix));

            nextRecordsUrlSuffix = nextRecordsUrlSuffix.TrimStart('/');
            if (nextRecordsUrlSuffix.StartsWith(_dataServicePath))
                nextRecordsUrlSuffix = nextRecordsUrlSuffix.Substring(_dataServicePath.Length + 1);

            var response = await SendRequestAsync(HttpMethod.Get, nextRecordsUrlSuffix, null)
                .ConfigureAwait(false);

            return ReadResult<JObject>.Parse(JObject.Parse(response));
        }

        public async Task<JObject> SelectAllFieldsAsync(string typeName, string id)
        {
            var describeResult = await DescribeAsync(typeName).ConfigureAwait(false);

            if (!describeResult.ContainsKey("fields"))
                throw new InvalidOperationException("Expected describe result to contains some fields");

            var fieldNames = describeResult["fields"].Select(field => field["name"].Value<string>()).ToArray();

            var query = $"SELECT {fieldNames.Aggregate((c, n) => $"{c},{n}")} FROM {typeName} WHERE Id = '{id}'";
            var result = await QueryAsync(query).ConfigureAwait(false);

            if (result?.Records.Length != 1)
                throw new InvalidOperationException($"Could not retrieve '{typeName}' '{id}'");

            return result.Records[0];
        }

        public async Task<string> CreateAsync(string typeName, JObject jobj)
        {
            if (jobj == null) throw new ArgumentNullException(nameof(jobj));
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(typeName));

            var response = await SendRequestWithBodyAsync(HttpMethod.Post, $"sobjects/{typeName}", jobj)
                .ConfigureAwait(false);

            return WriteResult.Parse(JObject.Parse(response)).Id;
        }

        public Task UpdateAsync(string typeName, string id, JObject jobj)
        {
            if (jobj == null) throw new ArgumentNullException(nameof(jobj));
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(typeName));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));

            return SendRequestWithBodyAsync(new HttpMethod("PATCH"), $"sobjects/{typeName}/{id}", jobj);
        }

        public Task DeleteAsync(string typeName, string id)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(typeName));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));

            return SendRequestAsync(HttpMethod.Delete, $"sobjects/{typeName}/{id}", null);
        }
    }
}