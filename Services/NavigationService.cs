using GestionApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GestionApp.Services
{
    /// <summary>
    /// Implementación del servicio de navegación.
    /// Gestiona la navegación entre ViewModels usando DI.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Stack<BaseViewModel> _navigationHistory;
        private BaseViewModel? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _navigationHistory = new Stack<BaseViewModel>();
        }

        /// <summary>
        /// ViewModel actualmente activo en la aplicación.
        /// </summary>
        public BaseViewModel? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    CurrentViewModelChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Evento para notificar cambios en el ViewModel actual.
        /// </summary>
        public event Action? CurrentViewModelChanged;

        /// <summary>
        /// Indica si hay historial de navegación para volver atrás.
        /// </summary>
        public bool CanGoBack => _navigationHistory.Count > 0;

        /// <summary>
        /// Navega a un nuevo ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel">Tipo del ViewModel destino</typeparam>
        /// <param name="parameter">Parámetro opcional</param>
        public void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : BaseViewModel
        {
            // Notificar al ViewModel actual que se está saliendo
            if (_currentViewModel != null)
            {
                _currentViewModel.OnNavigatedFrom();
                _navigationHistory.Push(_currentViewModel);
            }

            // Obtener el nuevo ViewModel desde DI
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            
            // Notificar al nuevo ViewModel que se está entrando
            viewModel.OnNavigatedTo(parameter);
            
            CurrentViewModel = viewModel;
        }

        /// <summary>
        /// Regresa al ViewModel anterior en el historial.
        /// </summary>
        public void GoBack()
        {
            if (!CanGoBack) return;

            _currentViewModel?.OnNavigatedFrom();
            
            var previousViewModel = _navigationHistory.Pop();
            previousViewModel.OnNavigatedTo();
            
            CurrentViewModel = previousViewModel;
        }
    }
}
