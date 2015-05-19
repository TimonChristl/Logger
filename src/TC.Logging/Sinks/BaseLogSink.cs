using System;

namespace TC.Logging.Sinks
{

	/// <summary>
	/// Abstract base class for implementing <see cref="ILogSink"/>.
	/// </summary>
	public abstract class BaseLogSink : ILogSink
	{

		#region Public methods

		/// <summary>
		/// Process the log message <paramref name="logMessage"/>.
		/// </summary>
		/// <param name="logMessage"></param>
		public abstract void Process(LogMessage logMessage);

		#endregion

		#region IDisposable implementation

		private bool isDisposed = false;

		/// <summary>
		/// Finalizes the <see cref="BaseLogSink"/> instance.
		/// Calls <see cref="DisposeCore"/> with argument <c>false</c>.
		/// </summary>
		~BaseLogSink()
		{
			if(!isDisposed)
				DisposeCore(false);
		}

		/// <summary>
		/// Disposes the <see cref="BaseLogSink"/> instance.
		/// Calls <see cref="DisposeCore"/> with argument <c>true</c>.
		/// </summary>
		public void Dispose()
		{
			if(!isDisposed)
			{
				DisposeCore(true);
				isDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		/// <summary>
		/// Allows derived classes to dispose their members, if needed.
		/// The argument <paramref name="disposing"/> tells whether the method
		/// was called as part of a call to <see cref="Dispose"/> or
		/// by the finalizer.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void DisposeCore(bool disposing)
		{
		}

		/// <summary>
		/// Flag whether this <see cref="BaseLogSink"/> is disposed.
		/// </summary>
		protected bool IsDisposed
		{
			get { return isDisposed; }
		}

		#endregion

	}

}
