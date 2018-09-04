namespace PixelInspector.Interop.User
{
	using System;
	using PixelInspector.Interop.Gdi;

	public class SafeWindowHdc : SafeHdc
	{
		#region Properties
		public IntPtr WindowHandle { get; set; }
		#endregion Properties

		#region Overrides
		protected override bool ReleaseHandle()
		{
			return UserApi.ReleaseDC(this.WindowHandle, base.handle);
		}
		#endregion Overrides
	}
}
