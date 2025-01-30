using System;
using System.Diagnostics;
using System.IO;
using TC.Logging.Formatters;

#if NET8_0_OR_GREATER
#pragma warning disable IDE0290
#endif

namespace TC.Logging.Sinks
{

	/// <summary>
	/// An <see cref="ILogSink"/> implementation that writes log messages to <see cref="System.Diagnostics.Trace"/>.
	/// </summary>
	public class TraceLogSink : BaseTextLogSink
	{

        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="TraceLogSink"/> instance.
        /// </summary>
        /// <param name="indentWidth"></param>
        /// <param name="formatter"></param>
#if NET8_0_OR_GREATER
        public TraceLogSink(int indentWidth = 4, ITextLogMessageFormatter? formatter = null)
#else
		public TraceLogSink(int indentWidth = 4, ITextLogMessageFormatter formatter = null)
#endif
            : base(indentWidth, formatter ?? new DefaultTextLogMessageFormatter())
		{
		}

		#endregion

		#region Public methods

		/// <inheritdoc/>
		public override void Process(LogMessage logMessage)
		{
#if NET8_0_OR_GREATER
			ObjectDisposedException.ThrowIf(IsDisposed, this);
#else
			if(IsDisposed)
				throw new ObjectDisposedException("TraceLogSink");
#endif

            Trace.Write(FormatLogMessage(logMessage));
		}

		#endregion

	}

}
