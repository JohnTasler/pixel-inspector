namespace PixelInspector.Model
{
	using System;
	using System.ComponentModel;
	using System.Xml.Serialization;
	using Tasler.ComponentModel;
	using PixelInspector.Interop.User;

	public class WindowPlacementModel : ViewModel
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
			if (hwnd == IntPtr.Zero)
				throw new ArgumentException("Must specify a non-zero window handle.", nameof(hwnd));

			_windowPlacement = UserApi.GetWindowPlacement(hwnd);
		}
		#endregion Constructors

		#region Properties

		[XmlAttribute]
		public bool IsMaximized
		{
			get { return _windowPlacement.showCmd == SW.ShowMaximized; }
			set { this.PropertyChanged.SetProperty(this, value ? SW.ShowMaximized : SW.ShowNormal, ref _windowPlacement.showCmd); }
		}

		[XmlAttribute]
		public int MaximizedX
		{
			get { return _windowPlacement.ptMaxPosition.x; }
			set { this.PropertyChanged.SetProperty(this, value, ref _windowPlacement.ptMaxPosition.x); }
		}

		[XmlAttribute]
		public int MaximizedY
		{
			get { return _windowPlacement.ptMaxPosition.y; }
			set { this.PropertyChanged.SetProperty(this, value, ref _windowPlacement.ptMaxPosition.y); }
		}

		[XmlAttribute]
		public int Left
		{
			get { return _windowPlacement.rcNormalPosition.left; }
			set { this.PropertyChanged.SetProperty(this, value, ref _windowPlacement.rcNormalPosition.left); }
		}

		[XmlAttribute]
		public int Top
		{
			get { return _windowPlacement.rcNormalPosition.top; }
			set { this.PropertyChanged.SetProperty(this, value, ref _windowPlacement.rcNormalPosition.top); }
		}

		[XmlAttribute]
		public int Right
		{
			get { return _windowPlacement.rcNormalPosition.right; }
			set { this.PropertyChanged.SetProperty(this, value, ref _windowPlacement.rcNormalPosition.right); }
		}

		[XmlAttribute]
		public int Bottom
		{
			get { return _windowPlacement.rcNormalPosition.bottom; }
			set { this.PropertyChanged.SetProperty(this, value, ref _windowPlacement.rcNormalPosition.bottom); }
		}

		#endregion Properties

		#region Methods

		public void Get(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero)
				throw new ArgumentException("Must specify a non-zero window handle.", nameof(hwnd));

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
			if (hwnd == IntPtr.Zero)
				throw new ArgumentException("Must specify a non-zero window handle.", nameof(hwnd));

			UserApi.SetWindowPlacement(hwnd, _windowPlacement);
		}

		#endregion Methods
	}
}
