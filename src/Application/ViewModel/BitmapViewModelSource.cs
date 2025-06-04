using Microsoft.Extensions.DependencyInjection;
using PixelInspector.Model;

namespace PixelInspector.ViewModel;

public class BitmapViewModelSource : BitmapViewModelBase
{
	public BitmapViewModelSource([FromKeyedServices("Source")] BitmapModel model)
		: base(model)
	{
	}
}
