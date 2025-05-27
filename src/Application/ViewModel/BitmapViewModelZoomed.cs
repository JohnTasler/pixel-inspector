using Microsoft.Extensions.DependencyInjection;
using PixelInspector.Model;

namespace PixelInspector.ViewModel;

public class BitmapViewModelZoomed : BitmapViewModelBase
{
	public BitmapViewModelZoomed([FromKeyedServices("Zoomed")] BitmapModel model)
		: base(model)
	{
	}
}
