using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tasler.ComponentModel;
using Tasler.Windows;
using Tasler.Windows.ComponentModel;

namespace PixelInspector.ViewModel;

public partial class SelectionViewModel : ChildViewModelBase<MainViewModel>
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="SelectionViewModel"/> class.
	/// </summary>
	public SelectionViewModel(MainViewModel parent)
		: base(parent)
	{
		// Subsribe to property changes
		parent.Subscribe(nameof(parent.SourceOrigin), s => this.UpdateZoomedRectangleActual());
		parent.ViewSettings.Model.Subscribe(nameof(parent.ViewSettings.Model.ZoomFactor), s => this.UpdateZoomedRectangleActual());
	}
	#endregion Constructors

	#region Properties

	#region SourceRectangle
	[ObservableProperty]
	private Rect? _sourceRectangle;

	partial void OnSourceRectangleChanged(Rect? value) => this.HasSelection = value.HasValue;
	#endregion SourceRectangle

	#region ZoomedRectangle
	[ObservableProperty]
	private Rect? _zoomedRectangle;

	partial void OnZoomedRectangleChanged(Rect? value) => this.UpdateSourceRectangle();
	#endregion ZoomedRectangle

	#region ZoomedRectangleActual
	[ObservableProperty]
	private Rect? _zoomedRectangleActual;
	#endregion ZoomedRectangleActual

	#region HasSelection
	[ObservableProperty]
	private bool _hasSelection;
	#endregion HasSelection

	#endregion Properties

	#region Commands

	#region ClearSelectionCommand

	/// <summary>
	/// Gets the ClearSelection command.
	/// </summary>
	[RelayCommand]
	private void ClearSelection() => this.UpdateZoomedRectangleActualFromInput(null);

	#endregion ClearSelectionCommand

	#endregion Commands

	#region Private Implementation

	private void UpdateSourceRectangle()
	{
		var newSourceSelection = (Rect?)null;
		var zoomedSelectionActual = this.ZoomedRectangleActual;
		if (zoomedSelectionActual.HasValue && this.Parent is not null)
		{
			var zoomFactor = this.Parent.ViewSettings.Model.ZoomFactor;
			var x = Math.Floor(zoomedSelectionActual.Value.X / zoomFactor);
			var y = Math.Floor(zoomedSelectionActual.Value.Y / zoomFactor);
			var width = Math.Ceiling(zoomedSelectionActual.Value.Width / zoomFactor);
			var height = Math.Ceiling(zoomedSelectionActual.Value.Height / zoomFactor);

			var selection = new Rect(x, y, width, height);
			selection.Offset(this.Parent.SourceOrigin.X, this.Parent.SourceOrigin.Y);
			newSourceSelection = selection;
		}

		this.SourceRectangle = newSourceSelection;
	}

	private void UpdateZoomedRectangleActual()
	{
		var newZoomedSelection = (Rect?)null;
		var sourceSelection = this.SourceRectangle;
		if (sourceSelection.HasValue && this.Parent is not null)
		{
			var zoomFactor = this.Parent.ViewSettings.Model.ZoomFactor;
			var x = (sourceSelection.Value.X - this.Parent.SourceOrigin.X) * zoomFactor;
			var y = (sourceSelection.Value.Y - this.Parent.SourceOrigin.Y) * zoomFactor;
			var width = sourceSelection.Value.Width * zoomFactor;
			var height = sourceSelection.Value.Height * zoomFactor;

			newZoomedSelection = new Rect(x, y, width, height);
		}

		this.ZoomedRectangleActual = newZoomedSelection;
		this.ZoomedRectangle = newZoomedSelection;
	}

	internal void UpdateZoomedRectangleActualFromInput(ExtentRect? rect)
	{
		if (rect is null)
		{
			this.ZoomedRectangleActual = null;
			this.ZoomedRectangle = null;
		}
		else
		{
			// Adjust the rectangle coordinates to the nearest zoom factor
			var newRect = rect.Value;
			var zoomFactor = this.Parent.ViewSettings.Model.ZoomFactor;

			if (newRect.Width >= 0)
			{
				newRect.X = Math.Floor(newRect.X / zoomFactor) * zoomFactor;
				newRect.Width = Math.Ceiling(newRect.Width / zoomFactor) * zoomFactor;
			}
			else
			{
				newRect.X = Math.Ceiling(newRect.X / zoomFactor) * zoomFactor;
				newRect.Width = Math.Floor(newRect.Width / zoomFactor) * zoomFactor;
			}

			if (newRect.Height >= 0)
			{
				newRect.Y = Math.Floor(newRect.Y / zoomFactor) * zoomFactor;
				newRect.Height = Math.Ceiling(newRect.Height / zoomFactor) * zoomFactor;
			}
			else
			{
				newRect.Y = Math.Ceiling(newRect.Y / zoomFactor) * zoomFactor;
				newRect.Height = Math.Floor(newRect.Height / zoomFactor) * zoomFactor;
			}

			// Adjust the TopLeft outward by a pixel if the space is needed for the grid lines
			if (this.Parent.ViewSettings.Model.ZoomFactor > 1.0)
			{
				var inflateLeft = newRect.Left > 0 ? 1 : 0;
				var inflateTop = newRect.Top > 0 ? 1 : 0;
				newRect.Inflate(inflateLeft, inflateTop, 0, 0);
			}

			// Save the new rectangles
			this.ZoomedRectangleActual = rect;
			this.ZoomedRectangle = newRect;
		}
	}

	#endregion Private Implementation
}
