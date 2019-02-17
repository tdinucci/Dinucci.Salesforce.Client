using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Data
{
    public interface IDataApi
    {
        Task<JObject> DescribeAsync(string typeName);

        Task<ReadResult<JObject>> QueryAsync(string soql);

        Task<ReadResult<JObject>> GetNextAsync(string nextRecordsUrlSuffix);

        Task<string> CreateAsync(string typeName, JObject jobj);

        Task UpdateAsync(string typeName, string id, JObject jobj);

        Task DeleteAsync(string typeName, string id);
    }
}