namespace ZoomIn.Model
{
	using System;
	using System.Xml.Serialization;
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Interop.User;

	public class WindowPlacementModel : ObservableObject
	{
		#region Instance Fields
		private WINDOWPLACEMENT windowPlacement;
		#endregion Instance Fields

		#region Constructors
		public WindowPlacementModel()
		{
			this.windowPlacement = new WINDOWPLACEMENT();
		}

		public WindowPlacementModel(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero)
				throw new ArgumentException("Must specify a non-zero window handle.", nameof(hwnd));

			this.windowPlacement = UserApi.GetWindowPlacement(hwnd);
		}
		#endregion Constructors

		#region Properties

		[XmlAttribute]
		public bool IsMaximized
		{
			get { return this.windowPlacement.showCmd == SW.ShowMaximized; }
			set { this.SetProperty(ref this.windowPlacement.showCmd, value ? SW.ShowMaximized : SW.ShowNormal, nameof(IsMaximized)); }
		}

		[XmlAttribute]
		public int MaximizedX
		{
			get { return this.windowPlacement.ptMaxPosition.x; }
			set { this.SetProperty(ref this.windowPlacement.ptMaxPosition.x, value, nameof(MaximizedX)); }
		}

		[XmlAttribute]
		public int MaximizedY
		{
			get { return this.windowPlacement.ptMaxPosition.y; }
			set { this.SetProperty(ref this.windowPlacement.ptMaxPosition.y, value, nameof(MaximizedY)); }
		}

		[XmlAttribute]
		public int Left
		{
			get { return this.windowPlacement.rcNormalPosition.left; }
			set { this.SetProperty(ref this.windowPlacement.rcNormalPosition.left, value, nameof(Left)); }
		}

		[XmlAttribute]
		public int Top
		{
			get { return this.windowPlacement.rcNormalPosition.top; }
			set { this.SetProperty(ref this.windowPlacement.rcNormalPosition.top, value, nameof(Top)); }
		}

		[XmlAttribute]
		public int Right
		{
			get { return this.windowPlacement.rcNormalPosition.right; }
			set { this.SetProperty(ref this.windowPlacement.rcNormalPosition.right, value, nameof(Right)); }
		}

		[XmlAttribute]
		public int Bottom
		{
			get { return this.windowPlacement.rcNormalPosition.bottom; }
			set { this.SetProperty(ref this.windowPlacement.rcNormalPosition.bottom, value, nameof(Bottom)); }
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

			UserApi.SetWindowPlacement(hwnd, this.windowPlacement);
		}

		#endregion Methods
	}
}
