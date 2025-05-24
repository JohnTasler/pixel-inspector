using PixelInspector.ViewModel;
using Tasler.Windows;

namespace PixelInspector.View;

public partial class SelectToolView : ToolViewUserControl
{
	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="SelectToolView"/> class.
	/// </summary>
	public SelectToolView()
	{
		this.InitializeComponent();
		this.HookDataContextAsViewModel(() => this.RaisePropertyChanged(nameof(this.ViewModel)));
	}
	#endregion Constructors

	public SelectToolViewModel ViewModel => (SelectToolViewModel)this.DataContext;
}
