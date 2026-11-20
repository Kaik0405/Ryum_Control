using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using GestionApp.Models;

namespace GestionApp.Helpers
{
    /// <summary>
    /// Convierte TipoCombo en colores para las fichas de costo.
    /// Combo → azul marino, Agrego → dorado, Festejo → azul acero.
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
                    "bg" => Brush("#EAF1F7"),
                    "text" => Brush("#173F63"),
                    _ => Brush("#173F63")
                },
                TipoCombo.Agrego => param switch
                {
                    "bg" => Brush("#FFF4D6"),
                    "text" => Brush("#8A6500"),
                    _ => Brush("#A47A08")
                },
                TipoCombo.Festejo => param switch
                {
                    "bg" => Brush("#EDF4F8"),
                    "text" => Brush("#2B658A"),
                    _ => Brush("#2B658A")
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
