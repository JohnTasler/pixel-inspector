using System.Windows;
using PixelInspector.ViewModel;

namespace PixelInspector.View;

/// <summary>
/// Interaction logic for LocatingToolView.xaml
/// </summary>
public partial class LocatingToolView : ToolViewUserControl
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="LocatingToolView"/> class.
	/// </summary>
	public LocatingToolView(LocatingToolViewModel viewModel)
	{
		this.InitializeComponent();
		this.Loaded += this.View_Loaded;
		this.DataContext = viewModel;
	}
	#endregion Constructors

	public LocatingToolViewModel ViewModel => (this.DataContext as LocatingToolViewModel)!;

	#region Private Implementation
	private void EnterDragWindow(Point offset, bool isFromMouseClick)
	{
		// Get the view settings
		var viewSettings = this.ViewModel.Parent.ViewSettings;

		// Compute the rectangle
		var rect = new Rect(viewSettings.Model.SourceSize);
		rect.Offset(offset.X, offset.Y);
		rect.Offset(-offset.X / viewSettings.Model.ZoomFactor, -offset.Y / viewSettings.Model.ZoomFactor);

		// Create and initialize the drag window
		var window = new DragWindow(this, rect, isFromMouseClick, this.Cursor);
		window.LocationChanged += (sender, e) => this.ViewModel.SourceOrigin = window.InnerContentRect.TopLeft;
		window.Closed += (sender, e) => this.ViewModel.ExitMode(window.WasCancelled);

		// Hide our view and show the drag window
		this.Visibility = Visibility.Hidden;
		window.Show();
	}

	#endregion Private Implementation

	#region Event Handlers
	private void View_Loaded(object sender, RoutedEventArgs e)
	{
		if (this.ViewModel is not null)
			this.EnterDragWindow(this.ViewModel.Offset, this.ViewModel.IsFromMouseClick);
	}
	#endregion Event Handlers
}
