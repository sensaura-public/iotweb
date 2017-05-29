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
        private Dictionary<string, object> _sessionData;
        private readonly ISessionStorageHandler _sessionStorageHandler;
        private bool _isChanged;
        internal bool IsSessionDestroyed { get; set; }

        internal string SessionId => _sessionId;

        internal bool IsChanged => _isChanged;
        
        internal SessionHandler(string sessionId, ISessionStorageHandler sessionStorageHandler)
        {
            _sessionId = sessionId;
            _sessionData = new Dictionary<string, object>();
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

            return sessionTask.Result;
        }

        public bool DestroyAndCreateNewSession()
        {
            _sessionData.Clear();
            _isChanged = false;

            var sessionTask = _sessionStorageHandler.DeleteSessionAsync(SessionId);
            sessionTask.Wait();

            _sessionId = Utilities.GetNewSessionIdentifier();
            var isSaved = SaveSessionData();
            
            IsSessionDestroyed = sessionTask.Result && isSaved;
            return IsSessionDestroyed;
        }

        public void SetSessionValue(string key, object value)
        {
            lock (_sessionData)
            {
                _isChanged = true;

                if (_sessionData.ContainsKey(key))
                    _sessionData[key] = value;
                else
                    _sessionData.Add(key, value);
            }
        }
        
        public object GetSessionValue(string key)
        {
            lock (_sessionData)
            {
                return _sessionData.ContainsKey(key) ? _sessionData[key] : null;
            }
        }
        
        public T GetSessionValue<T>(string key) where T : class
        {
            lock (_sessionData)
            {
                var returnValue = default(T);
                if (_sessionData.ContainsKey(key))
                {
                    var t = _sessionData[key].GetType();

                    if (t.Name == "JObject")
                    {
                        returnValue = JsonConvert.DeserializeObject<T>(_sessionData[key].ToString());
                        SetSessionValue(key, returnValue);
                        _isChanged = false;
                    }
                    else
                    {
                        returnValue = _sessionData[key] as T;
                    }
                }

                return returnValue;
            }
        }
        
        public T GetSessionStructValue<T>(string key) where T : struct
        {
            lock (_sessionData)
            {
                return _sessionData.ContainsKey(key) ? (T) Convert.ChangeType(_sessionData[key], typeof(T)) : default(T);
            }
        }
        
        internal bool UpdateSessionTimeOut()
        {
            return _sessionStorageHandler.UpdateSessionExpireTime(SessionId);
        }
    }
}
