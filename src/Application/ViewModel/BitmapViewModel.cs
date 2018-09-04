namespace ZoomIn.ViewModel
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using System.Windows;
	using System.Windows.Interop;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Model;

	public class BitmapViewModel : ObservableObject
	{
		public BitmapViewModel()
		{
			this.Model = new BitmapModel();
		}

		internal BitmapModel Model { get; private set; }

		public Size Size
		{
			get
			{
				int cx, cy;
				this.Model.GetSize(out cx, out cy);

				// TODO: DPI conversion
				return new Size(cx, cy);
			}

			internal set
			{
				var oldSize = this.Size;

				// TODO: DPI conversion
				this.Model.SetSize((int)value.Width, (int)value.Height);

				if (oldSize != this.Size)
				{
					this.bitmapSource = null;
					this.InvalidateBitmapSource();
					this.RaisePropertyChanged("Size");
				}
			}
		}

		public BitmapSource BitmapSource
		{
			get
			{
				if (this.bitmapSource == null)
				{
					//this.bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(
					//  this.Model.Bitmap.Handle, IntPtr.Zero, Int32Rect.Empty, null);

					int cx, cy;
					this.Model.GetSize(out cx, out cy);

					this.bitmapSource = Imaging.CreateBitmapSourceFromMemorySection(
						this.Model.Section.SafeMemoryMappedFileHandle.DangerousGetHandle(),
						cx, cy, PixelFormats.Bgr24, BitmapModel.GetStride(cx), 0);
				}

				return this.bitmapSource;
			}
		}
		private BitmapSource bitmapSource;

		public Color GetPixelColor(int x, int y)
		{
			var bgr = this.Model.GetPixelColor(x, y);
			var red = bgr & 0x0000FF;
			var green = (bgr & 0x00FF00) >> 8;
			var blue = (bgr & 0xFF0000) >> 16;
			return Color.FromRgb((byte)red, (byte)green, (byte)blue);
		}

		public IEnumerable<IEnumerable<Color>> GetPixelRows()
		{
			int cx, cy, stride = this.Model.Stride;
			this.Model.GetSize(out cx, out cy);
			var bytes = new byte[stride];

			var pEnd = this.Model.Bits + stride * cy;
			for (var pRow = this.Model.Bits; pRow.ToInt64() < pEnd.ToInt64(); pRow += stride)
			{
				Marshal.Copy(pRow, bytes, 0, bytes.Length);
				yield return GetPixelsInRows(bytes, pRow, cx);
			}
		}

		private static IEnumerable<Color> GetPixelsInRows(byte[] bytes, IntPtr pRow, int cx)
		{
			for (var x = 0; x < cx; ++x)
			{
				var index = x * 3;
				yield return Color.FromRgb(bytes[index + 2], bytes[index + 1], bytes[index]);
			}
		}

		internal void InvalidateBitmapSource()
		{
			if (this.bitmapSource != null)
				((InteropBitmap)this.bitmapSource).Invalidate();
			GC.Collect();

			//if (this.bitmapSource != null)
			//{
			//  this.bitmapSource = null;
			//  GC.Collect();
			//}
		}
	}
}
