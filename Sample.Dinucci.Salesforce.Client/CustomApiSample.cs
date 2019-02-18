using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Custom;
using Newtonsoft.Json.Linq;

namespace Sample.Dinucci.Salesforce.Client
{
    /// <summary>
    /// This is an example of calling a custom REST API which has been defined within a Salesforce Org.
    /// The Apex code for the Salesforce service this example calls is in Apex/EchoController.cls
    /// </summary>
    public class CustomApiSample
    {
        private readonly ICustomApi _api;

        public CustomApiSample(ICustomApi api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public async Task RunAsync()
        {
            Console.WriteLine("*** Custom API Sample ***");
            
            await GetAsync().ConfigureAwait(false);
            await PostAsync().ConfigureAwait(false);
            await PutAsync().ConfigureAwait(false);
            await PatchAsync().ConfigureAwait(false);
            await DeleteAsync().ConfigureAwait(false);
        }

        private async Task GetAsync()
        {
            var result = await _api.GetAsync("Echo", new Dictionary<string, string> {{"message", "One"}});

            Console.WriteLine($"Get result - {result}");
        }

        private async Task PostAsync()
        {
            var result = await _api.PostAsync("Echo", new JObject {{"message", "Two"}});

            Console.WriteLine($"Post result - {result}");
        }

        private async Task PutAsync()
        {
            var result = await _api.PutAsync("Echo", new JObject {{"message", "Three"}});

            Console.WriteLine($"Put result - {result}");
        }

        private async Task PatchAsync()
        {
            var result = await _api.PatchAsync("Echo", new JObject {{"message", "Four"}});

            Console.WriteLine($"Patch result - {result}");
        }

        private async Task DeleteAsync()
        {
            var result = await _api.DeleteAsync("Echo", new Dictionary<string, string> {{"message", "Five"}});

            Console.WriteLine($"Delete result - {result}");
        }
    }
}