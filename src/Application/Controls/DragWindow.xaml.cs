using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Tasler.Interop.Gdi;
using Tasler.Windows;

namespace PixelInspector;

/// <summary>
/// Interaction logic for DragWindow.xaml
/// </summary>
public partial class DragWindow : Window
{
	#region Instance Fields
	private readonly FrameworkElement _ownerElement;
	private readonly bool _isFromMouseClick;
	private readonly Rect _innerRect;
	private HwndSource? _hwndSource;
	private Thickness _outerContentThickness;
	#endregion Instance Fields

	#region Constructors
	public DragWindow(FrameworkElement ownerElement, Rect innerRect, bool isFromMouseClick, Cursor cursor)
	{
		_ownerElement = ownerElement;
		this.DataContext = ownerElement.DataContext;
		_innerRect = innerRect;
		_isFromMouseClick = isFromMouseClick;
		this.Cursor = cursor;

		// Set common window properties // TODO: Move to default style???
		TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
		this.UseLayoutRounding = true;
		this.AllowsTransparency = true;
		this.WindowStartupLocation = WindowStartupLocation.Manual;
		this.WindowStyle = WindowStyle.None;
		this.ShowActivated = false;
		this.ShowInTaskbar = false;
		this.Topmost = true;
		this.ResizeMode = ResizeMode.NoResize;

		// TODO: inflate based on some element in the visual tree (PART_InnerContent)

		//// TODO: Validate which button was pressed and that only one was pressed
		//if (Mouse.LeftButton != MouseButtonState.Pressed)
		//{
		//    throw new InvalidOperationException(SR.Get("DragMoveFail"));
		//}

		this.Owner = Window.GetWindow(_ownerElement);
		if (this.Owner is null)
			throw new InvalidOperationException(Properties.Resources.OwnerElementNotYetHosted);

		if (this.Owner.WindowState == WindowState.Normal)
		{
			// Hook the message proc of the ownerElement's window
			if (HwndSource.FromVisual(this.Owner) is HwndSource hwndSourceElement)
			{
				hwndSourceElement.AddHook(this.MessageHook_ForOwner);
				if (_isFromMouseClick)
					SendMessage(hwndSourceElement.Handle, WM_LBUTTONUP, nint.Zero, nint.Zero);
				Mouse.SetCursor(this.Cursor);
			}
			else
			{
				throw new InvalidOperationException("The specified ownerElement does not have an HwndSource.");
			}
		}

		this.InitializeComponent();

		this.Loaded += this.DragWindow_Loaded;
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
		if (_hwndSource is not null)
			_hwndSource.RemoveHook(this.MessageHook);

		base.OnSourceInitialized(e);

		_hwndSource = HwndSource.FromVisual(this) as HwndSource;
		if (_hwndSource is not null)
		{
			_hwndSource.AddHook(this.MessageHook);
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
		this.InnerContentRect = rect.Deflate(_outerContentThickness);

		// Perform default processing
		base.OnLocationChanged(e);
	}

	/// <summary>
	/// Raises the <see cref="E:System.Windows.Window.Closed"/> event.
	/// </summary>
	/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
	protected override void OnClosed(EventArgs e)
	{
		if (HwndSource.FromVisual(_ownerElement) is HwndSource hwndSourceElement)
			hwndSourceElement.RemoveHook(this.MessageHook_ForOwner);

		base.OnClosed(e);
	}
	#endregion Overrides

	#region Private Implementation
	private void EnterMoveMode()
	{
		if (_hwndSource is null)
			return;

		int sysCommand = _isFromMouseClick ? SC_MOVE_CAPTION : SC_MOVE;
		SendMessage(_hwndSource.Handle, WM_SYSCOMMAND, (nint)sysCommand, nint.Zero);
	}

	private void ExitMoveMode()
	{
		this.WasCancelled = Keyboard.IsKeyDown(Key.Escape);
		base.Dispatcher.BeginInvoke(new Action(() => this.Close()));
	}

	private nint MessageHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		switch (msg)
		{
			case WM_NCHITTEST:
				Mouse.SetCursor(this.Cursor);
				handled = true;
				return (nint)1;

			case WM_MOUSEMOVE:
				this.ExitMoveMode();
				handled = true;
				return nint.Zero;

			case WM_EXITSIZEMOVE:
				this.ExitMoveMode();
				break;

			case WM_MOUSEACTIVATE:
				handled = true;
				return (nint)3;
		}

		return nint.Zero;
	}

	private nint MessageHook_ForOwner(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		switch (msg)
		{
			case WM_NCHITTEST:
				Mouse.SetCursor(this.Cursor);
				handled = true;
				return (nint)1;

			case WM_NCACTIVATE:
				handled = true;
				return nint.Zero;
		}

		return nint.Zero;
	}
	#endregion Private Implementation

	#region Event Handlers
	private void DragWindow_Loaded(object sender, RoutedEventArgs e)
	{
		// Compute the offsets of the inner content
		var topLeft = this.PART_InnerContent.TranslatePoint(new Point(0, 0), this);
		var bottomRight = this.PART_InnerContent.TranslatePoint(
			new Point(this.PART_InnerContent.ActualWidth, this.PART_InnerContent.ActualHeight), this);

		_outerContentThickness = new Thickness(
			topLeft.X, topLeft.Y, this.ActualWidth - bottomRight.X, this.ActualHeight - bottomRight.Y);

		var outerRect = _innerRect.Inflate(_outerContentThickness);

		// Compute the window's position and size based on the specified rectangle
		var screenPoint = _ownerElement.PointToScreen(outerRect.TopLeft);

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
	private static extern nint SendMessage(nint hWnd, int msg, nint wParam, nint lParam);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern nint SetCursor(nint hcursor);

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
