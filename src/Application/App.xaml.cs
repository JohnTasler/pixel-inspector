namespace ZoomIn
{
	using System;
	using System.ComponentModel;
	using System.Windows;
	using ZoomIn.Configuration;
	using ZoomIn.View;
	using ZoomIn.ViewModel;

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private MainViewModel mainViewModel;

		/// <summary>
		/// Initializes a new instance of the <see cref="App"/> class.
		/// </summary>
		/// <exception cref="T:System.InvalidOperationException">
		/// More than one instance of the <see cref="T:System.Windows.Application"/> class is created
		/// per <see cref="T:System.AppDomain"/>.
		/// </exception>
		private App()
		{
			this.InitializeComponent();

			this.mainViewModel = new MainViewModel();
			this.mainViewModel.PropertyChanged += this.mainViewModel_PropertyChanged;

			this.MainWindow = new MainView();
			this.MainWindow.DataContext = mainViewModel;
			this.MainWindow.Show();
		}

		/// <summary>
		/// Application Entry Point.
		/// </summary>
		[STAThread]
		private static int Main()
		{
			ZoomIn.Properties.Settings.Default.SetAutoSaveDeferral(TimeSpan.FromSeconds(5));
			var app = new App();
			var result = app.Run();
			ZoomIn.Properties.Settings.Default.ClearAutoSaveDeferral();
			return result;
		}

		private void mainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "ApplicationState":
					if (this.mainViewModel.ApplicationState is ApplicationStateUnloading)
						this.MainWindow.Close();
					break;
			}
		}
	}
}
