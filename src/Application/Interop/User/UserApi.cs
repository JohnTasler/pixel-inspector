using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using PixelInspector.Interop.Gdi;

namespace PixelInspector.Interop.User;

public static class UserApi
{
	#region Constants
	private const string ApiLib = "user32.dll";
	#endregion Constants

	#region Safe Methods

	[DllImport(ApiLib, CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern nint SetCursor(nint hcursor);

	[DllImport(ApiLib, CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool ClipCursor(RECT rect);

	[DllImport(ApiLib, CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int GetSystemMetrics(SM nIndex);

	#endregion Safe Methods

	#region Unsafe Methods

	public static void FillRect(SafeHdc hdc, SafeGdiObject hbr, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect)
	{
		var rc = new RECT
		{
			Left = nLeftRect,
			Top = nTopRect,
			Right = nRightRect,
			Bottom = nBottomRect
		};

		if (!Private.FillRect(hdc, rc, hbr))
			throw new Win32Exception();
	}

	public static SafeWindowHdc GetDC(nint hwnd)
	{
		var hdc = Private.GetDC(hwnd);
		if (hdc.IsInvalid)
			throw new Win32Exception();

		hdc.WindowHandle = hwnd;
		return hdc;
	}

	[SecurityCritical]
	[SuppressUnmanagedCodeSecurity]
	[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool ReleaseDC(nint hwnd, nint hDC);

	[SuppressUnmanagedCodeSecurity]
	[SecurityCritical]
	[DllImport(ApiLib, CharSet = CharSet.Auto)]
	public static extern nint SendMessage(nint hwnd, int msg, nint wParam, nint lParam);

	public static WINDOWPLACEMENT GetWindowPlacement(nint hwnd)
	{
		WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
		GetWindowPlacement(hwnd, placement);
		return placement;
	}

	public static void GetWindowPlacement(nint hwnd, WINDOWPLACEMENT placement)
	{
		if (!Private.GetWindowPlacement(hwnd, placement))
			throw new Win32Exception();
	}

	public static void SetWindowPlacement(nint hwnd, WINDOWPLACEMENT placement)
	{
		if (!Private.SetWindowPlacement(hwnd, placement))
			throw new Win32Exception();
	}

	#endregion Unsafe Methods

	#region Nested Types
	internal static class Private
	{
		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern SafeWindowHdc GetDC(nint hwnd);

		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool FillRect(SafeHdc hdc, RECT rc, SafeGdiObject hbr);

		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowPlacement(nint hwnd, WINDOWPLACEMENT placement);

		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern bool SetWindowPlacement(nint hWnd, WINDOWPLACEMENT placement);
	}
	#endregion Nested Types
}
