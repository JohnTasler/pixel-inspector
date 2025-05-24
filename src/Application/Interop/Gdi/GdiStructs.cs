using System.Runtime.InteropServices;

namespace PixelInspector.Interop.Gdi;

[StructLayout(LayoutKind.Sequential)]
public class LOGBRUSH
{
	public BrushStyle Style;
	public uint       Color;
	public nint     Hatch;
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
