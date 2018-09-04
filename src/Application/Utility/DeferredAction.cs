namespace ZoomIn.Utility
{
	using System;
	using System.Diagnostics;
	using System.Windows.Threading;

	/// <summary>
	/// Manages a timer that, when expired, will trigger an <see cref="Action"/> to be executed.
	/// </summary>
	/// <remarks>
	/// <para>This class helps to coalesce multiple similar actions within a timeframe into a single action. The
	/// classic example is persisting a window's size/position when the user changes it. Rather than persisting the
	/// values each time one changes, the change events can simply trigger an instance of <see cref="DeferredAction"/>.
	/// The interval timer is reset each time the <see cref="Trigger"/> method is called.</para>
	/// <para>When creating an instance of this class, you must specify a <see cref="TimeSpan"/> and an
	/// <see cref="Action"/>. To trigger the action for deffered execution, you call the <see cref="Trigger"/> method.
	/// When an instance of this class is finalized, if a triggered action has not executed, it will be done so at this
	/// time. However, it is better to force this to happen at a more appropriate point in the application. Usually the
	/// best place to do so is when the object that created the <see cref="DeferredAction"/> is leaving scope.
	/// Referring again to the example of persisting the window size, the appropriate place would be when the window is
	/// closed. To force a triggered action to execute immediately, call the <see cref="Expire"/> method.
	/// </remarks>
	public class DeferredAction : IDisposable
	{
		#region Instance Fields
		private TimeSpan interval;
		private Action action;
		private DispatcherTimer timer;
		#endregion Instance Fields

		#region Constructors / Finalizer
		/// <summary>
		/// Initializes a new instance of the <see cref="DeferredAction"/> class.
		/// </summary>
		/// <param name="timeSpan">The time span.</param>
		/// <param name="action">The action to execute after each expiration of the timer <see cref="Interval"/>.</param>
		public DeferredAction(TimeSpan timeSpan, Action action)
		{
			if (action == null)
				throw new ArgumentNullException("action");

			this.interval = timeSpan;
			this.action = action;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="DeferredAction"/> is reclaimed by garbage collection.
		/// </summary>
		~DeferredAction()
		{
			this.Dispose();
		}
		#endregion Constructors / Finalizer

		#region Properties
		public TimeSpan Interval
		{
			get { return this.interval; }
		}
		#endregion Properties

		#region Methods
		/// <summary>
		/// Starts or restarts the timer interval.
		/// </summary>
		public void Trigger()
		{
			// Stop any pending timer
			if (this.timer != null)
			{
				this.timer.Stop();
			}
			else
			{
				// Create a timer to defer the action
				this.timer = new DispatcherTimer() { Interval = this.interval };

				// Subscribe to the Tick event
				this.timer.Tick += this.timer_Tick;
			}

			// Start the timer
			this.timer.Start();
		}

		/// <summary>
		/// Immediately expires the timer interval, forcing the action to execute.
		/// </summary>
		/// <remarks>
		/// This method does nothing if called when no deferral interval has been triggered.
		/// </remarks>
		public void Expire()
		{
			if (this.timer != null)
				this.timer_Tick(this.timer, EventArgs.Empty);
		}

		#endregion Methods

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Expire();
			this.action = null;
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Members

		#region Private Implementation
		private void ClearTimer()
		{
			// Stop any pending update timer
			if (this.timer != null)
			{
				this.timer.Stop();
				this.timer.Tick -= this.timer_Tick;
				this.timer = null;
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			// Clear the update timer
			this.ClearTimer();

			// Perform the action
			if (this.action != null)
			{
				try
				{
					this.action();
				}
				catch (Exception ex)
				{
					Debug.Fail("DeferredAction.timer_Tick: execution of action threw an exception", ex.Message);
				}
			}
		}
		#endregion Private Implementation
	}
}
