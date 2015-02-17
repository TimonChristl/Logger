using System;
using System.Collections.Generic;
using System.Text;
using Logging.Formatters;

namespace Logging.Sinks
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
				this.Background = background;
				this.Foreground = foreground;
			}
		}

		#endregion

		#region Private fields

		private static Dictionary<Severity, ConsoleColorTuple> colorsForSeverities = new Dictionary<Severity, ConsoleColorTuple>()
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

		private ConsoleLogSinkFlags flags;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="ConsoleLogSink"/> instance with the given flags and indent width.
		/// </summary>
		/// <param name="flags"></param>
		/// <param name="indentWidth"></param>
		/// <param name="formatter"></param>
		public ConsoleLogSink(ConsoleLogSinkFlags flags = ConsoleLogSinkFlags.UseColors, int indentWidth = 4, ITextLogMessageFormatter formatter = null)
			: base(indentWidth, formatter ?? new DefaultShortTextLogMessageFormatter())
		{
			this.flags = flags;
		}

		#endregion

		#region Public methods

		/// <inheritdoc/>
		public override void Process(LogMessage logMessage)
		{
			if(IsDisposed)
				throw new ObjectDisposedException("ConsoleLogSink");

			#region Prepare colors

			ConsoleColor oldBackgroundColor = Console.BackgroundColor;
			ConsoleColor oldForegroundColor = Console.ForegroundColor;

			ConsoleColor newBackgroundColor = oldBackgroundColor;
			ConsoleColor newForegroundColor = newBackgroundColor;
			ConsoleColorTuple colors;
			if(colorsForSeverities.TryGetValue(logMessage.Severity, out colors))
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
