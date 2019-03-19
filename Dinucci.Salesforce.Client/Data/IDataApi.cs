using System.Threading.Tasks;
using Dinucci.Salesforce.Client.Auth;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Data
{
    public interface IDataApi : IApi
    {
        Task<JObject> GetAsync(string servicePath);
        
        Task<JObject> DescribeAsync(string typeName);

        Task<ReadResult<JObject>> QueryAsync(string soql);

        Task<JObject> QuerySingleAsync(string soql);

        Task<ReadResult<JObject>> GetNextAsync(string nextRecordsUrlSuffix);

        Task<JObject> SelectAllFieldsAsync(string typeName, string id);

        Task<string> CreateAsync(string typeName, JObject jobj);

        Task UpdateAsync(string typeName, string id, JObject jobj);

        Task DeleteAsync(string typeName, string id);
    }
}