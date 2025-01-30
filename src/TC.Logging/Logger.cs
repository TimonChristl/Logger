using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using TC.Logging.Sinks;

namespace TC.Logging
{

	/// <summary>
	/// Main class for to a simple yet powerful logging system.
	/// </summary>
	public sealed class Logger : IDisposable
	{

        #region Private fields

#if NET8_0_OR_GREATER
        private readonly object lockObj = new();
        private readonly List<ILogSink> logSinks = [];
#else
		private readonly object lockObj = new object();
		private readonly List<ILogSink> logSinks = new List<ILogSink>();
#endif

        private Severity thresholdLogSeverity = Severity.Debug;
		private bool deferred = false;

#if NET8_0_OR_GREATER
        private readonly ConcurrentDictionary<int, int> nestingLevelsPerThread = [];
		private readonly Queue<LogMessage> pendingLogMessages = [];
#else
		private readonly ConcurrentDictionary<int, int> nestingLevelsPerThread = new ConcurrentDictionary<int, int>();
        private readonly Queue<LogMessage> pendingLogMessages = new Queue<LogMessage>();
#endif

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the logger and attaches any supplied log sinks.
        /// </summary>
        /// <param name="logSinks"></param>
        public Logger(params ILogSink[] logSinks)
		{
			this.logSinks.AddRange(logSinks);
		}

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a new log message with severity <see cref="Severity.Info"/>, text <paramref name="text"/>,
        /// and extra data <paramref name="extraData"/>, if any.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="extraData"></param>
#if NET8_0_OR_GREATER
        public void Log(string text, object? extraData = null)
#else
		public void Log(string text, object extraData = null)
#endif
        {
#if NET8_0_OR_GREATER
			ObjectDisposedException.ThrowIf(isDisposed, this);
#else
			if(isDisposed)
				throw new ObjectDisposedException("Logger");
#endif

            if(!nestingLevelsPerThread.TryGetValue(Environment.CurrentManagedThreadId, out int nestingLevel))
                nestingLevel = 0;

            ProcessMessage(new LogMessage(Severity.Info, text, nestingLevel, extraData));
		}

        /// <summary>
        /// Adds a new log message with severity <paramref name="severity"/>, text <paramref name="text"/>,
        /// and extra data <paramref name="extraData"/>, if any.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="text"></param>
        /// <param name="extraData"></param>
#if NET8_0_OR_GREATER
        public void Log(Severity severity, string text, object? extraData = null)
#else
		public void Log(Severity severity, string text, object extraData = null)
#endif
        {
#if NET8_0_OR_GREATER
			ObjectDisposedException.ThrowIf(isDisposed, this);
#else
			if(isDisposed)
				throw new ObjectDisposedException("Logger");
#endif

            if(!nestingLevelsPerThread.TryGetValue(Environment.CurrentManagedThreadId, out int nestingLevel))
                nestingLevel = 0;

            ProcessMessage(new LogMessage(severity, text, nestingLevel, extraData));
		}

		/// <summary>
		/// Adds a new log message with severity <see cref="Severity.Error"/> and text and extra data
		/// based on the exception <paramref name="exception"/>. 
		/// </summary>
		/// <remarks>
		/// Inner exceptions are logged by recursive calls to this method at appropriate nesting levels.
		/// </remarks>
		/// <param name="exception"></param>
		public void Log(Exception exception)
		{
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(isDisposed, this);
#else
			if(isDisposed)
				throw new ObjectDisposedException("Logger");
#endif

            if(!nestingLevelsPerThread.TryGetValue(Environment.CurrentManagedThreadId, out int nestingLevel))
                nestingLevel = 0;

            ProcessMessage(new LogMessage(Severity.Error, string.Format("{0}: {1}", exception.GetType().FullName, exception.Message), nestingLevel, exception.StackTrace));

			if(exception is AggregateException aggregateException)
			{
				BeginBlock();
				for(int i = 0; i < aggregateException.InnerExceptions.Count; i++)
				{
					Exception innerException = aggregateException.InnerExceptions[i];
					Log(innerException);
				}
				EndBlock();
			}
			else if(exception.InnerException != null)
			{
				BeginBlock();
				Log(exception.InnerException);
				EndBlock();
			}
		}

