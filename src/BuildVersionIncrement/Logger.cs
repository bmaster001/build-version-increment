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

namespace BuildVersionIncrement
{
	using System;
	using System.Reflection;
	using System.Text;

	using log4net;

	public class Logger
	{
		private static readonly ILog _log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly StringBuilder _contents = new StringBuilder();

		internal static event EventHandler<WriteEventArgs> WriteEvent;

		internal Logger()
		{
			Instance = this;
		}

		public static Logger Instance { get; private set; }

		internal string Contents => _contents.ToString();

		public static void Write(string message, LogLevel logLevel)
		{
			var globalLogLevel = BuildVersionIncrementSettings.Default.IsVerboseLogEnabled
				                     ? LogLevel.Debug
				                     : LogLevel.Info;

			if (globalLogLevel > logLevel)
			{
				return;
			}

			WriteEvent?.Invoke(Instance, new WriteEventArgs(message, logLevel));

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