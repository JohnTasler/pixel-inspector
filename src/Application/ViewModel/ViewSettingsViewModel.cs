using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixelInspector.Model;
using Tasler.ComponentModel;

namespace PixelInspector.ViewModel;

public partial class ViewSettingsViewModel : ObservableObject, IProvideSourceOrigin
{
	#region Constructors
	public ViewSettingsViewModel(ViewSettingsModel model)
	{
		_model = model;

		// Subscribe to model property changes and reflect as our own
		_model.Subscribe(nameof(model.AutoRefreshMilliseconds), s => this.OnPropertyChanged(nameof(this.AutoRefreshInterval)));
		_model.Subscribe(nameof(model.SourceOrigin), s => this.OnPropertyChanged(nameof(this.SourceOrigin)));
	}
	#endregion Constructors

	#region Properties

	[ObservableProperty]
	private ViewSettingsModel _model;

	public TimeSpan AutoRefreshInterval
	{
		get => TimeSpan.FromMilliseconds(this.Model.AutoRefreshMilliseconds);
		set => this.Model.AutoRefreshMilliseconds = value.TotalMilliseconds;
	}

	#endregion Properties

	#region SetZoomFactorCommand

	private bool CanSetZoomFactor(string parameter)
	{
		return int.TryParse(parameter, out var zoomFactor) && 1 <= zoomFactor && zoomFactor <= 32;
	}

	[RelayCommand(CanExecute = nameof(CanSetZoomFactor))]
	private void SetZoomFactor(string parameter)
	{
		var zoomFactor = int.Parse(parameter);
		this.Model.ZoomFactor = zoomFactor;
	}

	#endregion SetZoomFactorCommand

	#region IncreaseZoomCommand

	private bool CanIncreaseZoom()
	{
		return this.Model.ZoomFactor < 32;
	}

	[RelayCommand(CanExecute = nameof(CanIncreaseZoom))]
	private void IncreaseZoom()
	{
		if (this.Model.ZoomFactor < 1)
			this.Model.ZoomFactor = 1.0;
		else
			++this.Model.ZoomFactor;
	}

	#endregion IncreaseZoomCommand

	#region DecreaseZoomCommand

	private bool CanDecreaseZoom()
	{
		return this.Model.ZoomFactor > 1.0;
	}

	[RelayCommand(CanExecute = nameof(CanDecreaseZoom))]
	private void DecreaseZoom()
	{
		if (this.Model.ZoomFactor > 1)
			--this.Model.ZoomFactor;
	}

	#endregion DecreaseZoomCommand

	#region ToggleSwitchCommand

	private bool CanToggleSwitch(string propertyName)
	{
		var propertyDescriptor = TypeDescriptor.GetProperties(this.Model)[propertyName];
		Debug.Assert(propertyDescriptor is not null && propertyDescriptor.PropertyType == typeof(bool) && !propertyDescriptor.IsReadOnly);
		return propertyDescriptor is not null && propertyDescriptor.PropertyType == typeof(bool) && !propertyDescriptor.IsReadOnly;
	}

	[RelayCommand(CanExecute = nameof(CanToggleSwitch))]
	private void ToggleSwitch(string propertyName)
	{
		var propertyDescriptor = TypeDescriptor.GetProperties(this.Model)[propertyName];
		var value = (bool)(propertyDescriptor?.GetValue(this.Model)?? false);
		propertyDescriptor?.SetValue(this.Model, !value);
	}

	#endregion ToggleSwitchCommand

	#region SetColorValueDisplayFormatCommand

	private bool CanSetColorValueDisplayFormat(ColorValueDisplayFormat parameter)
		=> typeof(ColorValueDisplayFormat).IsEnumDefined(parameter);

	[RelayCommand(CanExecute = nameof(CanSetColorValueDisplayFormat))]
	private void SetColorValueDisplayFormat(ColorValueDisplayFormat parameter)
	{
		this.Model.ColorValueDisplayFormat = parameter;
	}

	#endregion SetColorValueDisplayFormatCommand

	#region IProvideSourceOrigin Members

	public Point SourceOrigin => this.Model.SourceOrigin;

	#endregion IProvideSourceOrigin Members
}
