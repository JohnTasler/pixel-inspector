namespace ZoomIn.Utility
{
	using System;

	public class DisposeActionScope : IDisposable
	{
		#region Instance Fields
		private Action disposeAction;
		#endregion Instance Fields

		#region Constructors / Finalizer
		public DisposeActionScope(Action initializeAction, Action disposeAction)
			: this(disposeAction)
		{
			if (initializeAction == null)
				throw new ArgumentNullException("initializeAction");

			initializeAction();
		}

		public DisposeActionScope(Action disposeAction)
		{
			if (disposeAction == null)
				throw new ArgumentNullException("disposeAction");

			this.disposeAction = disposeAction;
		}

		~DisposeActionScope()
		{
			this.Dispose();
		}
		#endregion Constructors / Finalizer

		#region IDisposable Members
		public void Dispose()
		{
			if (this.disposeAction != null)
			{
				var action = this.disposeAction;
				this.disposeAction = null;
				action();
			}

			GC.SuppressFinalize(this);
		}
		#endregion IDisposable Members
	}
}
