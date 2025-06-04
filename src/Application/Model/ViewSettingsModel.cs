using System.ComponentModel;
using System.Windows;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Tasler.Windows.Model;

namespace PixelInspector.Model;

public partial class ViewSettingsModel : ObservableObject
{
	#region Constants
	private const int c_defaultWidth = 525;
	private const int c_defaultHeight = 350;
	private const double c_defaultAutoRefreshIntervalMilliseconds = 200;
	private const double c_defaultZoomFactor = 6;
	private const ColorValueDisplayFormat c_defaultColorValueDisplayFormat = ColorValueDisplayFormat.Hex;
	#endregion Constants

	#region Constructors
	public ViewSettingsModel()
	{
		this.WindowPlacement = new(c_defaultWidth, c_defaultHeight);
	}
	#endregion Constructors

	#region Properties

	[ObservableProperty]
	[property: XmlAttribute]
	private double _autoRefreshMilliseconds = c_defaultAutoRefreshIntervalMilliseconds;

	[ObservableProperty]
	[property: XmlAttribute]
	private ColorValueDisplayFormat _colorValueDisplayFormat = c_defaultColorValueDisplayFormat;

	[ObservableProperty]
	[property: XmlAttribute]
	private bool _isAlwaysOnTop;

	[ObservableProperty]
	[property: XmlAttribute]
	private bool _isAutoRefreshing = true;

	[ObservableProperty]
	[property: XmlAttribute]
	private bool _isGridVisibleWhenZoomed = true;

	[ObservableProperty]
	[property: XmlAttribute]
	private bool _isMenuAlwaysVisible = true;

	[ObservableProperty]
	[property: XmlAttribute]
	private bool _isStatusBarVisible = true;

	[ObservableProperty]
	[property: XmlIgnore]
	private Size _renderSize;

	[XmlAttribute]
	public double SourceX
	{
		get => this.SourceOrigin.X;
		set => this.SourceOrigin = new(value, this.SourceOrigin.Y);
	}

	[XmlAttribute]
	public double SourceY
	{
		get => this.SourceOrigin.Y;
		set => this.SourceOrigin = new(this.SourceOrigin.X, value);
	}

	[ObservableProperty]
	[property: XmlIgnore]
	private Point _sourceOrigin = new(0, 0);

	public Size SourceSize
	{
		get
		{
			var cx = (int)(this.RenderSize.Width / this.ZoomFactor) + 1;
			var cy = (int)(this.RenderSize.Height / this.ZoomFactor) + 1;
			return new Size(cx, cy);
		}
	}

	[ObservableProperty]
	[property: XmlElement]
	private WindowPlacementModel _windowPlacement;

	partial void OnWindowPlacementChanged(WindowPlacementModel? oldValue, WindowPlacementModel newValue)
	{
		if (oldValue is not null)
			oldValue.PropertyChanged -= this.WindowPlacement_PropertyChanged;

		if (newValue is not null)
			newValue.PropertyChanged += this.WindowPlacement_PropertyChanged;
	}

	[ObservableProperty]
	[property: XmlAttribute]
	private double _zoomFactor = c_defaultZoomFactor;
	#endregion Properties

	#region Event Handlers
	private void WindowPlacement_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		this.OnPropertyChanged(nameof(this.WindowPlacement));
	}
	#endregion Event Handlers
}
