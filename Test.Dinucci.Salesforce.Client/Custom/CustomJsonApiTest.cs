using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client;
using Dinucci.Salesforce.Client.Auth;
using Dinucci.Salesforce.Client.Custom;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Test.Dinucci.Salesforce.Client.Custom
{
    public class CustomJsonApiTest : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ICustomApi _api;

        public CustomJsonApiTest()
        {
            _httpClient = new HttpClient();

            var authenticator = new Authenticator(SalesforceConfig.ClientId, SalesforceConfig.ClientSecret,
                SalesforceConfig.Username, SalesforceConfig.Password, SalesforceConfig.AuthEndpoint, _httpClient);
            
            _api = new CustomApi(authenticator, _httpClient);
        }

        [Fact]
        public async Task GetGood()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"message", message}, {"abc", "123"}};
            var response = await _api.GetAsync("Echo", parameters).ConfigureAwait(false);

            Assert.Equal($"[GET] {message}", response);
        }

        [Fact]
        public async Task GetBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.GetAsync("Echo", parameters))
                .ConfigureAwait(false);

            Assert.Equal("GET failed. HTTP 400 - BadRequest - \"Error: No message\"", ex.Message);
        }

        [Fact]
        public async Task GetMissingParameter()
        {
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.GetAsync("Echo", null))
                .ConfigureAwait(false);

            Assert.Equal("GET failed. HTTP 400 - BadRequest - \"Error: No message\"", ex.Message);
        }

        [Fact]
        public async Task GetBadService()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"message", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.GetAsync("xEcho", parameters))
                .ConfigureAwait(false);

            Assert.StartsWith("GET failed. HTTP 404 - NotFound", ex.Message);
        }

        [Fact]
        public async Task GetObjectGood()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"message", message}, {"abc", "123"}};
            var response = await _api.GetAsync("EchoObject", parameters).ConfigureAwait(false);

            var jobj = JObject.Parse(response);
            Assert.Equal("GET", jobj["Method"].Value<string>());
            Assert.Equal(message, jobj["Value"].Value<string>());
        }

        [Fact]
        public async Task GetObjectBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.GetAsync("EchoObject", parameters))
                .ConfigureAwait(false);

            Assert.StartsWith("GET failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task GetObjectMissingParameter()
        {
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.GetAsync("EchoObject", null))
                .ConfigureAwait(false);

            Assert.StartsWith("GET failed. HTTP 400 - BadRequest", ex.Message);
        }
        
        [Fact]
        public async Task PostGood()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"message", message}};
            var response = await _api.PostAsync("Echo", request).ConfigureAwait(false);

            Assert.Equal($"[POST] {message}", response);
        }

        [Fact]
        public async Task PostBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PostAsync("Echo", request))
                .ConfigureAwait(false);

            Assert.StartsWith("POST failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PostMissingParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PostAsync("Echo", request))
                .ConfigureAwait(false);

            Assert.StartsWith("POST failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PostBadService()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"message", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PostAsync("xEcho", request))
                .ConfigureAwait(false);

            Assert.StartsWith("POST failed. HTTP 404 - NotFound", ex.Message);
        }

        [Fact]
        public async Task PostObjectGood()
        {
            var message = Guid.NewGuid().ToString();
            var msgJobj = new JObject{{"Value", message}};
            var request = new JObject {{"message", msgJobj}};

            var response = await _api.PostAsync("EchoObject", request).ConfigureAwait(false);

            var jobj = JObject.Parse(response);
            Assert.Equal("POST", jobj["Method"].Value<string>());
            Assert.Equal(message, jobj["Value"].Value<string>());
        }

        [Fact]
        public async Task PostObjectBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert
                .ThrowsAsync<SalesforceException>(() => _api.PostAsync("EchoObject", request))
                .ConfigureAwait(false);

            Assert.StartsWith("POST failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PostObjectMissingParameter()
        {
            var request = new JObject();
            var ex = await Assert
                .ThrowsAsync<SalesforceException>(() => _api.PostAsync("EchoObject", request))
                .ConfigureAwait(false);

            Assert.StartsWith("POST failed. HTTP 400 - BadRequest", ex.Message);
        }
        
        [Fact]
        public async Task PatchGood()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"message", message}};
            var response = await _api.PatchAsync("Echo", request).ConfigureAwait(false);

            Assert.Equal($"[PATCH] {message}", response);
        }

        [Fact]
        public async Task PatchBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PatchAsync("Echo", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PATCH failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PatchMissingParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PatchAsync("Echo", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PATCH failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PatchBadService()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"message", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PatchAsync("xEcho", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PATCH failed. HTTP 404 - NotFound", ex.Message);
        }

        [Fact]
        public async Task PatchObjectGood()
        {
            var message = Guid.NewGuid().ToString();
            var msgJobj = new JObject {{"Value", message}};
            var request = new JObject {{"message", msgJobj}};
            var response = await _api.PatchAsync("EchoObject", request).ConfigureAwait(false);

            var jobj = JObject.Parse(response);
            Assert.Equal("PATCH", jobj["Method"].Value<string>());
            Assert.Equal(message, jobj["Value"].Value<string>());
        }

        [Fact]
        public async Task PatchObjectBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PatchAsync("EchoObject", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PATCH failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PatchObjectMissingParameter()
        {
            var request = new JObject();
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PatchAsync("EchoObject", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PATCH failed. HTTP 400 - BadRequest", ex.Message);
        }
        
        [Fact]
        public async Task PutGood()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"message", message}};
            var response = await _api.PutAsync("Echo", request).ConfigureAwait(false);

            Assert.Equal($"[PUT] {message}", response);
        }

        [Fact]
        public async Task PutBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PutAsync("Echo", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PUT failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PutMissingParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PutAsync("Echo", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PUT failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PutBadService()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"message", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PutAsync("xEcho", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PUT failed. HTTP 404 - NotFound", ex.Message);
        }

        [Fact]
        public async Task PutObjectGood()
        {
            var message = Guid.NewGuid().ToString();
            var msgJobj = new JObject {{"Value", message}};
            var request = new JObject {{"message", msgJobj}};
            var response = await _api.PutAsync("EchoObject", request).ConfigureAwait(false);

            var jobj = JObject.Parse(response);
            Assert.Equal("PUT", jobj["Method"].Value<string>());
            Assert.Equal(message, jobj["Value"].Value<string>());
        }

        [Fact]
        public async Task PutObjectBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var request = new JObject {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PutAsync("EchoObject", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PUT failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task PutObjectMissingParameter()
        {
            var request = new JObject();
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.PutAsync("EchoObject", request))
                .ConfigureAwait(false);

            Assert.StartsWith("PUT failed. HTTP 400 - BadRequest", ex.Message);
        }
        
        [Fact]
        public async Task DeleteGood()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"message", message}, {"abc", "123"}};
            var response = await _api.DeleteAsync("Echo", parameters).ConfigureAwait(false);

            Assert.Equal($"[DELETE] {message}", response);
        }

        [Fact]
        public async Task DeleteBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.DeleteAsync("Echo", parameters))
                .ConfigureAwait(false);

            Assert.Equal("DELETE failed. HTTP 400 - BadRequest - \"Error: No message\"", ex.Message);
        }

        [Fact]
        public async Task DeleteMissingParameter()
        {
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.DeleteAsync("Echo", null))
                .ConfigureAwait(false);

            Assert.Equal("DELETE failed. HTTP 400 - BadRequest - \"Error: No message\"", ex.Message);
        }

        [Fact]
        public async Task DeleteBadService()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"message", message}};
            var ex = await Assert
                .ThrowsAsync<SalesforceException>(() => _api.DeleteAsync("xEcho", parameters))
                .ConfigureAwait(false);

            Assert.StartsWith("DELETE failed. HTTP 404 - NotFound", ex.Message);
        }

        [Fact]
        public async Task DeleteObjectGood()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"message", message}, {"abc", "123"}};
            var response = await _api.DeleteAsync("EchoObject", parameters).ConfigureAwait(false);

            var jobj = JObject.Parse(response);
            Assert.Equal("DELETE", jobj["Method"].Value<string>());
            Assert.Equal(message, jobj["Value"].Value<string>());
        }

        [Fact]
        public async Task DeleteObjectBadParameter()
        {
            var message = Guid.NewGuid().ToString();
            var parameters = new Dictionary<string, string> {{"xmessage", message}};
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.DeleteAsync("EchoObject", parameters))
                .ConfigureAwait(false);

            Assert.StartsWith("DELETE failed. HTTP 400 - BadRequest", ex.Message);
        }

        [Fact]
        public async Task DeleteObjectMissingParameter()
        {
            var ex = await Assert.ThrowsAsync<SalesforceException>(() => _api.DeleteAsync("EchoObject", null))
                .ConfigureAwait(false);

            Assert.StartsWith("DELETE failed. HTTP 400 - BadRequest", ex.Message);
        }
        
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}