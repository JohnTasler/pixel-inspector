namespace ZoomIn.ViewModel
{
	using System.ComponentModel;
	using System.Windows;

	public interface IProvideSourceOrigin : INotifyPropertyChanged
	{
		Point SourceOrigin { get; }
	}
}
