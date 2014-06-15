namespace Umbraco.Core.Sync.QueryNotification.Services
{
    public interface IPayloadService
    {
        T[] Deserialize<T>(string input);
        string Serialize(object input);
    }
}