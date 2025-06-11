using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tasler.Windows;

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
		this.HookDataContextAsViewModel(_propertyChanged);
	}
	#endregion Constructors

	#region Events
	public event PropertyChangedEventHandler? PropertyChanged
	{
		add
		{
			bool wasNull = _propertyChanged is null;
			_propertyChanged += value;
			if (wasNull)
				this.HookDataContextAsViewModel(_propertyChanged);
		}
		remove => _propertyChanged -= value;
	}
	private PropertyChangedEventHandler? _propertyChanged;
	#endregion Events

	#region Event Handlers

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		var focusScope = FocusManager.GetFocusScope(this);
		FocusManager.SetFocusedElement(focusScope, this);
		Keyboard.Focus(this);
	}
	#endregion Event Handlers
}
