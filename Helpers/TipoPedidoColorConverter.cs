using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestionApp.Models;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte TipoCombo en colores para las fichas de costo.
    /// Combo → Azul, Agrego → Verde, Festejo → Naranja.
    /// Parámetro "bg" → color claro, "text" → color fuerte, otro → borde.
    /// </summary>
    public class TipoPedidoColorConverter : IValueConverter
    {
        private static readonly BrushConverter _bc = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tipo = value is TipoCombo tc ? tc : TipoCombo.Combo;
            var param = parameter?.ToString() ?? "border";

            return tipo switch
            {
                TipoCombo.Combo => param switch
                {
                    "bg" => Brush("#E3F2FD"),
                    "text" => Brush("#1565C0"),
                    _ => Brush("#1976D2")
                },
                TipoCombo.Agrego => param switch
                {
                    "bg" => Brush("#E8F5E9"),
                    "text" => Brush("#2E7D32"),
                    _ => Brush("#388E3C")
                },
                TipoCombo.Festejo => param switch
                {
                    "bg" => Brush("#FFF3E0"),
                    "text" => Brush("#E65100"),
                    _ => Brush("#F57C00")
                },
                _ => Brushes.Gray
            };
        }

        private static Brush Brush(string hex)
        {
            return _bc.ConvertFromString(hex) as Brush ?? Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Formatea decimal a moneda con $ y separador de miles.
    /// </summary>
    public class CurrencyFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return $"${d:N2}";
            return "$0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
