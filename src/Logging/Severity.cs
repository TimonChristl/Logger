namespace Logging
{

	/// <summary>
	/// Possible severity levels for log messages. These are numerically identical to the severities specified in RFC5424 (Syslog).
	/// </summary>
	public enum Severity
	{
		/// <summary>
		/// System is unusable
		/// </summary>
		Emergency = 0,

		/// <summary>
		/// Action must be taken immediately
		/// </summary>
		Alert = 1,

		/// <summary>
		/// Critical condition
		/// </summary>
		Critical = 2,

		/// <summary>
		/// Error conditions
		/// </summary>
		Error = 3,

		/// <summary>
		/// warning conditions
		/// </summary>
		Warning = 4,

		/// <summary>
		/// Normal but significant condition
		/// </summary>
		Notice = 5,

		/// <summary>
		/// Informational messages
		/// </summary>
		Info = 6,

		/// <summary>
		/// Debug-level messages
		/// </summary>
		Debug = 7
	}

}
