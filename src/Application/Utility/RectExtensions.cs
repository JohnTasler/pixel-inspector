namespace ZoomIn.Utility
{
	using System.Windows;

	/// <summary>
	/// Extension methods for the <see cref="Rect"/> structure.
	/// </summary>
	public static class RectExtensions
	{
		public static Rect Inflate(this Rect rect, double left, double top, double right, double bottom)
		{
			rect.X -= left;
			rect.Y -= top;
			rect.Width += left + right;
			rect.Height += top + bottom;
			return rect;
		}

		public static Rect Inflate(this Rect rect, Thickness thickness)
		{
			return rect.Inflate(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
		}

		public static Rect Deflate(this Rect rect, double left, double top, double right, double bottom)
		{
			return rect.Inflate(-left, -top, -right, -bottom);
		}

		public static Rect Deflate(this Rect rect, Thickness thickness)
		{
			return rect.Deflate(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
		}
	}
}
