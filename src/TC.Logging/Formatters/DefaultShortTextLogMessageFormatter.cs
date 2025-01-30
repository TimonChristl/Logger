using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.Logging.Formatters
{

	/// <summary>
	/// A default implementation of <see cref="ITextLogMessageFormatter"/> that is similar
	/// to <see cref="DefaultTextLogMessageFormatter"/>, but does not include timestamp and severity name.
	/// </summary>
	public class DefaultShortTextLogMessageFormatter : ITextLogMessageFormatter
	{

		#region ITextLogMessageFormatter Members

		/// <inheritdoc/>
		public string Format(LogMessage logMessage, int indentWidth)
		{
			var sb = new StringBuilder();

			sb.Append(string.Empty.PadLeft(logMessage.NestingDepth * indentWidth, ' '));
			sb.Append(logMessage.Text);
			sb.AppendLine();
#if NET8_0_OR_GREATER
            string? extraDataAsString = logMessage.GetExtraDataAsString();
#else
			string extraDataAsString = logMessage.GetExtraDataAsString();
#endif
            if(extraDataAsString != null)
				sb.AppendLine(extraDataAsString);

			var sb2 = new StringBuilder();

			string indentation = string.Empty.PadRight(logMessage.NestingDepth * indentWidth, ' ');

			string[] lines = sb.ToString().Replace("\r\n", "\n").Split('\n');
			for(int i = 0; i < lines.Length - 1; i++) // -1 because the last line is always an empty one due to the AppendLine in the loop above
			{
				string line = lines[i];

				if(i > 0)
					sb2.Append(indentation);
				sb2.AppendLine(line);
			}

			return sb2.ToString();
		}

		#endregion

	}

}
