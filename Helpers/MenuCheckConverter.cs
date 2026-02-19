using System;
using System.Globalization;
using System.Windows.Data;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte el nombre del menú seleccionado a bool para RadioButton.IsChecked.
    /// ConverterParameter = nombre esperado del menú.
    /// Retorna true si MenuSeleccionado == ConverterParameter.
    /// </summary>
    public class MenuCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string selected && parameter is string expected)
                return string.Equals(selected, expected, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // No necesitamos ConvertBack porque el binding es OneWay
            return Binding.DoNothing;
        }
    }
}
