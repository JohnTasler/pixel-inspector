namespace ZoomIn.Interop.User
{
	#region Window Messages
	public enum WM
	{
		Destroy       = 0x0002,
		Move          = 0x0003,
		Size          = 0x0005,
		MouseLeave    = 0x02A3,
		ExitSizeMove  = 0x0232,
		MouseMove     = 0x0200,
		SysCommand    = 0x0112,
		LButtonUp     = 0x0202,
		NcActivate    = 0x0086,
		NcHitTest     = 0x0084,
		MouseActivate = 0x0021,
		SetCursor     = 0x0020,
		ShowWindow    = 0x0018,
		DisplayChange = 0x007E,
	}
	#endregion Window Messages

	#region System Menu Commands
	public enum SC
	{
		Size            = 0xF000,
		SizeLeft        = 0xF001,
		SizeRight       = 0xF002,
		SizeTop         = 0xF003,
		SizeTopLeft     = 0xF004,
		SizeTopRight    = 0xF005,
		SizeBottom      = 0xF006,
		SizeBottemLeft  = 0xF007,
		SizeBottomRight = 0xF008,
		Move            = 0xF010,
		MoveCaption     = 0xF012,
		Minimize        = 0xF020,
		Maximize        = 0xF030,
		NextWindow      = 0xF040,
		PrevWindow      = 0xF050,
		Close           = 0xF060,
		VScroll         = 0xF070,
		HScroll         = 0xF080,
		MouseMenu       = 0xF090,
		KeyMenu         = 0xF100,
		Arrange         = 0xF110,
		Restore         = 0xF120,
		TaskList        = 0xF130,
		ScreenSave      = 0xF140,
		HotKey          = 0xF150,
		Default         = 0xF160,
		MonitorPower    = 0xF170,
		ContextHelp     = 0xF180,
		Separator       = 0xF00F,
	}
	#endregion System Menu Commands

	#region WM_NCHITTEST Return Codes
	public enum HT
	{
		Error       = (-2),
		Transparent = (-1),
		NoWhere     = 0,
		Client      = 1,
		Caption     = 2,
		SysMenu     = 3,
		GrowBox     = 4,
		Size        = GrowBox,
		Menu        = 5,
		HScroll     = 6,
		VScroll     = 7,
		MinButton   = 8,
		MaxButton   = 9,
		Left        = 10,
		Right       = 11,
		Top         = 12,
		TopLeft     = 13,
		TopRight    = 14,
		Bottom      = 15,
		BottomLeft  = 16,
		BottomRight = 17,
		Border      = 18,
		Reduce      = MinButton,
		Zoom        = MaxButton,
		SizeFirst   = Left,
		SizeLast    = BottomRight,
		Object      = 19,
		Close       = 20,
		Help        = 21,
	}
	#endregion WM_NCHITTEST Return Codes

	#region ShowWindow Commands
	public enum SW
	{
		Hide            = 0,
		ShowNormal      = 1,
		Normal          = 1,
		ShowMinimized   = 2,
		ShowMaximized   = 3,
		Maximize        = 3,
		ShowNoActivate  = 4,
		Show            = 5,
		Minimize        = 6,
		ShowMinNoActive = 7,
		ShowNA          = 8,
		Restore         = 9,
		ShowDefault     = 10,
		ForceMinimize   = 11,
	}
	#endregion ShowWindow Commands

	#region System Metrics
	public enum SM
	{
		CxDrag = 68,
		CyDrag = 69,
	}
	#endregion System Metrics

}
