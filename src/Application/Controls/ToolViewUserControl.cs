namespace ZoomIn
{
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;

	/// <summary>
	/// The base class for views representing a tool mode.
	/// </summary>
	public class ToolViewUserControl : UserControl
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="ToolViewUserControl"/> class.
		/// </summary>
		public ToolViewUserControl()
		{
			this.Background = Brushes.Transparent;
			this.IsTabStop = true;
			this.Focusable = true;
			this.FocusVisualStyle = null;
			this.Loaded += this.this_Loaded;
		}
		#endregion Constructors

		#region Event Handlers
		private void this_Loaded(object sender, RoutedEventArgs e)
		{
			var focusScope = FocusManager.GetFocusScope(this);
			FocusManager.SetFocusedElement(focusScope, this);
			Keyboard.Focus(this);
		}
		#endregion Event Handlers
	}
}
