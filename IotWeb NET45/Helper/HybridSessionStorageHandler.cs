using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common.Interfaces;
using IotWeb.Common.Util;
using System.Net.Mime;
using System.Security.AccessControl;
using Windows.Storage;

using System.Windows.Forms;
using IotWeb.Common.Http;
using Newtonsoft.Json;

namespace IotWeb.Server.Helper
{
    public class HybridSessionStorageHandler : ISessionStorageHandler
    {
        private readonly SessionStorageConfiguration _sessionStorageConfiguration;
        private const string StorageFolder = "IoTSession";
        private List<SessionCacheObject> _sessionDataCache;
        private readonly string _sessionFileExtension;

        public HybridSessionStorageHandler(SessionStorageConfiguration sessionStorageConfiguration)
        {
            _sessionStorageConfiguration = sessionStorageConfiguration;
            _sessionDataCache = new List<SessionCacheObject>();
            _sessionFileExtension = "sess";
            LoadSessionFiles();
        }

        private void LoadSessionFiles()
        {
            try
            {
                string[] files = Directory.GetFiles(GetStoragePath());

                foreach (string file in files)
                {
                    var sessionData = File.ReadAllText(file);
                    if (!string.IsNullOrEmpty(sessionData))
                    {
                        IDictionary<string, string> sessionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(sessionData);
                        _sessionDataCache.Add(new SessionCacheObject(Path.GetFileNameWithoutExtension(file), DateTime.Now, sessionDictionary));
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public async Task<bool> DeleteSessionsAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var sessionIds =
                        _sessionDataCache.Where(
                            s =>
                                s.LastAccessTime <
                                DateTime.Now.AddMinutes(-_sessionStorageConfiguration.SessionExpiredTime)).Select(s => s.SessionId);

                    _sessionDataCache.RemoveAll(s => sessionIds.Contains(s.SessionId));
                    
                    string[] files = Directory.GetFiles(GetStoragePath());

                    var filesToDelete = files.Where(s => sessionIds.Contains(Path.GetFileNameWithoutExtension(s)));

                    foreach (string file in filesToDelete)
                    {
                        FileInfo fi = new FileInfo(file);
                        fi.Delete();
                    }
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            try
            {
                await Task.Run(() =>
                {
                    var sessionData = _sessionDataCache.FirstOrDefault(s => s.SessionId == sessionId);
                    if (sessionData != null)
                    {
                        _sessionDataCache.Remove(sessionData);
                    }

                    string[] files = Directory.GetFiles(GetStoragePath());

                    foreach (string file in files)
                    {
                        if (file.Contains(sessionId))
                        {
                            FileInfo fi = new FileInfo(file);
                            fi.Delete();
                            break;
                        }
                    }
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Dictionary<string, string>> GetDataAsync(string sessionId)
        {
            try
            {
                Dictionary<string, string> data = null;

                await Task.Run(() =>
                {
                    var sessionData = _sessionDataCache.FirstOrDefault(s => s.SessionId == sessionId);
                    if (sessionData != null)
                    {
                        data = (Dictionary<string, string>)sessionData.SessionData;
                    }
                });

                return data;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Dictionary<SessionAttributes, object>> GetSessionMetadata(string sessionId)
        {
            Dictionary<SessionAttributes, object> fileMetaData = null;

            await Task.Run(() =>
            {
                string[] files = Directory.GetFiles(GetStoragePath());

                foreach (string file in files)
                {
                    if (file.Contains(sessionId))
                    {
                        FileInfo fileInfo = new FileInfo(file);

                        fileMetaData = new Dictionary<SessionAttributes, object>
                        {
                            {SessionAttributes.LastAccessTime, fileInfo.LastAccessTime},
                            {SessionAttributes.Size, fileInfo.Length}
                        };
                        break;
                    }
                }
            });

            return fileMetaData;
        }

        public async Task<bool> SaveDataAsync(string sessionId, IDictionary<string, string> data)
        {
            try
            {
                bool isSaved = false;
                await Task.Run(() =>
                {
                    _sessionDataCache.Add(new SessionCacheObject(sessionId, DateTime.Now, data));

                    var filePath = GetFilePath(sessionId);
                    var sessionData = JsonConvert.SerializeObject(data);
                    File.WriteAllText(filePath, sessionData);
                    
                    isSaved = true;
                });

                return isSaved;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateSessionExpireTime(string sessionId)
        {
            var session = _sessionDataCache.FirstOrDefault(s => s.SessionId == sessionId);
            if (session != null)
            {
                session.LastAccessTime = DateTime.Now;
                return true;
            }

            return false;
        }

        private string GetStoragePath()
        {
            string fullStorageFilePath;

            fullStorageFilePath = !string.IsNullOrWhiteSpace(_sessionStorageConfiguration.StoragePath)
                ? _sessionStorageConfiguration.StoragePath
                : Path.Combine(Application.LocalUserAppDataPath, StorageFolder);

            if (!Directory.Exists(fullStorageFilePath))
            {
                Directory.CreateDirectory(fullStorageFilePath);
            }

            return fullStorageFilePath;
        }

        private string GetFilePath(string fileName)
        {
            string fullFilePath;

            fullFilePath = Path.Combine(GetStoragePath(), fileName);

            if (!string.IsNullOrWhiteSpace(_sessionFileExtension))
            {
                fullFilePath = fullFilePath + "." + _sessionFileExtension;
            }

            return fullFilePath;
        }
    }

    public class SessionCacheObject
    {
        public string SessionId { get; set; }
        public DateTime LastAccessTime { get; set; }
        public IDictionary<string, string> SessionData { get; set; }

        public SessionCacheObject(string sessionId, DateTime lastAccessTime, IDictionary<string, string> sessionData)
        {
            SessionId = sessionId;
            LastAccessTime = lastAccessTime;
            SessionData = sessionData;
        }
    }
}
