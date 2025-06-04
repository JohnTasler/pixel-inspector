using System.Windows;
using System.Windows.Controls.Primitives;
using CommunityToolkit.Mvvm.Input;
using Tasler.Windows;
using Tasler.Windows.ComponentModel;

namespace PixelInspector.ViewModel;

public partial class SelectToolViewModel
	: ChildViewModelBase<MainViewModel>
	, IToolMode
{
	#region Instance Fields
	private Point _dragStart;
	private Rect _dragRect = Rect.Empty;
	private double _lastHorizontalChange;
	private double _lastVerticalChange;
	#endregion Instance Fields

	#region Constructors
	public SelectToolViewModel(MainViewModel parent)
		: base(parent)
	{
	}
	#endregion Constructors

	#region Commands

	#region DragStartedCommand
	[RelayCommand]
	private void DragStarted(DragStartedEventArgs e)
	{
		// Get the point where the mouse was clicked
		_dragStart = this.Parent.ZoomedMousePosition;

		// Get the drag rect
		_dragRect = new Rect(_dragStart.X, _dragStart.Y, 0, 0);
		_dragRect.Inflate(
				Math.Abs(SystemParameters.MinimumHorizontalDragDistance),
				Math.Abs(SystemParameters.MinimumVerticalDragDistance));

		_lastHorizontalChange = _lastVerticalChange = 0;
		this.Parent.Selection.UpdateZoomedRectangleActualFromInput(null);
		e.Handled = true;
	}
	#endregion DragStartedCommand

	#region DragDeltaCommand
	[RelayCommand]
	private void DragDelta(DragDeltaEventArgs e)
	{
		// Calculate the change
		double horizontalChange = e.HorizontalChange - _lastHorizontalChange;
		double verticalChange = e.VerticalChange - _lastVerticalChange;
		_lastHorizontalChange = e.HorizontalChange;
		_lastVerticalChange = e.VerticalChange;

		// Check to see if the mosue has moved outside of the drag rect
		if (!_dragRect.IsEmpty && _dragRect.Contains(this.Parent.ZoomedMousePosition))
			_dragRect = Rect.Empty;

		// Only process if the mouse has moved outside of the drag rect
		if (_dragRect.IsEmpty)
		{
			var rect = new ExtentRect(_dragStart, this.Parent.ZoomedMousePosition);
			this.Parent.Selection.UpdateZoomedRectangleActualFromInput(rect);
		}

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
	}

	/// <summary>
	/// Called to exit the mode.
	/// </summary>
	/// <param name="isReverting">If set to <see langword="true"/> the tool should revert any changes it made;
	/// otherwise, it should commit its changes.</param>
	public void ExitMode(bool isReverting)
	{
	}

	#endregion IToolMode Members
}
