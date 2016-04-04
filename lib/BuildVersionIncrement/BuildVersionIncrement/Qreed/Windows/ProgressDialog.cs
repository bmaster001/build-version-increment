using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace Qreed.Windows.Forms
{
    /// <summary>
    /// A simple dialog that hosts a BackgroundWorker thread and allows to display the progress of the 
    /// thread.<br/>
    /// Set the <see cref="ProgressDialog.DoWork"/> to implement a worker thread.<br/>
    /// Call <see cref="ProgressDialog.ReportProgress"/> to update the status.<br/>
    /// Check <see cref="ProgressDialog.CancellationPending"/> to see if the user is trying to cancel the thread.<br/>
    /// Set the <see cref="ProgressDialog.Argument"/> property to a value which should be passed
    /// to the <see cref="ProgressDialog.DoWork"/> event.<br/>
    /// Check <see cref="ProgressDialog.Result"/> to get the result.
    /// </summary>
    /// <remarks>
    /// Debugging .net threads isn't a very fun thing to do with visual studio. There's a big
    /// chance that you'll end up with a debugger that freezes once in a while. This has to do
    /// with 'function evaluation'; the debugger will try to display the values of properties in
    /// various property windows (Autos/Watch/Locals). If one of these properties does something
    /// like an invoke into a different thread then the one you're currently attached to it will 
    /// freeze because the debugger is stalling all other threads.<br/>
    /// Besides disabling the function evalution completely you can prevent the freezes by telling
    /// the debugger to leave some properties alone.<br/>
    /// <br/>
    /// See http://blogs.msdn.com/greggm/archive/2005/11/18/494648.aspx
    /// </remarks>
    [DebuggerDisplay("ProgressDialog")]
    public partial class ProgressDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialog"/> class.
        /// </summary>
        public ProgressDialog()
        {
            InitializeComponent();
            statusLabel.Text = "";   
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Shown"></see> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.EventArgs"></see> that contains the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            backgroundWorker.RunWorkerAsync(Argument);
            base.OnShown(e);
        }

        #region Events

        /// <summary>
        /// Occurs when the thread has been started.
        /// </summary>
        public event DoWorkEventHandler DoWork;

        #endregion

        #region Properties

        private object _argument;
        /// <summary>
        /// Gets or sets the argument that should be passed to the <see cref="DoWork"/> event.
        /// </summary>
        /// <value>The argument.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public object Argument
        {
            get { return _argument; }
            set { _argument = value; }
        }

        private ProgressDialogResult _result;
        /// <summary>
        /// Gets the result of the worker thread.
        /// </summary>
        /// <value>The result.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public ProgressDialogResult Result
        {
            get { return _result; }
        }

        /// <summary>
        /// Gets a value indicating whether cancellation is pending.
        /// </summary>
        /// <value><c>true</c> if cancellation pending; otherwise, <c>false</c>.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool CancellationPending
        {
            get { return backgroundWorker.CancellationPending; }
        }

        private volatile bool _autoClose;
        /// <summary>
        /// Gets or sets a value indicating whether auto close the dialog.
        /// </summary>
        /// <value><c>true</c> to auto close; otherwise, <c>false</c>.</value>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool AutoClose
        {
            get { return this._autoClose; }
            set { this._autoClose = value; }
        }

        #endregion

        #region Public Methodes

        /// <summary>
        /// Call this to report the progress of your thread.
        /// </summary>
        /// <param name="percent">The percent to display.</param>
        /// <param name="statusText">The status text to display. Set to <c>null</c> to leave the status text as it is.</param>
        public void ReportProgress(int percent, string statusText)
        {
            backgroundWorker.ReportProgress(percent, statusText);
        }

        #endregion

        #region BackgroundWorker Events

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Debug.WriteLine("BackgroundWorker_DoWork " + Thread.CurrentThread.ManagedThreadId);

            if (DoWork != null)
            {
                DoWork.Invoke(this, e);
            }
        }
        
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Debug.WriteLine("BackgroundWorker_ProgressChanged " + Thread.CurrentThread.ManagedThreadId);

            statusLabel.Text = (string)e.UserState;
            ProgressBar.Value = e.ProgressPercentage;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Debug.WriteLine("BackgroundWorker_RunWorkerCompleted " + Thread.CurrentThread.ManagedThreadId);

            bool isCancelled = e.Cancelled;
            Exception ex = e.Error;
            object result = null;

            // The property e.Result throws an exception if requested when Error is set or Cancelled is true
            if (ex == null && !isCancelled) 
                result = e.Result;

            _result = new ProgressDialogResult(result, isCancelled, ex);

            if (AutoClose || _result.Exception != null)
                Close();
            else
            {
                cancelButton.Text = "&Close";
                cancelButton.Enabled = true;
                cancelButton.Focus();
            }
        }

        #endregion

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (EndBackgroundWorker())
                Close();
        }

        private bool EndBackgroundWorker()
        {
            if (backgroundWorker.IsBusy)
            {
                backgroundWorker.CancelAsync();
                Cursor.Current = Cursors.WaitCursor;
                cancelButton.Enabled = false;

                return false;
            }

            return true;
        }

        private void ProgressDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!EndBackgroundWorker())
                e.Cancel = true;
            else
            {
                if (Result.IsCancelled || Result.Exception != null)
                    DialogResult = DialogResult.Cancel;
                else
                    DialogResult = DialogResult.OK;
            }
        }
    }

    /// <summary>
    /// Class to wrap the result of the worker thread in <see cref="ProgressDialog"/>
    /// </summary>
    public class ProgressDialogResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProgressDialogResult"/> class.
        /// </summary>
        /// <param name="value">The result value.</param>
        /// <param name="isCancelled">if set to <c>true</c> [is cancelled].</param>
        /// <param name="ex">The ex.</param>
        internal ProgressDialogResult(object value, bool isCancelled, Exception ex)
        {
            _value = value;
            _isCancelled = isCancelled;
            _exception = ex;
        }

        private object _value;
        /// <summary>
        /// Gets the result value set by the worker thread.
        /// </summary>
        /// <value>The result.</value>
        public object Value
        {
            get { return _value; }
        }

        private bool _isCancelled;
        /// <summary>
        /// Gets a value indicating whether the thread has been cancelled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is cancelled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCancelled
        {
            get { return _isCancelled; }
        }

        private Exception _exception;
        /// <summary>
        /// Gets the exception that has occured.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception
        {
            get { return _exception; }
        }
    }
}