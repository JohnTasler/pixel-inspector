using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelInspector.ViewModel;
using	Tasler.ComponentModel;
using	Tasler.Windows;

namespace PixelInspector.View;

public class ScreenImageView : Panel, INotifyPropertyChanged
{
	#region Instance Fields
	private bool _isRendering;
	private ViewSettingsViewModel _viewSettings;
	#endregion Instance Fields

	#region Constructors
	public ScreenImageView()
	{
	}

	public ScreenImageView(ScreenImageViewModel viewModel, ViewSettingsViewModel viewSettings)
	{
		_viewSettings = viewSettings;
		this.HookDataContextAsViewModel(() => this.PropertyChanged?.Raise(this, nameof(this.ViewModel)));
		this.DataContext = viewModel;
	}
	#endregion Constructors

	#region Overrides
	protected override void OnInitialized(EventArgs e)
	{
		// Initialize Visual rendering properties
		base.VisualBitmapScalingMode = BitmapScalingMode.HighQuality;

		// Perform default processing
		base.OnInitialized(e);
	}

	protected override void OnRender(DrawingContext dc)
	{
		if (this.ViewModel is null)
			return;

		this._isRendering = true;
		_viewSettings.Model.RenderSize = this.RenderSize;
		this._isRendering = false;

		// TODO: Account for DPI, etc.
		// TODO: Figure out how to prevent multiple redraws of same size (if that is actually happening)
		var renderSize = //this.RenderSize;
			_viewSettings.Model.RenderSize; // = renderSize;

		// Determine which bitmap to used based on the zoom factor
		var zoomFactor = _viewSettings.Model.ZoomFactor;
		var isZoomedIn = zoomFactor > 1;
		var bitmapViewModel = isZoomedIn ? this.ViewModel.ZoomedBitmap : this.ViewModel.SourceBitmap;
		if (bitmapViewModel is not null)
		{
			var bitmapSource = bitmapViewModel.BitmapSource;

			// Determine which size to use based on the zoom factor
			var rect = new Rect(0, 0, bitmapSource.Width, bitmapSource.Height);
			if (zoomFactor < 1)
			{
				rect.Width *= zoomFactor;
				rect.Height *= zoomFactor;
			}

			// Draw the source bitmap
			dc.DrawImage(bitmapSource, rect);
		}
	}
	#endregion Overrides

	#region Events
	public event PropertyChangedEventHandler? PropertyChanged;
	#endregion Events

	#region Properties
	public ScreenImageViewModel ViewModel => (ScreenImageViewModel)this.DataContext;
	#endregion Properties

	#region Event Handlers
	private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(ScreenImageViewModel.ZoomedBitmap):
				if (!this._isRendering)
					this.InvalidateVisual();
				break;
		}
	}
	#endregion Event Handlers

}