		/// <summary>
		/// Adds a new log message. Note that the nesting level of the log message will be overridden with the current nesting level
		/// for the current thread. This method is ideal for feeding log messages from a different logging system to this one,
		/// or to replay a stream of log messages from an <see cref="TC.Logging.Sources.ILogSource"/>.
		/// </summary>
		/// <param name="logMessage"></param>
		public void Log(LogMessage logMessage)
		{
#if NET8_0_OR_GREATER
            ObjectDisposedException.ThrowIf(isDisposed, this);
#else
			if(isDisposed)
				throw new ObjectDisposedException("Logger");
#endif

            if(!nestingLevelsPerThread.TryGetValue(Environment.CurrentManagedThreadId, out int nestingLevel))
                nestingLevel = 0;

            ProcessMessage(new LogMessage(logMessage.Severity, logMessage.Text, nestingLevel, logMessage.ExtraData));
		}

		/// <summary>
		/// Increases the nesting level for log messages in the current thread by one.
		/// </summary>
		public void BeginBlock()
		{
			nestingLevelsPerThread.AddOrUpdate(Environment.CurrentManagedThreadId, 1, (k, v) => v + 1);
		}

		/// <summary>
		/// Decreases the nesting level for log messages in the current thread by one.
		/// </summary>
		public void EndBlock()
		{
			nestingLevelsPerThread.AddOrUpdate(Environment.CurrentManagedThreadId, 0, (k, v) => Math.Max(0, v - 1));
		}

