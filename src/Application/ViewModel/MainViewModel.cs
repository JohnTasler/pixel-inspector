using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PixelInspector.Model;
using Tasler;
using Tasler.ComponentModel;
using Tasler.Interop.Gdi;
using Tasler.Text;

namespace PixelInspector.ViewModel;

public partial class MainViewModel : ObservableObject
{
	#region Instance Fields
	private readonly IHost _host;
	private bool _isInTimerRefresh;
	private DispatcherTimer? _refreshTimer;
	private BitmapModel? _sourceBitmapBuffer;
	private BitmapModel? _zoomedBitmapBuffer;
	private IPropertyObserverItem _screenImageObserver;
	#endregion Instance Fields

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="MainViewModel"/> class.
	/// </summary>
	public MainViewModel(IHost host, ViewSettingsViewModel viewSettingsViewModel, ScreenImageViewModel screenImageViewModel)
	{
		_host = host;

		// Initial state
		this.ApplicationState = new ApplicationStateLoading();

		// Get the latest view settings model from persistence
		// PixelInspector.Properties.Settings.Default.LatestViewSettings = viewSettings;
		this.ViewSettings = viewSettingsViewModel;

		// Create a scaling transform if the system is using a non-standard DPI
		var dpiScaleX = GdiApi.DpiScaleX;
		var dpiScaleY = GdiApi.DpiScaleY;
		if (dpiScaleX != 1.0 || dpiScaleY != 1.0)
		{
			this.DpiTransform = new ScaleTransform(dpiScaleX, dpiScaleY, 0.5, 0.5);
		}
		else
		{
			this.DpiTransform = Transform.Identity;
		}

		// Create the ScreenImageViewModel
		this.ScreenImage = screenImageViewModel;
		_screenImageObserver = this.ScreenImage.Subscribe(nameof(this.ScreenImage.IsRefreshNeeded),
			s =>
			{
				if (s.IsRefreshNeeded)
					this.RefreshBitmaps();
			});

		// Create the SelectionViewModel
		this.Selection = new SelectionViewModel(this);

		// Choose the Select tool
		Application.Current.Dispatcher.BeginInvoke(() =>
		{
			this.ChooseToolSelectCommand.Execute(null);
		});

		// Normal state
		this.ApplicationState = new ApplicationStateRunning();
	}
	#endregion Constructors

	#region Properties

	#region Models

	public BitmapModel? SourceBitmap => _sourceBitmapBuffer;

	public BitmapModel? ZoomedBitmap => _zoomedBitmapBuffer;

	#endregion Models

	#region Child View Models

	#region ViewSettings

	partial void OnViewSettingsChanged(ViewSettingsViewModel value)
	{
		if (_viewSettingsObservers is not null)
		{
			foreach (var observer in _viewSettingsObservers)
				observer.Unsubscribe();

			_viewSettingsObservers = null;
		}

		this.ResetRefreshTimer();

		_viewSettingsObservers =
		[
			value.Subscribe(nameof(_viewSettings.AutoRefreshInterval), s => this.ResetRefreshTimer()),
			value.Model.Subscribe(nameof(_viewSettings.Model.IsAutoRefreshing), s => this.ResetRefreshTimer()),
			value.Model.Subscribe(nameof(_viewSettings.Model.ZoomFactor), s => this.UpdateSourceMousePosition()),
		];
	}

	[ObservableProperty]
	private ViewSettingsViewModel _viewSettings;
	private List<IPropertyObserverItem>? _viewSettingsObservers;

	#endregion ViewSettings

	#region ScreenImage
	public ScreenImageViewModel ScreenImage { get; private set; }
	#endregion ScreenImage

	#region Selection
	public SelectionViewModel Selection { get; private set; }
	#endregion Selection

	#region ToolState

	partial void OnToolStateChanging(object? oldValue, object? newValue)
	{
		// Revert the current tool mode, if any
		if (oldValue is IToolMode oldToolMode)
			oldToolMode.ExitMode(true);

		// Enter the new tool mode, if any
		if (newValue is IToolMode newToolMode)
			newToolMode.EnterMode();
	}

