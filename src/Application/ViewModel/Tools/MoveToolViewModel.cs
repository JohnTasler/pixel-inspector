namespace ZoomIn.ViewModel
{
	using System.Windows;
	using System.Windows.Controls.Primitives;
	using System.Windows.Input;
	using ZoomIn.ComponentModel.Mvvm;

	public class MoveToolViewModel
		: ParentedObservableObject<MainViewModel>
		, IToolMode
		, IProvideSourceOrigin
	{
		#region Instance Fields
		private Point previousSourceOrigin;
		private double lastHorizontalChange;
		private double lastVerticalChange;
		#endregion Instance Fields

		#region Constructors
		public MoveToolViewModel(MainViewModel parent)
			: base(parent)
		{
		}
		#endregion Constructors

		#region Properties

		public Point SourceOrigin
		{
			get { return this.sourceOrigin; }
			private set { this.SetProperty(ref this.sourceOrigin, value, "SourceOrigin"); }
		}
		private Point sourceOrigin;

		public Point SourceOriginActual
		{
			get { return this.sourceOriginActual; }
			set
			{
				if (this.SetProperty(ref this.sourceOriginActual, value, "SourceOriginActual"))
					this.SourceOrigin = new Point((int)value.X, (int)value.Y);
			}
		}
		private Point sourceOriginActual;

		#endregion Properties

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
			this.lastHorizontalChange = this.lastVerticalChange = 0;
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
			var horizontalChange = e.HorizontalChange - this.lastHorizontalChange;
			var verticalChange = e.VerticalChange - this.lastVerticalChange;
			this.lastHorizontalChange = e.HorizontalChange;
			this.lastVerticalChange = e.VerticalChange;

			var sourceOrigin = this.SourceOriginActual;
			var zoomFactor = this.Parent.ViewSettings.Model.ZoomFactor;
			var xOffset = -horizontalChange / zoomFactor;
			var yOffset = -verticalChange / zoomFactor;

			sourceOrigin.Offset(xOffset, yOffset);
			this.SourceOriginActual = sourceOrigin;

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
			this.previousSourceOrigin = this.SourceOriginActual = this.Parent.ViewSettings.Model.SourceOrigin;
		}

		/// <summary>
		/// Called to exit the mode.
		/// </summary>
		/// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
		/// otherwise, it should commit its changes.</param>
		public void ExitMode(bool isReverting)
		{
			if (isReverting)
			{
				this.SourceOriginActual = this.previousSourceOrigin;

				// TODO: Restore previous bitmap images
			}
			else
			{
				this.Parent.ViewSettings.Model.SourceOrigin = this.SourceOrigin;
				this.EnterMode();
			}
		}

		#endregion IToolMode Members
	}
}
