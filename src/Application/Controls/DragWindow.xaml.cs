namespace ZoomIn
{
	using System;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Interop;
	using System.Windows.Media;
	using ZoomIn.Interop.Gdi;
	using ZoomIn.Utility;

	/// <summary>
	/// Interaction logic for DragWindow.xaml
	/// </summary>
	public partial class DragWindow : Window
	{
		#region Instance Fields
		private readonly FrameworkElement ownerElement;
		private readonly bool isFromMouseClick;
		private readonly Rect innerRect;
		private HwndSource hwndSource;
		private Thickness outerContentThickness;
		#endregion Instance Fields

		#region Constructors
		public DragWindow(FrameworkElement ownerElement, Rect innerRect, bool isFromMouseClick, Cursor cursor)
		{
			this.ownerElement = ownerElement;
			this.DataContext = ownerElement.DataContext;
			this.innerRect = innerRect;
			this.isFromMouseClick = isFromMouseClick;
			this.Cursor = cursor;

			// Set common window properties // TODO: Move to default style???
			TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
			this.UseLayoutRounding     = true;
			this.AllowsTransparency    = true;
			this.WindowStartupLocation = WindowStartupLocation.Manual;
			this.WindowStyle           = WindowStyle.None;
			this.ShowActivated         = false;
			this.ShowInTaskbar         = false;
			this.Topmost               = true;
			this.ResizeMode            = ResizeMode.NoResize;

			// TODO: inflate based on some element in the visual tree (PART_InnerContent)

			//// TODO: Validate which button was pressed and that only one was pressed
			//if (Mouse.LeftButton != MouseButtonState.Pressed)
			//{
			//    throw new InvalidOperationException(SR.Get("DragMoveFail"));
			//}

			this.Owner = Window.GetWindow(this.ownerElement);
			if (this.Owner == null)
				throw new InvalidOperationException("The specified ownerElement is not yet hosted within a Window.");

			if (this.Owner.WindowState == WindowState.Normal)
			{
				// Hook the message proc of the ownerElement's window
				var hwndSourceElement = HwndSource.FromVisual(this.Owner) as HwndSource;
				if (hwndSourceElement != null)
				{
					hwndSourceElement.AddHook(this.MessageHook_ForOwner);
					if (this.isFromMouseClick)
						SendMessage(hwndSourceElement.Handle, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
					Mouse.SetCursor(this.Cursor);
				}
				else
				{
					throw new InvalidOperationException("The specified ownerElement does not have an HwndSource.");
				}
			}

			this.InitializeComponent();

			this.Loaded += this.this_Loaded;
		}
		#endregion Constructors

		#region Properties
		public bool WasCancelled { get; private set; }

		public Rect InnerContentRect { get; private set; }
		#endregion Properties

		#region Overrides
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Window.SourceInitialized"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnSourceInitialized(EventArgs e)
		{
			if (this.hwndSource != null)
				this.hwndSource.RemoveHook(this.MessageHook);

			base.OnSourceInitialized(e);

			this.hwndSource = HwndSource.FromVisual(this) as HwndSource;
			if (this.hwndSource != null)
			{
				this.hwndSource.AddHook(this.MessageHook);
				this.Dispatcher.BeginInvoke(new Action(this.EnterMoveMode));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Window.LocationChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLocationChanged(EventArgs e)
		{
			// Calculate the new InnerContentRect
			var rect = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);

			// Account for DPI
			if (GdiApi.DpiScaleX != 1.0 || GdiApi.DpiScaleY != 1.0)
			{
				rect.X /= GdiApi.DpiScaleX;
				rect.Y /= GdiApi.DpiScaleY;
				rect.Width /= GdiApi.DpiScaleX;
				rect.Height /= GdiApi.DpiScaleY;
			}

			// Set the property
			this.InnerContentRect = rect.Deflate(this.outerContentThickness);

			// Perform default processing
			base.OnLocationChanged(e);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Window.Closed"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnClosed(EventArgs e)
		{
			var hwndSourceElement = HwndSource.FromVisual(this.ownerElement) as HwndSource;
			if (hwndSourceElement != null)
				hwndSourceElement.RemoveHook(this.MessageHook_ForOwner);

			base.OnClosed(e);
		}
		#endregion Overrides

		#region Private Implementation
		private void EnterMoveMode()
		{
			int sysCommand = this.isFromMouseClick ? SC_MOVE_CAPTION : SC_MOVE;
			SendMessage(this.hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)sysCommand, IntPtr.Zero);
		}

		private void ExitMoveMode()
		{
			this.WasCancelled = Keyboard.IsKeyDown(Key.Escape);
			base.Dispatcher.BeginInvoke(new Action(() => this.Close()));
		}

		private IntPtr MessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_NCHITTEST:
					Mouse.SetCursor(this.Cursor);
					handled = true;
					return (IntPtr)1;

				case WM_MOUSEMOVE:
					this.ExitMoveMode();
					handled = true;
					return IntPtr.Zero;

				case WM_EXITSIZEMOVE:
					this.ExitMoveMode();
					break;

				case WM_MOUSEACTIVATE:
					handled = true;
					return (IntPtr)3;
			}

			return IntPtr.Zero;
		}

		private IntPtr MessageHook_ForOwner(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (msg)
			{
				case WM_NCHITTEST:
					Mouse.SetCursor(this.Cursor);
					handled = true;
					return (IntPtr)1;

				case WM_NCACTIVATE:
					handled = true;
					return IntPtr.Zero;
			}

			return IntPtr.Zero;
		}
		#endregion Private Implementation

		#region Event Handlers
		private void this_Loaded(object sender, RoutedEventArgs e)
		{
			// Compute the offsets of the inner content
			var topLeft = this.PART_InnerContent.TranslatePoint(new Point(0, 0), this);
			var bottomRight = this.PART_InnerContent.TranslatePoint(
				new Point(this.PART_InnerContent.ActualWidth, this.PART_InnerContent.ActualHeight), this);

			this.outerContentThickness = new Thickness(
				topLeft.X, topLeft.Y, this.ActualWidth - bottomRight.X, this.ActualHeight - bottomRight.Y);

			var outerRect = this.innerRect.Inflate(this.outerContentThickness);

			// Compute the window's position and size based on the specified rectangle
			var screenPoint = this.ownerElement.PointToScreen(outerRect.TopLeft);

			// Account for DPI
			if (GdiApi.DpiScaleX != 1.0 || GdiApi.DpiScaleY != 1.0)
			{
				screenPoint.X *= GdiApi.DpiScaleX;
				screenPoint.Y *= GdiApi.DpiScaleY;
				outerRect.Width *= GdiApi.DpiScaleX;
				outerRect.Height *= GdiApi.DpiScaleY;
			}

			// Set the window's position and size
			this.Left = screenPoint.X;
			this.Top = screenPoint.Y;
			this.Width = outerRect.Width;
			this.Height = outerRect.Height;
		}
		#endregion Event Handlers

		#region Platform Invoke

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr SetCursor(IntPtr hcursor);

		private const int WM_MOUSELEAVE = 0x02A3;
		private const int WM_EXITSIZEMOVE = 0x0232;
		private const int WM_MOUSEMOVE = 0x0200;
		private const int WM_SYSCOMMAND = 0x0112;
		private const int WM_LBUTTONUP = 0x0202;
		private const int WM_NCACTIVATE = 0x0086;
		private const int WM_NCHITTEST = 0x0084;
		private const int WM_MOUSEACTIVATE = 0x0021;
		private const int WM_SETCURSOR = 0x0020;
		private const int WM_SHOWWINDOW = 0x0018;

		private const int SC_MOVE = 0xF010;
		private const int SC_MOVE_CAPTION = 0xF012;

		#endregion Platform Invoke
	}
}
