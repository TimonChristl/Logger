using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logging.Sources
{

	/// <summary>
	/// Base class for implementations of <see cref="ILogSource"/>.
	/// </summary>
	public abstract class BaseLogSource : ILogSource
	{

		#region ILogSource Members

		/// <inheritdoc/>
		public abstract IEnumerable<LogMessage> GetMessages();

		#endregion

		#region IDisposable implementation

		private bool isDisposed = false;

		/// <summary>
		/// Finalizes the <see cref="BaseLogSource"/> instance.
		/// Calls <see cref="DisposeCore"/> with argument <c>false</c>.
		/// </summary>
		~BaseLogSource()
		{
			if(!isDisposed)
				DisposeCore(false);
		}

		/// <summary>
		/// Disposes the <see cref="BaseLogSource"/> instance.
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
		/// Flag whether this <see cref="BaseLogSource"/> is disposed.
		/// </summary>
		protected bool IsDisposed
		{
			get { return isDisposed; }
		}

		#endregion

	}

}
