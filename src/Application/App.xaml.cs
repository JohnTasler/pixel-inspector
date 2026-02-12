using System.ComponentModel;
using System.Configuration;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PixelInspector.Model;
using PixelInspector.Properties;
using PixelInspector.View;
using PixelInspector.ViewModel;
using Tasler.ComponentModel;
using Tasler.Configuration;
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
		#if DEBUG
		builder.Environment.EnvironmentName = Environments.Development;
		#endif // DEBUG

		// Add additional services and such
		builder.Services
			.AddSingleton<IInteractionService, InteractionService>()

			.AddKeyedSingleton<BitmapModel, BitmapModel>("Source")
			.AddKeyedSingleton<BitmapModel, BitmapModel>("Zoomed")
			.AddActivatedSingleton<Settings, Settings>(p => Settings.Default)
			.AddActivatedSingleton<ViewSettingsModel, ViewSettingsModel>(p => Settings.Default.LatestViewSettings)

			.AddKeyedSingleton<BitmapViewModelBase, BitmapViewModelSource>("Source")
			.AddKeyedSingleton<BitmapViewModelBase, BitmapViewModelZoomed>("Zoomed")
			.AddSingleton<ScreenImageViewModel, ScreenImageViewModel>()
			.AddSingleton<SelectionViewModel, SelectionViewModel>()
			.AddSingleton<ViewSettingsViewModel, ViewSettingsViewModel>()
			.AddSingleton<LocateToolViewModel, LocateToolViewModel>()
			.AddSingleton<LocatingToolViewModel, LocatingToolViewModel>()
			.AddSingleton<MoveToolViewModel, MoveToolViewModel>()
			.AddSingleton<SelectToolViewModel, SelectToolViewModel>()

			.AddSingleton<LocateToolView, LocateToolView>()
			.AddSingleton<LocatingToolView, LocatingToolView>()
			.AddSingleton<MoveToolView, MoveToolView>()
			.AddSingleton<ScreenImageView, ScreenImageView>()
			.AddSingleton<SelectionView, SelectionView>()
			.AddSingleton<SelectToolView, SelectToolView>()
			;

		// Configure logging
		builder.Logging
			.ClearProviders()
			.AddDebug();
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
	public static async Task<int> Main(string[] args)
	{
		Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
		Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

		Settings.Default.SettingsLoaded += Default_SettingsLoaded;
		Settings.Default.SetAutoSaveDeferral(TimeSpan.FromSeconds(2));
		int result = await MainCore<App, MainView, MainViewModel>(args);
		Settings.Default.ClearAutoSaveDeferral();
		return result;
	}

	private static void Default_SettingsLoaded(object sender, SettingsLoadedEventArgs e)
	{
		Settings.Default.SettingsLoaded -= Default_SettingsLoaded;

		var viewSettings = Settings.Default.LatestViewSettings;
		if (viewSettings is null)
		{
			// Create a new ViewSettingsModel if it doesn't exist
			Settings.Default.LatestViewSettings = new();
		}
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
