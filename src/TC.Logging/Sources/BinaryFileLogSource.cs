using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TC.Logging.Sources
{

	/// <summary>
	/// Implementation of <see cref="ILogSource"/> that reads a binary log file produced by <see cref="TC.Logging.Sinks.BinaryFileLogSink"/> to produce
	/// a stream of log messages.
	/// </summary>
	public class BinaryFileLogSource : BaseLogSource
	{

		#region Private fields

		private string filename;
		private BinaryReader binaryReader;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="TC.Logging.Sources.BinaryFileLogSource"/> with the given filename.
		/// </summary>
		/// <param name="filename"></param>
		public BinaryFileLogSource(string filename)
		{
			this.filename = filename;

			FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

			binaryReader = new BinaryReader(stream);
		}

		#endregion

		#region Public methods

		/// <inheritdoc/>
		public override IEnumerable<LogMessage> GetMessages()
		{
			while(true)
			{
				if(IsDisposed)
					throw new ObjectDisposedException("BinaryFileLogSource");

				if(binaryReader.BaseStream.Position >= binaryReader.BaseStream.Length)
					yield break;

				byte logMessageVersion = binaryReader.ReadByte();
				switch(logMessageVersion)
				{
				case 1:
					{
						DateTime timeStamp = DateTime.FromBinary(binaryReader.ReadInt64());
						Severity severity = (Severity)binaryReader.ReadByte();
						string text = binaryReader.ReadString();
						int nestingDepth = binaryReader.ReadInt32();
						string extraData = binaryReader.ReadString();

						yield return new LogMessage(timeStamp, severity, text, nestingDepth, extraData);
					}
					break;
				default:
					throw new UnknownBinaryLogMessageVersionException();
				}
			}
		}

		#endregion

		#region Protected methods

		/// <inheritdoc/>
		protected override void Dispose(bool disposing)
		{
			if(disposing)
				binaryReader.Close();

			base.Dispose(disposing);
		}

		#endregion

	}

	/// <summary>
	/// Indicates that a log message has an unknown version while reading a binary log file using <see cref="TC.Logging.Sources.BinaryFileLogSource"/>.
	/// </summary>
	[Serializable]
	public class UnknownBinaryLogMessageVersionException : Exception
	{
		/// <inheritdoc/>
		public UnknownBinaryLogMessageVersionException() { }

		/// <inheritdoc/>
		public UnknownBinaryLogMessageVersionException(string message) : base(message) { }

		/// <inheritdoc/>
		public UnknownBinaryLogMessageVersionException(string message, Exception inner) : base(message, inner) { }

		/// <inheritdoc/>
		protected UnknownBinaryLogMessageVersionException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}

}
