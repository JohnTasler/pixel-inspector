using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PixelInspector.Interop;
using PixelInspector.Interop.Gdi;
using PixelInspector.Interop.User;
using PixelInspector.Model;
using Tasler.Windows.ComponentModel;

namespace PixelInspector.ViewModel;

public partial class ScreenImageViewModel : ObservableObject
{
	#region Static Fields
#if DEBUG
	private static int s_invalidHandleCount;
#endif // DEBUG
	#endregion Static Fields

	#region Instance Fields
	private ViewSettingsViewModel _viewSettings;
	private BitmapViewModelBase _sourceBitmap;
	private BitmapViewModelBase _zoomedBitmap;
	#endregion Instance Fields

	#region Constructors
	public ScreenImageViewModel(
		ViewSettingsViewModel viewSettings,
		[FromKeyedServices("Source")] BitmapViewModelBase sourceBitmap,
		[FromKeyedServices("Zoomed")] BitmapViewModelBase zoomedBitmap)
	{
		_viewSettings = viewSettings;
		_sourceBitmap = sourceBitmap;
		_zoomedBitmap = zoomedBitmap;

		viewSettings.Model.PropertyChanged += this.ViewSettings_PropertyChanged;

		ComponentDispatcher.ThreadFilterMessage += this.ComponentDispatcher_ThreadPreprocessMessage;
	}
	#endregion Constructors

	#region Properties

	[ObservableProperty]
	private bool _isRefreshNeeded;

	public BitmapViewModelBase SourceBitmap =>  _sourceBitmap;

	public BitmapViewModelBase ZoomedBitmap => _zoomedBitmap;

	#endregion Properties

	#region Methods
	public void Refresh(Point sourceOrigin)
	{
		Guard.IsNotNull(_viewSettings);

		// Compute the origin and extents of the source rectangle
		var sourceSize = _viewSettings.Model.SourceSize;
		sourceSize.Width = Math.Min(sourceSize.Width, SystemParameters.VirtualScreenWidth);
		sourceSize.Height = Math.Min(sourceSize.Height, SystemParameters.VirtualScreenHeight);
		var sourceRect = new Rect(sourceOrigin, sourceSize);
		int xSrc = (int)sourceRect.X;
		int ySrc = (int)sourceRect.Y;
		int cxSrc = (int)sourceRect.Width;
		int cySrc = (int)sourceRect.Height;

		// Flush the GDI pipeline
		GdiApi.GdiFlush();

		// Copy from the screen to our source bitmap buffer
		var hdcSource = this.SourceBitmap.Model.DC!;
		using (var hdcScreen = UserApi.GetDC(nint.Zero))
		{
			UserApi.FillRect(hdcSource, GdiApi.GetStockObject(StockObject.GrayBrush), 0, 0, cxSrc, cySrc);

			try
			{
				GdiApi.BitBlt(
					hdcSource, 0, 0, cxSrc, cySrc,
					hdcScreen, xSrc, ySrc, ROP3.SrcCopy);
			}
			catch (Win32Exception e)
			{
#if DEBUG
				{
					bool isHdcSourceValid = GdiApi.GetPixel(hdcSource, 0, 0) != -1;
					bool isHdcScreenValid = GdiApi.GetPixel(hdcScreen, 0, 0) != -1;

					Debug.WriteLine("{3}: ScreenImageViewModel.Refresh: BitBlt failed: {0} isHdcSourceValid={1} isHdcScreenValid={2}",
						e.Message, isHdcSourceValid, isHdcScreenValid, s_invalidHandleCount++);
				}
#endif // DEBUG

				// Ignore (until it doesn't fail)
				// TODO: raise an event so that the MainViewModel can adjust its timer
				return;
			}
		}

		// Compute the origin and extents of the zoomed rectangle
		var zoomFactor = _viewSettings.Model.ZoomFactor;
		int cxDest = (int)(cxSrc * zoomFactor);
		int cyDest = (int)(cySrc * zoomFactor);

		// Stretch from the source bitmap buffer to our zoomed bitmap buffer
		var hdcZoomed = this.ZoomedBitmap.Model.DC!;
		using (GdiApi.SetStretchBltMode(hdcZoomed, StretchBltMode.ColorOnColor))
		{
			GdiApi.StretchBlt(
				hdcZoomed, 0, 0, cxDest, cyDest,
				hdcSource, 0, 0, cxSrc, cySrc, ROP3.SrcCopy);
		}

		// Draw the pixel grid as needed
		this.DrawPixelGrid(hdcZoomed, cxDest, cyDest);

		// Flush the GDI pipeline
		GdiApi.GdiFlush();

		// Indvalidate the BitmapViewModel objects
		this.SourceBitmap.InvalidateBitmapSource();
		this.ZoomedBitmap.InvalidateBitmapSource();

		// Reset the flag
		this.IsRefreshNeeded = false;

		// Indicate changed bitmaps
		this.OnPropertyChanged(nameof(ZoomedBitmap));
		this.OnPropertyChanged(nameof(SourceBitmap));
	}
	#endregion Methods

