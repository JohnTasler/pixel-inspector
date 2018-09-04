namespace ZoomIn.Interop.User
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public class WINDOWPLACEMENT
	{
		public static readonly int MarshalSizeOf = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
		private int length = MarshalSizeOf;
		public int flags;
		public SW showCmd;
		public POINT ptMinPosition;
		public POINT ptMaxPosition;
		public RECTstruct rcNormalPosition;
	}
}
