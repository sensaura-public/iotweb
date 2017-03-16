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
        private string _sessionId;
        private Dictionary<string, string> _sessionData;
        private readonly ISessionStorageHandler _sessionStorageHandler;
        private bool _isChanged;
        
        internal string SessionId => _sessionId;

        internal bool IsChanged => _isChanged;

        internal SessionHandler(string sessionId, ISessionStorageHandler sessionStorageHandler)
        {
            _sessionId = sessionId;
            _sessionData = new Dictionary<string, string>();
            _sessionStorageHandler = sessionStorageHandler;
            _isChanged = false;
        }

        internal bool SaveSessionData()
        {
            var sessionTask = _sessionStorageHandler.SaveDataAsync(_sessionId, _sessionData);
            sessionTask.Wait();
            var isSaved = sessionTask.Result;
            _isChanged = false;
            return isSaved;
        }

        internal bool GetSessionData()
        {
            try
            {
                bool isRetrieved = false;

                var sessionTask = _sessionStorageHandler.GetDataAsync(_sessionId);
                sessionTask.Wait();
                
                var sessionData = sessionTask.Result;
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

        internal bool DestroyExpiredSessions()
        {
            var sessionTask = _sessionStorageHandler.DeleteSessionsAsync();
            sessionTask.Wait();
            return sessionTask.Result;
        }

        public bool DestroySession()
        {
            _sessionData.Clear();
            _isChanged = false;

            var sessionTask = _sessionStorageHandler.DeleteSessionAsync(SessionId);
            sessionTask.Wait();

            _sessionId = Utilities.GetNewSessionIdentifier();
            var isSaved = SaveSessionData();

            return sessionTask.Result && isSaved;
        }

        public void SetSessionValue(string key, string value)
        {
            _isChanged = true;
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
            return _sessionData.ContainsKey(key) ? _sessionData[key] : null;
        }

        internal bool UpdateSessionTimeOut()
        {
            return _sessionStorageHandler.UpdateSessionExpireTime(SessionId);
        }
    }
}
