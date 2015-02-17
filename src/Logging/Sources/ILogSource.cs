using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logging.Sources
{

	/// <summary>
	///  A log source produces <see cref="LogMessage"/> objects. Normally log messages are produced in real time by the
	///  <see cref="Logger"/> class, but log sources can be used to later replay a stream of log messages for analyzing them,
	/// or for feeding them to a different type of log sink (for example to convert a binary log file to a text file log).
	/// </summary>
	public interface ILogSource : IDisposable
	{

		/// <summary>
		/// Returns a sequence of log messages.
		/// </summary>
		/// <returns></returns>
		IEnumerable<LogMessage> GetMessages();

	}

}
