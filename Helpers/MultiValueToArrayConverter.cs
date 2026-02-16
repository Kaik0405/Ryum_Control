using System.Globalization;
using System.Windows.Data;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte múltiples valores en un array de objetos para pasar como CommandParameter.
    /// Uso: MultiBinding con este converter para pasar ComboProducto + ComboProductoInventario al comando.
    /// </summary>
    public class MultiValueToArrayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Clone();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
