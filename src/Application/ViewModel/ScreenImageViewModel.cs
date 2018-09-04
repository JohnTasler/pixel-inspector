namespace ZoomIn.ViewModel
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Interop;
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Interop;
	using ZoomIn.Interop.Gdi;
	using ZoomIn.Interop.User;

	public class ScreenImageViewModel : ParentedObservableObject<MainViewModel>
	{
		#region Static Fields
			#if DEBUG
				private static int invalidHandleCount;
			#endif // DEBUG
		#endregion Static Fields

		#region Instance Fields
		private ViewSettingsViewModel viewSettings;
		#endregion Instance Fields

		#region Constructors
		public ScreenImageViewModel(MainViewModel parent)
			: base(parent)
		{
			this.Parent_PropertyChanged(parent, new PropertyChangedEventArgs(nameof(parent.ViewSettings)));

			ComponentDispatcher.ThreadFilterMessage += this.ComponentDispatcher_ThreadPreprocessMessage;
		}
		#endregion Constructors

		#region Properties
		public bool IsRefreshNeeded
		{
			get { return this.isRefreshNeeded; }
			set { this.SetProperty(ref this.isRefreshNeeded, value, nameof(IsRefreshNeeded)); }
		}
		private bool isRefreshNeeded;

		public BitmapViewModel SourceBitmap
		{
			get { return this.sourceBitmap; }
		}
		private BitmapViewModel sourceBitmap = new BitmapViewModel();

		public BitmapViewModel ZoomedBitmap
		{
			get { return this.zoomedBitmap; }
		}
		private BitmapViewModel zoomedBitmap = new BitmapViewModel();
		#endregion Properties

		#region Methods
		public void Refresh(Point sourceOrigin)
		{
			// Compute the origin and extents of the source rectangle
			var sourceSize = this.viewSettings.Model.SourceSize;
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
			var hdcSource = this.SourceBitmap.Model.DC;
			using (var hdcScreen = UserApi.GetDC(IntPtr.Zero))
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
							e.Message, isHdcSourceValid, isHdcScreenValid, invalidHandleCount++);
					}
					#endif // DEBUG

					// Ignore (until it doesn't fail)
					// TODO: raise an event so that the MainViewModel can adjust its timer
					return;
				}
			}

			// Compute the origin and extents of the zoomed rectangle
			var zoomFactor = this.viewSettings.Model.ZoomFactor;
			int cxDest = (int)(cxSrc * zoomFactor);
			int cyDest = (int)(cySrc * zoomFactor);

			// Stretch from the source bitmap buffer to our zoomed bitmap buffer
			var hdcZoomed = this.ZoomedBitmap.Model.DC;
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
			this.RaisePropertyChanged(nameof(ZoomedBitmap), nameof(SourceBitmap));
		}
		#endregion Methods

		#region Private Implementation
		private void DrawPixelGrid(SafeHdc hdc, int cxDest, int cyDest)
		{
			// Draw the pixel grid onto the zoomed bitmap
			var zoomFactor = (int)this.viewSettings.Model.ZoomFactor;
			if (this.viewSettings.Model.IsGridVisibleWhenZoomed && zoomFactor > 1)
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

		private void ResizeSourceBitmap()
		{
			// Resize the SourceBitmap
			this.sourceBitmap.Size = this.viewSettings.Model.SourceSize;
		}

		private void ResizeZoomedBitmap()
		{
			// Resize the ZoomedBitmap
			this.zoomedBitmap.Size = this.viewSettings.Model.RenderSize;
		}
		#endregion Private Implementation

		#region Event Handlers
		private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
		{
			if (msg.message == (int)WM.DisplayChange)
			{
				Debug.WriteLine("ScreenImageViewModel: WM_DISPLAYCHANGE wParam={0:X16} lParam={1:X16}", msg.wParam, msg.lParam);
			}
		}

		private void Parent_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(this.Parent.ViewSettings):
					if (this.viewSettings != null)
						this.viewSettings.Model.PropertyChanged -= this.viewSettings_PropertyChanged;

					this.viewSettings = this.Parent.ViewSettings;

					if (this.viewSettings != null)
					{
						this.viewSettings.Model.PropertyChanged += this.viewSettings_PropertyChanged;
						this.viewSettings_PropertyChanged(this.viewSettings.Model, new PropertyChangedEventArgs(nameof(this.viewSettings.Model.RenderSize)));
					}
					break;
			}
		}

		private void viewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(this.viewSettings.Model.IsGridVisibleWhenZoomed):
					this.IsRefreshNeeded = true;
					break;

				case nameof(this.viewSettings.Model.RenderSize):
					this.ResizeSourceBitmap();
					this.ResizeZoomedBitmap();
					this.IsRefreshNeeded = true;
					break;

				case nameof(this.viewSettings.Model.ZoomFactor):
					this.ResizeSourceBitmap();
					this.IsRefreshNeeded = true;
					break;
			}
		}
		#endregion Event Handlers
	}
}
