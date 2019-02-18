using System;
using System.Linq;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Data;
using Newtonsoft.Json.Linq;

namespace Sample.Dinucci.Salesforce.Client
{
    /// <summary>
    /// This is an example of using the basic operations on the data API
    /// </summary>
    public class DataApiSample
    {
        private readonly IDataApi _api;

        public DataApiSample(IDataApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public async Task RunAsync()
        {
            Console.WriteLine("*** Data API Sample ***");

            var contactId = await CreateAsync().ConfigureAwait(false);
            await QueryAsync(contactId).ConfigureAwait(false);

            await UpdateAsync(contactId).ConfigureAwait(false);
            await QueryAsync(contactId).ConfigureAwait(false);

            await DeleteAsync(contactId).ConfigureAwait(false);
            await QueryAsync(contactId).ConfigureAwait(false);

            await DescribeAsync().ConfigureAwait(false);
        }

        private async Task<string> CreateAsync()
        {
            var contact = new Contact
            {
                Salutation = "Mr",
                FirstName = "Jim",
                LastName = "Smith",
                MailingCountry = "United Kingdom",
                MailingPostalCode = "SW15 8GE"
            };

            var id = await _api.CreateAsync("Contact", JObject.FromObject(contact)).ConfigureAwait(false);

            Console.WriteLine($"Created contact with id: {id}");

            return id;
        }

        private async Task QueryAsync(string contactId)
        {
            var contactJobj = await _api
                .QueryAsync($"SELECT Id, Salutation, FirstName, LastName FROM Contact WHERE Id = '{contactId}'")
                .ConfigureAwait(false);

            if (contactJobj.TotalSize == 0)
                Console.WriteLine($"No contact found with Id: {contactId}");
            else
            {
                var contact = contactJobj.Records.First().ToObject<Contact>();
                Console.WriteLine(
                    $"Read Contact - Id: {contact.Id}, Name: {contact.Salutation} {contact.FirstName} {contact.LastName}");
            }
        }

        private async Task UpdateAsync(string contactId)
        {
            var updateObj = new JObject {{"FirstName", "Fred"}, {"LastName", "Black"}};
            await _api.UpdateAsync("Contact", contactId, updateObj).ConfigureAwait(false);

            Console.WriteLine("Updated contact");
        }

        private async Task DeleteAsync(string contactId)
        {
            await _api.DeleteAsync("Contact", contactId).ConfigureAwait(false);

            Console.WriteLine("Deleted contact");
        }

        private async Task DescribeAsync()
        {
            var result = await _api.DescribeAsync("Contact").ConfigureAwait(false);

            var names = result["fields"]
                .Take(15)
                .Select(f => $"{f["name"].Value<string>()}:{f["type"].Value<string>()}")
                .Aggregate((c, n) => $"{c}, {n}");

            Console.WriteLine($"Some fields on Contact - {names}");
        }
    }
}