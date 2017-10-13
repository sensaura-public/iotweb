using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IotWeb.Common.Interfaces;

namespace IotWeb.Common.Util
{
    public class SessionConfiguration
    {
        #region Data Members

        /// <summary>
        /// Gets the relative path of storage folder for session files.
        /// </summary>
        /// <value>The storage folder name.</value>
        public string StorageFolder { get; }

        /// <summary>
        /// Gets the session expired time in minutes.
        /// </summary>
        /// <value>The session expired time.</value>
        public uint SessionTimeOut { get; }
        
        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionConfiguration"/> class.
        /// </summary>
        public SessionConfiguration()
        {
            StorageFolder = "IoTSession";
            SessionTimeOut = 15;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionConfiguration"/> class.
        /// </summary>
        /// <param name="storageFolder">The relative path of storage folder for session files.</param>
        /// <param name="sessionTimeOut">Sets the session timeout in minutes.</param>
        public SessionConfiguration(string storageFolder, uint sessionTimeOut)
        {
            if (string.IsNullOrWhiteSpace(storageFolder))
                throw new ArgumentException("Storage folder can not be null, empty or white space.");
            
            StorageFolder = storageFolder;
            SessionTimeOut = sessionTimeOut;
        }

        #endregion
    }
}
