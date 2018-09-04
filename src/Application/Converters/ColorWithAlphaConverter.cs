namespace ZoomIn
{
	using System;
	using System.Globalization;
	using System.Windows.Data;
	using System.Windows.Media;

	public class ColorWithAlphaConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color)
			{
				var color = (Color)value;

				int alpha = 0xFF;
				var alphaDouble = double.NaN;
				var alphaString = parameter as string;
				if (alphaString != null && double.TryParse(alphaString, NumberStyles.Float, culture, out alphaDouble))
				{
					alphaDouble = Math.Min(Math.Max(alphaDouble, 0.0), 1.0);
					alpha = (int)(alpha * alphaDouble);
				}

				value = Color.FromArgb((byte)alpha, (byte)color.R, (byte)color.G, (byte)color.B);
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion IValueConverter Members
	}
}
