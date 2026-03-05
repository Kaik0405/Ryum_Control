using System.Globalization;
using System.Windows.Data;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte una fecha sumándole un día.
    /// Usado en el Modelo de Conformidad: la fecha de entrega es el día siguiente a la orden.
    /// </summary>
    public class FechaMasUnDiaConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime fecha)
                return fecha.AddDays(1).ToString("dd/MM/yyyy");
            return "Pendiente";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
