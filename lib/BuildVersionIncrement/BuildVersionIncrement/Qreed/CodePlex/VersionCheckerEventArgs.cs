using System;
using System.Collections.Generic;
using System.Text;

namespace Qreed.CodePlex
{
    public class VersionCheckerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionCheckerEventArgs"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        internal VersionCheckerEventArgs(Exception error, bool newVersionAvailable)
        {
            _error = error;
            _newVersionAvailable = newVersionAvailable;
        }

        private Exception _error;
        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        public Exception Error
        {
            get { return this._error; }
        }

        private bool _newVersionAvailable;
        /// <summary>
        /// Gets a value indicating whether a new version is available.
        /// </summary>
        /// <value><c>true</c> if a new version is available; otherwise, <c>false</c>.</value>
        public bool NewVersionAvailable
        {
            get { return this._newVersionAvailable; }
        }
    }
}
