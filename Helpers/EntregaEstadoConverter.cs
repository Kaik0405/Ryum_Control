using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestionApp.Models;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte una Entrega en su color de estado:
    /// Rojo → Vencida, Naranja → Urgente (≤2 días), Verde → Entregada, Gris → Normal.
    /// Parámetro "bg" → color claro (fondo de fila), otro → color fuerte (texto/icono).
    /// </summary>
    public class EntregaEstadoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Entrega entrega)
                return Brushes.Transparent;

            var param = parameter?.ToString() ?? "fg";

            if (entrega.Entregada)
            {
                return param == "bg"
                    ? (Brush)new SolidColorBrush(Color.FromRgb(232, 245, 233))   // #E8F5E9
                    : (Brush)new SolidColorBrush(Color.FromRgb(46, 125, 50));     // #2E7D32
            }

            if (entrega.Vencida)
            {
                return param == "bg"
                    ? (Brush)new SolidColorBrush(Color.FromRgb(255, 235, 238))   // #FFEBEE
                    : (Brush)new SolidColorBrush(Color.FromRgb(198, 40, 40));     // #C62828
            }

            if (entrega.Urgente)
            {
                return param == "bg"
                    ? (Brush)new SolidColorBrush(Color.FromRgb(255, 243, 224))   // #FFF3E0
                    : (Brush)new SolidColorBrush(Color.FromRgb(230, 81, 0));      // #E65100
            }

            // Normal (pendiente sin urgencia)
            return param == "bg"
                ? Brushes.White
                : (Brush)new SolidColorBrush(Color.FromRgb(66, 66, 66)); // #424242
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte una Entrega en un texto de estado legible.
    /// </summary>
    public class EntregaEstadoTextoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Entrega entrega)
                return "—";

            if (entrega.Entregada)
                return $"✅ Entregada ({entrega.FechaEntregada:dd/MM})";

            if (entrega.Vencida)
                return $"🔴 Vencida ({Math.Abs(entrega.DiasRestantes)} día(s))";

            if (entrega.Urgente)
                return $"🟠 Urgente ({entrega.DiasRestantes} día(s))";

            return $"⏳ {entrega.DiasRestantes} día(s) restantes";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convierte un booleano invertido a Visibility.
    /// true → Collapsed, false → Visible.
    /// </summary>
    public class InverseBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return System.Windows.Visibility.Collapsed;
            return System.Windows.Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Invierte un valor booleano: true → false, false → true.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return false;
        }
    }
}
