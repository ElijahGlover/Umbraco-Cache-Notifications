using Newtonsoft.Json;

namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public class JsonPayloadService : IPayloadService
    {
        public virtual T[] Deserialize<T>(string input)
        {
            return JsonConvert.DeserializeObject<T[]>(input);
        }

        public virtual string Serialize(object input)
        {
            return JsonConvert.SerializeObject(input);
        }
    }
}
