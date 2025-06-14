using System.Windows;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixelInspector.Model;
using Tasler.Windows.ComponentModel;

namespace PixelInspector.ViewModel;

public partial class MoveToolViewModel
	: ChildViewModelBase<MainViewModel>
	, IToolMode
	, IProvideSourceOrigin
{
	#region Instance Fields
	private readonly ViewSettingsModel _viewSettingsModel;
	private Point _previousSourceOrigin;
	private double _lastHorizontalChange;
	private double _lastVerticalChange;
	#endregion Instance Fields

	#region Constructors
	public MoveToolViewModel(MainViewModel mainViewModel, ViewSettingsModel model)
		: base(mainViewModel)
	{
		_viewSettingsModel = model;
	}
	#endregion Constructors

	#region Properties

	[ObservableProperty]
	private Point _sourceOrigin;

	[ObservableProperty]
	private Point _sourceOriginActual;

	partial void OnSourceOriginActualChanged(Point value)
		=> this.SourceOrigin = new Point((int)value.X, (int)value.Y);

	#endregion Properties

	#region Commands

	#region DragStartedCommand

	[RelayCommand]
	private void DragStarted(DragStartedEventArgs e)
	{
		_lastHorizontalChange = _lastVerticalChange = 0;
		e.Handled = true;
	}
	#endregion DragStartedCommand

	#region DragDeltaCommand
	[RelayCommand]
	private void DragDelta(DragDeltaEventArgs e)
	{
		var horizontalChange = e.HorizontalChange - _lastHorizontalChange;
		var verticalChange = e.VerticalChange - _lastVerticalChange;
		_lastHorizontalChange = e.HorizontalChange;
		_lastVerticalChange = e.VerticalChange;

		var sourceOrigin = this.SourceOriginActual;
		var zoomFactor = _viewSettingsModel.ZoomFactor;
		var xOffset = -horizontalChange / zoomFactor;
		var yOffset = -verticalChange / zoomFactor;

		sourceOrigin.Offset(xOffset, yOffset);
		this.SourceOriginActual = sourceOrigin;

		e.Handled = true;
	}
	#endregion DragDeltaCommand

	#region DragCompletedCommand
	[RelayCommand]
	private void DragCompleted(DragCompletedEventArgs e)
	{
		this.ExitMode(e.Canceled);
		e.Handled = true;
	}
	#endregion DragCompletedCommand

	#endregion Commands

	#region IToolMode Members

	/// <summary>
	/// Called before the mode is entered.
	/// </summary>
	public void EnterMode()
	{
		_previousSourceOrigin = this.SourceOriginActual = _viewSettingsModel.SourceOrigin;
	}

	/// <summary>
	/// Called to exit the mode.
	/// </summary>
	/// <param name="isReverting">If set to <see langword="true"/> the tool should revert any changes it made;
	/// otherwise, it should commit its changes.</param>
	public void ExitMode(bool isReverting)
	{
		if (isReverting)
		{
			this.SourceOriginActual = _previousSourceOrigin;

			// TODO: Restore previous bitmap images
		}
		else
		{
			_viewSettingsModel.SourceOrigin = this.SourceOrigin;
			this.EnterMode();
		}
	}

	#endregion IToolMode Members
}
