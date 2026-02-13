namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para el Dashboard principal.
    /// Muestra resumen de fondos, ventas y estado general.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private decimal _fondosActuales;
        private decimal _fondosIniciales;
        private int _ventasDelMes;
        private decimal _gananciasDelMes;
        private decimal _gastosDelMes;
        private string _mesActual = string.Empty;

        #region Properties

        public decimal FondosActuales
        {
            get => _fondosActuales;
            set => SetProperty(ref _fondosActuales, value);
        }

        public decimal FondosIniciales
        {
            get => _fondosIniciales;
            set => SetProperty(ref _fondosIniciales, value);
        }

        public int VentasDelMes
        {
            get => _ventasDelMes;
            set => SetProperty(ref _ventasDelMes, value);
        }

        public decimal GananciasDelMes
        {
            get => _gananciasDelMes;
            set => SetProperty(ref _gananciasDelMes, value);
        }

        public decimal GastosDelMes
        {
            get => _gastosDelMes;
            set => SetProperty(ref _gastosDelMes, value);
        }

        public string MesActual
        {
            get => _mesActual;
            set => SetProperty(ref _mesActual, value);
        }

        /// <summary>
        /// Balance del mes: Ingresos - Egresos
        /// </summary>
        public decimal BalanceMes => GananciasDelMes - GastosDelMes;

        #endregion

        public DashboardViewModel()
        {
            MesActual = DateTime.Now.ToString("MMMM yyyy");
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            CargarDatos();
        }

        /// <summary>
        /// Carga los datos del dashboard desde los servicios.
        /// </summary>
        private void CargarDatos()
        {
            // TODO: Implementar carga desde servicios/base de datos
            // Por ahora valores de ejemplo
            FondosIniciales = 0;
            FondosActuales = 0;
            VentasDelMes = 0;
            GananciasDelMes = 0;
            GastosDelMes = 0;
        }
    }
}
