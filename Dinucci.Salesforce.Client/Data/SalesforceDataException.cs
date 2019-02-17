using System;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Data
{
    public class SalesforceDataException : SalesforceException
    {
        private const string MessageProperty = "message";

        public static SalesforceDataException Parse(string message)
        {
            try
            {
                var jArray = JArray.Parse(message);
                return Parse(jArray);
            }
            catch
            {
                return new SalesforceDataException($"Error communicating with Salesforce: {message}");
            }
        }

        public static SalesforceDataException Parse(JArray jArray)
        {
            var message = string.Empty;
            var errorLevel = 1;
            if (jArray != null && jArray.Count > 0)
            {
                foreach (var jToken in jArray)
                {
                    if (jToken is JObject jObject && jObject.ContainsKey(MessageProperty))
                    {
                        message += $"==== Level {errorLevel++} ===={Environment.NewLine}" +
                                   $"{jObject[MessageProperty].Value<string>().Trim()}{Environment.NewLine}";
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(message))
                message = $"An unexpected error occurred: {jArray}";

            return new SalesforceDataException(message);
        }

        public SalesforceDataException(string message) : base(message)
        {
        }
    }
}