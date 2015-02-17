using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logging.Formatters
{

	/// <summary>
	/// 
	/// </summary>
	public interface ITextLogMessageFormatter
	{

		/// <summary>
		/// Formats the log message <paramref name="logMessage"/> into a string.
		/// </summary>
		/// <param name="logMessage"></param>
		/// <param name="indentWidth"></param>
		/// <returns></returns>
		string Format(LogMessage logMessage, int indentWidth);

	}

}
