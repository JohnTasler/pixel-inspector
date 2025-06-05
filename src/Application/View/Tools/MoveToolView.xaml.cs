using PixelInspector.ViewModel;

namespace PixelInspector.View;

/// <summary>
/// Implements the view for the <see cref="MoveToolViewModel"/>.
/// </summary>
public partial class MoveToolView : ToolViewUserControl
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="MoveToolView"/> class.
	/// </summary>
	public MoveToolView(MoveToolViewModel viewModel)
	{
		this.InitializeComponent();
		this.DataContext = viewModel;
	}
	#endregion Constructors

	public MoveToolViewModel ViewModel => (MoveToolViewModel)this.DataContext;
}
