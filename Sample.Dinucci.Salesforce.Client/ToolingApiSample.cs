using System;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Tooling;
using Newtonsoft.Json.Linq;

namespace Sample.Dinucci.Salesforce.Client
{
    /// <summary>
    /// This is an example of querying metadata and executing anonymous Apex with the tooling API
    /// </summary>
    public class ToolingApiSample
    {
        private readonly IToolingApi _api;

        public ToolingApiSample(IToolingApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public async Task RunAsync()
        {
            Console.WriteLine("*** Tooling API Sample ***");

            await QueryAsync().ConfigureAwait(false);
            await ExecuteApexAsync().ConfigureAwait(false);
        }

        private async Task QueryAsync()
        {
            var query =
                "SELECT Id, SourceObject, Active, IsOptional, DeveloperName, Metadata FROM LookupFilter LIMIT 1";
            var result = await _api.QueryAsync(query).ConfigureAwait(false);

            Console.WriteLine($"Query Result:{Environment.NewLine}{result}");
        }

        private async Task ExecuteApexAsync()
        {
            var apex = @"
Contact contact = new Contact();
contact.Salutation = 'Mrs';
contact.FirstName = 'Betty';
contact.LastName = 'Boo';
contact.MailingCountry = 'United Kingdom';
contact.MailingPostalCode = 'AB12 3CD';

insert contact;";

            var result = await _api.ExecuteApexAsync(apex).ConfigureAwait(false);
            Console.WriteLine($"Execute APEX - Success: {result["success"].Value<bool>()}");
        }
    }
}