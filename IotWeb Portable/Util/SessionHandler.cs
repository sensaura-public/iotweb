using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common.Http;
using IotWeb.Common.Interfaces;
using Newtonsoft.Json;

namespace IotWeb.Common.Util
{
    public class SessionHandler
    {
        private readonly string _sessionId;
        private Dictionary<string, string> _sessionData;
        private readonly ISessionStorageHandler _sessionStorageHandler;
        private bool _isChanged;
        

        public string SessionId
        {
            get { return _sessionId; }
        }

        public bool IsChanged
        {
            get { return _isChanged; }
        }

        public SessionHandler(string sessionId, ISessionStorageHandler sessionStorageHandler)
        {
            _sessionId = sessionId;
            _sessionData = new Dictionary<string, string>();
            _sessionStorageHandler = sessionStorageHandler;
            _isChanged = false;
        }

        public async Task<bool> SaveSessionData()
        {
            var isSaved = await _sessionStorageHandler.SaveDataAsync(_sessionId, _sessionData);
            _isChanged = false;
            return isSaved;
        }

        public async Task<bool> GetSessionData(string sessionId)
        {
            try
            {
                bool isRetrieved = false;

                var sessionData = await _sessionStorageHandler.GetDataAsync(_sessionId);
                if (sessionData != null)
                {
                    _sessionData = sessionData;
                    isRetrieved = true;
                }

                return isRetrieved;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ClearExpiredSessions()
        {
            return await _sessionStorageHandler.DeleteSessionsAsync();
        }

        public bool ClearAndDeleteSession()
        {
            _sessionData.Clear();
            _isChanged = false;
            var sessionTask = _sessionStorageHandler.DeleteSessionAsync(SessionId);
            sessionTask.Wait();
            return sessionTask.Result;
        }

        public void SetSessionValue(string key, string value)
        {
            if (_sessionData.ContainsKey(key))
            {
                _sessionData[key] = value;
            }
            else
            {
                _sessionData.Add(key, value);
            }
        }

        public string GetSessionValue(string key)
        {
            if (_sessionData.ContainsKey(key))
            {
                return _sessionData[key];
            }

            return null;

        }
    }
}
