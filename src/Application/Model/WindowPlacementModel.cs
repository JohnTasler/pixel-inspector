using System.Xml.Serialization;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using PixelInspector.Interop.User;

namespace PixelInspector.Model;

public class WindowPlacementModel : ObservableObject
{
	#region Instance Fields
	private WINDOWPLACEMENT _windowPlacement;
	#endregion Instance Fields

	#region Constructors
	public WindowPlacementModel()
	{
		_windowPlacement = new WINDOWPLACEMENT();
	}

	public WindowPlacementModel(IntPtr hwnd)
	{
		Guard.IsNotDefault(hwnd);
		_windowPlacement = UserApi.GetWindowPlacement(hwnd);
	}
	#endregion Constructors

	#region Properties

	[XmlAttribute]
	public bool IsMaximized
	{
		get => _windowPlacement.showCmd == SW.ShowMaximized;
		set => this.SetProperty(ref _windowPlacement.showCmd, value ? SW.ShowMaximized : SW.ShowNormal);
	}

	[XmlAttribute]
	public int MaximizedX
	{
		get => _windowPlacement.ptMaxPosition.x;
		set => this.SetProperty(ref _windowPlacement.ptMaxPosition.x, value);
	}

	[XmlAttribute]
	public int MaximizedY
	{
		get => _windowPlacement.ptMaxPosition.y;
		set => this.SetProperty(ref _windowPlacement.ptMaxPosition.y, value);
	}

	[XmlAttribute]
	public int Left
	{
		get => _windowPlacement.rcNormalPosition.left;
		set => this.SetProperty(ref _windowPlacement.rcNormalPosition.left, value);
	}

	[XmlAttribute]
	public int Top
	{
		get => _windowPlacement.rcNormalPosition.top;
		set => this.SetProperty(ref _windowPlacement.rcNormalPosition.top, value);
	}

	[XmlAttribute]
	public int Right
	{
		get => _windowPlacement.rcNormalPosition.right;
		set => this.SetProperty(ref _windowPlacement.rcNormalPosition.right, value);
	}

	[XmlAttribute]
	public int Bottom
	{
		get => _windowPlacement.rcNormalPosition.bottom;
		set => this.SetProperty(ref _windowPlacement.rcNormalPosition.bottom, value);
	}

	#endregion Properties

	#region Methods

	public void Get(IntPtr hwnd)
	{
		Guard.IsNotDefault(hwnd);

		var wp = new WINDOWPLACEMENT();
		UserApi.GetWindowPlacement(hwnd, wp);

		this.IsMaximized = wp.showCmd == SW.ShowMaximized;
		this.MaximizedX = wp.ptMaxPosition.x;
		this.MaximizedY = wp.ptMaxPosition.y;
		this.Left = wp.rcNormalPosition.left;
		this.Top = wp.rcNormalPosition.top;
		this.Right = wp.rcNormalPosition.right;
		this.Bottom = wp.rcNormalPosition.bottom;
	}

	public void Set(IntPtr hwnd)
	{
		Guard.IsNotDefault(hwnd);
		UserApi.SetWindowPlacement(hwnd, _windowPlacement);
	}

	#endregion Methods
}
