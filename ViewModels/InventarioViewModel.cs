using System.Collections.ObjectModel;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de inventario de productos.
    /// </summary>
    public class InventarioViewModel : BaseViewModel
    {
        private ObservableCollection<Producto> _productos;
        private Producto? _productoSeleccionado;
        private string _filtro = string.Empty;
        private bool _mostrarSoloEnStock = false;

        #region Properties

        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            set => SetProperty(ref _productos, value);
        }

        public Producto? ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set => SetProperty(ref _productoSeleccionado, value);
        }

        public string Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value, FiltrarProductos);
        }

        public bool MostrarSoloEnStock
        {
            get => _mostrarSoloEnStock;
            set => SetProperty(ref _mostrarSoloEnStock, value, FiltrarProductos);
        }

        #endregion

        #region Commands

        public ICommand AgregarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand ActualizarStockCommand { get; }

        #endregion

        public InventarioViewModel()
        {
            _productos = new ObservableCollection<Producto>();

            // Inicializar comandos
            AgregarProductoCommand = new RelayCommand(_ => AgregarProducto());
            EditarProductoCommand = new RelayCommand(_ => EditarProducto(), _ => ProductoSeleccionado != null);
            EliminarProductoCommand = new RelayCommand(_ => EliminarProducto(), _ => ProductoSeleccionado != null);
            ActualizarStockCommand = new RelayCommand(_ => ActualizarStock());
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            CargarProductos();
        }

        private void CargarProductos()
        {
            // TODO: Cargar desde base de datos
            Productos.Clear();
        }

        private void FiltrarProductos()
        {
            // TODO: Implementar filtrado
        }

        private void AgregarProducto()
        {
            // TODO: Abrir diálogo para agregar producto
        }

        private void EditarProducto()
        {
            // TODO: Abrir diálogo para editar producto
        }

        private void EliminarProducto()
        {
            // TODO: Confirmar y eliminar producto
        }

        private void ActualizarStock()
        {
            // TODO: Actualizar stock de productos
        }
    }
}
