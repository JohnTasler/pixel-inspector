using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using PixelInspector.ViewModel;
using Tasler.ComponentModel;
using Tasler.Windows;

namespace PixelInspector.View;

/// <summary>
/// Interaction logic for LocateToolView.xaml
/// </summary>
public partial class LocateToolView : ToolViewUserControl
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LocateToolView"/> class.
	/// </summary>
	public LocateToolView(LocateToolViewModel viewModel)
	{
		this.InitializeComponent();
		this.HookDataContextAsViewModel(() => this.RaisePropertyChanged(nameof(this.ViewModel)));
		this.DataContext = viewModel;
	}

	public LocateToolViewModel ViewModel => (LocateToolViewModel)this.DataContext;

	protected override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.Key)
		{
			case Key.Enter:
				this.ViewModel.Parent.ChooseToolLocatingCommand.Execute(
						new LocatingToolViewModel.Parameters
						{
							Offset = new Point(this.ActualWidth / 2, this.ActualHeight / 2),
							IsFromMouseClick = false
						}
				);
				e.Handled = true;
				break;

			case Key.Escape:
				this.ViewModel.ExitMode(true);
				e.Handled = true;
				break;
		}

		base.OnKeyDown(e);
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		e.Handled = true;

		this.ViewModel.Parent.ChooseToolLocatingCommand.Execute(
				new LocatingToolViewModel.Parameters
				{
					Offset = e.GetPosition(this),
					IsFromMouseClick = true
				}
		);
	}
}
