using System.ComponentModel;
using System.Windows;

namespace PixelInspector.ViewModel;

public interface IProvideSourceOrigin : INotifyPropertyChanged
{
	Point SourceOrigin { get; }
}
