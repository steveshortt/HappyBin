using System;
using System.Windows.Data;

namespace HappyBin.AutoUpdater
{
	public class BooleanToValueConverter : IValueConverter
	{
		public object FalseValue { get; set; }
		public object TrueValue { get; set; }

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if( value == null )
				return FalseValue;

			return (bool)value ? TrueValue : FalseValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value != null ? value.Equals( TrueValue ) : false;
		}
	}
}