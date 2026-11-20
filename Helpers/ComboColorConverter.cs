using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Palette cromática consistente para el catálogo: azul marino, azul acero y dorado.
    /// El parámetro "bg" devuelve el fondo suave; cualquier otro devuelve el color principal.
    /// </summary>
    public class ComboColorConverter : IValueConverter
    {
        private static readonly string[] StrongColors =
        {
            "#173F63", "#2B658A", "#9B7100"
        };

        private static readonly string[] LightColors =
        {
            "#EAF1F7", "#EDF4F8", "#FFF4D6"
        };

        private static readonly BrushConverter BrushConverter = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int id)
                return Brushes.Gray;

            var index = ((id - 1) % StrongColors.Length + StrongColors.Length) % StrongColors.Length;
            var variant = parameter?.ToString() ?? "border";
            var hex = variant == "bg" ? LightColors[index] : StrongColors[index];
            return BrushConverter.ConvertFromString(hex) as Brush ?? Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
