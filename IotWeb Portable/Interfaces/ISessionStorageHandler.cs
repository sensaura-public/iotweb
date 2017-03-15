using System.Collections.Generic;
using System.Threading.Tasks;
using IotWeb.Common.Util;

namespace IotWeb.Common.Interfaces
{
    public interface ISessionStorageHandler
    {
        SessionStorageConfiguration SessionStorageConfiguration
        {
            get;
        }

        Task<bool> SaveDataAsync(string fileName, string data);
        Task<string> GetDataAsync(string fileName);
        Task<bool> DeleteSessionsAsync();
        Task<bool> DeleteSessionAsync(string sessionId);
        Task<Dictionary<SessionAttributes, object>> GetSessionMetadata(string fileName);

    }
}