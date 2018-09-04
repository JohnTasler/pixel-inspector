namespace ZoomIn.Model
{
	using System;
	using System.Diagnostics;
	using System.IO.MemoryMappedFiles;
	using ZoomIn.Interop.Gdi;
	using ZoomIn.Interop.User;

	public class BitmapModel : IDisposable
	{
		#region Constants
		private const ushort bitsPerPixel = 24;
		private const int bytesPerPixel = bitsPerPixel / 8;
		#endregion Constants

		#region Instance Fields
		private bool isDisposed;
		private int? width;
		private int? height;
		private SafePrivateHdc hdc;
		private SafeGdiObject hbm;
		private SafeGdiObject hbmPrevious;
		private MemoryMappedFile section;
		private long sectionCapacity;
		private IntPtr ppvBits;
		#endregion Instance Fields

		#region Finalizer
		~BitmapModel()
		{
			if (!this.IsDisposed)
				this.Dispose();
		}
		#endregion Finalizer

		#region Properties
		public bool IsDisposed
		{
			get
			{
				if (this.isDisposed)
				{
					Debug.Assert(this.hdc == null);
					Debug.Assert(this.hbmPrevious == null);
					Debug.Assert(this.hbm == null);
					Debug.Assert(this.section == null);
					Debug.Assert(this.sectionCapacity == 0);
					Debug.Assert(this.ppvBits == IntPtr.Zero);
				}
				else if (this.hdc != null && !this.hdc.IsInvalid)
				{
					Debug.Assert(this.hbmPrevious != null && !this.hbmPrevious.IsInvalid);
					Debug.Assert(this.hbm != null && !this.hbm.IsInvalid);
					Debug.Assert(this.section != null);
					Debug.Assert(this.sectionCapacity != 0);
					Debug.Assert(this.ppvBits != IntPtr.Zero);
				}

				return this.isDisposed;
			}
		}

		public SafePrivateHdc DC
		{
			get
			{
				this.VerifyNotDisposed();
				this.EnsureInitialized();
				return this.hdc;
			}
		}

		public SafeGdiObject Bitmap
		{
			get
			{
				this.VerifyNotDisposed();
				this.EnsureInitialized();
				return this.hbm;
			}
		}

		public MemoryMappedFile Section
		{
			get
			{
				this.VerifyNotDisposed();
				return this.section;
			}
		}

		public IntPtr Bits
		{
			get
			{
				this.VerifyNotDisposed();
				return this.ppvBits;
			}
		}

		public int Stride
		{
			get { return GetStride(width.GetValueOrDefault()); }
		}
		#endregion Properties

		#region Methods
		public void GetSize(out int cx, out int cy)
		{
			cx = this.width.GetValueOrDefault(1);
			cy = this.height.GetValueOrDefault(1);
		}

		public void SetSize(int cx, int cy)
		{
			this.VerifyNotDisposed();

			// Do nothing if the size has not changed
			if (this.width == cx && this.height == cy)
				return;

			// Enforce the minimum size
			cx = Math.Max(1, cx);
			cy = Math.Max(1, cy);

			// Release the current bitmap
			if (this.hbm != null && !this.hbm.IsInvalid)
			{
				Debug.Assert(this.hdc != null && !this.hdc.IsInvalid);
				Debug.Assert(this.hbmPrevious != null && !this.hbmPrevious.IsInvalid);
				Debug.Assert(this.section != null);
				Debug.Assert(this.sectionCapacity != 0);

				// Release the current bitmap
				using (GdiApi.SelectObject(this.hdc, this.hbmPrevious))
				using (this.hbm)
				using (this.section)
				{
					this.section = null;
					GC.RemoveMemoryPressure(this.sectionCapacity);
					this.sectionCapacity = 0;
					this.hbm = null;
				}
			}

			// Create the memory DC if needed
			if (this.hdc == null || this.hdc.IsInvalid)
			{
				using (var hdcScreen = UserApi.GetDC(IntPtr.Zero))
					this.hdc = GdiApi.CreateCompatibleDC(hdcScreen);
			}

			// Create a new bitmap section
			this.sectionCapacity = GetStride(cx) * cy;
			this.section = MemoryMappedFile.CreateNew(null, this.sectionCapacity);
			this.hbm = GdiApi.CreateDIBSection(SafeHdc.Null, cx, -cy, bitsPerPixel, this.section, ref this.ppvBits);
			GC.AddMemoryPressure(this.sectionCapacity);

			// Select the new bitmap into the memory DC
			this.hbmPrevious = GdiApi.Private.SelectObject(this.hdc, this.hbm);

			// Save the new size
			this.width = cx;
			this.height = cy;
		}

		public int GetPixelColor(int x, int y)
		{
			this.VerifyNotDisposed();
			return GdiApi.GetPixel(this.hdc, x, y);
		}

		public static int GetStride(int cx)
		{
			return ((int)(cx * bytesPerPixel + 3) / 4) * 4;
		}
		#endregion Methods

		#region IDisposable Members

		public void Dispose()
		{
			if (!this.IsDisposed)
			{
				using (this.hdc)
				using (this.hbm)
				using (this.hbmPrevious)
				using (this.section)
				using (GdiApi.SelectObject(this.hdc, this.hbmPrevious))
				{
					this.section = null;
					this.sectionCapacity = 0;
					this.hbmPrevious = null;
					this.hbm = null;
					this.hdc = null;
					this.ppvBits = IntPtr.Zero;
				}

				GC.SuppressFinalize(this);
				this.isDisposed = true;
			}
		}

		#endregion IDisposable Members

		#region Private Implementation
		private void EnsureInitialized()
		{
			if (this.width == null || this.height == null)
				this.SetSize(1, 1);
		}

		private void VerifyNotDisposed()
		{
			if (this.IsDisposed)
				throw new ObjectDisposedException(this.GetType().FullName);
		}
		#endregion Private Implementation
	}
}
