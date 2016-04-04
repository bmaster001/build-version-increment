using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;

namespace BuildVersionIncrement
{
    /// <summary>
    /// Specifies the level of log information.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Debug level.
        /// </summary>
        Debug = 0,
        /// <summary>
        /// Information level.
        /// </summary>
        Info,
        /// <summary>
        /// Warning level.
        /// </summary>
        Warning,
        /// <summary>
        /// Error level.
        /// </summary>
        Error
    }

    /// <summary>
    /// The logger class
    /// </summary>
    public class Logger
    {
        internal class WriteEventArgs : EventArgs
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WriteEventArgs"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="logLevel">The log level.</param>
            public WriteEventArgs(string message, LogLevel logLevel)
            {
                Message = message;
                LogLevel = logLevel;
            }

            /// <summary>
            /// The message of the log entry
            /// </summary>
            public string Message;

            /// <summary>
            /// The log level
            /// </summary>
            public LogLevel LogLevel;
        }

        internal static event EventHandler<WriteEventArgs> WriteEvent;

        private StringBuilder _contents = new StringBuilder();
        /// <summary>
        /// Gets the contents.
        /// </summary>
        /// <value>The contents.</value>
        internal string Contents
        {
            get
            {
                return _contents.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="connect">The connect.</param>
        internal Logger(Connect connect)
        {
            _connect = connect;
            _instance = this;
        }

        private Connect _connect;
        
        private static Logger _instance;
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Logger Instance
        {
            get { return _instance; }
        }

        /// <summary>
        // Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        public static void Write(string message, LogLevel logLevel)
        {
            Write(message, logLevel, null, 0);
        }

        /// <summary>
        /// Logs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="line">The line number.</param>
        public static void Write(string message, LogLevel logLevel, string filename, int line)
        {
			LogLevel globalLogLevel = GlobalAddinSettings.Default.IsVerboseLogEnabled
				? LogLevel.Debug : LogLevel.Info;

			if (globalLogLevel > logLevel)
                return;

            object[] args = new object[]{logLevel, message};

            string fmsg = string.Format("[{0}] {1}\r\n", args);

            Instance._contents.AppendFormat(fmsg);

            if (WriteEvent != null)
                WriteEvent(Instance, new WriteEventArgs(fmsg, logLevel));

            vsTaskPriority priority;
            vsTaskIcon icon;
            const string category = "BuildVersionIncrement";

            switch (logLevel)
            {
                case LogLevel.Error:
                    priority = vsTaskPriority.vsTaskPriorityHigh;
                    icon = vsTaskIcon.vsTaskIconCompile;
                    break;

                case LogLevel.Warning:
                    priority = vsTaskPriority.vsTaskPriorityMedium;
                    icon = vsTaskIcon.vsTaskIconSquiggle;
                    break;

                default:
                    string output = string.Format("{0}: {1}\n", category, message);

                    if (!Instance._connect.IsCommandLineBuild && Instance._connect.OutputBuildWindow != null)
                        Instance._connect.OutputBuildWindow.OutputString(output);
                    else
                        Console.Write(output);

                    System.Diagnostics.Debug.WriteLine(message);

                    return;
            }

            if (!Instance._connect.IsCommandLineBuild && Instance._connect.OutputBuildWindow != null)
            {
                Instance._connect.OutputBuildWindow.OutputTaskItemString(message, priority, category, icon,
                                                       filename, line, message, true);
                Instance._connect.ApplicationObject.ToolWindows.ErrorList.Parent.Activate();
            }
            else
            {
                Console.WriteLine("Error: " + message);

                // Not sure what to do here ... I'm unable to find a way to add an error to the build progress and
                // if we don't exit here the build will continue like nothing ever happened.

                Environment.Exit(1);
            }
        }
    }
}
