namespace ZoomIn.Model
{
	using System.ComponentModel;
	using System.Windows;
	using System.Xml.Serialization;
	using ZoomIn.ComponentModel.Mvvm;

	public class ViewSettingsModel : ObservableObject
	{
		#region Constants
		private const double defaultAutoRefreshIntervalMilliseconds = 200;
		private const double defaultZoomFactor = 6;
		#endregion Constants

		#region Constructors
		public ViewSettingsModel()
		{
		}
		#endregion Constructors

		#region Properties

		[XmlAttribute]
		public double AutoRefreshMilliseconds
		{
			get { return this.autoRefreshMilliseconds; }
			set { this.SetProperty(ref this.autoRefreshMilliseconds, value, nameof(this.AutoRefreshMilliseconds)); }
		}
		private double autoRefreshMilliseconds = defaultAutoRefreshIntervalMilliseconds;

		[XmlAttribute]
		public ColorValueDisplayFormat ColorValueDisplayFormat
		{
			get { return this.colorValueDisplayFormat; }
			set { this.SetProperty(ref this.colorValueDisplayFormat, value, nameof(this.ColorValueDisplayFormat)); }
		}
		private ColorValueDisplayFormat colorValueDisplayFormat = ColorValueDisplayFormat.Hex;

		[XmlAttribute]
		public bool IsAlwaysOnTop
		{
			get { return this.isAlwaysOnTop; }
			set { this.SetProperty(ref this.isAlwaysOnTop, value, nameof(this.IsAlwaysOnTop)); }
		}
		private bool isAlwaysOnTop;

		[XmlAttribute]
		public bool IsAutoRefreshing
		{
			get { return this.isAutoRefreshing; }
			set { this.SetProperty(ref this.isAutoRefreshing, value, nameof(this.IsAutoRefreshing)); }
		}
		private bool isAutoRefreshing = true;

		[XmlAttribute]
		public bool IsGridVisibleWhenZoomed
		{
			get { return this.isGridVisibleWhenZoomed; }
			set { this.SetProperty(ref this.isGridVisibleWhenZoomed, value, nameof(this.IsGridVisibleWhenZoomed)); }
		}
		private bool isGridVisibleWhenZoomed = true;

		[XmlAttribute]
		public bool IsMenuAlwaysVisible
		{
			get { return this.isMenuAlwaysVisible; }
			set { this.SetProperty(ref this.isMenuAlwaysVisible, value, nameof(this.IsMenuAlwaysVisible)); }
		}
		private bool isMenuAlwaysVisible = true;

		[XmlAttribute]
		public bool IsStatusBarVisible
		{
			get { return this.isStatusBarVisible; }
			set { this.SetProperty(ref this.isStatusBarVisible, value, nameof(this.IsStatusBarVisible)); }
		}
		private bool isStatusBarVisible = true;

		[XmlIgnore]
		public Size RenderSize
		{
			get { return this.renderSize; }
			set { this.SetProperty(ref this.renderSize, value, nameof(this.SourceSize), nameof(this.RenderSize)); }
		}
		private Size renderSize;

		[XmlAttribute]
		public double SourceX
		{
			get { return this.SourceOrigin.X; }
			set { this.SourceOrigin = new Point(value, this.SourceOrigin.Y); }
		}

		[XmlAttribute]
		public double SourceY
		{
			get { return this.SourceOrigin.Y; }
			set { this.SourceOrigin = new Point(this.SourceOrigin.X, value); }
		}

		[XmlIgnore]
		public Point SourceOrigin
		{
			get { return this.sourceOrigin; }
			set { this.SetProperty(ref this.sourceOrigin, value, nameof(this.SourceOrigin)); }
		}
		private Point sourceOrigin = new Point(0, 0);

		public Size SourceSize
		{
			get
			{
				var cx = (int)(this.renderSize.Width / this.ZoomFactor) + 1;
				var cy = (int)(this.renderSize.Height / this.ZoomFactor) + 1;
				return new Size(cx, cy);
			}
		}

		[XmlElement]
		public WindowPlacementModel WindowPlacement
		{
			get { return this.windowPlacement; }
			set
			{
				if (this.windowPlacement != value)
				{
					if (this.windowPlacement != null)
						this.windowPlacement.PropertyChanged -= this.WindowPlacement_PropertyChanged;

					this.windowPlacement = value;

					if (this.windowPlacement != null)
						this.windowPlacement.PropertyChanged += this.WindowPlacement_PropertyChanged;

					this.WindowPlacement_PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.WindowPlacement)));
				}
			}
		}
		private WindowPlacementModel windowPlacement;

		[XmlAttribute]
		public double ZoomFactor
		{
			get { return this.zoomFactor; }
			set { this.SetProperty(ref this.zoomFactor, value, nameof(this.ZoomFactor)); }
		}
		private double zoomFactor = defaultZoomFactor;
		#endregion Properties

		#region Event Handlers
		private void WindowPlacement_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.RaisePropertyChanged(nameof(this.WindowPlacement));
		}
		#endregion Event Handlers
	}
}
