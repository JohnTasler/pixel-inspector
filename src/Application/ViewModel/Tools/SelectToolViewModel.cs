namespace ZoomIn.ViewModel
{
	using System;
	using System.Windows;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Interop.User;
	using ZoomIn.Utility;

	public class SelectToolViewModel
		: ParentedObservableObject<MainViewModel>
		, IToolMode
	{
		#region Instance Fields
		private Point dragStart;
		private Rect dragRect = Rect.Empty;
		private double lastHorizontalChange;
		private double lastVerticalChange;
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
				return this.dragStartedCommand ??
					(this.dragStartedCommand = new RelayCommand<DragStartedEventArgs>(this.DragStarted));
			}
		}
		private RelayCommand<DragStartedEventArgs> dragStartedCommand;

		private void DragStarted(DragStartedEventArgs e)
		{
			// Get the point where the mouse was clicked
			this.dragStart = this.Parent.ZoomedMousePosition;

			// Get the drag rect
			this.dragRect = new Rect(this.dragStart.X, this.dragStart.Y, 0, 0);
			this.dragRect.Inflate(
				Math.Abs(UserApi.GetSystemMetrics(SM.CxDrag)),
				Math.Abs(UserApi.GetSystemMetrics(SM.CyDrag)));

			this.lastHorizontalChange = this.lastVerticalChange = 0;
			this.Parent.Selection.UpdateZoomedRectangleActualFromInput(null);
			e.Handled = true;
		}
		#endregion DragStartedCommand

		#region DragDeltaCommand
		public ICommand DragDeltaCommand
		{
			get
			{
				return this.dragDeltaCommand ??
					(this.dragDeltaCommand = new RelayCommand<DragDeltaEventArgs>(this.DragDelta));
			}
		}
		private RelayCommand<DragDeltaEventArgs> dragDeltaCommand;

		private void DragDelta(DragDeltaEventArgs e)
		{
			// Calculate the change
			var horizontalChange = e.HorizontalChange - this.lastHorizontalChange;
			var verticalChange = e.VerticalChange - this.lastVerticalChange;
			this.lastHorizontalChange = e.HorizontalChange;
			this.lastVerticalChange = e.VerticalChange;

			// Check to see if the mosue has moved outside of the drag rect
			if (!this.dragRect.IsEmpty && this.dragRect.Contains(this.Parent.ZoomedMousePosition))
				this.dragRect = Rect.Empty;

			// Only process if the mouse has moved outside of the drag rect
			if (this.dragRect.IsEmpty)
			{
				var rect = new ExtentRect(this.dragStart, this.Parent.ZoomedMousePosition);
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
				return this.dragCompletedCommand ??
					(this.dragCompletedCommand = new RelayCommand<DragCompletedEventArgs>(this.DragCompleted));
			}
		}
		private RelayCommand<DragCompletedEventArgs> dragCompletedCommand;

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
