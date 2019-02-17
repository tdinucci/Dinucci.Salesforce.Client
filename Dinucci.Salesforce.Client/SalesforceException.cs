using System;

namespace Dinucci.Salesforce.Client
{
    public class SalesforceException : Exception
    {
        public string SalesforceResponse { get; }

        public SalesforceException(string message) : base(message)
        {
        }

        public SalesforceException(string message, string salesforceResponse) : base(message)
        {
            SalesforceResponse = salesforceResponse;
        }
    }
}