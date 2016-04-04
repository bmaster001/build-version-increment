using System;
using System.Collections.Generic;
using System.Text;

namespace BuildVersionIncrement
{
    /// <summary>
    /// Winwrapper class for getting window handle
    /// </summary>
    internal class WinWrapper : System.Windows.Forms.IWin32Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinWrapper"/> class.
        /// </summary>
        /// <param name="c">The object implementing the add-in.</param>
        public WinWrapper(Connect connect)
        {
            _c = connect;
        }

        private Connect _c;

        /// <summary>
        /// Window Handle
        /// </summary>
        public System.IntPtr Handle
        {
            get
            {
                System.IntPtr iptr = new System.IntPtr(_c.ApplicationObject.MainWindow.HWnd);

                return iptr;
            }
        }
    }
}
