namespace ZoomIn.View
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Linq;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Interop;
	using ZoomIn.Interop.User;
	using ZoomIn.ViewModel;

	/// <summary>
	/// Interaction logic for MainView.xaml, a view of <see cref="MainViewModel"/>.
	/// </summary>
	public partial class MainView : Window
	{
		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="MainView"/> class.
		/// </summary>
		public MainView()
		{
			this.InitializeComponent();
			this.Loaded += this.this_Loaded;
			this.DataContextChanged += this.this_DataContextChanged;
		}
		#endregion Constructors

		#region Overrides
		protected override void OnSourceInitialized(EventArgs e)
		{
			this.HwndSource = (HwndSource)HwndSource.FromVisual(this);
			this.HwndSource.AddHook(this.HwndSource_Hook);
			base.OnSourceInitialized(e);
		}
		#endregion Overrides

		#region Private Implementation
		private bool HasSetWindowPlacement { get; set; }
		private MainViewModel ViewModel { get; set; }
		private HwndSource HwndSource { get; set; }
		private List<InputBinding> SavedInputBindings { get; set; }
		#endregion Private Implementation

		#region Event Handlers
		private void this_Loaded(object sender, RoutedEventArgs e)
		{
			Keyboard.Focus(this.mainContent);
		}

		private void this_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var oldViewModel = e.OldValue as MainViewModel;
			if (oldViewModel != null)
				oldViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;

			this.ViewModel = e.NewValue as MainViewModel;
			if (this.ViewModel != null)
				this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;

			this.InvalidateVisual();
		}

		private IntPtr HwndSource_Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			var message = (WM)msg;
			switch (message)
			{
				case WM.Move:
				case WM.Size:
					if (this.ViewModel != null)
					{
						if (this.ViewModel.ViewSettings.Model.WindowPlacement == null)
							this.ViewModel.ViewSettings.Model.WindowPlacement = new Model.WindowPlacementModel();
						this.ViewModel.ViewSettings.Model.WindowPlacement.Get(hwnd);
					}
					break;

				case WM.ShowWindow:
					if (!this.HasSetWindowPlacement && this.ViewModel != null)
					{
						if (this.ViewModel.ViewSettings.Model.WindowPlacement != null)
							this.ViewModel.ViewSettings.Model.WindowPlacement.Set(hwnd);
						this.HasSetWindowPlacement = true;
					}
					break;

				case WM.Destroy:
					this.HwndSource.RemoveHook(this.HwndSource_Hook);
					break;
			}

			return IntPtr.Zero;
		}

		private void mainContent_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if (this.ViewModel != null)
				this.ViewModel.ZoomedMousePosition = e.GetPosition(this.mainContent);
		}

		private void mainContent_MouseEnter(object sender, MouseEventArgs e)
		{
			if (this.ViewModel != null)
				this.ViewModel.IsMouseInZoomedBounds = true;
		}

		private void mainContent_MouseLeave(object sender, MouseEventArgs e)
		{
			if (this.ViewModel != null)
				this.ViewModel.IsMouseInZoomedBounds = false;
		}

		private void mainContent_MouseWheel(object sender, MouseWheelEventArgs e)
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

		private void mainContent_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "ApplicationState":
					break;
			}
		}

		private void menuBar_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var isKeyboardFocusWithinMenu = (bool)e.NewValue;
			if (isKeyboardFocusWithinMenu)
			{
				this.SavedInputBindings = this.InputBindings.OfType<InputBinding>().ToList();
				this.InputBindings.Clear();
			}
			else
			{
				Debug.Assert(this.SavedInputBindings != null);
				this.InputBindings.AddRange(this.SavedInputBindings);
				this.SavedInputBindings = null;
			}
		}
		#endregion Event Handlers
	}
}
