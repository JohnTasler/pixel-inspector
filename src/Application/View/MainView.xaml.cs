using System.ComponentModel;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using PixelInspector.ViewModel;
using Tasler.ComponentModel;
using Tasler.Interop.User;
using Tasler.Windows;

namespace PixelInspector.View;

/// <summary>
/// Interaction logic for MainView.xaml, a view of <see cref="MainViewModel"/>.
/// </summary>
public partial class MainView : Window, INotifyPropertyChanged
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="MainView"/> class.
	/// </summary>
	public MainView(MainViewModel viewModel)
	{
		this.InitializeComponent();
		this.Loaded += this.MainView_Loaded;
		this.HookDataContextAsViewModel(() => this.PropertyChanged?.Raise(this, nameof(this.ViewModel)));
		this.DataContext = viewModel;
	}
	#endregion Constructors

	#region Overrides
	protected override void OnSourceInitialized(EventArgs e)
	{
		this.HwndSource = (HwndSource)HwndSource.FromVisual(this);
		this.HwndSource?.AddHook(this.HwndSource_Hook);
		base.OnSourceInitialized(e);
	}
	#endregion Overrides

	#region Events
	public event PropertyChangedEventHandler? PropertyChanged;
	#endregion Events

	#region Private Implementation
	private bool HasSetWindowPlacement { get; set; }
	public MainViewModel ViewModel => (MainViewModel)this.DataContext;
	private HwndSource? HwndSource { get; set; }
	#endregion Private Implementation

	#region Event Handlers
	private void MainView_Loaded(object? sender, RoutedEventArgs e)
	{
		Keyboard.Focus(this.mainContent);
	}

	private nint HwndSource_Hook(nint hwndHandle, int msg, nint wParam, nint lParam, ref bool handled)
	{
		SafeHwnd hwnd = new() {Handle = hwndHandle };
		var message = (WM)msg;
		switch (message)
		{
			case WM.MOVE:
			case WM.SIZE:
				if (this.ViewModel is not null)
				{
					if (this.ViewModel.ViewSettings.Model.WindowPlacement is null)
						this.ViewModel.ViewSettings.Model.WindowPlacement = new(hwnd);
					else
						this.ViewModel.ViewSettings.Model.WindowPlacement.Get(hwnd);
				}
				break;

			case WM.SHOWWINDOW:
				if (!this.HasSetWindowPlacement && this.ViewModel is not null)
				{
					this.ViewModel.ViewSettings.Model.WindowPlacement?.Set(hwnd);
					this.HasSetWindowPlacement = true;
				}
				break;

			case WM.DESTROY:
				this.HwndSource?.RemoveHook(this.HwndSource_Hook);
				break;
		}

		return nint.Zero;
	}

	private void MainContent_PreviewMouseMove(object? sender, MouseEventArgs e)
	{
		if (this.ViewModel is not null)
			this.ViewModel.ZoomedMousePosition = e.GetPosition(this.mainContent);
	}

	private void MainContent_MouseEnter(object? sender, MouseEventArgs e)
	{
		if (this.ViewModel is not null)
			this.ViewModel.IsMouseInZoomedBounds = true;
	}

	private void MainContent_MouseLeave(object? sender, MouseEventArgs e)
	{
		if (this.ViewModel is not null)
			this.ViewModel.IsMouseInZoomedBounds = false;
	}

	private void MainContent_MouseWheel(object? sender, MouseWheelEventArgs e)
	{
		if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
		{
			var command = e.Delta > 0
				? this.ViewModel.ViewSettings.IncreaseZoomCommand
				: this.ViewModel.ViewSettings.DecreaseZoomCommand;
			if (command.CanExecute(null))
				command.Execute(null);
		}
	}

	private void MainContent_PreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount == 2)
		{
			e.Handled = true;

			this.ViewModel.ChooseToolLocatingCommand.Execute(
				new LocatingToolViewModel.Parameters
				{
					Offset = e.GetPosition((IInputElement)e.Source),
					IsFromMouseClick = true
				}
			);
		}
	}

	private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(this.ViewModel.ApplicationState):
				break;
		}
	}
	#endregion Event Handlers
}
