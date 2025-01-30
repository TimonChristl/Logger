using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TC.Logging
{

	/// <summary>
	/// A single log message. Log messages have a severity, text, nesting depth, and can have extra data in the form of an
	/// object.
	/// </summary>
	public sealed class LogMessage
	{

		#region Private fields

		private readonly DateTime timeStamp;
		private readonly Severity severity;
		private readonly string text;
		private readonly int nestingDepth;
#if NET8_0_OR_GREATER
        private readonly object? extraData;
#else
		private readonly object extraData;
#endif

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="LogMessage"/> with the given timeStamp, severity, text, nesting depth and extra data.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="severity"></param>
        /// <param name="text"></param>
        /// <param name="nestingDepth"></param>
        /// <param name="extraData"></param>
#if NET8_0_OR_GREATER
        internal LogMessage(DateTime timeStamp, Severity severity, string text, int nestingDepth, object? extraData = null)
#else
		internal LogMessage(DateTime timeStamp, Severity severity, string text, int nestingDepth, object extraData = null)
#endif
        {
            this.timeStamp = timeStamp;
            this.severity = severity;
            this.text = text;
            this.nestingDepth = nestingDepth;
            this.extraData = extraData;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="LogMessage"/> with the given severity, text, nesting depth and extra data.
        /// </summary>
        /// <param name="severity"></param>
        /// <param name="text"></param>
        /// <param name="nestingDepth"></param>
        /// <param name="extraData"></param>
#if NET8_0_OR_GREATER
        public LogMessage(Severity severity, string text, int nestingDepth, object? extraData = null)
#else
        public LogMessage(Severity severity, string text, int nestingDepth, object extraData = null)
#endif
        {
            timeStamp = DateTime.UtcNow;
            this.severity = severity;
            this.text = text;
            this.nestingDepth = nestingDepth;
            this.extraData = extraData;
        }

		#endregion

		#region Public methods

		/// <inheritdoc/>
		public override string ToString()
		{
			return string.Format("[{0}] [{1}] {2}", timeStamp.ToString("yyyy-MM-dd HH:mm:ss.fff"), severity, text);
		}

        /// <summary>
        /// Returns the extra data converted to a string.
        /// </summary>
        /// <returns></returns>
#if NET8_0_OR_GREATER
        public string? GetExtraDataAsString()
#else
		public string GetExtraDataAsString()
#endif
		{
			if(extraData == null)
				return null;

			TypeConverter typeConverter = TypeDescriptor.GetConverter(extraData);
			if(typeConverter != null)
				return typeConverter.ConvertToInvariantString(extraData);
			else
				return extraData.ToString();
		}

        #endregion

        #region Public properties

        /// <summary>
        /// Timestamp of the log message.
        /// </summary>
        public DateTime TimeStamp
		{
			get { return timeStamp; }
		}

		/// <summary>
		/// Severity of the log message.
		/// </summary>
		public Severity Severity
		{
			get { return severity; }
		}

		/// <summary>
		/// Text of the log message.
		/// </summary>
		public string Text
		{
			get { return text; }
		}

		/// <summary>
		/// Nesting depth of the log message.
		/// </summary>
		public int NestingDepth
		{
			get { return nestingDepth; }
		}

        /// <summary>
        /// User-defined extra data of the log message.
        /// </summary>
#if NET8_0_OR_GREATER
        public object? ExtraData
#else
		public object ExtraData
#endif
        {
            get { return extraData; }
        }

        #endregion

    }

}
