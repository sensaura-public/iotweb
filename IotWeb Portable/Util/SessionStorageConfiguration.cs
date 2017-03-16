using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IotWeb.Common.Util
{
    public class SessionStorageConfiguration
    {
        #region Data Members

        /// <summary>
        /// Gets or sets the storage path for session files.
        /// </summary>
        /// <value>The storage path.</value>
        public string StoragePath { get; set; }

        /// <summary>
        /// Gets or sets the session expired time in minutes.
        /// </summary>
        /// <value>The session expired time.</value>
        public int SessionExpiredTime { get; set; }
        
        #endregion

        #region ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionStorageConfiguration"/> class.
        /// </summary>
        public SessionStorageConfiguration()
        {
            StoragePath = "";
            SessionExpiredTime = 15;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionStorageConfiguration"/> class.
        /// </summary>
        /// <param name="storagePath">The storage path.</param>
        /// <param name="sessionExpiredTime">The session expired time.</param>
        public SessionStorageConfiguration(string storagePath, int sessionExpiredTime)
        {
            StoragePath = storagePath;
            SessionExpiredTime = sessionExpiredTime;
        }

        #endregion
    }
}