	partial void OnToolStateChanged(object? oldValue, object? newValue)
	{
		// Set the tool as the new SourceOriginProvider, if it is one
		this.SourceOriginProvider = newValue as IProvideSourceOrigin ?? this.ViewSettings;

		// Update the source mouse position
		this.UpdateSourceMousePosition();
	}

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(IsToolStateLocate), nameof(IsToolStateLocating), nameof(IsToolStateMove), nameof(IsToolStateSelect))]
	private object? _toolState;

	public bool IsToolStateLocate => this.ToolState is LocateToolViewModel;

	public bool IsToolStateLocating => this.ToolState is LocatingToolViewModel;

	public bool IsToolStateMove => this.ToolState is MoveToolViewModel;

	public bool IsToolStateSelect => this.ToolState is SelectToolViewModel;

	#endregion ToolState

	#endregion Child View Models

	#region DpiTransform
	public Transform DpiTransform { get; private set; }
	#endregion DpiTransform

	#region ApplicationState

	partial void OnApplicationStateChanged(object? value)
	{
		if (value is ApplicationStateUnloading)
		{
			using (_sourceBitmapBuffer)
			using (_zoomedBitmapBuffer)
			{
				_sourceBitmapBuffer = null;
				_zoomedBitmapBuffer = null;
			}
		}
	}

	[ObservableProperty]
	private object? _applicationState;

	#endregion ApplicationState

	#region SourceOrigin
	public Point SourceOrigin
	{
		get
		{
			Debug.Assert(this.SourceOriginProvider is not null);
			return this.SourceOriginProvider.SourceOrigin;
		}
	}
	#endregion SourceOrigin

	#region SourceMousePosition
	[ObservableProperty]
	private Point _sourceMousePosition;
	#endregion SourceMousePosition

	#region IsMouseOverSelection
	[ObservableProperty]
	private bool _isMouseOverSelection;
	#endregion IsMouseOverSelection

	#region SourceMousePositionColor
	[ObservableProperty]
	private Color _sourceMousePositionColor;
	#endregion SourceMousePositionColor

	#region ZoomedMousePosition

	partial void OnZoomedMousePositionChanged(Point value)
	{
		this.UpdateSourceMousePosition();
	}

	[ObservableProperty]
	private Point _zoomedMousePosition;

	#endregion ZoomedMousePosition

	#region IsMouseInZoomedBounds
	[ObservableProperty]
	private bool _isMouseInZoomedBounds;
	#endregion IsMouseInZoomedBounds

	#endregion Properties

	#region Commands

	#region SaveAsCommand

	private bool CanSaveAs() => false;

	[RelayCommand(CanExecute = nameof(CanSaveAs))]
	private void SaveAs()
	{
	}

	#endregion SaveAsCommand

	#region PrintCommand

	private bool CanPrint()
	{
		return false;
	}

	[RelayCommand(CanExecute = nameof(CanPrint))]
	private void Print()
	{
	}

	#endregion PrintCommand

	#region ExitCommand

	[RelayCommand]
	private void Exit()
	{
		this.ApplicationState = new ApplicationStateUnloading();
	}

	#endregion ExitCommand

	#region CopyCommand

	private bool CanCopy()
	{
		return true;
	}

	[RelayCommand(CanExecute = nameof(CanCopy))]
	private void Copy()
	{
		var sb = new StringBuilder();

		if (this.ViewSettings.Model.ZoomFactor >= 1)
		{
			// TODO: Use the selection bitmap if there is a selection
			var bitmap = this.ScreenImage.SourceBitmap;
			bitmap.Model.GetSize(out var cx, out var cy);

			// Determine the pixel color value string format and format its maximum width
			// TODO: Could be user options
			var fieldSeparator = "\t";
			var format = "{0:X2}{1:X2}{2:X2}";
			var template = string.Format(fieldSeparator + format, byte.MaxValue, byte.MaxValue, byte.MaxValue);

			// Format the bitmap bits into a grid of RGB color values
			sb.EnsureCapacity(cx * cy * template.Length + 1);
			foreach (var pixelRow in bitmap.GetPixelRows())
			{
				foreach (var pixel in pixelRow)
					sb.AppendFormat(format, pixel.R, pixel.G, pixel.B).Append(fieldSeparator);
				sb.DiscardFromEnd(fieldSeparator);
				sb.AppendLine();
			}
			sb.DiscardLineFromEnd();
		}

		// Create and populate the data object to put onto the clipboard
		var dataObject = new DataObject();
		dataObject.SetImage(this.ScreenImage.SourceBitmap.BitmapSource);
		if (sb.Length > 0)
			dataObject.SetText(sb.ToString());

		// Put the data object onto the clipboard
		Clipboard.SetDataObject(dataObject);
	}

	#endregion CopyCommand

	#region RefreshCommand

	private bool CanRefresh()
	{
		return !this.ViewSettings.Model.IsAutoRefreshing;
	}

	[RelayCommand(CanExecute = nameof(CanRefresh))]
	private void Refresh()
	{
		this.RefreshBitmaps();
	}

	#endregion RefreshCommand

	#region ChooseToolLocateCommand
	[RelayCommand]
	private void ChooseToolLocate() => this.ToolState = this.LocateTool;
	#endregion ChooseToolLocateCommand

	#region ChooseToolLocatingCommand
	[RelayCommand]
	private void ChooseToolLocating(LocatingToolViewModel.Parameters parameters)
	{
		this.ToolState = _host.Services.GetService<LocatingToolViewModel>()?
			.Initialize(parameters);
	}
	#endregion ChooseToolLocatingCommand

	#region ChooseToolMoveCommand
	[RelayCommand]
	private void ChooseToolMove() => this.ToolState = this.MoveTool;
	#endregion ChooseToolMoveCommand

	#region ChooseToolSelectCommand
	[RelayCommand]
	private void ChooseToolSelect() => this.ToolState = this.SelectTool;
	#endregion ChooseToolSelectCommand

	#region ShowOptionsCommand
	[RelayCommand]
	private void ShowOptions()
	{
		System.Console.Beep(880, 200);
	}
	#endregion ShowOptionsCommand

	#region ShowAboutBoxCommand
	[RelayCommand]
	private static void ShowAboutBox()
	{
		System.Console.Beep(440, 200);
	}
	#endregion ShowAboutBoxCommand

	#region CollectGarbageCommand

	[RelayCommand]
	private static void CollectGarbage()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	#endregion CollectGarbageCommand

	#endregion Commands

	#region Private Implementation

	#region Private Properties

	private LocateToolViewModel LocateTool
		=> _locateTool ??= _host.Services.GetService <LocateToolViewModel>()!;
	private LocateToolViewModel? _locateTool;

	private MoveToolViewModel MoveTool
		=> _moveTool ??= _host.Services.GetService<MoveToolViewModel>()!;
	private MoveToolViewModel? _moveTool;

	private SelectToolViewModel SelectTool
		=> _selectTool ??= _host.Services.GetService<SelectToolViewModel>()!;
	private SelectToolViewModel? _selectTool;

	#region SourceOriginProvider

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(SourceOrigin))]
	private IProvideSourceOrigin? _sourceOriginProvider;
	private IPropertyObserverItem? _sourceOriginProviderObserver;

	partial void OnSourceOriginProviderChanged(IProvideSourceOrigin? oldValue, IProvideSourceOrigin? newValue)
	{
		Guard.IsNotNull(newValue);

		// Get the current provider's SourceOrigin
		Point? previousSourceOrigin = oldValue?.SourceOrigin;

		// Unsubscribe from property change events on the old value
		using (_sourceOriginProviderObserver)
		{
			_sourceOriginProviderObserver = null;
		}

		// Subscribe to property change events on the new value
		_sourceOriginProviderObserver = newValue.Subscribe(nameof(newValue.SourceOrigin), s =>
		{
			this.RefreshBitmaps();
			this.UpdateSourceMousePosition();
		});

		// Respond to a change in SourceOrigin, if any
		if (previousSourceOrigin != _sourceOriginProvider?.SourceOrigin && _sourceOriginProviderObserver is not null)
			_sourceOriginProviderObserver.Refresh();
	}

	#endregion SourceOriginProvider

	#endregion Private Properties

	private void RefreshBitmaps()
	{
		if (this.SourceOriginProvider is null)
			return;

		// Refresh the screen image
		this.ScreenImage.Refresh(this.SourceOrigin);

		// Update the mouse position and color
		this.UpdateSourceMousePosition();

		// Indicate changed bitmaps
		this.OnPropertyChanged(nameof(ZoomedBitmap));
		this.OnPropertyChanged(nameof(SourceBitmap));

		// Reset the refresh timer if something else caused a refresh
		if (!_isInTimerRefresh)
			this.ResetRefreshTimer();
	}

	private void ResetRefreshTimer()
	{
		CommandManager.InvalidateRequerySuggested();

		if (this.ViewSettings.Model.IsAutoRefreshing)
		{
			if (_refreshTimer is null)
			{
				_refreshTimer = new DispatcherTimer(this.ViewSettings.AutoRefreshInterval,
					DispatcherPriority.Render, this.RefreshTimer_Tick, Dispatcher.CurrentDispatcher);
			}
			else
			{
				if (_refreshTimer.Interval != this.ViewSettings.AutoRefreshInterval)
					_refreshTimer.Interval = this.ViewSettings.AutoRefreshInterval;
			}

			_refreshTimer.Start();
		}
		else
		{
			if (_refreshTimer is not null)
			{
				_refreshTimer.Stop();
				_refreshTimer.Tick -= this.RefreshTimer_Tick;
				_refreshTimer = null;
			}
		}
	}

	private void UpdateSourceMousePosition()
	{
		var zoomFactor = this.ViewSettings.Model.ZoomFactor;
		var zoomedMousePosition = this.ZoomedMousePosition;
		var x = (int)(zoomedMousePosition.X / zoomFactor);
		var y = (int)(zoomedMousePosition.Y / zoomFactor);

		var sourceMousePosition = this.SourceOrigin;
		sourceMousePosition.Offset(x, y);
		this.SourceMousePosition = sourceMousePosition;

		// Update the SourceMousePositionColor property
		this.SourceMousePositionColor = this.IsMouseInZoomedBounds
			? this.ScreenImage.SourceBitmap.GetPixelColor(x, y)
			: Colors.Transparent;

		// Update the IsMouseOverSelection property
		this.UpdateIsMouseOverSelection();
	}

	private void UpdateIsMouseOverSelection()
	{
		this.IsMouseOverSelection = this.Selection.ZoomedRectangle.HasValue
			&& this.Selection.ZoomedRectangle.Value.Contains(this.ZoomedMousePosition);
	}

	#endregion Private Implementation

	#region Event Handlers

	private void RefreshTimer_Tick(object? sender, EventArgs e)
	{
		using var refreshScope = new DisposeScopeExit(
			() => _isInTimerRefresh = true,
			() => _isInTimerRefresh = false);

		this.RefreshBitmaps();
	}

	#endregion Event Handlers
}
