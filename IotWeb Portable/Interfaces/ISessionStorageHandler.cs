using System.Collections.Generic;
using System.Threading.Tasks;
using IotWeb.Common.Util;

namespace IotWeb.Common.Interfaces
{
    public interface ISessionStorageHandler
    {
        Task<bool> SaveDataAsync(string sessionId, IDictionary<string, string> data);
        Task<Dictionary<string, string>> GetDataAsync(string sessionId);
        Task<bool> DeleteSessionsAsync();
        Task<bool> DeleteSessionAsync(string sessionId);
        Task<Dictionary<SessionAttributes, object>> GetSessionMetadata(string sessionId);
        bool UpdateSessionExpireTime(string sessionId);
    }
}