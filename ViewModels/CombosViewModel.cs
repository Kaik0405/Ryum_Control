using System.Collections.ObjectModel;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de combos.
    /// Permite crear, editar y eliminar combos de productos.
    /// </summary>
    public class CombosViewModel : BaseViewModel
    {
        private ObservableCollection<Combo> _combos;
        private Combo? _comboSeleccionado;
        private string _filtro = string.Empty;
        private TipoCombo? _filtroTipo;

        #region Properties

        public ObservableCollection<Combo> Combos
        {
            get => _combos;
            set => SetProperty(ref _combos, value);
        }

        public Combo? ComboSeleccionado
        {
            get => _comboSeleccionado;
            set => SetProperty(ref _comboSeleccionado, value);
        }

        public string Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value, FiltrarCombos);
        }

        public TipoCombo? FiltroTipo
        {
            get => _filtroTipo;
            set => SetProperty(ref _filtroTipo, value, FiltrarCombos);
        }

        /// <summary>
        /// Tipos de combo disponibles para filtrar.
        /// </summary>
        public Array TiposCombo => Enum.GetValues(typeof(TipoCombo));

        #endregion

        #region Commands

        public ICommand CrearComboCommand { get; }
        public ICommand EditarComboCommand { get; }
        public ICommand DuplicarComboCommand { get; }
        public ICommand EliminarComboCommand { get; }
        public ICommand VerDetalleCommand { get; }

        #endregion

        public CombosViewModel()
        {
            _combos = new ObservableCollection<Combo>();

            CrearComboCommand = new RelayCommand(_ => CrearCombo());
            EditarComboCommand = new RelayCommand(_ => EditarCombo(), _ => ComboSeleccionado != null);
            DuplicarComboCommand = new RelayCommand(_ => DuplicarCombo(), _ => ComboSeleccionado != null);
            EliminarComboCommand = new RelayCommand(_ => EliminarCombo(), _ => ComboSeleccionado != null);
            VerDetalleCommand = new RelayCommand(_ => VerDetalle(), _ => ComboSeleccionado != null);
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            CargarCombos();
        }

        private void CargarCombos()
        {
            // TODO: Cargar desde base de datos
            Combos.Clear();
        }

        private void FiltrarCombos()
        {
            // TODO: Implementar filtrado por nombre y tipo
        }

        private void CrearCombo()
        {
            // TODO: Abrir diálogo para crear combo
        }

        private void EditarCombo()
        {
            // TODO: Abrir diálogo para editar combo
        }

        private void DuplicarCombo()
        {
            // TODO: Crear copia del combo seleccionado
        }

        private void EliminarCombo()
        {
            // TODO: Confirmar y eliminar combo
        }

        private void VerDetalle()
        {
            // TODO: Mostrar detalle del combo
        }
    }
}
