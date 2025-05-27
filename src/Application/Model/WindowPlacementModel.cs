using System.Xml.Serialization;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using Tasler.Interop.User;

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

	public WindowPlacementModel(SafeHwnd hwnd)
	{
		Guard.IsNotDefault(hwnd.Handle);
		_windowPlacement = hwnd.GetWindowPlacement();
	}
	#endregion Constructors

	#region Properties

	[XmlAttribute]
	public bool IsMaximized
	{
		get => _windowPlacement.ShowCommand == SW.ShowMaximized;
		set => this.SetProperty(ref _windowPlacement.ShowCommand, value ? SW.ShowMaximized : SW.ShowNormal);
	}

	[XmlAttribute]
	public int MaximizedX
	{
		get => _windowPlacement.MaximizedPosition.X;
		set => this.SetProperty(ref _windowPlacement.MaximizedPosition.X, value);
	}

	[XmlAttribute]
	public int MaximizedY
	{
		get => _windowPlacement.MaximizedPosition.Y;
		set => this.SetProperty(ref _windowPlacement.MaximizedPosition.Y, value);
	}

	[XmlAttribute]
	public int Left
	{
		get => _windowPlacement.NormalPosition.Left;
		set => this.SetProperty(ref _windowPlacement.NormalPosition.Left, value);
	}

	[XmlAttribute]
	public int Top
	{
		get => _windowPlacement.NormalPosition.Top;
		set => this.SetProperty(ref _windowPlacement.NormalPosition.Top, value);
	}

	[XmlAttribute]
	public int Right
	{
		get => _windowPlacement.NormalPosition.Right;
		set => this.SetProperty(ref _windowPlacement.NormalPosition.Right, value);
	}

	[XmlAttribute]
	public int Bottom
	{
		get => _windowPlacement.NormalPosition.Bottom;
		set => this.SetProperty(ref _windowPlacement.NormalPosition.Bottom, value);
	}

	#endregion Properties

	#region Methods

	public void Get(SafeHwnd hwnd)
	{
		Guard.IsNotDefault(hwnd.Handle);

		var wp = hwnd.GetWindowPlacement();

		this.IsMaximized = wp.ShowCommand == SW.ShowMaximized;
		this.MaximizedX = wp.MaximizedPosition.X;
		this.MaximizedY = wp.MaximizedPosition.Y;
		this.Left = wp.NormalPosition.Left;
		this.Top = wp.NormalPosition.Top;
		this.Right = wp.NormalPosition.Right;
		this.Bottom = wp.NormalPosition.Bottom;
	}

	public void Set(SafeHwnd hwnd)
	{
		Guard.IsNotDefault(hwnd.Handle);
		hwnd.SetWindowPlacement(ref _windowPlacement);
	}

	#endregion Methods
}
