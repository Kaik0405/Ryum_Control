using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// Clase base para todos los ViewModels.
    /// Implementa INotifyPropertyChanged para notificación de cambios en propiedades.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Notifica a la UI que una propiedad ha cambiado.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Establece el valor de una propiedad y notifica el cambio si es diferente.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Establece el valor y ejecuta una acción adicional si el valor cambió.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string? propertyName = null)
        {
            if (!SetProperty(ref field, value, propertyName))
                return false;

            onChanged?.Invoke();
            return true;
        }

        #endregion

        #region Lifecycle

        /// <summary>
        /// Método virtual para inicialización del ViewModel al navegar hacia él.
        /// </summary>
        public virtual void OnNavigatedTo(object? parameter = null) { }

        /// <summary>
        /// Método virtual para limpieza al salir del ViewModel.
        /// </summary>
        public virtual void OnNavigatedFrom() { }

        #endregion
    }
}
