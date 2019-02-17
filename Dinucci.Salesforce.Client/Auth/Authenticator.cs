using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Auth
{
    public class Authenticator : IAuthenticator
    {
        private static readonly object AuthLock = new object();

        protected string ClientId { get; }
        protected string ClientSecret { get; }
        protected string Username { get; }
        protected string Password { get; }
        protected string AuthServiceEndpoint { get; }
        public TimeSpan ReauthenticateFrequency { get; }
        protected HttpClient HttpClient { get; }

        protected AuthInfo LastAuthInfo { get; private set; }

        public Authenticator(string clientId, string clientSecret, string username, string password,
            string authServiceEndpoint, TimeSpan reauthenticateFrequency, HttpClient httpClient)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(clientSecret))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientSecret));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(password));
            if (string.IsNullOrWhiteSpace(authServiceEndpoint))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(authServiceEndpoint));
            if (!Uri.IsWellFormedUriString(authServiceEndpoint, UriKind.Absolute))
                throw new FormatException($"'{nameof(authServiceEndpoint)}' is not a valid URL");

            ClientId = clientId;
            ClientSecret = clientSecret;
            Username = username;
            Password = password;
            AuthServiceEndpoint = authServiceEndpoint;
            ReauthenticateFrequency = reauthenticateFrequency;
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public IAuthInfo Authenticate()
        {
            lock (AuthLock)
            {
                var responseString = string.Empty;
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Post, AuthServiceEndpoint))
                    {
                        request.Content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("client_id", ClientId),
                            new KeyValuePair<string, string>("client_secret", ClientSecret),
                            new KeyValuePair<string, string>("username", Username),
                            new KeyValuePair<string, string>("password", Password)
                        });

                        using (var response = HttpClient.SendAsync(request).GetAwaiter().GetResult())
                        {
                            responseString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var responseJobj = JObject.Parse(responseString);

                            if (response.IsSuccessStatusCode)
                            {
                                LastAuthInfo = AuthInfo.Parse(responseJobj);
                                return LastAuthInfo;
                            }

                            throw AuthenticationException.Parse(responseJobj);
                        }
                    }
                }
                catch (Exception ex) when (!(ex is AuthenticationException))
                {
                    var message = "An error occured authenticating, see inner exception";
                    if (!string.IsNullOrWhiteSpace(responseString))
                        message += $": {responseString}";

                    throw new AuthenticationException(message, ex);
                }
            }
        }

        public IAuthInfo GetAuthInfo()
        {
            if (IsAuthRequired())
                Authenticate();

            return LastAuthInfo;
        }

        protected virtual bool IsAuthRequired()
        {
            return LastAuthInfo == null || ReauthenticateFrequency == TimeSpan.Zero ||
                   (ReauthenticateFrequency < TimeSpan.MaxValue &&
                    LastAuthInfo.IssuedAt < DateTime.Now.Subtract(ReauthenticateFrequency));
        }
    }
}