using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace GestionApp.Views
{
    /// <summary>
    /// Vista de gestión de finanzas.
    /// </summary>
    public partial class MovimientosView : UserControl
    {
        public MovimientosView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Cierra el overlay de formulario al hacer clic en el fondo oscuro.
        /// </summary>
        private void OverlayBackground_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is GestionApp.ViewModels.MovimientosViewModel vm && vm.CancelarCommand.CanExecute(null))
            {
                vm.CancelarCommand.Execute(null);
            }
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
