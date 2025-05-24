using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tasler.ComponentModel;

namespace PixelInspector;

/// <summary>
/// The base class for views representing a tool mode.
/// </summary>
public class ToolViewUserControl : UserControl, INotifyPropertyChanged
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
		this.Loaded += this.UserControl_Loaded;
	}
	#endregion Constructors


	#region Events
	public event PropertyChangedEventHandler? PropertyChanged;
	#endregion Events

	protected void RaisePropertyChanged(string propertyName) => this.PropertyChanged?.Raise(this, propertyName);

	#region Event Handlers

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		var focusScope = FocusManager.GetFocusScope(this);
		FocusManager.SetFocusedElement(focusScope, this);
		Keyboard.Focus(this);
	}
	#endregion Event Handlers
}
