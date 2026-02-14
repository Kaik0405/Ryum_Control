using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte el Id de un combo en un color de una paleta rotativa.
    /// Cada combo obtiene un color diferente.
    /// Parámetro "bg" → color claro (fondo header), otro → color fuerte (borde/texto).
    /// </summary>
    public class ComboColorConverter : IValueConverter
    {
        private static readonly string[] StrongColors =
        {
            "#7B1FA2", // purple
            "#1976D2", // blue
            "#E65100", // orange
            "#00796B", // teal
            "#C62828", // red
            "#2E7D32", // green
            "#AD1457", // pink
            "#283593", // indigo
            "#00838F", // cyan
            "#6D4C41"  // brown
        };

        private static readonly string[] LightColors =
        {
            "#F3E5F5", // purple light
            "#E3F2FD", // blue light
            "#FFF3E0", // orange light
            "#E0F2F1", // teal light
            "#FFEBEE", // red light
            "#E8F5E9", // green light
            "#FCE4EC", // pink light
            "#E8EAF6", // indigo light
            "#E0F7FA", // cyan light
            "#EFEBE9"  // brown light
        };

        private static readonly BrushConverter _bc = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int id)
                return Brushes.Gray;

            var index = ((id - 1) % StrongColors.Length + StrongColors.Length) % StrongColors.Length;
            var param = parameter?.ToString() ?? "border";
            var hex = param == "bg" ? LightColors[index] : StrongColors[index];

            return _bc.ConvertFromString(hex) as Brush ?? Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
