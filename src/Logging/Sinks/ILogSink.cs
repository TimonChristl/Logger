using System;

namespace Logging.Sinks
{

	/// <summary>
	/// A log sink consumes <see cref="LogMessage"/> objects.
	/// </summary>
	public interface ILogSink : IDisposable
	{

		/// <summary>
		/// Process log message <paramref name="logMessage"/>.
		/// </summary>
		/// <param name="logMessage"></param>
		void Process(LogMessage logMessage);

	}

}
