using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Custom
{
    public interface ICustomApi
    {
        Task<string> GetAsync(string servicePath, IDictionary<string, string> parameters = null);
        Task<string> PostAsync(string servicePath, JObject jObject);
        Task<string> PatchAsync(string servicePath, JObject jObject);
        Task<string> PutAsync(string servicePath, JObject jObject);
        Task<string> DeleteAsync(string servicePath, IDictionary<string, string> parameters = null);
    }
}