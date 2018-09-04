namespace ZoomIn.Interop
{
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Sequential)]
	public struct POINT
	{
		public int x;
		public int y;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECTstruct
	{
		public int left;
		public int top;
		public int right;
		public int bottom;

		public int Width
		{
			get { return this.right - this.left; }
		}

		public int Height
		{
			get { return this.bottom - this.top; }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public class RECT
	{
		public RECTstruct rect;

		public int Left
		{
			get { return this.rect.left; }
			set { this.rect.left = value; }
		}

		public int Top
		{
			get { return this.rect.top; }
			set { this.rect.top = value; }
		}

		public int Right
		{
			get { return this.rect.right; }
			set { this.rect.right = value; }
		}

		public int Bottom
		{
			get { return this.rect.bottom; }
			set { this.rect.bottom = value; }
		}

		public int Width
		{
			get { return this.rect.Width; }
		}

		public int Height
		{
			get { return this.rect.Height; }
		}
	}
}
