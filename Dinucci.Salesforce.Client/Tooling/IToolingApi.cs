using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Tooling
{
    public interface IToolingApi : IApi
    {
        Task<JObject> QueryAsync(string query);
        Task<JObject> ExecuteApexAsync(string apex);
    }
}