using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TC.Logging.Formatters;

namespace TC.Logging.Sinks
{

	/// <summary>
	/// Abstract base class for text-format implementations of <see cref="ILogSink"/>.
	/// </summary>
	public abstract class BaseTextLogSink : BaseLogSink
	{

		#region Private fields

		private int indentWidth;
		private ITextLogMessageFormatter formatter;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="BaseTextLogSink"/> with the given indent width.
		/// </summary>
		/// <param name="indentWidth"></param>
		/// <param name="formatter"></param>
		public BaseTextLogSink(int indentWidth, ITextLogMessageFormatter formatter = null)
		{
			this.indentWidth = indentWidth;
			this.formatter = formatter ?? new DefaultShortTextLogMessageFormatter();
		}

		#endregion

		#region Protected methods

		/// <summary>
		/// Formats the log message <paramref name="logMessage"/> into a string.
		/// </summary>
		/// <param name="logMessage"></param>
		/// <returns></returns>
		protected string FormatLogMessage(LogMessage logMessage)
		{
			return formatter.Format(logMessage, indentWidth);
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Width of indentation for one nesting level.
		/// </summary>
		public int IndentWidth
		{
			get { return indentWidth; }
		}

		#endregion

	}

}
