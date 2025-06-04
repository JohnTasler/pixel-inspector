using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Tasler;
using Tasler.Windows.ComponentModel;

namespace PixelInspector.ViewModel;

public partial class LocatingToolViewModel
	: ChildViewModelBase<MainViewModel>
	, IProvideSourceOrigin
	, IToolMode
{
	#region Instance Fields
	private object? _previousToolState;
	private bool _isExiting;
	#endregion Instance Fields

	#region Constructors
	public LocatingToolViewModel(MainViewModel parent)
		: base(parent)
	{
	}
	#endregion Constructors

	public LocatingToolViewModel Initialize(LocatingToolViewModel.Parameters parameters)
	{
		this.Offset = parameters.Offset;
		this.IsFromMouseClick = parameters.IsFromMouseClick;
		return this;
	}

	#region Properties

	public Point Offset { get; private set; }

	public bool IsFromMouseClick { get; private set; }

	[ObservableProperty]
	private Point _sourceOrigin;

	#endregion Properties

	#region IToolMode Members

	/// <summary>
	/// Called before the mode is entered.
	/// </summary>
	public void EnterMode()
	{
		SourceOrigin = this.Parent.ViewSettings.Model.SourceOrigin;
		_previousToolState = this.Parent.ToolState;
	}

	/// <summary>
	/// Called to exit the mode.
	/// </summary>
	/// <param name="isReverting">If set to <see langword="true"/> the tool should revert any changes it made;
	/// otherwise, it should commit its changes.</param>
	public void ExitMode(bool isReverting)
	{
		if (_isExiting)
			return;

		if (isReverting)
		{
			// TODO: Restore previous bitmap images
		}
		else
		{
			this.Parent.ViewSettings.Model.SourceOrigin = this.SourceOrigin;
		}

		using var scope = new DisposeScopeExit(() => _isExiting = true, () => _isExiting = false);

		this.Parent.ToolState = _previousToolState;
	}

	#endregion IToolMode Members

	#region Nested Types
	public class Parameters
	{
		public Point Offset { get; set; }
		public bool IsFromMouseClick { get; set; }
	}
	#endregion Nested Types
}
