using System.Collections.ObjectModel;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de fichas de costo.
    /// Permite crear, ver y exportar fichas de costo de envíos.
    /// </summary>
    public class FichasCostoViewModel : BaseViewModel
    {
        private ObservableCollection<FichaCosto> _fichas;
        private FichaCosto? _fichaSeleccionada;
        private DateTime _fechaDesde;
        private DateTime _fechaHasta;
        private string _filtro = string.Empty;

        #region Properties

        public ObservableCollection<FichaCosto> Fichas
        {
            get => _fichas;
            set => SetProperty(ref _fichas, value);
        }

        public FichaCosto? FichaSeleccionada
        {
            get => _fichaSeleccionada;
            set => SetProperty(ref _fichaSeleccionada, value);
        }

        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set => SetProperty(ref _fechaDesde, value, CargarFichas);
        }

        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set => SetProperty(ref _fechaHasta, value, CargarFichas);
        }

        public string Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value, FiltrarFichas);
        }

        /// <summary>
        /// Total de fichas en el período seleccionado.
        /// </summary>
        public int TotalFichas => Fichas?.Count ?? 0;

        /// <summary>
        /// Suma de ventas en el período.
        /// </summary>
        public decimal TotalVentas => Fichas?.Sum(f => f.PrecioVentaUSD) ?? 0;

        #endregion

        #region Commands

        public ICommand CrearFichaCommand { get; }
        public ICommand EditarFichaCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand ExportarPDFCommand { get; }
        public ICommand ExportarExcelCommand { get; }
        public ICommand ImprimirCommand { get; }

        #endregion

        public FichasCostoViewModel()
        {
            _fichas = new ObservableCollection<FichaCosto>();
            
            // Establecer rango de fechas por defecto (mes actual)
            _fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _fechaHasta = _fechaDesde.AddMonths(1).AddDays(-1);

            CrearFichaCommand = new RelayCommand(_ => CrearFicha());
            EditarFichaCommand = new RelayCommand(_ => EditarFicha(), _ => FichaSeleccionada != null);
            VerDetalleCommand = new RelayCommand(_ => VerDetalle(), _ => FichaSeleccionada != null);
            ExportarPDFCommand = new RelayCommand(_ => ExportarPDF());
            ExportarExcelCommand = new RelayCommand(_ => ExportarExcel());
            ImprimirCommand = new RelayCommand(_ => Imprimir(), _ => FichaSeleccionada != null);
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            CargarFichas();
        }

        private void CargarFichas()
        {
            // TODO: Cargar desde base de datos con filtro de fechas
            Fichas.Clear();
            OnPropertyChanged(nameof(TotalFichas));
            OnPropertyChanged(nameof(TotalVentas));
        }

        private void FiltrarFichas()
        {
            // TODO: Implementar filtrado
        }

        private void CrearFicha()
        {
            // TODO: Abrir diálogo para crear ficha de costo
        }

        private void EditarFicha()
        {
            // TODO: Abrir diálogo para editar ficha
        }

        private void VerDetalle()
        {
            // TODO: Mostrar detalle completo de la ficha
        }

        private void ExportarPDF()
        {
            // TODO: Exportar fichas del período a PDF
        }

        private void ExportarExcel()
        {
            // TODO: Exportar fichas del período a Excel
        }

        private void Imprimir()
        {
            // TODO: Imprimir ficha seleccionada
        }
    }
}
