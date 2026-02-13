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
        private string _titulo = "Sistema de Gestión";
        private string _statusMessage = "Listo";
        private string _menuSeleccionado = "Dashboard";

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

        /// <summary>
        /// ViewModel actual mostrado en el área de contenido.
        /// </summary>
        public BaseViewModel? CurrentViewModel => _navigationService.CurrentViewModel;

        #endregion

        #region Commands

        public ICommand NavigateToDashboardCommand { get; }
        public ICommand NavigateToInventarioCommand { get; }
        public ICommand NavigateToCombosCommand { get; }
        public ICommand NavigateToFichasCostoCommand { get; }
        public ICommand NavigateToMovimientosCommand { get; }
        public ICommand NavigateToReportesCommand { get; }

        #endregion

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            // Suscribirse a cambios de navegación
            _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

            // Inicializar comandos de navegación
            NavigateToDashboardCommand = new RelayCommand(_ => NavigateTo<DashboardViewModel>("Dashboard"));
            NavigateToInventarioCommand = new RelayCommand(_ => NavigateTo<InventarioViewModel>("Inventario"));
            NavigateToCombosCommand = new RelayCommand(_ => NavigateTo<CombosViewModel>("Combos"));
            NavigateToFichasCostoCommand = new RelayCommand(_ => NavigateTo<FichasCostoViewModel>("Fichas de Costo"));
            NavigateToMovimientosCommand = new RelayCommand(_ => NavigateTo<MovimientosViewModel>("Movimientos"));
            NavigateToReportesCommand = new RelayCommand(_ => NavigateTo<ReportesViewModel>("Reportes"));

            // Navegar al Dashboard por defecto
            NavigateTo<DashboardViewModel>("Dashboard");
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
