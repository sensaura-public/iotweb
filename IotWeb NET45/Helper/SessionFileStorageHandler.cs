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

namespace IotWeb.Server.Helper
{
    public class SessionFileStorageHandler : ISessionStorageHandler
    {
        private readonly SessionStorageConfiguration _sessionStorageConfiguration;
        private const string StorageFolder = "IoTSession";

        public SessionFileStorageHandler(SessionStorageConfiguration sessionStorageConfiguration)
        {
            _sessionStorageConfiguration = sessionStorageConfiguration;
        }

        public SessionStorageConfiguration SessionStorageConfiguration
        {
            get
            {
                return _sessionStorageConfiguration;
            }
        }

        public async Task<bool> DeleteSessionsAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var path = GetStoragePath();

                    if (File.Exists(path))
                    {
                        string[] files = Directory.GetFiles(GetStoragePath());

                        foreach (string file in files)
                        {
                            FileInfo fi = new FileInfo(file);
                            if (fi.LastAccessTime < DateTime.Now.AddMinutes(-SessionStorageConfiguration.SessionExpiredTime))
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

        public async Task<bool> DeleteSessionAsync(string sessionId)
        {
            try
            {
                await Task.Run(() =>
                {
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

        public async Task<string> GetDataAsync(string fileName)
        {
            try
            {
                string data = null;

                await Task.Run(() => {
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        string fullFilePath = GetFilePath(fileName);

                        if (File.Exists(fullFilePath))
                        {
                            data = File.ReadAllText(fullFilePath);
                        }
                    }
                });

                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Task<Dictionary<SessionAttributes, object>> GetSessionMetadata(string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SaveDataAsync(string fileName, string data)
        {
            try
            {
                bool isSaved = false;
                await Task.Run(() =>
                {
                    var filePath = Path.GetDirectoryName(GetFilePath(fileName));
                    if (!Directory.Exists(filePath))
                    {
                        Directory.CreateDirectory(filePath);
                    }

                    File.WriteAllText(GetFilePath(fileName), data);
                    isSaved = true;
                });

                return isSaved;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetStoragePath()
        {
            string fullStorageFilePath;

            fullStorageFilePath = !string.IsNullOrWhiteSpace(SessionStorageConfiguration.StoragePath)
                ? SessionStorageConfiguration.StoragePath
                : Path.Combine(Application.LocalUserAppDataPath, StorageFolder);

            return fullStorageFilePath;
        }

        private string GetFilePath(string fileName)
        {
            string fullFilePath;

            fullFilePath = Path.Combine(GetStoragePath(), fileName);

            if (!string.IsNullOrWhiteSpace(SessionStorageConfiguration.SessionFileExtension))
            {
                fullFilePath = fullFilePath + "." + SessionStorageConfiguration.SessionFileExtension;
            }

            return fullFilePath;
        }
    }
}
