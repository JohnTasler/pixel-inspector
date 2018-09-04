namespace ZoomIn.ViewModel
{
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Windows;
	using System.Windows.Input;
	using ZoomIn.ComponentModel.Mvvm;
	using ZoomIn.Model;

	public class ViewSettingsViewModel
		: ParentedObservableObject<MainViewModel>
		, IProvideSourceOrigin
	{
		#region Constructors
		public ViewSettingsViewModel(MainViewModel parent, ViewSettingsModel model)
			: base(parent)
		{
			this.model = model;

			// Subscribe to property changes
			this.model.Subscribe(s => s.AutoRefreshMilliseconds, s => this.RaisePropertyChanged(() => this.AutoRefreshInterval));
			this.model.Subscribe(s => s.SourceOrigin           , s => this.RaisePropertyChanged(() => this.SourceOrigin));
		}
		#endregion Constructors

		#region Properties

		public ViewSettingsModel Model
		{
			get { return this.model; }
			set { this.SetProperty(ref this.model, value, nameof(Model)); }
		}
		private ViewSettingsModel model;

		public TimeSpan AutoRefreshInterval
		{
			get { return TimeSpan.FromMilliseconds(this.Model.AutoRefreshMilliseconds); }
			set { this.Model.AutoRefreshMilliseconds = value.TotalMilliseconds; }
		}

		#endregion Properties

		#region SetZoomFactorCommand

		/// <summary>
		/// Gets the SetZoomFactor command.
		/// </summary>
		public ICommand SetZoomFactorCommand
		{
			get
			{
				return this.setZoomFactorCommand ??
					(this.setZoomFactorCommand = new RelayCommand<string>(this.SetZoomFactorCommandExecute, this.SetZoomFactorCommandCanExecute));
			}
		}
		private RelayCommand<string> setZoomFactorCommand;

		private bool SetZoomFactorCommandCanExecute(string parameter)
		{
			int zoomFactor;
			return int.TryParse(parameter, out zoomFactor) && 1 <= zoomFactor && zoomFactor <= 32;
		}

		private void SetZoomFactorCommandExecute(string parameter)
		{
			var zoomFactor = int.Parse(parameter);
			this.Model.ZoomFactor = zoomFactor;
		}

		#endregion SetZoomFactorCommand

		#region IncreaseZoomCommand

		/// <summary>
		/// Gets the IncreaseZoom command.
		/// </summary>
		public ICommand IncreaseZoomCommand
		{
			get
			{
				return this.increaseZoomCommand ??
					(this.increaseZoomCommand = new RelayCommand(
						this.IncreaseZoomCommandExecute, this.IncreaseZoomCommandCanExecute));
			}
		}
		private RelayCommand increaseZoomCommand;

		private bool IncreaseZoomCommandCanExecute()
		{
			return this.Model.ZoomFactor < 32;
		}

		private void IncreaseZoomCommandExecute()
		{
			if (this.Model.ZoomFactor < 1)
				this.Model.ZoomFactor = 1.0;
			else
				++this.Model.ZoomFactor;
		}

		#endregion IncreaseZoomCommand

		#region DecreaseZoomCommand

		/// <summary>
		/// Gets the DecreaseZoom command.
		/// </summary>
		public ICommand DecreaseZoomCommand
		{
			get
			{
				return this.decreaseZoomCommand ??
					(this.decreaseZoomCommand = new RelayCommand(
						this.DecreaseZoomCommandExecute, this.DecreaseZoomCommandCanExecute));
			}
		}
		private RelayCommand decreaseZoomCommand;

		private bool DecreaseZoomCommandCanExecute()
		{
			return this.Model.ZoomFactor > 1.0;
		}

		private void DecreaseZoomCommandExecute()
		{
			if (this.Model.ZoomFactor > 1)
				--this.Model.ZoomFactor;
		}

		#endregion DecreaseZoomCommand

		#region ToggleSwitchCommand

		/// <summary>
		/// Gets the ToggleSwitch command.
		/// </summary>
		public ICommand ToggleSwitchCommand
		{
			get
			{
				return this.toggleSwitchCommand ??
					(this.toggleSwitchCommand = new RelayCommand<string>(
						this.ToggleSwitchCommandExecute, this.ToggleSwitchCommandCanExecute));
			}
		}
		private RelayCommand<string> toggleSwitchCommand;

		private bool ToggleSwitchCommandCanExecute(string propertyName)
		{
			var propertyDescriptor = TypeDescriptor.GetProperties(this.Model)[propertyName];
			Debug.Assert(propertyDescriptor != null && propertyDescriptor.PropertyType == typeof(bool) && !propertyDescriptor.IsReadOnly);
			return propertyDescriptor != null && propertyDescriptor.PropertyType == typeof(bool) && !propertyDescriptor.IsReadOnly;
		}

		private void ToggleSwitchCommandExecute(string propertyName)
		{
			var propertyDescriptor = TypeDescriptor.GetProperties(this.Model)[propertyName];
			var value = (bool)propertyDescriptor.GetValue(this.Model);
			propertyDescriptor.SetValue(this.Model, !value);
		}

		#endregion ToggleSwitchCommand

		#region SetColorValueDisplayFormatCommand

		/// <summary>
		/// Gets the SetColorValueDisplayFormat command.
		/// </summary>
		public ICommand SetColorValueDisplayFormatCommand
		{
			get
			{
				return this.setColorValueDisplayFormatCommand ??
					(this.setColorValueDisplayFormatCommand =
						new RelayCommand<ColorValueDisplayFormat>(
							this.SetColorValueDisplayFormatCommandExecute,
							this.SetColorValueDisplayFormatCommandCanExecute));
			}
		}
		private RelayCommand<ColorValueDisplayFormat> setColorValueDisplayFormatCommand;

		private bool SetColorValueDisplayFormatCommandCanExecute(ColorValueDisplayFormat parameter)
		{
			return typeof(ColorValueDisplayFormat).IsEnumDefined(parameter);
		}

		private void SetColorValueDisplayFormatCommandExecute(ColorValueDisplayFormat parameter)
		{
			this.Model.ColorValueDisplayFormat = parameter;
		}

		#endregion SetColorValueDisplayFormatCommand

		#region IProvideSourceOrigin Members

		public Point SourceOrigin
		{
			get { return this.Model.SourceOrigin; }
		}

		#endregion IProvideSourceOrigin Members
	}
}