		/// <summary>
		/// Adds a new log message with severity <paramref name="severity"/>, text <paramref name="text"/>,
		/// and as extra data a string with the hex dump of a range of byte array <paramref name="data"/>, starting at
		/// <paramref name="offset"/>, with length <paramref name="length"/>.
		/// If <paramref name="useUnicodeChars"/> is <c>true</c>, the frame around the dump is created using Unicode line drawing characters,
		/// otherwise ASCII characters are used.
		/// <paramref name="bytesPerLine"/> bytes are shown per line.
		/// </summary>
		/// <param name="severity"></param>
		/// <param name="text"></param>
		/// <param name="data"></param>
		/// <param name="offset"></param>
		/// <param name="length"></param>
		/// <param name="useUnicodeChars"></param>
		/// <param name="bytesPerLine"></param>
		public void HexDump(Severity severity, string text, byte[] data, int offset = 0, int? length = null, bool useUnicodeChars = false, int bytesPerLine = 16)
		{
			int length2 = length.GetValueOrDefault(data.Length);

			var sb = new StringBuilder();
			var charactersSB = new StringBuilder();

			string lineDrawingCharacters = useUnicodeChars
				? "┌─┬┐│├┼┤└┴┘"
				: "+-++|++++++";

			int maxOffset = offset + length2;
			int offsetWidth = Math.Max(4, (int)Math.Ceiling(Math.Log(maxOffset, 16)));

			int bytesWidth = bytesPerLine * 3 - 1;
			int charactersWidth = bytesPerLine;

			// Upper frame
			sb.Append(lineDrawingCharacters[0]);
			sb.Append(string.Empty.PadRight(offsetWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[2]);
			sb.Append(string.Empty.PadRight(bytesWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[2]);
			sb.Append(string.Empty.PadRight(charactersWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[3]);
			sb.AppendLine();

			// Header line
			sb.Append(lineDrawingCharacters[4]);
#if NET8_0_OR_GREATER
            sb.Append("Offset".PadRight(offsetWidth, ' ').AsSpan(0, offsetWidth));
#else
			sb.Append("Offset".PadRight(offsetWidth, ' ').Substring(0, offsetWidth));
#endif
            sb.Append(lineDrawingCharacters[4]);
			for(int i = 0; i < bytesPerLine; i++)
			{
				if(i > 0)
					sb.Append(' ');
                if(i < data.Length)
				    sb.AppendFormat("{0,-2:x2}", data[i]);
                else
                    sb.Append("  ");
            }
			sb.Append(lineDrawingCharacters[4]);
#if NET8_0_OR_GREATER
            sb.Append("Characters".PadRight(charactersWidth, ' ').AsSpan(0, charactersWidth));
#else
			sb.Append("Characters".PadRight(charactersWidth, ' ').Substring(0, charactersWidth));
#endif
            sb.Append(lineDrawingCharacters[4]);
			sb.AppendLine();

			// Frame between header and body
			sb.Append(lineDrawingCharacters[5]);
			sb.Append(string.Empty.PadRight(offsetWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[6]);
			sb.Append(string.Empty.PadRight(bytesWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[6]);
			sb.Append(string.Empty.PadRight(charactersWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[7]);
			sb.AppendLine();

			// Body lines
			for(int i = 0; i < length2; i++)
			{
				int addr = offset + i;

				bool isBoL = addr % bytesPerLine == 0;
				bool isEoL = addr % bytesPerLine == bytesPerLine - 1;
				bool isBoB = i == 0;
				bool isEoB = i == length2 - 1;

				// Address

				if(isBoB || isBoL)
					sb.AppendFormat(string.Format("|{{0,-{0}:x{0}}}|", offsetWidth), addr);

				// Leading blanks if necessary

				if(isBoB && !isBoL)
				{
					for(int j = 0; j < (addr % bytesPerLine); j++)
					{
						if(j > 0)
							sb.Append(' ');
						sb.Append("··");
						charactersSB.Append(' ');
					}
				}

				if(!isBoL)
					sb.Append(' ');

				// Byte

				sb.AppendFormat("{0,-2:x2}", data[i]);
				char c = (char)data[i];
				//charactersSB.Append(c >= ' ' && c < 127 ? c : ' ');
				charactersSB.Append(char.IsControl(c) ? ' ' : c);

				// Trailing blanks if necessary

				if(isEoB && !isEoL)
				{
					for(int j = (addr % bytesPerLine) + 1; j < bytesPerLine; j++)
					{
						sb.Append(" ··");
						charactersSB.Append(' ');
					}
				}

				// Line end

				if(isEoL || isEoB)
				{
					sb.Append(lineDrawingCharacters[4]);
					sb.Append(charactersSB);
					sb.Append(lineDrawingCharacters[4]);
					sb.AppendLine();

					charactersSB = new StringBuilder();
				}
			}

			// Lower frame
			sb.Append(lineDrawingCharacters[8]);
			sb.Append(string.Empty.PadRight(offsetWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[9]);
			sb.Append(string.Empty.PadRight(bytesWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[9]);
			sb.Append(string.Empty.PadRight(charactersWidth, lineDrawingCharacters[1]));
			sb.Append(lineDrawingCharacters[10]);
			sb.AppendLine();

			Log(severity, text, sb.ToString());
		}

		#endregion

		#region Private methods

		private void ProcessMessage(LogMessage logMessage)
		{
			if(deferred)
				pendingLogMessages.Enqueue(logMessage);
			else
				lock(lockObj)
				{
					ProcessMessageCore(logMessage);
				}
		}

		private void ProcessMessageCore(LogMessage logMessage)
		{
			if(logMessage.Severity <= thresholdLogSeverity)
				foreach(ILogSink logSink in logSinks)
					logSink.Process(logMessage);
		}

		private void FlushPendingLogMessages()
		{
			lock(lockObj)
			{
				while(pendingLogMessages.Count > 0)
				{
					LogMessage logMessage = pendingLogMessages.Dequeue();
					ProcessMessageCore(logMessage);
				}
			}
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Provides access to the list of log sinks attached to the logger.
		/// </summary>
		public IList<ILogSink> LogSinks
		{
			get { return logSinks; }
		}

		/// <summary>
		/// Gets or sets deferred mode. In deferred mode, log messages are queued up until deferred mode is
		/// disabled, at which time they are flushed. This can be useful during application initialization.
		/// The logger starts with deferred mode deactivated.
		/// </summary>
		public bool Deferred
		{
			get { return deferred; }
			set
			{
#if NET8_0_OR_GREATER
                ObjectDisposedException.ThrowIf(isDisposed, this);
#else
				if(isDisposed)
					throw new ObjectDisposedException("Logger");
#endif

                if(deferred != value)
				{
					deferred = value;
					if(deferred == false)
						FlushPendingLogMessages();
				}
			}
		}

		/// <summary>
		/// Maximum severity to process. Log messages whose severity is above this severity are discarded.
		/// </summary>
		public Severity ThresholdSeverity
		{
			get { return thresholdLogSeverity; }
			set { thresholdLogSeverity = value; }
		}

		#endregion

		#region IDisposable implementation

		private bool isDisposed = false;

		/// <summary>
		/// Finalizes the logger.
		/// </summary>
		~Logger()
		{
			if(!isDisposed)
				Dispose(false);
		}

		/// <summary>
		/// Disposes all log sinks attached to the logger.
		/// </summary>
		public void Dispose()
		{
			if(!isDisposed)
			{
				Dispose(true);
				isDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				foreach(ILogSink logSink in logSinks)
					logSink.Dispose();
			}
		}

		#endregion

	}

}
