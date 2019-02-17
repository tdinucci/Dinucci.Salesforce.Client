using System;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Auth
{
    public class AuthenticationException : Exception
    {
        public const string ErrorProperty = "error";
        public const string ErrorDescriptionProperty = "error_description";

        private static string GetMessage(string error, string description)
        {
            var message = string.IsNullOrWhiteSpace(error) ? string.Empty : $"{error}: ";
            message += string.IsNullOrWhiteSpace(description) ? string.Empty : description;

            return string.IsNullOrWhiteSpace(message) ? "An unexpected authentication error occurred" : message;
        }

        public static AuthenticationException Parse(JObject jObject)
        {
            var error = JsonUtils.GetJObjectProperty<string>(jObject, ErrorProperty, false);
            var description = JsonUtils.GetJObjectProperty<string>(jObject, ErrorDescriptionProperty, false);

            return new AuthenticationException(error, description);
        }

        public AuthenticationException(string error, string description) : base(GetMessage(error, description))
        {
        }

        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}