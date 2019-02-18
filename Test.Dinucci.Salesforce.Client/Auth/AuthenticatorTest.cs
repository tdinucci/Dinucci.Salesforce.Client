using System;
using System.Net.Http;
using Dinucci.Salesforce.Client.Auth;
using Xunit;

namespace Test.Dinucci.Salesforce.Client.Auth
{
    public class AuthenticatorTest : IDisposable
    {
        private readonly HttpClient _httpClient;

        public AuthenticatorTest()
        {
            _httpClient = new HttpClient();
        }

        [Fact]
        public void GoodLogin()
        {
            var authenticator = new Authenticator(SalesforceConfig.ClientId, SalesforceConfig.ClientSecret,
                SalesforceConfig.Username, SalesforceConfig.Password, SalesforceConfig.AuthEndpoint, _httpClient);

            var authResult = authenticator.GetAuthInfo();

            Assert.True(Uri.IsWellFormedUriString(authResult.Url, UriKind.Absolute));
            Assert.False(string.IsNullOrWhiteSpace(authResult.Url));
        }


        [Fact]
        public void BadClientId()
        {
            var authenticator = new Authenticator("x", SalesforceConfig.ClientSecret,
                SalesforceConfig.Username, SalesforceConfig.Password, SalesforceConfig.AuthEndpoint, _httpClient);

            var ex = Assert.Throws<AuthenticationException>(() => authenticator.GetAuthInfo());

            Assert.Equal("invalid_client_id: client identifier invalid", ex.Message);
        }

        [Fact]
        public void BadClientSecret()
        {
            var authenticator = new Authenticator(SalesforceConfig.ClientId, "x",
                SalesforceConfig.Username, SalesforceConfig.Password, SalesforceConfig.AuthEndpoint, _httpClient);

            var ex = Assert.Throws<AuthenticationException>(() => authenticator.GetAuthInfo());

            Assert.Equal("invalid_client: invalid client credentials", ex.Message);
        }

        [Fact]
        public void BadUsername()
        {
            var authenticator = new Authenticator(SalesforceConfig.ClientId, SalesforceConfig.ClientSecret,
                "x", SalesforceConfig.Password, SalesforceConfig.AuthEndpoint, _httpClient);

            var ex = Assert.Throws<AuthenticationException>(() => authenticator.GetAuthInfo());

            Assert.Equal("invalid_grant: authentication failure", ex.Message);
        }

        [Fact]
        public void BadPassword()
        {
            var authenticator = new Authenticator(SalesforceConfig.ClientId, SalesforceConfig.ClientSecret,
                SalesforceConfig.Username, "x", SalesforceConfig.AuthEndpoint, _httpClient);

            var ex = Assert.Throws<AuthenticationException>(() => authenticator.GetAuthInfo());

            Assert.Equal("invalid_grant: authentication failure", ex.Message);
        }

        [Fact]
        public void BadAuthEndpoint()
        {
            var authenticator = new Authenticator(SalesforceConfig.ClientId, SalesforceConfig.ClientSecret,
                SalesforceConfig.Username, SalesforceConfig.Password, SalesforceConfig.AuthEndpoint + "x",
                _httpClient);

            var ex = Assert.Throws<AuthenticationException>(() => authenticator.GetAuthInfo());

            Assert.StartsWith("An error occured authenticating, see inner exception:", ex.Message);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}