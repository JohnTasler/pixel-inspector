namespace PixelInspector.ViewModel
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Threading;
	using PixelInspector.Interop.Gdi;
	using PixelInspector.Model;
	using Tasler;
	using Tasler.ComponentModel;

	public class MainViewModel : ViewModel
	{
		#region Instance Fields
		private bool _isInTimerRefresh;
		private DispatcherTimer _refreshTimer;
		private BitmapModel _sourceBitmapBuffer;
		private BitmapModel _zoomedBitmapBuffer;
		#endregion Instance Fields

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="MainViewModel"/> class.
		/// </summary>
		public MainViewModel()
		{
			// Initial state
			this.ApplicationState = new ApplicationStateLoading();

			// Get the latest view settings model from persistence
			if (PixelInspector.Properties.Settings.Default.LatestViewSettings == null)
				PixelInspector.Properties.Settings.Default.LatestViewSettings = new ViewSettingsModel();
			this.ViewSettings = new ViewSettingsViewModel(this, PixelInspector.Properties.Settings.Default.LatestViewSettings);

			// Create a scaling transform if the system is using a non-standard DPI
			var dpiScaleX = GdiApi.DpiScaleX;
			var dpiScaleY = GdiApi.DpiScaleY;
			if (dpiScaleX != 1.0 || dpiScaleY != 1.0)
				this.DpiTransform = new ScaleTransform(dpiScaleX, dpiScaleY, 0.5, 0.5);

			// Create the ScreenImageViewModel
			this.ScreenImage = new ScreenImageViewModel(this);
			this.ScreenImage.Subscribe(nameof(this.ScreenImage.IsRefreshNeeded),
				s =>
				{
					if (s.IsRefreshNeeded)
						this.RefreshBitmaps();
				});

			// Create the SelectionViewModel
			this.Selection = new SelectionViewModel(this);

			// Choose the Select tool
			this.ChooseToolSelectCommand.Execute(null);

			// Normal state
			this.ApplicationState = new ApplicationStateRunning();
		}
		#endregion Constructors

		#region Properties

		#region Models
		public BitmapModel SourceBitmap
		{
			get { return _sourceBitmapBuffer; }
		}

		public BitmapModel ZoomedBitmap
		{
			get { return _zoomedBitmapBuffer; }
		}
		#endregion Models

		#region Child View Models

		#region ViewSettings
		public ViewSettingsViewModel ViewSettings
		{
			get { return _viewSettings; }
			private set
			{
				var oldValue = _viewSettings;
				if (this.PropertyChanged.SetProperty(this, value, ref _viewSettings))
				{
					if (_viewSettingsObservers != null)
					{
						foreach (var observer in _viewSettingsObservers)
							observer.Unsubscribe();
						_viewSettingsObservers = null;
					}

					this.ResetRefreshTimer();

					if (_viewSettings != null)
					{
						_viewSettingsObservers = new List<IPropertyObserverItem>
						{
							_viewSettings.Subscribe(nameof(_viewSettings.AutoRefreshInterval), s => this.ResetRefreshTimer()),
							_viewSettings.Model.Subscribe(nameof(_viewSettings.Model.IsAutoRefreshing), s => this.ResetRefreshTimer()),
							_viewSettings.Model.Subscribe(nameof(_viewSettings.Model.ZoomFactor), s => this.UpdateSourceMousePosition()),
						};
					}
				}
			}
		}
		private ViewSettingsViewModel _viewSettings;
		private List<IPropertyObserverItem> _viewSettingsObservers;
		#endregion ViewSettings

		#region ScreenImage
		public ScreenImageViewModel ScreenImage { get; private set; }
		#endregion ScreenImage

		#region Selection
		public SelectionViewModel Selection { get; private set; }
		#endregion Selection

		#region ToolState
		public object ToolState
		{
			get { return this.toolState; }
			internal set
			{
				if (this.toolState != value)
				{
					// Revert the current tool mode, if any
					var toolMode = this.toolState as IToolMode;
					if (toolMode != null)
						toolMode.ExitMode(true);

					// Enter the new tool mode, if any
					toolMode = value as IToolMode;
					if (toolMode != null)
						toolMode.EnterMode();

					// Save the new value
					this.toolState = value;

					// Set the tool as the new SourceOriginProvider, if it is one
					this.SourceOriginProvider = this.toolState as IProvideSourceOrigin ?? this.ViewSettings;

					// Raise property change events
					this.PropertyChanged.Raise(this, nameof(ToolState), nameof(IsToolStateLocate), nameof(IsToolStateMove), nameof(IsToolStateSelect));

					// Update the source mouse position
					this.UpdateSourceMousePosition();
				}
			}
		}
		private object toolState;

		public bool IsToolStateLocate
		{
			get { return this.ToolState is LocateToolViewModel; }
		}

		public bool IsToolStateLocating
		{
			get { return this.ToolState is LocatingToolViewModel; }
		}

		public bool IsToolStateMove
		{
			get { return this.ToolState is MoveToolViewModel; }
		}

		public bool IsToolStateSelect
		{
			get { return this.ToolState is SelectToolViewModel; }
		}
		#endregion ToolState

		#endregion Child View Models

		public Transform DpiTransform { get; private set; }

		public object ApplicationState
		{
			get { return _applicationState; }
			private set
			{
				if (this.PropertyChanged.SetProperty(this, value, ref _applicationState) &&
					_applicationState is ApplicationStateUnloading)
				{
					using (_sourceBitmapBuffer)
					using (_zoomedBitmapBuffer)
					{
						_sourceBitmapBuffer = null;
						_zoomedBitmapBuffer = null;
					}
				}
			}
		}
		private object _applicationState;

		public Point SourceOrigin
		{
			get
			{
				Debug.Assert(this.SourceOriginProvider != null);
				return this.SourceOriginProvider.SourceOrigin;
			}
		}

		public Point SourceMousePosition
		{
			get { return _sourceMousePosition; }
			private set { this.PropertyChanged.SetProperty(this, value, ref _sourceMousePosition); }
		}
		private Point _sourceMousePosition;

		public bool IsMouseOverSelection
		{
			get { return _isMouseOverSelection; }
			private set { this.PropertyChanged.SetProperty(this, value, ref _isMouseOverSelection); }
		}
		private bool _isMouseOverSelection;

		public Color SourceMousePositionColor
		{
			get { return _sourceMousePositionColor; }
			private set { this.PropertyChanged.SetProperty(this, value, ref _sourceMousePositionColor); }
		}
		private Color _sourceMousePositionColor;

		public Point ZoomedMousePosition
		{
			get { return _zoomedMousePosition; }
			set
			{
				if (this.PropertyChanged.SetProperty(this, value, ref _zoomedMousePosition))
					this.UpdateSourceMousePosition();
			}
		}
		private Point _zoomedMousePosition;

		public bool IsMouseInZoomedBounds
		{
			get { return _isMouseInZoomedBounds; }
			set { this.PropertyChanged.SetProperty(this, value, ref _isMouseInZoomedBounds); }
		}
		private bool _isMouseInZoomedBounds;

		#endregion Properties

		#region Commands

		#region SaveAsCommand

		/// <summary>
		/// Gets the SaveAs command.
		/// </summary>
		public ICommand SaveAsCommand
		{
			get
			{
				return _saveAsCommand ??
					(_saveAsCommand = new RelayCommand(this.SaveAsCommandExecute, this.SaveAsCommandCanExecute));
			}
		}
		private RelayCommand _saveAsCommand;

		private bool SaveAsCommandCanExecute()
		{
			return false;
		}

		private void SaveAsCommandExecute()
		{
		}

		#endregion SaveAsCommand

		#region PrintCommand

		/// <summary>
		/// Gets the Print command.
		/// </summary>
		public ICommand PrintCommand
		{
			get
			{
				return _printCommand ??
					(_printCommand = new RelayCommand(this.PrintCommandExecute, this.PrintCommandCanExecute));
			}
		}
		private RelayCommand _printCommand;

		private bool PrintCommandCanExecute()
		{
			return false;
		}

		private void PrintCommandExecute()
		{
		}

		#endregion PrintCommand

		#region ExitCommand

		/// <summary>
		/// Gets the Exit command.
		/// </summary>
		public ICommand ExitCommand
		{
			get
			{
				return _exitCommand ??
					(_exitCommand = new RelayCommand(this.ExitCommandExecute));
			}
		}
		private RelayCommand _exitCommand;

		private void ExitCommandExecute()
		{
			this.ApplicationState = new ApplicationStateUnloading();
		}

		#endregion ExitCommand

		#region CopyCommand

		/// <summary>
		/// Gets the Copy command.
		/// </summary>
		public ICommand CopyCommand
		{
			get
			{
				return _copyCommand ??
					(_copyCommand = new RelayCommand(this.CopyCommandExecute, this.CopyCommandCanExecute));
			}
		}
		private RelayCommand _copyCommand;

		private bool CopyCommandCanExecute()
		{
			return true;
		}

		private void CopyCommandExecute()
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
					sb.Discard(fieldSeparator);
					sb.AppendLine();
				}
				sb.DiscardLine();
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

		/// <summary>
		/// Gets the Refresh command.
		/// </summary>
		public ICommand RefreshCommand
		{
			get
			{
				return _refreshCommand ??
					(_refreshCommand = new RelayCommand(this.RefreshCommandExecute, this.RefreshCommandCanExecute));
			}
		}
		private RelayCommand _refreshCommand;

		private bool RefreshCommandCanExecute()
		{
			return !this.ViewSettings.Model.IsAutoRefreshing;
		}

		private void RefreshCommandExecute()
		{
			this.RefreshBitmaps();
		}

		#endregion RefreshCommand

		#region ChooseToolLocateCommand

		/// <summary>
		/// Gets the ChooseToolLocate command.
		/// </summary>
		public ICommand ChooseToolLocateCommand
		{
			get
			{
				return _chooseToolLocateCommand ??
					(_chooseToolLocateCommand = new RelayCommand(this.ChooseToolLocateCommandExecute));
			}
		}
		private RelayCommand _chooseToolLocateCommand;

		private void ChooseToolLocateCommandExecute()
		{
			this.ToolState = new LocateToolViewModel(this);
		}

		#endregion ChooseToolLocateCommand

		#region ChooseToolLocatingCommand

		/// <summary>
		/// Gets the ChooseToolLocating command.
		/// </summary>
		public ICommand ChooseToolLocatingCommand
		{
			get
			{
				return _chooseToolLocatingCommand ??
					(_chooseToolLocatingCommand = new RelayCommand<LocatingToolViewModel.Parameters>(this.ChooseToolLocatingCommandExecute));
			}
		}
		private RelayCommand<LocatingToolViewModel.Parameters> _chooseToolLocatingCommand;

		private void ChooseToolLocatingCommandExecute(LocatingToolViewModel.Parameters parameters)
		{
			this.ToolState = new LocatingToolViewModel(this, parameters);
		}

		#endregion ChooseToolLocatingCommand

		#region ChooseToolMoveCommand

		/// <summary>
		/// Gets the ChooseToolMove command.
		/// </summary>
		public ICommand ChooseToolMoveCommand
		{
			get
			{
				return _chooseToolMoveCommand ??
					(_chooseToolMoveCommand = new RelayCommand(this.ChooseToolMoveCommandExecute));
			}
		}
		private RelayCommand _chooseToolMoveCommand;

		private void ChooseToolMoveCommandExecute()
		{
			this.ToolState = this.MoveTool;
		}

		#endregion ChooseToolMoveCommand

		#region ChooseToolSelectCommand

		/// <summary>
		/// Gets the ChooseToolSelect command.
		/// </summary>
		public ICommand ChooseToolSelectCommand
		{
			get
			{
				return _chooseToolSelectCommand ??
					(_chooseToolSelectCommand = new RelayCommand(this.ChooseToolSelectCommandExecute));
			}
		}
		private RelayCommand _chooseToolSelectCommand;

		private void ChooseToolSelectCommandExecute()
		{
			this.ToolState = this.SelectTool;
		}

		#endregion ChooseToolSelectCommand

		#region OptionsCommand

		/// <summary>
		/// Gets the Options command.
		/// </summary>
		public ICommand OptionsCommand
		{
			get
			{
				return _optionsCommand ??
					(_optionsCommand = new RelayCommand(this.OptionsCommandExecute, () => false));
			}
		}
		private RelayCommand _optionsCommand;

		private void OptionsCommandExecute()
		{
		}

		#endregion OptionsCommand

		#region AboutCommand

		/// <summary>
		/// Gets the About command.
		/// </summary>
		public ICommand AboutCommand
		{
			get
			{
				return _aboutCommand ??
					(_aboutCommand = new RelayCommand(this.AboutCommandExecute));
			}
		}
		private RelayCommand _aboutCommand;

		private void AboutCommandExecute()
		{
			System.Console.Beep();
		}

		#endregion AboutCommand

		#region GarbageCollectCommand

		/// <summary>
		/// Gets the GarbageCollect command.
		/// </summary>
		public ICommand GarbageCollectCommand
		{
			get
			{
				return _garbageCollectCommand ??
					(_garbageCollectCommand = new RelayCommand(this.GarbageCollectCommandExecute));
			}
		}
		private RelayCommand _garbageCollectCommand;

		private void GarbageCollectCommandExecute()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}

		#endregion GarbageCollectCommand

		#endregion Commands

		#region Private Implementation

		#region Private Properties

		private LocateToolViewModel LocateTool
		{
			get { return this.locateTool ?? (this.locateTool = new LocateToolViewModel(this)); }
		}
		private LocateToolViewModel locateTool;

		private MoveToolViewModel MoveTool
		{
			get { return this.moveTool ?? (this.moveTool = new MoveToolViewModel(this)); }
		}
		private MoveToolViewModel moveTool;

		private SelectToolViewModel SelectTool
		{
			get { return this.selectTool ?? (this.selectTool = new SelectToolViewModel(this)); }
		}
		private SelectToolViewModel selectTool;

		#region SourceOriginProvider
		private IProvideSourceOrigin SourceOriginProvider
		{
			get { return _sourceOriginProvider; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				if (_sourceOriginProvider != value)
				{
					// Get the current provider's SourceOrigin
					Point? previousSourceOrigin = null;
					if (_sourceOriginProvider != null)
						previousSourceOrigin = _sourceOriginProvider.SourceOrigin;

					// Unsubscribe from property change events on the old value
					if (_sourceOriginProviderObserver != null)
					{
						_sourceOriginProviderObserver.Unsubscribe();
						_sourceOriginProviderObserver = null;
					}

					// Save the new value
					_sourceOriginProvider = value;

					// Subscribe to property change events on the new value
					if (_sourceOriginProvider != null)
					{
						_sourceOriginProviderObserver = _sourceOriginProvider.Subscribe(nameof(_sourceOriginProvider.SourceOrigin), s =>
						{
							this.RefreshBitmaps();
							this.UpdateSourceMousePosition();
							this.PropertyChanged.Raise(this, nameof(this.SourceOrigin));
						});
					}

					// Respond to a change in SourceOrigin, if any
					if (previousSourceOrigin != _sourceOriginProvider.SourceOrigin && _sourceOriginProviderObserver != null)
						_sourceOriginProviderObserver.Refresh();
				}
			}
		}
		private IProvideSourceOrigin _sourceOriginProvider;
		private IPropertyObserverItem _sourceOriginProviderObserver;
		#endregion SourceOriginProvider

		#endregion Private Properties

		private void RefreshBitmaps()
		{
			// Refresh the screen image
			this.ScreenImage.Refresh(this.SourceOrigin);

			// Update the mouse position and color
			this.UpdateSourceMousePosition();

			// Indicate changed bitmaps
			this.PropertyChanged.Raise(this, nameof(ZoomedBitmap), nameof(SourceBitmap));

			// Reset the refresh timer if something else caused a refresh
			if (!_isInTimerRefresh)
				this.ResetRefreshTimer();
		}

		private void ResetRefreshTimer()
		{
			CommandManager.InvalidateRequerySuggested();

			if (this.ViewSettings.Model.IsAutoRefreshing)
			{
				if (_refreshTimer == null)
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
				if (_refreshTimer != null)
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

		private void RefreshTimer_Tick(object sender, EventArgs e)
		{
			_isInTimerRefresh = true;
			try
			{
				this.RefreshBitmaps();
			}
			finally
			{
				_isInTimerRefresh = false;
			}
		}

		#endregion Event Handlers
	}
}
