using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace GestionApp.Views
{
    /// <summary>
    /// Vista de gestión de inventario.
    /// </summary>
    public partial class InventarioView : UserControl
    {
        public InventarioView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Solo permite dígitos, coma y punto en campos numéricos.
        /// </summary>
        private void NumericOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[\d.,]$");
        }
    }
}
