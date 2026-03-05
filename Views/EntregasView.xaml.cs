using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace GestionApp.Views
{
    public partial class EntregasView : UserControl
    {
        public EntregasView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Exporta el panel de conformidad como imagen PNG.
        /// </summary>
        private void ExportarConformidadImagen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var panel = ConformidadPanel;
                if (panel == null) return;

                // Medir el panel a su tamaño natural
                var naturalWidth = panel.Width > 0 ? panel.Width : 650;
                panel.Measure(new Size(naturalWidth, double.PositiveInfinity));
                var desiredSize = panel.DesiredSize;
                var renderWidth = naturalWidth;
                var renderHeight = desiredSize.Height > 0 ? desiredSize.Height : panel.ActualHeight;

                // Forzar layout al tamaño completo
                panel.Arrange(new Rect(0, 0, renderWidth, renderHeight));
                panel.UpdateLayout();

                // Render a bitmap a 2x para buena calidad
                var dpi = 192.0;
                var scaleFactor = dpi / 96.0;
                var renderBitmap = new RenderTargetBitmap(
                    (int)(renderWidth * scaleFactor),
                    (int)(renderHeight * scaleFactor),
                    dpi, dpi,
                    PixelFormats.Pbgra32);

                // Usar DrawingVisual para capturar correctamente
                var drawingVisual = new DrawingVisual();
                using (var context = drawingVisual.RenderOpen())
                {
                    var brush = new VisualBrush(panel)
                    {
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top
                    };
                    context.DrawRectangle(brush, null, new Rect(0, 0, renderWidth, renderHeight));
                }
                renderBitmap.Render(drawingVisual);

                // Diálogo guardar
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Imagen PNG (*.png)|*.png",
                    FileName = $"Conformidad_{DateTime.Now:yyyyMMdd_HHmmss}.png",
                    Title = "Guardar Modelo de Conformidad"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                    using var stream = File.Create(saveDialog.FileName);
                    encoder.Save(stream);

                    MessageBox.Show(
                        $"Imagen guardada en:\n{saveDialog.FileName}",
                        "Exportación exitosa",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

                // Invalidar layout para que el Viewbox lo reacomode
                panel.InvalidateMeasure();
                panel.InvalidateArrange();
                panel.UpdateLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al exportar imagen: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Imprime el panel de conformidad.
        /// </summary>
        private void ImprimirConformidad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var panel = ConformidadPanel;
                if (panel == null) return;

                var printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Obtener tamaño de página imprimible
                    var pageWidth = printDialog.PrintableAreaWidth;
                    var pageHeight = printDialog.PrintableAreaHeight;

                    // Medir a tamaño natural
                    var naturalWidth = panel.Width > 0 ? panel.Width : 650;
                    panel.Measure(new Size(naturalWidth, double.PositiveInfinity));
                    var naturalHeight = panel.DesiredSize.Height;

                    // Calcular escala para que quepa en la página con margen
                    var margin = 30.0;
                    var scaleX = (pageWidth - 2 * margin) / naturalWidth;
                    var scaleY = (pageHeight - 2 * margin) / naturalHeight;
                    var scale = Math.Min(scaleX, scaleY);

                    // Aplicar transformación temporal
                    var originalTransform = panel.LayoutTransform;
                    panel.LayoutTransform = new ScaleTransform(scale, scale);

                    // Medir y posicionar
                    panel.Measure(new Size(pageWidth, pageHeight));
                    panel.Arrange(new Rect(new Point(margin, margin), 
                        new Size(panel.DesiredSize.Width, panel.DesiredSize.Height)));

                    printDialog.PrintVisual(panel, "Modelo de Conformidad");

                    // Restaurar
                    panel.LayoutTransform = originalTransform;
                    panel.InvalidateMeasure();
                    panel.InvalidateArrange();
                    panel.UpdateLayout();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al imprimir: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
