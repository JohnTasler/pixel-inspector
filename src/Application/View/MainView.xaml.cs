using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using PixelInspector.Interop.User;
using PixelInspector.ViewModel;
using Tasler.ComponentModel;

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
	public MainView()
	{
		this.InitializeComponent();
		this.Loaded += this.MainView_Loaded;
		this.DataContextChanged += this.MainView_DataContextChanged;
	}
	#endregion Constructors

	public event PropertyChangedEventHandler? PropertyChanged;

	#region Overrides
	protected override void OnSourceInitialized(EventArgs e)
	{
		this.HwndSource = (HwndSource)HwndSource.FromVisual(this);
		this.HwndSource?.AddHook(this.HwndSource_Hook);
		base.OnSourceInitialized(e);
	}
	#endregion Overrides

	#region Private Implementation
	private bool HasSetWindowPlacement { get; set; }
	private MainViewModel ViewModel => (MainViewModel)this.DataContext;
	private HwndSource? HwndSource { get; set; }
	#endregion Private Implementation

	#region Event Handlers
	private void MainView_Loaded(object? sender, RoutedEventArgs e)
	{
		Keyboard.Focus(this.mainContent);
	}

	private void MainView_DataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is INotifyPropertyChanged oldViewModel)
			oldViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;

		if (e.NewValue is MainViewModel viewModel)
		{
			viewModel.PropertyChanged += this.ViewModel_PropertyChanged;
			this.PropertyChanged?.Raise(this, nameof(this.ViewModel));
		}

		this.InvalidateVisual();
	}

	private IntPtr HwndSource_Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		var message = (WM)msg;
		switch (message)
		{
			case WM.Move:
			case WM.Size:
				if (this.ViewModel is not null)
				{
					if (this.ViewModel.ViewSettings.Model.WindowPlacement is null)
						this.ViewModel.ViewSettings.Model.WindowPlacement = new Model.WindowPlacementModel();
					this.ViewModel.ViewSettings.Model.WindowPlacement.Get(hwnd);
				}
				break;

			case WM.ShowWindow:
				if (!this.HasSetWindowPlacement && this.ViewModel is not null)
				{
					if (this.ViewModel.ViewSettings.Model.WindowPlacement is not null)
						this.ViewModel.ViewSettings.Model.WindowPlacement.Set(hwnd);
					this.HasSetWindowPlacement = true;
				}
				break;

			case WM.Destroy:
				this.HwndSource?.RemoveHook(this.HwndSource_Hook);
				break;
		}

		return IntPtr.Zero;
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
