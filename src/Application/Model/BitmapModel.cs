using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using PixelInspector.Interop.Gdi;
using PixelInspector.Interop.User;

namespace PixelInspector.Model;

public class BitmapModel : IDisposable
{
	#region Constants
	private const ushort BitsPerPixel = 24;
	private const int BytesPerPixel = BitsPerPixel / 8;
	#endregion Constants

	#region Instance Fields
	private bool _isDisposed;
	private int? _width;
	private int? _height;
	private SafePrivateHdc _hdc;
	private SafeGdiObject _hbm;
	private SafeGdiObject _hbmPrevious;
	private MemoryMappedFile _section;
	private long _sectionCapacity;
	private IntPtr _ppvBits;
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
			if (_isDisposed)
			{
				Debug.Assert(_hdc is null);
				Debug.Assert(_hbmPrevious is null);
				Debug.Assert(_hbm is null);
				Debug.Assert(_section is null);
				Debug.Assert(_sectionCapacity == 0);
				Debug.Assert(_ppvBits == IntPtr.Zero);
			}
			else if (_hdc is not null && !_hdc.IsInvalid)
			{
				Debug.Assert(_hbmPrevious is not null && !_hbmPrevious.IsInvalid);
				Debug.Assert(_hbm is not null && !_hbm.IsInvalid);
				Debug.Assert(_section is not null);
				Debug.Assert(_sectionCapacity != 0);
				Debug.Assert(_ppvBits != IntPtr.Zero);
			}

			return _isDisposed;
		}
	}

	public SafePrivateHdc DC
	{
		get
		{
			this.VerifyNotDisposed();
			this.EnsureInitialized();
			return _hdc;
		}
	}

	public SafeGdiObject Bitmap
	{
		get
		{
			this.VerifyNotDisposed();
			this.EnsureInitialized();
			return _hbm;
		}
	}

	public MemoryMappedFile Section
	{
		get
		{
			this.VerifyNotDisposed();
			return _section;
		}
	}

	public IntPtr Bits
	{
		get
		{
			this.VerifyNotDisposed();
			return _ppvBits;
		}
	}

	public int Stride
	{
		get { return GetStride(_width.GetValueOrDefault()); }
	}
	#endregion Properties

	#region Methods
	public void GetSize(out int cx, out int cy)
	{
		cx = _width.GetValueOrDefault(1);
		cy = _height.GetValueOrDefault(1);
	}

	public void SetSize(int cx, int cy)
	{
		this.VerifyNotDisposed();

		// Do nothing if the size has not changed
		if (_width == cx && _height == cy)
			return;

		// Enforce the minimum size
		cx = Math.Max(1, cx);
		cy = Math.Max(1, cy);

		// Release the current bitmap
		if (_hbm is not null && !_hbm.IsInvalid)
		{
			Debug.Assert(_hdc is not null && !_hdc.IsInvalid);
			Debug.Assert(_hbmPrevious is not null && !_hbmPrevious.IsInvalid);
			Debug.Assert(_section is not null);
			Debug.Assert(_sectionCapacity != 0);

			// Release the current bitmap
			using (GdiApi.SelectObject(_hdc, _hbmPrevious))
			using (_hbm)
			using (_section)
			{
				_section = null;
				GC.RemoveMemoryPressure(_sectionCapacity);
				_sectionCapacity = 0;
				_hbm = null;
			}
		}

		// Create the memory DC if needed
		if (_hdc is null || _hdc.IsInvalid)
		{
			using (var hdcScreen = UserApi.GetDC(IntPtr.Zero))
				_hdc = GdiApi.CreateCompatibleDC(hdcScreen);
		}

		// Create a new bitmap section
		_sectionCapacity = GetStride(cx) * cy;
		_section = MemoryMappedFile.CreateNew(null, _sectionCapacity);
		_hbm = GdiApi.CreateDIBSection(SafeHdc.Null, cx, -cy, BitsPerPixel, _section, ref _ppvBits);
		GC.AddMemoryPressure(_sectionCapacity);

		// Select the new bitmap into the memory DC
		_hbmPrevious = GdiApi.Private.SelectObject(_hdc, _hbm);

		// Save the new size
		_width = cx;
		_height = cy;
	}

	public int GetPixelColor(int x, int y)
	{
		this.VerifyNotDisposed();
		return GdiApi.GetPixel(_hdc, x, y);
	}

	public static int GetStride(int cx)
	{
		return ((int)(cx * BytesPerPixel + 3) / 4) * 4;
	}
	#endregion Methods

	#region IDisposable Members

	public void Dispose()
	{
		if (!this.IsDisposed)
		{
			using (_hdc)
			using (_hbm)
			using (_hbmPrevious)
			using (_section)
			using (GdiApi.SelectObject(_hdc, _hbmPrevious))
			{
				_section = null;
				_sectionCapacity = 0;
				_hbmPrevious = null;
				_hbm = null;
				_hdc = null;
				_ppvBits = IntPtr.Zero;
			}

			GC.SuppressFinalize(this);
			_isDisposed = true;
		}
	}

	#endregion IDisposable Members

	#region Private Implementation
	private void EnsureInitialized()
	{
		if (_width is null || _height is null)
			this.SetSize(1, 1);
	}

	private void VerifyNotDisposed()
	{
		if (this.IsDisposed)
			throw new ObjectDisposedException(this.GetType().FullName);
	}
	#endregion Private Implementation
}
