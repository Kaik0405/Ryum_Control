using System.Collections.ObjectModel;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de movimientos financieros.
    /// Registra ingresos y egresos de fondos.
    /// </summary>
    public class MovimientosViewModel : BaseViewModel
    {
        private ObservableCollection<Movimiento> _movimientos;
        private Movimiento? _movimientoSeleccionado;
        private DateTime _fechaDesde;
        private DateTime _fechaHasta;
        private TipoMovimiento? _filtroTipo;
        private CategoriaMovimiento? _filtroCategoria;

        #region Properties

        public ObservableCollection<Movimiento> Movimientos
        {
            get => _movimientos;
            set => SetProperty(ref _movimientos, value);
        }

        public Movimiento? MovimientoSeleccionado
        {
            get => _movimientoSeleccionado;
            set => SetProperty(ref _movimientoSeleccionado, value);
        }

        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set => SetProperty(ref _fechaDesde, value, CargarMovimientos);
        }

        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set => SetProperty(ref _fechaHasta, value, CargarMovimientos);
        }

        public TipoMovimiento? FiltroTipo
        {
            get => _filtroTipo;
            set => SetProperty(ref _filtroTipo, value, FiltrarMovimientos);
        }

        public CategoriaMovimiento? FiltroCategoria
        {
            get => _filtroCategoria;
            set => SetProperty(ref _filtroCategoria, value, FiltrarMovimientos);
        }

        /// <summary>
        /// Total de ingresos en el período.
        /// </summary>
        public decimal TotalIngresos => Movimientos?
            .Where(m => m.Tipo == TipoMovimiento.Ingreso)
            .Sum(m => m.Monto) ?? 0;

        /// <summary>
        /// Total de egresos en el período.
        /// </summary>
        public decimal TotalEgresos => Movimientos?
            .Where(m => m.Tipo == TipoMovimiento.Egreso)
            .Sum(m => m.Monto) ?? 0;

        /// <summary>
        /// Balance del período.
        /// </summary>
        public decimal Balance => TotalIngresos - TotalEgresos;

        /// <summary>
        /// Tipos de movimiento disponibles para filtrar.
        /// </summary>
        public Array TiposMovimiento => Enum.GetValues(typeof(TipoMovimiento));

        /// <summary>
        /// Categorías disponibles para filtrar.
        /// </summary>
        public Array Categorias => Enum.GetValues(typeof(CategoriaMovimiento));

        #endregion

        #region Commands

        public ICommand RegistrarIngresoCommand { get; }
        public ICommand RegistrarEgresoCommand { get; }
        public ICommand EditarMovimientoCommand { get; }
        public ICommand EliminarMovimientoCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }

        #endregion

        public MovimientosViewModel()
        {
            _movimientos = new ObservableCollection<Movimiento>();

            // Rango de fechas por defecto (mes actual)
            _fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _fechaHasta = _fechaDesde.AddMonths(1).AddDays(-1);

            RegistrarIngresoCommand = new RelayCommand(_ => RegistrarIngreso());
            RegistrarEgresoCommand = new RelayCommand(_ => RegistrarEgreso());
            EditarMovimientoCommand = new RelayCommand(_ => EditarMovimiento(), _ => MovimientoSeleccionado != null);
            EliminarMovimientoCommand = new RelayCommand(_ => EliminarMovimiento(), _ => MovimientoSeleccionado != null);
            VerDetalleCommand = new RelayCommand(_ => VerDetalle(), _ => MovimientoSeleccionado != null);
            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            CargarMovimientos();
        }

        private void CargarMovimientos()
        {
            // TODO: Cargar desde base de datos
            Movimientos.Clear();
            ActualizarTotales();
        }

        private void FiltrarMovimientos()
        {
            // TODO: Implementar filtrado
            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            OnPropertyChanged(nameof(TotalIngresos));
            OnPropertyChanged(nameof(TotalEgresos));
            OnPropertyChanged(nameof(Balance));
        }

        private void RegistrarIngreso()
        {
            // TODO: Abrir diálogo para registrar ingreso
        }

        private void RegistrarEgreso()
        {
            // TODO: Abrir diálogo para registrar egreso
        }

        private void EditarMovimiento()
        {
            // TODO: Editar movimiento seleccionado
        }

        private void EliminarMovimiento()
        {
            // TODO: Confirmar y eliminar movimiento
        }

        private void VerDetalle()
        {
            // TODO: Mostrar detalle del movimiento
        }

        private void LimpiarFiltros()
        {
            FiltroTipo = null;
            FiltroCategoria = null;
        }
    }
}
