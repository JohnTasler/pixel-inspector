using PixelInspector.ViewModel;
using Tasler.Windows;

namespace PixelInspector.View;

/// <summary>
/// Interaction logic for SelectionView.xaml
/// </summary>
public partial class SelectionView : ToolViewUserControl
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="SelectionView"/> class.
	/// </summary>
	public SelectionView()
	{
		this.InitializeComponent();
		this.HookDataContextAsViewModel(() => this.RaisePropertyChanged(nameof(this.ViewModel)));
	}
	#endregion Constructors

	public SelectionViewModel ViewModel => (SelectionViewModel)this.DataContext;
}
