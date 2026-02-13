using GestionApp.ViewModels;

namespace GestionApp.Services
{
    /// <summary>
    /// Interfaz para el servicio de navegación entre ViewModels.
    /// Permite desacoplar la lógica de navegación del código de los ViewModels.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// ViewModel actualmente activo.
        /// </summary>
        BaseViewModel? CurrentViewModel { get; }

        /// <summary>
        /// Evento que se dispara cuando cambia el ViewModel actual.
        /// </summary>
        event Action? CurrentViewModelChanged;

        /// <summary>
        /// Navega a un ViewModel específico.
        /// </summary>
        /// <typeparam name="TViewModel">Tipo del ViewModel destino</typeparam>
        /// <param name="parameter">Parámetro opcional para pasar al ViewModel</param>
        void NavigateTo<TViewModel>(object? parameter = null) where TViewModel : BaseViewModel;

        /// <summary>
        /// Navega al ViewModel anterior si existe historial.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Indica si se puede navegar hacia atrás.
        /// </summary>
        bool CanGoBack { get; }
    }
}
