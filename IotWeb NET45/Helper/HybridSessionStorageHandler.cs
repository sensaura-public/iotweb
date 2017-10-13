﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common.Interfaces;
using IotWeb.Common.Util;
using System.Net.Mime;
using System.Security.AccessControl;
using IotWeb.Common.Http;
using Newtonsoft.Json;

namespace IotWeb.Server.Helper
{
    public class HybridSessionStorageHandler : ISessionStorageHandler
    {
        private readonly SessionConfiguration _sessionConfiguration;
        private Dictionary<string, SessionCacheObject> _sessionDataCache;
        private readonly string _sessionFileExtension;

        public HybridSessionStorageHandler(SessionConfiguration sessionConfiguration)
        {
            _sessionConfiguration = sessionConfiguration;
            _sessionDataCache = new Dictionary<string, SessionCacheObject>();
            _sessionFileExtension = "sess";
            LoadSessionFiles();
        }

        private void LoadSessionFiles()
        {
            string[] files = Directory.GetFiles(GetStoragePath());

            foreach (string file in files)
            {
                var sessionData = File.ReadAllText(file);
                IDictionary<string, object> sessionDictionary = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(sessionData))
                    sessionDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(sessionData);

                _sessionDataCache[Path.GetFileNameWithoutExtension(file)] = new SessionCacheObject(DateTime.Now, sessionDictionary);
            }
        }

        public async Task<bool> DeleteSessionsAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_sessionDataCache)
                    {
                        var sessionIds =
                        _sessionDataCache.Where(
                            s =>
                                s.Value.LastAccessTime <
                                DateTime.Now.AddMinutes(-_sessionConfiguration.SessionTimeOut)).Select(s => s.Key).ToList();

                        if (sessionIds.Count > 0)
                        {
                            foreach (var sid in sessionIds)
                            {
                                _sessionDataCache.Remove(sid);
                            }

                            string[] files = Directory.GetFiles(GetStoragePath());

                            var filesToDelete = files.Where(s => sessionIds.Contains(Path.GetFileNameWithoutExtension(s))).ToList();

                            foreach (string file in filesToDelete)
                            {
                                FileInfo fi = new FileInfo(file);
                                fi.Delete();
                            }
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

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_sessionDataCache)
                    {
                        _sessionDataCache.Remove(sessionId);

                        if (File.Exists(GetFilePath(sessionId)))
                        {
                            FileInfo fi = new FileInfo(GetFilePath(sessionId));
                            fi.Delete();
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

        public async Task<Dictionary<string, object>> GetDataAsync(string sessionId)
        {
            try
            {
                Dictionary<string, object> data = null;

                await Task.Run(() =>
                {
                    lock (_sessionDataCache)
                    {
                        if (_sessionDataCache.ContainsKey(sessionId))
                        {
                            _sessionDataCache[sessionId].LastAccessTime = DateTime.Now;
                            data = (Dictionary<string, object>)_sessionDataCache[sessionId].SessionData;
                        }
                        else if (File.Exists(GetFilePath(sessionId)))
                        {
                            var fileData = File.ReadAllText(GetFilePath(sessionId));
                            data = new Dictionary<string, object>();

                            if (!string.IsNullOrEmpty(fileData))
                                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(fileData);

                            _sessionDataCache[sessionId] = new SessionCacheObject(DateTime.Now, data);
                        }
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
            throw new NotImplementedException();
        }

        public async Task<bool> SaveDataAsync(string sessionId, IDictionary<string, object> data)
        {
            try
            {
                bool isSaved = false;
                await Task.Run(() =>
                {
                    lock (_sessionDataCache)
                    {
                        _sessionDataCache[sessionId] = new SessionCacheObject(DateTime.Now, data);

                        var filePath = GetFilePath(sessionId);
                        var sessionData = JsonConvert.SerializeObject(data);
                        File.WriteAllText(filePath, sessionData);

                        isSaved = true;
                    }
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
            lock (_sessionDataCache)
            {
                if (_sessionDataCache.ContainsKey(sessionId))
                {
                    _sessionDataCache[sessionId].LastAccessTime = DateTime.Now;
                    return true;
                }

                return false;
            }
        }

        private string GetStoragePath()
        {
            string fullStorageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _sessionConfiguration.StorageFolder);

            if (!Directory.Exists(fullStorageFilePath))
                Directory.CreateDirectory(fullStorageFilePath);

            return fullStorageFilePath;
        }

        private string GetFilePath(string fileName)
        {
            string fullFilePath = Path.Combine(GetStoragePath(), fileName);

            if (!string.IsNullOrWhiteSpace(_sessionFileExtension))
                fullFilePath = fullFilePath + "." + _sessionFileExtension;

            return fullFilePath;
        }
    }

    public class SessionCacheObject
    {
        public DateTime LastAccessTime { get; set; }
        public IDictionary<string, object> SessionData { get; set; }

        public SessionCacheObject(DateTime lastAccessTime, IDictionary<string, object> sessionData)
        {
            LastAccessTime = lastAccessTime;
            SessionData = sessionData;
        }
    }
}
