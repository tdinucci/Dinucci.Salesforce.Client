using System;
using Newtonsoft.Json.Linq;

namespace Dinucci.Salesforce.Client.Data
{
    public class ReadResult<T>
    {
        private const string DoneProperty = "done";
        private const string TotalSizeProperty = "totalSize";
        private const string NextRecordsUrlProperty = "nextRecordsUrl";
        private const string RecordsProperty = "records";

        public bool Done { get; private set; }
        public int TotalSize { get; private set; }
        public string NextRecordsUrl { get; private set; }
        public T[] Records { get; private set; }

        public static ReadResult<T> Parse(JObject jObject)
        {
            if (jObject == null) throw new ArgumentNullException(nameof(jObject));

            var result = new ReadResult<T>
            {
                Done = JsonUtils.GetJObjectProperty<bool>(jObject, DoneProperty),
                TotalSize = JsonUtils.GetJObjectProperty<int>(jObject, TotalSizeProperty),
                NextRecordsUrl = JsonUtils.GetJObjectProperty<string>(jObject, NextRecordsUrlProperty, false),
            };

            JsonUtils.EnsurePropertyExists(jObject, RecordsProperty);

            result.Records = jObject[RecordsProperty].ToObject<T[]>();

            return result;
        }
    }
}