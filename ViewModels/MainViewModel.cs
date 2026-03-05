using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación.
    /// Controla la navegación y el estado general de la UI.
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IConfiguracionService _configuracionService;
        private string _titulo = "Ryum Control";
        private string _statusMessage = "Listo";
        private string _menuSeleccionado = "Inicio";
        private string _nombreUsuario = "";
        private string _inicialesUsuario = "";
        private bool _mostrarConfigNombre = false;
        private string _formNombreUsuario = "";

        #region Properties

        public string Titulo
        {
            get => _titulo;
            set => SetProperty(ref _titulo, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string MenuSeleccionado
        {
            get => _menuSeleccionado;
            set => SetProperty(ref _menuSeleccionado, value);
        }

        public string NombreUsuario
        {
            get => _nombreUsuario;
            set
            {
                SetProperty(ref _nombreUsuario, value);
                InicialesUsuario = GenerarIniciales(value);
            }
        }

        public string InicialesUsuario
        {
            get => _inicialesUsuario;
            set => SetProperty(ref _inicialesUsuario, value);
        }

        public bool MostrarConfigNombre
        {
            get => _mostrarConfigNombre;
            set => SetProperty(ref _mostrarConfigNombre, value);
        }

        public string FormNombreUsuario
        {
            get => _formNombreUsuario;
            set => SetProperty(ref _formNombreUsuario, value);
        }

        /// <summary>
        /// ViewModel actual mostrado en el área de contenido.
        /// </summary>
        public BaseViewModel? CurrentViewModel => _navigationService.CurrentViewModel;

        #endregion

        #region Commands

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToInventarioCommand { get; }
        public ICommand NavigateToCombosCommand { get; }
        public ICommand NavigateToEntregasCommand { get; }
        public ICommand NavigateToFichasCostoCommand { get; }
        public ICommand NavigateToMovimientosCommand { get; }
        public ICommand NavigateToReportesCommand { get; }
        public ICommand NavigateToAjustesCommand { get; }
        public ICommand GuardarNombreCommand { get; }
        public ICommand EditarNombreCommand { get; }

        #endregion

        public MainViewModel(INavigationService navigationService, IConfiguracionService configuracionService)
        {
            _navigationService = navigationService;
            _configuracionService = configuracionService;

            // Suscribirse a cambios de navegación
            _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // Inicializar comandos de navegación
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateTo<DashboardViewModel>("Inicio"));
            NavigateToInventarioCommand = new RelayCommand(_ => NavigateTo<InventarioViewModel>("Inventario"));
            NavigateToCombosCommand = new RelayCommand(_ => NavigateTo<CombosViewModel>("Combos"));
            NavigateToEntregasCommand = new RelayCommand(_ => NavigateTo<EntregasViewModel>("Entregas"));
            NavigateToFichasCostoCommand = new RelayCommand(_ => NavigateTo<FichasCostoViewModel>("Fichas de Costo"));
            NavigateToMovimientosCommand = new RelayCommand(_ => NavigateTo<MovimientosViewModel>("Finanzas"));
            NavigateToReportesCommand = new RelayCommand(_ => NavigateTo<ReportesViewModel>("Reportes"));
            NavigateToAjustesCommand = new RelayCommand(_ => NavigateTo<AjustesViewModel>("Ajustes"));
            GuardarNombreCommand = new RelayCommand(async _ => await GuardarNombreAsync(), _ => !string.IsNullOrWhiteSpace(FormNombreUsuario));
            EditarNombreCommand = new RelayCommand(_ =>
            {
                FormNombreUsuario = NombreUsuario;
                MostrarConfigNombre = true;
            });

            // Navegar al Inicio por defecto
            NavigateTo<DashboardViewModel>("Inicio");

            // Cargar nombre del distribuidor
            _ = CargarNombreUsuarioAsync();
        }

        private async Task CargarNombreUsuarioAsync()
        {
            try
            {
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                NombreUsuario = config.Nombre;

                // Si el nombre es el default, pedir al usuario que lo configure
                if (config.Nombre == "Distribuidor" || string.IsNullOrWhiteSpace(config.Nombre))
                {
                    MostrarConfigNombre = true;
                    FormNombreUsuario = "";
                }
            }
            catch
            {
                NombreUsuario = "Usuario";
            }
        }

        private async Task GuardarNombreAsync()
        {
            if (string.IsNullOrWhiteSpace(FormNombreUsuario)) return;

            try
            {
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                config.Nombre = FormNombreUsuario.Trim();
                await _configuracionService.ActualizarConfiguracionAsync(config);
                NombreUsuario = config.Nombre;
                MostrarConfigNombre = false;
                StatusMessage = $"¡Bienvenido, {NombreUsuario}!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private static string GenerarIniciales(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "?";
            var partes = nombre.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length >= 2)
                return $"{char.ToUpper(partes[0][0])}{char.ToUpper(partes[1][0])}";
            return $"{char.ToUpper(partes[0][0])}";
        }

        private void NavigateTo<TViewModel>(string menuName) where TViewModel : BaseViewModel
        {
            MenuSeleccionado = menuName;
            _navigationService.NavigateTo<TViewModel>();
            StatusMessage = $"Sección: {menuName}";
        }

        private void OnCurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