	#region Private Implementation
	private void DrawPixelGrid(SafeHdc hdc, int cxDest, int cyDest)
	{
		if (_viewSettings is null)
			return;

		// Draw the pixel grid onto the zoomed bitmap
		var zoomFactor = (int)_viewSettings.Model.ZoomFactor;
		if (_viewSettings.Model.IsGridVisibleWhenZoomed && zoomFactor > 1)
		{
			// Create the pen and select it into the HDC of the zoomed bitmap buffer
			// TODO: Since this is a private DC, only create/select the pen when pen properties change
			// TODO: Allow selectable pixel grid pen as a view setting.
			//using (GdiApi.SetBkMode(hdcZoomed, BackgroundMode.Opaque))
			//using (GdiApi.SetROP2(hdcZoomed, ROP2.CopyPen))
			//using (var hpen = GdiApi.CreatePen(PenStyle.Solid, 1, 0x00FFFFFF, null))
			//using (GdiApi.SelectObject(hdcZoomed, hpen))
			using (GdiApi.SelectObject(hdc, GdiApi.GetStockObject(StockObject.BlackPen)))
			{
				// Compute the number of lines to draw
				var horizontalLineCount = cyDest / (int)zoomFactor;
				var verticalLineCount = cxDest / (int)zoomFactor;
				var totalLineCount = horizontalLineCount + verticalLineCount;

				// Allocate arrays for the points in the lines and the count of points in each polyline
				POINT[] points = new POINT[totalLineCount * 2];
				int[] polyPoints = new int[totalLineCount];

				// Calculate the horizontal grid line points
				int i = 0;
				for (int y = zoomFactor - 1; y < cyDest; y += zoomFactor)
				{
					points[i].x = 0;
					points[i].y = y;
					++i;
					points[i].x = cxDest;
					points[i].y = y;
					++i;
				}

				// Calculate the vertical grid line points
				for (int x = zoomFactor - 1; x < cxDest; x += zoomFactor)
				{
					points[i].x = x;
					points[i].y = 0;
					++i;
					points[i].x = x;
					points[i].y = cyDest;
					++i;
				}

				// Draw all the lines at once
				for (i = 0; i < polyPoints.Length; ++i)
					polyPoints[i] = 2;
				GdiApi.PolyPolyline(hdc, points, polyPoints);
			}
		}
	}

	private void ResizeSourceBitmap() => _sourceBitmap.Size = _viewSettings!.Model.SourceSize;

	private void ResizeZoomedBitmap() => _zoomedBitmap.Size = _viewSettings!.Model.RenderSize;

	#endregion Private Implementation

	#region Event Handlers
	private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
	{
		if (msg.message == (int)WM.DisplayChange)
		{
			Debug.WriteLine("ScreenImageViewModel: WM_DISPLAYCHANGE wParam={0:X16} lParam={1:X16}", msg.wParam, msg.lParam);
		}
	}

	private void ViewSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ViewSettingsModel.IsGridVisibleWhenZoomed):
				this.IsRefreshNeeded = true;
				break;

			case nameof(ViewSettingsModel.RenderSize):
				this.ResizeSourceBitmap();
				this.ResizeZoomedBitmap();
				this.IsRefreshNeeded = true;
				break;

			case nameof(ViewSettingsModel.ZoomFactor):
				this.ResizeSourceBitmap();
				this.IsRefreshNeeded = true;
				break;
		}
	}
	#endregion Event Handlers
}
