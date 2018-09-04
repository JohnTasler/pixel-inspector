namespace ZoomIn.ViewModel
{
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Utility;

	public class LocateToolViewModel
		: ParentedObservableObject<MainViewModel>
		, IToolMode
	{
		#region Instance Fields
		private object previousToolState;
		private bool isExiting;
		#endregion Instance Fields

		#region Constructors
		public LocateToolViewModel(MainViewModel parent)
			: base(parent)
		{
		}
		#endregion Constructors

		#region IToolMode Members

		/// <summary>
		/// Called before the mode is entered.
		/// </summary>
		public void EnterMode()
		{
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

			using (new DisposeActionScope(() => this.isExiting = true, () => this.isExiting = false))
				this.Parent.ToolState = this.previousToolState;
		}

		#endregion IToolMode Members
	}
}
