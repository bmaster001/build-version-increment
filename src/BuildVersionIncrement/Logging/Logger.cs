// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: Logger.cs
// ----------------------------------------------------------------------
// Created and maintained by Paul J. Melia.
// Copyright © 2016 Paul J. Melia.
// All rights reserved.
// ----------------------------------------------------------------------
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT 
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR 
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT 
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, 
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT 
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// ----------------------------------------------------------------------

namespace BuildVersionIncrement.Logging
{
	using System;
	using System.Reflection;
	using System.Text;

	using log4net;
	using log4net.Appender;
	using log4net.Core;
	using log4net.Layout;
	using log4net.Repository.Hierarchy;

	using Properties;

	public class Logger
	{
		private const string LOG_PATTERN = "%utcdate UTC %-6level%logger - %message%newline";

		private static readonly ILog _log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly StringBuilder _contents = new StringBuilder();

		internal static event EventHandler<WriteEventArgs> WriteEvent;

		private Logger(bool isCommandLine)
		{
			var hierarchy = (Hierarchy)LogManager.GetRepository();

			var activityLogAppender = new ActivityLogAppender();
			var outputWindowAppender = new OutputWindowAppender();
			

			var layout = new PatternLayout { ConversionPattern = LOG_PATTERN };
			layout.ActivateOptions();

			activityLogAppender.Layout = layout;
			activityLogAppender.Threshold = Level.Debug;

			activityLogAppender.ActivateOptions();
			hierarchy.Root.AddAppender(activityLogAppender);

			outputWindowAppender.Layout = layout;
			outputWindowAppender.Threshold = Level.Info;

			outputWindowAppender.ActivateOptions();
			hierarchy.Root.AddAppender(outputWindowAppender);

			if (isCommandLine)
			{
				// ReSharper disable once UseObjectOrCollectionInitializer
				var consoleAppender = new ConsoleAppender {Layout = layout};
#if DEBUG
				consoleAppender.Threshold = Level.Debug;
#else
				consoleAppender.Threshold = Level.Info;
#endif
				hierarchy.Root.AddAppender(consoleAppender);
			}
			
			hierarchy.Root.Level = Level.All;
			hierarchy.Configured = true;
			_layout.ActivateOptions();
		}

		public static Logger Instance { get; private set; }

		internal string Contents => _contents.ToString();

		// ReSharper disable once InconsistentNaming
		private static PatternLayout _layout => new PatternLayout {ConversionPattern = LOG_PATTERN};

		public static void Write(string message, LogLevel logLevel = LogLevel.Info)
		{
			var globalLogLevel = Settings.Default.IsVerboseLogEnabled ? LogLevel.Debug : LogLevel.Info;

			if (globalLogLevel > logLevel)
			{
				return;
			}

			WriteEvent?.Invoke(Instance, new WriteEventArgs(message, logLevel));
			Instance._contents.Append(message);

			switch (logLevel)
			{
				case LogLevel.Debug:
					if (_log.IsDebugEnabled)
					{
						_log.Debug(message);
					}
					break;
				case LogLevel.Info:
					if (_log.IsInfoEnabled)
					{
						_log.Info(message);
					}
					break;
				case LogLevel.Warn:
					if (_log.IsWarnEnabled)
					{
						_log.Warn(message);
					}
					break;
				case LogLevel.Error:
					if (_log.IsErrorEnabled)
					{
						_log.Error(message);
					}
					break;
				case LogLevel.Fatal:
					if (_log.IsFatalEnabled)
					{
						_log.Fatal(message);
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
			}
		}

		internal static void Initialise(bool isCommandLine)
		{
			Instance = new Logger(isCommandLine);
		}

		internal class WriteEventArgs : EventArgs
		{
			public WriteEventArgs(string message, LogLevel level)
			{
				Message = message;
				Level = level;
			}

			public LogLevel Level { get; set; }
			public string Message { get; set; }
		}
	}
}