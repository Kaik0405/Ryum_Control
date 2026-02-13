using System.Windows.Input;
using GestionApp.Helpers;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la generación de reportes.
    /// Permite generar reportes mensuales y exportar a PDF/Excel.
    /// </summary>
    public class ReportesViewModel : BaseViewModel
    {
        private int _mesSeleccionado;
        private int _añoSeleccionado;
        private bool _incluirFichasCosto = true;
        private bool _incluirMovimientos = true;
        private bool _incluirResumenFinanciero = true;
        private bool _generandoReporte;
        private string _ultimoReporteGenerado = string.Empty;

        #region Properties

        public int MesSeleccionado
        {
            get => _mesSeleccionado;
            set => SetProperty(ref _mesSeleccionado, value);
        }

        public int AñoSeleccionado
        {
            get => _añoSeleccionado;
            set => SetProperty(ref _añoSeleccionado, value);
        }

        public bool IncluirFichasCosto
        {
            get => _incluirFichasCosto;
            set => SetProperty(ref _incluirFichasCosto, value);
        }

        public bool IncluirMovimientos
        {
            get => _incluirMovimientos;
            set => SetProperty(ref _incluirMovimientos, value);
        }

        public bool IncluirResumenFinanciero
        {
            get => _incluirResumenFinanciero;
            set => SetProperty(ref _incluirResumenFinanciero, value);
        }

        public bool GenerandoReporte
        {
            get => _generandoReporte;
            set => SetProperty(ref _generandoReporte, value);
        }

        public string UltimoReporteGenerado
        {
            get => _ultimoReporteGenerado;
            set => SetProperty(ref _ultimoReporteGenerado, value);
        }

        /// <summary>
        /// Lista de meses para seleccionar.
        /// </summary>
        public List<KeyValuePair<int, string>> Meses { get; } = new()
        {
            new(1, "Enero"), new(2, "Febrero"), new(3, "Marzo"),
            new(4, "Abril"), new(5, "Mayo"), new(6, "Junio"),
            new(7, "Julio"), new(8, "Agosto"), new(9, "Septiembre"),
            new(10, "Octubre"), new(11, "Noviembre"), new(12, "Diciembre")
        };

        /// <summary>
        /// Lista de años disponibles.
        /// </summary>
        public List<int> Años { get; }

        #endregion

        #region Commands

        public ICommand GenerarReportePDFCommand { get; }
        public ICommand GenerarReporteExcelCommand { get; }
        public ICommand VistaPreviewCommand { get; }
        public ICommand AbrirCarpetaReportesCommand { get; }

        #endregion

        public ReportesViewModel()
        {
            // Inicializar con fecha actual
            _mesSeleccionado = DateTime.Now.Month;
            _añoSeleccionado = DateTime.Now.Year;

            // Años disponibles: últimos 5 años
            Años = Enumerable.Range(DateTime.Now.Year - 4, 5).Reverse().ToList();

            GenerarReportePDFCommand = new RelayCommand(_ => GenerarReportePDF(), _ => !GenerandoReporte && HayContenidoSeleccionado());
            GenerarReporteExcelCommand = new RelayCommand(_ => GenerarReporteExcel(), _ => !GenerandoReporte && HayContenidoSeleccionado());
            VistaPreviewCommand = new RelayCommand(_ => MostrarVistaPrevia(), _ => !GenerandoReporte);
            AbrirCarpetaReportesCommand = new RelayCommand(_ => AbrirCarpetaReportes());
        }

        private bool HayContenidoSeleccionado()
        {
            return IncluirFichasCosto || IncluirMovimientos || IncluirResumenFinanciero;
        }

        private async void GenerarReportePDF()
        {
            GenerandoReporte = true;
            try
            {
                // TODO: Implementar generación de PDF
                await Task.Delay(1000); // Simular generación
                UltimoReporteGenerado = $"Reporte_{Meses[MesSeleccionado - 1].Value}_{AñoSeleccionado}.pdf";
            }
            finally
            {
                GenerandoReporte = false;
            }
        }

        private async void GenerarReporteExcel()
        {
            GenerandoReporte = true;
            try
            {
                // TODO: Implementar generación de Excel
                await Task.Delay(1000); // Simular generación
                UltimoReporteGenerado = $"Reporte_{Meses[MesSeleccionado - 1].Value}_{AñoSeleccionado}.xlsx";
            }
            finally
            {
                GenerandoReporte = false;
            }
        }

        private void MostrarVistaPrevia()
        {
            // TODO: Mostrar vista previa del reporte
        }

        private void AbrirCarpetaReportes()
        {
            // TODO: Abrir carpeta donde se guardan los reportes
        }
    }
}
