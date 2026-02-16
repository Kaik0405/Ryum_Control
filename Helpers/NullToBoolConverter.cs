using System.Globalization;
using System.Windows.Data;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Devuelve true si el valor es null, false si no lo es.
    /// Útil para habilitar/deshabilitar campos: el TextBox se habilita solo si no hay selección de inventario.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Devuelve true si la colección tiene elementos.
    /// </summary>
    public class CollectionAnyToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.ICollection col)
                return col.Count > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
