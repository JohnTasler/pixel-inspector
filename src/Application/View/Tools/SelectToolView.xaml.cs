using PixelInspector.ViewModel;

namespace PixelInspector.View;

public partial class SelectToolView : ToolViewUserControl
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="SelectToolView"/> class.
	/// </summary>
	public SelectToolView(SelectToolViewModel viewModel)
	{
		this.InitializeComponent();
		this.DataContext = viewModel;
	}
	#endregion Constructors

	public SelectToolViewModel ViewModel => (SelectToolViewModel)this.DataContext;
}
