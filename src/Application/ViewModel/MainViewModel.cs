namespace ZoomIn.ViewModel
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Windows.Threading;
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Interop.Gdi;
	using ZoomIn.Model;
	using ZoomIn.Utility;

	public class MainViewModel : ObservableObject
	{
		#region Instance Fields
		private bool isInTimerRefresh;
		private DispatcherTimer refreshTimer;
		private BitmapModel sourceBitmapBuffer;
		private BitmapModel zoomedBitmapBuffer;
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
			if (ZoomIn.Properties.Settings.Default.LatestViewSettings == null)
				ZoomIn.Properties.Settings.Default.LatestViewSettings = new ViewSettingsModel();
			this.ViewSettings = new ViewSettingsViewModel(this, ZoomIn.Properties.Settings.Default.LatestViewSettings);

			// Create a scaling transform if the system is using a non-standard DPI
			var dpiScaleX = GdiApi.DpiScaleX;
			var dpiScaleY = GdiApi.DpiScaleY;
			if (dpiScaleX != 1.0 || dpiScaleY != 1.0)
				this.DpiTransform = new ScaleTransform(dpiScaleX, dpiScaleY, 0.5, 0.5);

			// Create the ScreenImageViewModel
			this.ScreenImage = new ScreenImageViewModel(this);
			this.ScreenImage.Subscribe(s => s.IsRefreshNeeded,
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
			get { return this.sourceBitmapBuffer; }
		}

		public BitmapModel ZoomedBitmap
		{
			get { return this.zoomedBitmapBuffer; }
		}
		#endregion Models

		#region Child View Models

		#region ViewSettings
		public ViewSettingsViewModel ViewSettings
		{
			get { return this.viewSettings; }
			private set
			{
				var oldValue = this.viewSettings;
				if (this.SetProperty(ref this.viewSettings, value, nameof(ViewSettings)))
				{
					if (this.viewSettingsObservers != null)
					{
						foreach (var observer in this.viewSettingsObservers)
							observer.Unsubscribe();
						this.viewSettingsObservers = null;
					}

					this.ResetRefreshTimer();

					if (this.viewSettings != null)
					{
						this.viewSettingsObservers = new List<IPropertyObserverItem>
						{
							this.viewSettings.Subscribe(s => s.AutoRefreshInterval, s => this.ResetRefreshTimer()),
							this.viewSettings.Model.Subscribe(s => s.IsAutoRefreshing, s => this.ResetRefreshTimer()),
							this.viewSettings.Model.Subscribe(s => s.ZoomFactor, s => this.UpdateSourceMousePosition()),
						};
					}
				}
			}
		}
		private ViewSettingsViewModel viewSettings;
		private List<IPropertyObserverItem> viewSettingsObservers;
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
					this.RaisePropertyChanged(nameof(ToolState), nameof(IsToolStateLocate), nameof(IsToolStateMove), nameof(IsToolStateSelect));

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
			get { return this.applicationState; }
			private set
			{
				if (this.SetProperty(ref this.applicationState, value, nameof(ApplicationState))
				 && this.applicationState is ApplicationStateUnloading)
				{
					using (this.sourceBitmapBuffer)
					using (this.zoomedBitmapBuffer)
					{
						this.sourceBitmapBuffer = null;
						this.zoomedBitmapBuffer = null;
					}
				}
			}
		}
		private object applicationState;

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
			get { return this.sourceMousePosition; }
			private set { this.SetProperty(ref this.sourceMousePosition, value, nameof(SourceMousePosition)); }
		}
		private Point sourceMousePosition;

		public bool IsMouseOverSelection
		{
			get { return this.isMouseOverSelection; }
			private set { this.SetProperty(ref this.isMouseOverSelection, value, nameof(IsMouseOverSelection)); }
		}
		private bool isMouseOverSelection;

		public Color SourceMousePositionColor
		{
			get { return this.sourceMousePositionColor; }
			private set { this.SetProperty(ref this.sourceMousePositionColor, value, nameof(SourceMousePositionColor)); }
		}
		private Color sourceMousePositionColor;

		public Point ZoomedMousePosition
		{
			get { return this.zoomedMousePosition; }
			set
			{
				if (this.SetProperty(ref this.zoomedMousePosition, value, nameof(ZoomedMousePosition)))
					this.UpdateSourceMousePosition();
			}
		}
		private Point zoomedMousePosition;

		public bool IsMouseInZoomedBounds
		{
			get { return this.isMouseInZoomedBounds; }
			set { this.SetProperty(ref this.isMouseInZoomedBounds, value, nameof(IsMouseInZoomedBounds)); }
		}
		private bool isMouseInZoomedBounds;

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
				return this.saveAsCommand ??
					(this.saveAsCommand = new RelayCommand(this.SaveAsCommandExecute, this.SaveAsCommandCanExecute));
			}
		}
		private RelayCommand saveAsCommand;

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
				return this.printCommand ??
					(this.printCommand = new RelayCommand(this.PrintCommandExecute, this.PrintCommandCanExecute));
			}
		}
		private RelayCommand printCommand;

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
				return this.exitCommand ??
					(this.exitCommand = new RelayCommand(this.ExitCommandExecute));
			}
		}
		private RelayCommand exitCommand;

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
				return this.copyCommand ??
					(this.copyCommand = new RelayCommand(this.CopyCommandExecute, this.CopyCommandCanExecute));
			}
		}
		private RelayCommand copyCommand;

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
				int cx, cy;
				bitmap.Model.GetSize(out cx, out cy);

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
				return this.refreshCommand ??
					(this.refreshCommand = new RelayCommand(this.RefreshCommandExecute, this.RefreshCommandCanExecute));
			}
		}
		private RelayCommand refreshCommand;

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
		/// Gets the ChooseToolCapture command.
		/// </summary>
		public ICommand ChooseToolLocateCommand
		{
			get
			{
				return this.chooseToolLocateCommand ??
					(this.chooseToolLocateCommand = new RelayCommand(this.ChooseToolLocateCommandExecute));
			}
		}
		private RelayCommand chooseToolLocateCommand;

		private void ChooseToolLocateCommandExecute()
		{
			this.ToolState = new LocateToolViewModel(this);
		}

		#endregion ChooseToolLocateCommand

		#region ChooseToolLocatingCommand

		/// <summary>
		/// Gets the ChooseToolCapture command.
		/// </summary>
		public ICommand ChooseToolLocatingCommand
		{
			get
			{
				return this.chooseToolLocatingCommand ??
					(this.chooseToolLocatingCommand = new RelayCommand<LocatingToolViewModel.Parameters>(this.ChooseToolLocatingCommandExecute));
			}
		}
		private RelayCommand<LocatingToolViewModel.Parameters> chooseToolLocatingCommand;

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
				return this.chooseToolMoveCommand ??
					(this.chooseToolMoveCommand = new RelayCommand(this.ChooseToolMoveCommandExecute));
			}
		}
		private RelayCommand chooseToolMoveCommand;

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
				return this.chooseToolSelectCommand ??
					(this.chooseToolSelectCommand = new RelayCommand(this.ChooseToolSelectCommandExecute));
			}
		}
		private RelayCommand chooseToolSelectCommand;

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
				return this.optionsCommand ??
					(this.optionsCommand = new RelayCommand(this.OptionsCommandExecute, () => false));
			}
		}
		private RelayCommand optionsCommand;

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
				return this.aboutCommand ??
					(this.aboutCommand = new RelayCommand(this.AboutCommandExecute));
			}
		}
		private RelayCommand aboutCommand;

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
				return this.garbageCollectCommand ??
					(this.garbageCollectCommand = new RelayCommand(this.GarbageCollectCommandExecute));
			}
		}
		private RelayCommand garbageCollectCommand;

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
			get { return this.sourceOriginProvider; }
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				if (this.sourceOriginProvider != value)
				{
					// Get the current provider's SourceOrigin
					Point? previousSourceOrigin = null;
					if (this.sourceOriginProvider != null)
						previousSourceOrigin = this.sourceOriginProvider.SourceOrigin;

					// Unsubscribe from property change events on the old value
					if (this.sourceOriginProviderObserver != null)
					{
						this.sourceOriginProviderObserver.Unsubscribe();
						this.sourceOriginProviderObserver = null;
					}

					// Save the new value
					this.sourceOriginProvider = value;

					// Subscribe to property change events on the new value
					if (this.sourceOriginProvider != null)
					{
						this.sourceOriginProviderObserver = this.sourceOriginProvider.Subscribe(s => s.SourceOrigin, s =>
						{
							this.RefreshBitmaps();
							this.UpdateSourceMousePosition();
							this.RaisePropertyChanged(() => this.SourceOrigin);
						});
					}

					// Respond to a change in SourceOrigin, if any
					if (previousSourceOrigin != this.sourceOriginProvider.SourceOrigin && this.sourceOriginProviderObserver != null)
						this.sourceOriginProviderObserver.Refresh();
				}
			}
		}
		private IProvideSourceOrigin sourceOriginProvider;
		private IPropertyObserverItem sourceOriginProviderObserver;
		#endregion SourceOriginProvider

		#endregion Private Properties

		private void RefreshBitmaps()
		{
			// Refresh the screen image
			this.ScreenImage.Refresh(this.SourceOrigin);

			// Update the mouse position and color
			this.UpdateSourceMousePosition();

			// Indicate changed bitmaps
			this.RaisePropertyChanged(nameof(ZoomedBitmap), nameof(SourceBitmap));

			// Reset the refresh timer if something else caused a refresh
			if (!this.isInTimerRefresh)
				this.ResetRefreshTimer();
		}

		private void ResetRefreshTimer()
		{
			CommandManager.InvalidateRequerySuggested();

			if (this.ViewSettings.Model.IsAutoRefreshing)
			{
				if (this.refreshTimer == null)
				{
					this.refreshTimer = new DispatcherTimer(this.ViewSettings.AutoRefreshInterval,
						DispatcherPriority.Render, this.refreshTimer_Tick, Dispatcher.CurrentDispatcher);
				}
				else
				{
					if (this.refreshTimer.Interval != this.ViewSettings.AutoRefreshInterval)
						this.refreshTimer.Interval = this.ViewSettings.AutoRefreshInterval;
				}

				this.refreshTimer.Start();
			}
			else
			{
				if (this.refreshTimer != null)
				{
					this.refreshTimer.Stop();
					this.refreshTimer.Tick -= this.refreshTimer_Tick;
					this.refreshTimer = null;
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
			this.SourceMousePositionColor = this.ScreenImage.SourceBitmap.GetPixelColor(x, y);

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

		private void refreshTimer_Tick(object sender, EventArgs e)
		{
			this.isInTimerRefresh = true;
			try
			{
				this.RefreshBitmaps();
			}
			finally
			{
				this.isInTimerRefresh = false;
			}
		}

		#endregion Event Handlers
	}
}
