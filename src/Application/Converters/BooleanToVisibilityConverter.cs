namespace ZoomIn
{
	using System;
	using System.Globalization;
	using System.Windows;
	using System.Windows.Data;

	/// <summary>
	/// An imlementation of <see cref="IValueConverter"/> that converts a boolean value to one of two
	/// <see cref="Visibility"/> values, specified by the <see cref="True"/> and <see cref="False"/> properties.
	/// </summary>
	public class BooleanToVisibilityConverter : IValueConverter
	{
		#region Constructors
		public BooleanToVisibilityConverter()
			: this(Visibility.Visible, Visibility.Collapsed)
		{
		}

		public BooleanToVisibilityConverter(Visibility trueValue, Visibility falseValue)
		{
			this.TrueValue = trueValue;
			this.FalseValue = falseValue;
		}
		#endregion Constructors

		#region Properties
		public Visibility TrueValue { get; set; }

		public Visibility FalseValue { get; set; }
		#endregion Properties

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool boolValue = System.Convert.ToBoolean(value, culture);

			return boolValue ? this.TrueValue : this.FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion IValueConverter Members
	}
}
