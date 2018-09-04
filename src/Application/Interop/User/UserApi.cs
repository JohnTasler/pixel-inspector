namespace ZoomIn.Interop.User
{
	using System;
	using System.ComponentModel;
	using System.Runtime.ConstrainedExecution;
	using System.Runtime.InteropServices;
	using System.Security;
	using ZoomIn.Interop.Gdi;

	public static class UserApi
	{
		#region Constants
		private const string ApiLib = "user32.dll";
		#endregion Constants

		#region Safe Methods

		[DllImport(ApiLib, CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern IntPtr SetCursor(IntPtr hcursor);

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

		public static SafeWindowHdc GetDC(IntPtr hwnd)
		{
			var hdc = Private.GetDC(hwnd);
			if (hdc.IsInvalid)
				throw new Win32Exception();

			hdc.WindowHandle = hwnd;
			return hdc;
		}

		[SecurityCritical]
		[SuppressUnmanagedCodeSecurity]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		[return:MarshalAs(UnmanagedType.Bool)]
		public static extern bool ReleaseDC(IntPtr hwnd, IntPtr hDC);

		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		[DllImport(ApiLib, CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);

		public static WINDOWPLACEMENT GetWindowPlacement(IntPtr hwnd)
		{
			WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
			GetWindowPlacement(hwnd, placement);
			return placement;
		}

		public static void GetWindowPlacement(IntPtr hwnd, WINDOWPLACEMENT placement)
		{
			if (!Private.GetWindowPlacement(hwnd, placement))
				throw new Win32Exception();
		}

		public static void SetWindowPlacement(IntPtr hwnd, WINDOWPLACEMENT placement)
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
			public static extern SafeWindowHdc GetDC(IntPtr hwnd);

			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool FillRect(SafeHdc hdc, RECT rc, SafeGdiObject hbr);

			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool GetWindowPlacement(IntPtr hwnd, WINDOWPLACEMENT placement);

			[SecurityCritical]
			[SuppressUnmanagedCodeSecurity]
			[DllImport(ApiLib, CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
			public static extern bool SetWindowPlacement(IntPtr hWnd, WINDOWPLACEMENT placement);
		}
		#endregion Nested Types
	}
}
