namespace PixelInspector.ViewModel
{
	using System;
	using System.Windows;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using Tasler.ComponentModel;
	using PixelInspector.Interop.User;
	using PixelInspector.Utility;

	public class SelectToolViewModel
		: ChildViewModel<MainViewModel>
		, IToolMode
	{
		#region Instance Fields
		private Point _dragStart;
		private Rect _dragRect = Rect.Empty;
		private double _lastHorizontalChange;
		private double _lastVerticalChange;
		#endregion Instance Fields

		#region Constructors
		public SelectToolViewModel(MainViewModel parent)
			: base(parent)
		{
		}
		#endregion Constructors

		#region Commands

		#region DragStartedCommand
		public ICommand DragStartedCommand
		{
			get
			{
				return this._dragStartedCommand ??
					(this._dragStartedCommand = new RelayCommand<DragStartedEventArgs>(this.DragStarted));
			}
		}
		private RelayCommand<DragStartedEventArgs> _dragStartedCommand;

		private void DragStarted(DragStartedEventArgs e)
		{
			// Get the point where the mouse was clicked
			this._dragStart = this.Parent.ZoomedMousePosition;

			// Get the drag rect
			this._dragRect = new Rect(this._dragStart.X, this._dragStart.Y, 0, 0);
			this._dragRect.Inflate(
				Math.Abs(UserApi.GetSystemMetrics(SM.CxDrag)),
				Math.Abs(UserApi.GetSystemMetrics(SM.CyDrag)));

			this._lastHorizontalChange = this._lastVerticalChange = 0;
			this.Parent.Selection.UpdateZoomedRectangleActualFromInput(null);
			e.Handled = true;
		}
		#endregion DragStartedCommand

		#region DragDeltaCommand
		public ICommand DragDeltaCommand
		{
			get
			{
				return this._dragDeltaCommand ??
					(this._dragDeltaCommand = new RelayCommand<DragDeltaEventArgs>(this.DragDelta));
			}
		}
		private RelayCommand<DragDeltaEventArgs> _dragDeltaCommand;

		private void DragDelta(DragDeltaEventArgs e)
		{
			// Calculate the change
			var horizontalChange = e.HorizontalChange - this._lastHorizontalChange;
			var verticalChange = e.VerticalChange - this._lastVerticalChange;
			this._lastHorizontalChange = e.HorizontalChange;
			this._lastVerticalChange = e.VerticalChange;

			// Check to see if the mosue has moved outside of the drag rect
			if (!this._dragRect.IsEmpty && this._dragRect.Contains(this.Parent.ZoomedMousePosition))
				this._dragRect = Rect.Empty;

			// Only process if the mouse has moved outside of the drag rect
			if (this._dragRect.IsEmpty)
			{
				var rect = new ExtentRect(this._dragStart, this.Parent.ZoomedMousePosition);
				this.Parent.Selection.UpdateZoomedRectangleActualFromInput(rect);
			}

			e.Handled = true;
		}
		#endregion DragDeltaCommand

		#region DragCompletedCommand
		public ICommand DragCompletedCommand
		{
			get
			{
				return this._dragCompletedCommand ??
					(this._dragCompletedCommand = new RelayCommand<DragCompletedEventArgs>(this.DragCompleted));
			}
		}
		private RelayCommand<DragCompletedEventArgs> _dragCompletedCommand;

		private void DragCompleted(DragCompletedEventArgs e)
		{
			this.ExitMode(e.Canceled);
			e.Handled = true;
		}
		#endregion DragCompletedCommand

		#endregion Commands

		#region IToolMode Members

		/// <summary>
		/// Called before the mode is entered.
		/// </summary>
		public void EnterMode()
		{
		}

		/// <summary>
		/// Called to exit the mode.
		/// </summary>
		/// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
		/// otherwise, it should commit its changes.</param>
		public void ExitMode(bool isReverting)
		{
		}

		#endregion IToolMode Members
	}
}
