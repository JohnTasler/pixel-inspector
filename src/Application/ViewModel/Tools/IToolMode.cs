namespace ZoomIn.ViewModel
{
	/// <summary>
	/// Represents a mode that can be entered and exited.
	/// </summary>
	public interface IToolMode
	{
		/// <summary>
		/// Called before the mode is entered.
		/// </summary>
		void EnterMode();

		/// <summary>
		/// Called to exit the mode.
		/// </summary>
		/// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
		/// otherwise, it should commit its changes.</param>
		void ExitMode(bool isReverting);
	}
}
