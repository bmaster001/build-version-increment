// ----------------------------------------------------------------------
// Project:     BuildVersionIncrement
// Module Name: OutputWindowAppender.cs
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
	using System.Linq;

	using EnvDTE;

	using EnvDTE80;

	using log4net;
	using log4net.Appender;
	using log4net.Core;

	public class OutputWindowAppender : AppenderSkeleton
	{
		public OutputWindowAppender()
		{
			ServiceProvider = (IServiceProvider)GlobalContext.Properties["package"];
		}

		public IServiceProvider ServiceProvider { get; set; }

		protected override void Append(LoggingEvent loggingEvent)
		{
			var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));

			var panes = dte.ToolWindows.OutputWindow.OutputWindowPanes;
			var errorList = dte.ToolWindows.ErrorList;
			var message = RenderLoggingEvent(loggingEvent);
			foreach (var pane in panes.Cast<OutputWindowPane>().Where(pane => pane.Name.Contains("Build")))
			{
				if (loggingEvent.Level == Level.Debug || loggingEvent.Level == Level.Info)
				{
					pane.OutputString(message);
					pane.Activate();
				}
				else
				{
					var priority = vsTaskPriority.vsTaskPriorityHigh;
					var icon = vsTaskIcon.vsTaskIconCompile;
					if (loggingEvent.Level == Level.Warn)
					{
						priority = vsTaskPriority.vsTaskPriorityMedium;
						icon = vsTaskIcon.vsTaskIconSquiggle;
					}
					pane.OutputTaskItemString(message,
					                          priority,
					                          "BuildVersionIncrement",
					                          icon,
					                          null,
					                          0,
					                          message);
					errorList.Parent.Activate();
				}
				return;
			}
		}
	}
}