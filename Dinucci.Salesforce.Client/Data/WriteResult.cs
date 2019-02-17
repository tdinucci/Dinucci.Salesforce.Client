using System;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Data
{
    public class WriteResult
    {
        private const string IdProperty = "id";
        private const string SuccessProperty = "success";
        private const string ErrorsProperty = "errors";

        public string Id { get; private set; }
        public bool Success { get; private set; }
        public string[] Errors { get; private set; }

        public static WriteResult Parse(JObject jObject)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

            var id = JsonUtils.GetJObjectProperty<string>(jObject, IdProperty);
            var success = JsonUtils.GetJObjectProperty<bool>(jObject, SuccessProperty);
            var errors = JsonUtils.GetJObjectArrayProperty<string>(jObject, ErrorsProperty);

            return new WriteResult
            {
                Id = id,
                Success = success,
                Errors = errors
            };
        }
    }
}