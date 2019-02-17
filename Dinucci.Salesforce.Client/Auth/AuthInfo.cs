using System;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Auth
{
    public class AuthInfo : IAuthInfo
    {
        private const string IdProperty = "id";
        private const string IssuedAtProperty = "issued_at";
        private const string InstanceUrlProperty = "instance_url";
        private const string SignatureProperty = "signature";
        private const string AccessTokenProperty = "access_token";
        private const string RefreshTokenProperty = "refresh_token";

        public string Id { get; private set; }
        public DateTime IssuedAt { get; private set; }
        public string Url { get; private set; }
        public string Signature { get; private set; }
        public string Token { get; private set; }
        public string RefreshToken { get; private set; }

        public static AuthInfo Parse(JObject jObject)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

            var result = new AuthInfo
            {
                Id = JsonUtils.GetJObjectProperty<string>(jObject, IdProperty),
                IssuedAt = new DateTime(1970, 1, 1).AddMilliseconds(
                    JsonUtils.GetJObjectProperty<long>(jObject, IssuedAtProperty)),
                Url = JsonUtils.GetJObjectProperty<string>(jObject, InstanceUrlProperty),
                Signature = JsonUtils.GetJObjectProperty<string>(jObject, SignatureProperty),
                Token = JsonUtils.GetJObjectProperty<string>(jObject, AccessTokenProperty),
                RefreshToken = JsonUtils.GetJObjectProperty<string>(jObject, RefreshTokenProperty, false)
            };

            return result;
        }
    }
}