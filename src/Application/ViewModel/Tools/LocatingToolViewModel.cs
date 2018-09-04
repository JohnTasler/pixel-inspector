namespace ZoomIn.ViewModel
{
	using System.Windows;
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Utility;

	public class LocatingToolViewModel
		: ParentedObservableObject<MainViewModel>
		, IProvideSourceOrigin
		, IToolMode
	{
		#region Instance Fields
		private object previousToolState;
		private bool isExiting;
		#endregion Instance Fields

		#region Constructors
		public LocatingToolViewModel(MainViewModel parent, LocatingToolViewModel.Parameters parameters)
			: base(parent)
		{
			if (parameters != null)
			{
				this.Offset = parameters.Offset;
				this.IsFromMouseClick = parameters.IsFromMouseClick;
			}
		}
		#endregion Constructors

		#region Properties

		public Point Offset { get; private set; }

		public bool IsFromMouseClick { get; private set; }

		public Point SourceOrigin
		{
			get { return this.sourceOrigin; }
			set { this.SetProperty(ref this.sourceOrigin, value, "SourceOrigin"); }
		}
		private Point sourceOrigin;

		#endregion Properties

		#region IToolMode Members

		/// <summary>
		/// Called before the mode is entered.
		/// </summary>
		public void EnterMode()
		{
			this.sourceOrigin = this.Parent.ViewSettings.Model.SourceOrigin;
			this.previousToolState = this.Parent.ToolState;
		}

		/// <summary>
		/// Called to exit the mode.
		/// </summary>
		/// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
		/// otherwise, it should commit its changes.</param>
		public void ExitMode(bool isReverting)
		{
			if (this.isExiting)
				return;

			if (isReverting)
			{
				// TODO: Restore previous bitmap images
			}
			else
			{
				this.Parent.ViewSettings.Model.SourceOrigin = this.SourceOrigin;
			}

			using (new DisposeActionScope(() => this.isExiting = true, () => this.isExiting = false))
				this.Parent.ToolState = this.previousToolState;
		}

		#endregion IToolMode Members

		#region Nested Types
		public class Parameters
		{
			public Point Offset { get; set; }
			public bool IsFromMouseClick { get; set; }
		}
		#endregion Nested Types
	}
}
