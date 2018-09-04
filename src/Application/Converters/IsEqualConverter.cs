namespace ZoomIn
{
	using System;
	using System.ComponentModel;
	using System.Globalization;
	using System.Windows.Data;

	public class IsEqualConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return parameter == null;

			var valueType = value.GetType();
			if (parameter != null && parameter.GetType() != valueType)
			{
				var converter = TypeDescriptor.GetConverter(value);
				parameter = converter.ConvertTo(null, culture, parameter, valueType);
			}

			return object.Equals(value, parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion IValueConverter Members
	}
}
