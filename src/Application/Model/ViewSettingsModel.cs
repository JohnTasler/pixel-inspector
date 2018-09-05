namespace PixelInspector.Model
{
	using System.ComponentModel;
	using System.Windows;
	using System.Xml.Serialization;
	using Tasler.ComponentModel;

	public class ViewSettingsModel : ViewModel
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
			get { return _autoRefreshMilliseconds; }
			set { this.PropertyChanged.SetProperty(this, value, ref _autoRefreshMilliseconds, nameof(this.AutoRefreshMilliseconds)); }
		}
		private double _autoRefreshMilliseconds = defaultAutoRefreshIntervalMilliseconds;

		[XmlAttribute]
		public ColorValueDisplayFormat ColorValueDisplayFormat
		{
			get { return _colorValueDisplayFormat; }
			set { this.PropertyChanged.SetProperty(this, value, ref _colorValueDisplayFormat, nameof(this.ColorValueDisplayFormat)); }
		}
		private ColorValueDisplayFormat _colorValueDisplayFormat = ColorValueDisplayFormat.Hex;

		[XmlAttribute]
		public bool IsAlwaysOnTop
		{
			get { return _isAlwaysOnTop; }
			set { this.PropertyChanged.SetProperty(this, value, ref _isAlwaysOnTop, nameof(this.IsAlwaysOnTop)); }
		}
		private bool _isAlwaysOnTop;

		[XmlAttribute]
		public bool IsAutoRefreshing
		{
			get { return _isAutoRefreshing; }
			set { this.PropertyChanged.SetProperty(this, value, ref _isAutoRefreshing, nameof(this.IsAutoRefreshing)); }
		}
		private bool _isAutoRefreshing = true;

		[XmlAttribute]
		public bool IsGridVisibleWhenZoomed
		{
			get { return _isGridVisibleWhenZoomed; }
			set { this.PropertyChanged.SetProperty(this, value, ref _isGridVisibleWhenZoomed, nameof(this.IsGridVisibleWhenZoomed)); }
		}
		private bool _isGridVisibleWhenZoomed = true;

		[XmlAttribute]
		public bool IsMenuAlwaysVisible
		{
			get { return _isMenuAlwaysVisible; }
			set { this.PropertyChanged.SetProperty(this, value, ref _isMenuAlwaysVisible, nameof(this.IsMenuAlwaysVisible)); }
		}
		private bool _isMenuAlwaysVisible = true;

		[XmlAttribute]
		public bool IsStatusBarVisible
		{
			get { return _isStatusBarVisible; }
			set { this.PropertyChanged.SetProperty(this, value, ref _isStatusBarVisible, nameof(this.IsStatusBarVisible)); }
		}
		private bool _isStatusBarVisible = true;

		[XmlIgnore]
		public Size RenderSize
		{
			get { return _renderSize; }
			set { this.PropertyChanged.SetProperty(this, value, ref _renderSize, nameof(this.SourceSize), nameof(this.RenderSize)); }
		}
		private Size _renderSize;

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
			get { return _sourceOrigin; }
			set { this.PropertyChanged.SetProperty(this, value, ref _sourceOrigin, nameof(this.SourceOrigin)); }
		}
		private Point _sourceOrigin = new Point(0, 0);

		public Size SourceSize
		{
			get
			{
				var cx = (int)(_renderSize.Width / this.ZoomFactor) + 1;
				var cy = (int)(_renderSize.Height / this.ZoomFactor) + 1;
				return new Size(cx, cy);
			}
		}

		[XmlElement]
		public WindowPlacementModel WindowPlacement
		{
			get { return _windowPlacement; }
			set
			{
				if (_windowPlacement != value)
				{
					if (_windowPlacement != null)
						((INotifyPropertyChanged)_windowPlacement).PropertyChanged -= this.WindowPlacement_PropertyChanged;

					_windowPlacement = value;

					if (_windowPlacement != null)
						((INotifyPropertyChanged)_windowPlacement).PropertyChanged += this.WindowPlacement_PropertyChanged;

					this.WindowPlacement_PropertyChanged(this, new PropertyChangedEventArgs(nameof(this.WindowPlacement)));
				}
			}
		}
		private WindowPlacementModel _windowPlacement;

		[XmlAttribute]
		public double ZoomFactor
		{
			get { return _zoomFactor; }
			set { this.PropertyChanged.SetProperty(this, value, ref _zoomFactor, nameof(this.ZoomFactor)); }
		}
		private double _zoomFactor = defaultZoomFactor;
		#endregion Properties

		#region Event Handlers
		private void WindowPlacement_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.PropertyChanged.Raise(this, nameof(this.WindowPlacement));
		}
		#endregion Event Handlers
	}
}
