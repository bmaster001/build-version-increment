namespace BuildVersionIncrement.Logging
{
	using System;

	using log4net;
	using log4net.Appender;
	using log4net.Core;

	using Microsoft.VisualStudio.Shell.Interop;

	public class ActivityLogAppender : AppenderSkeleton
	{
		public ActivityLogAppender()
		{
			ServiceProvider = (IServiceProvider)GlobalContext.Properties["package"];
		}

		public IServiceProvider ServiceProvider { get; set; }


		protected override void Append(LoggingEvent loggingEvent)
		{
			Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
			var log = ServiceProvider.GetService(typeof(IVsActivityLog)) as IVsActivityLog; 

			if (log == null)
			{
				return;
			}

			var entryType = (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_INFORMATION;
			if (loggingEvent.Level == Level.Warn)
			{
				entryType = (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_WARNING;
			} else if (loggingEvent.Level == Level.Error)
			{
				entryType = (UInt32)__ACTIVITYLOG_ENTRYTYPE.ALE_ERROR;
			}

			var message = RenderLoggingEvent(loggingEvent);
			log.LogEntry(entryType, message, $"Called for: {message}");
		}
	}
}