using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PixelInspector.Configuration;
using PixelInspector.Model;
using PixelInspector.View;
using PixelInspector.ViewModel;
using Tasler.ComponentModel;
using Tasler.Windows;

namespace PixelInspector;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
	: HostedApplication
	, IConfigureHostBuilder
	, IPopulateViewModelMapper
{
	public App(IHost host)
		: base(host)
	{
	}

	public static void ConfigureHostBuilder(IHostApplicationBuilder builder)
	{
		// Add additional services and such
		builder.Services
			.AddActivatedSingleton<IInteractionService, InteractionService>()

			.AddActivatedSingleton<BitmapModel, BitmapModel>()
			.AddActivatedSingleton<ViewSettingsModel, ViewSettingsModel>()
			.AddTransient<WindowPlacementModel, WindowPlacementModel>()

			.AddTransient<BitmapViewModel, BitmapViewModel>()
			.AddSingleton<ScreenImageViewModel, ScreenImageViewModel>()
			.AddSingleton<SelectionViewModel, SelectionViewModel>()
			.AddSingleton<ViewSettingsViewModel, ViewSettingsViewModel>()
			.AddSingleton<LocateToolViewModel, LocateToolViewModel>()
			.AddSingleton<LocatingToolViewModel, LocatingToolViewModel>()
			.AddSingleton<MoveToolViewModel, MoveToolViewModel>()
			.AddSingleton<SelectToolViewModel, SelectToolViewModel>()

			.AddSingleton<ScreenImageView, ScreenImageView>()
			.AddTransient<SelectionView, SelectionView>()
			.AddTransient<LocateToolView, LocateToolView>()
			.AddTransient<LocatingToolView, LocatingToolView>()
			.AddTransient<MoveToolView, MoveToolView>()
			.AddTransient<SelectToolView, SelectToolView>()
			;
	}

	public static void Populate(IViewModelMapper mapper)
	{
		mapper
			.AddMapping<LocateToolViewModel, LocateToolView>()
			.AddMapping<LocatingToolViewModel, LocatingToolView>()
			.AddMapping<MoveToolViewModel, MoveToolView>()
			.AddMapping<ScreenImageViewModel, ScreenImageView>()
			.AddMapping<SelectionViewModel, SelectionView>()
			.AddMapping<SelectToolViewModel, SelectToolView>();
	}

	/// <summary>
	/// Application Entry Point
	/// </summary>
	[STAThread]
	public static int Main(string[] args)
	{
		PixelInspector.Properties.Settings.Default.SetAutoSaveDeferral(TimeSpan.FromSeconds(2));
		int result = HostedApplication.MainCore<App, MainView, MainViewModel>(args);
		PixelInspector.Properties.Settings.Default.ClearAutoSaveDeferral();
		return result;
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		if (this.MainWindow.DataContext is INotifyPropertyChanged notifyPropertyChanged)
			notifyPropertyChanged.PropertyChanged += this.MainViewModel_PropertyChanged;
	}

	private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(MainViewModel.ApplicationState):
				if (sender is MainViewModel mainViewModel && mainViewModel.ApplicationState is ApplicationStateUnloading)
					this.MainWindow.Close();
				break;
		}
	}
}
