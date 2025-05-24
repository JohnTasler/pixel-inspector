using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tasler.Windows.ComponentModel;

namespace PixelInspector.ViewModel;

public partial class MoveToolViewModel
	: ChildViewModelBase<MainViewModel>
	, IToolMode
	, IProvideSourceOrigin
{
	#region Instance Fields
	private Point _previousSourceOrigin;
	private double _lastHorizontalChange;
	private double _lastVerticalChange;
	#endregion Instance Fields

	#region Constructors
	public MoveToolViewModel(MainViewModel parent)
		: base(parent)
	{
	}
	#endregion Constructors

	#region Properties

	[ObservableProperty]
	private Point _sourceOrigin;

	partial void OnSourceOriginActualChanged(Point value)
		=> this.SourceOrigin = new Point((int)value.X, (int)value.Y);

	[ObservableProperty]
	private Point _sourceOriginActual;

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
		var zoomFactor = this.Parent.ViewSettings.Model.ZoomFactor;
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
		_previousSourceOrigin = this.SourceOriginActual = this.Parent.ViewSettings.Model.SourceOrigin;
	}

	/// <summary>
	/// Called to exit the mode.
	/// </summary>
	/// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
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
			this.Parent.ViewSettings.Model.SourceOrigin = this.SourceOrigin;
			this.EnterMode();
		}
	}

	#endregion IToolMode Members
}
