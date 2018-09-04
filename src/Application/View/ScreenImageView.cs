namespace ZoomIn.View
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Media;
	using ZoomIn.ViewModel;

	public class ScreenImageView : Panel
	{
		#region Instance Fields
		private bool isRendering;
		#endregion Instance Fields

		#region Constructors
		public ScreenImageView()
		{
			this.DataContextChanged += this.RenderPanel_DataContextChanged;
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
			if (this.ViewModel == null)
				return;

			this.isRendering = true;
			this.ViewModel.Parent.ViewSettings.Model.RenderSize = this.RenderSize;
			this.isRendering = false;

			// TODO: Account for DPI, etc.
			// TODO: Figure out how to prevent multiple redraws of same size (if that is actually happening)
			var renderSize = //this.RenderSize;
				this.ViewModel.Parent.ViewSettings.Model.RenderSize; // = renderSize;

			// Determine which bitmap to used based on the zoom factor
			var zoomFactor = this.ViewModel.Parent.ViewSettings.Model.ZoomFactor;
			var isZoomedIn = zoomFactor > 1;
			var bitmapViewModel = isZoomedIn ? this.ViewModel.ZoomedBitmap : this.ViewModel.SourceBitmap;
			if (bitmapViewModel != null)
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

		#region Private Implementation
		private ScreenImageViewModel ViewModel { get; set; }
		#endregion Private Implementation

		#region Event Handlers
		private void RenderPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var oldViewModel = e.OldValue as ScreenImageViewModel;
			if (oldViewModel != null)
			{
				oldViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
			}

			this.ViewModel = e.NewValue as ScreenImageViewModel;
			if (this.ViewModel != null)
			{
				this.ViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
			}

			this.InvalidateVisual();
		}

		private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "ZoomedBitmap":
					if (!this.isRendering)
						this.InvalidateVisual();
					break;
			}
		}
		#endregion Event Handlers

	}
}
