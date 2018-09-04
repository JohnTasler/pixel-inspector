namespace ZoomIn.Interop.Gdi
{
	using System;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public class LOGBRUSH
	{
		public BrushStyle Style;
		public uint       Color;
		public IntPtr     Hatch;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class BITMAPINFOHEADER
	{
		public int size;
		public int width;
		public int height;
		public ushort planes;
		public ushort bitCount;
		public uint compression;
		public uint sizeImage;
		public int xPelsPerMeter;
		public int yPelsPerMeter;
		public uint clrUsed;
		public uint clrImportant;

		public static readonly int MarhalSizeOf = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
	}
}
