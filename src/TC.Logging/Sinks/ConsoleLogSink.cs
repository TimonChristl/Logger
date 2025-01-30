using System;
using System.Collections.Generic;
using System.Text;
using TC.Logging.Formatters;

#if NET8_0_OR_GREATER
#pragma warning disable IDE0290
#endif

namespace TC.Logging.Sinks
{

	/// <summary>
	/// Flags to set options on a <see cref="ConsoleLogSink"/>.
	/// </summary>
	[Flags]
	public enum ConsoleLogSinkFlags
	{
		/// <summary>
		/// No option.
		/// </summary>
		None = 0,

		/// <summary>
		/// Enables use of console colors.
		/// </summary>
		UseColors = 1,
	}

	/// <summary>
	/// An <see cref="ILogSink"/> implementation that writes log messages to <see cref="System.Console"/>.
	/// </summary>
	public class ConsoleLogSink : BaseTextLogSink
	{

		#region Nested types

		private class ConsoleColorTuple
		{
			public readonly ConsoleColor Foreground;
			public readonly ConsoleColor Background;

			public ConsoleColorTuple(ConsoleColor background, ConsoleColor foreground)
			{
				Background = background;
				Foreground = foreground;
			}
		}

        #endregion

        #region Private fields

#if NET8_0_OR_GREATER
        private static readonly Dictionary<Severity, ConsoleColorTuple> colorsForSeverities = new()
#else
		private static readonly Dictionary<Severity, ConsoleColorTuple> colorsForSeverities = new Dictionary<Severity, ConsoleColorTuple>()
#endif
		{
            { Severity.Emergency, new ConsoleColorTuple(ConsoleColor.Red, ConsoleColor.White) },
			{ Severity.Alert, new ConsoleColorTuple(ConsoleColor.DarkRed, ConsoleColor.Yellow) },
			{ Severity.Critical, new ConsoleColorTuple(ConsoleColor.Black, ConsoleColor.Red) },
			{ Severity.Error, new ConsoleColorTuple(ConsoleColor.Black, ConsoleColor.DarkRed) },
			{ Severity.Warning, new ConsoleColorTuple(ConsoleColor.Black, ConsoleColor.Yellow) },
			{ Severity.Notice, new ConsoleColorTuple(ConsoleColor.Black, ConsoleColor.Cyan) },
			{ Severity.Info, new ConsoleColorTuple(ConsoleColor.Black, ConsoleColor.Gray) },
			{ Severity.Debug, new ConsoleColorTuple(ConsoleColor.Black, ConsoleColor.DarkGray) },
		};

		private readonly ConsoleLogSinkFlags flags;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new <see cref="ConsoleLogSink"/> instance with the given flags and indent width.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="indentWidth"></param>
        /// <param name="formatter"></param>
#if NET8_0_OR_GREATER
        public ConsoleLogSink(ConsoleLogSinkFlags flags = ConsoleLogSinkFlags.UseColors, int indentWidth = 4, ITextLogMessageFormatter? formatter = null)
#else
		public ConsoleLogSink(ConsoleLogSinkFlags flags = ConsoleLogSinkFlags.UseColors, int indentWidth = 4, ITextLogMessageFormatter formatter = null)
#endif
            : base(indentWidth, formatter ?? new DefaultShortTextLogMessageFormatter())
		{
			this.flags = flags;
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
				throw new ObjectDisposedException("ConsoleLogSink");
#endif

            #region Prepare colors

            ConsoleColor oldBackgroundColor = Console.BackgroundColor;
			ConsoleColor oldForegroundColor = Console.ForegroundColor;

			ConsoleColor newBackgroundColor = oldBackgroundColor;
			ConsoleColor newForegroundColor = newBackgroundColor;
#if NET8_0_OR_GREATER
            if(colorsForSeverities.TryGetValue(logMessage.Severity, out ConsoleColorTuple? colors))
#else
			if(colorsForSeverities.TryGetValue(logMessage.Severity, out ConsoleColorTuple colors))
#endif
			{
                newBackgroundColor = colors.Background;
                newForegroundColor = colors.Foreground;
            }

            #endregion

            #region Output

            if((flags & ConsoleLogSinkFlags.UseColors) == ConsoleLogSinkFlags.UseColors)
			{
				Console.BackgroundColor = newBackgroundColor;
				Console.ForegroundColor = newForegroundColor;
			}

			Console.Write(FormatLogMessage(logMessage));

			if((flags & ConsoleLogSinkFlags.UseColors) == ConsoleLogSinkFlags.UseColors)
			{
				Console.BackgroundColor = oldBackgroundColor;
				Console.ForegroundColor = oldForegroundColor;
			}

			#endregion
		}

		#endregion

	}

}
