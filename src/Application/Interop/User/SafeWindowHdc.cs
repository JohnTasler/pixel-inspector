using PixelInspector.Interop.Gdi;

namespace PixelInspector.Interop.User;

public class SafeWindowHdc : SafeHdc
{
	#region Properties
	public nint WindowHandle { get; set; }
	#endregion Properties

	#region Overrides
	protected override bool ReleaseHandle()
	{
		return UserApi.ReleaseDC(this.WindowHandle, base.handle);
	}
	#endregion Overrides
}
