using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de inventario de productos.
    /// 
    /// EXPLICACIÓN DEL PATRÓN:
    /// Este ViewModel actúa como "cerebro" de la vista de inventario.
    /// - La vista (XAML) se CONECTA a las propiedades de aquí mediante Bindings.
    /// - Cuando el usuario hace clic en un botón, se ejecuta un Command de aquí.
    /// - Cuando una propiedad cambia, la UI se actualiza automáticamente (INotifyPropertyChanged).
    /// 
    /// FLUJO TÍPICO:
    /// 1. Usuario abre la vista → OnNavigatedTo → CargarDatosAsync (carga productos de la BD)
    /// 2. Usuario escribe en el filtro → Filtro cambia → FiltrarProductos (filtra la lista)
    /// 3. Usuario hace clic en "Agregar" → MostrarFormulario → llena datos → GuardarProducto
    /// </summary>
    public class InventarioViewModel : BaseViewModel
    {
        // ═══════════════════════════════════════════════════
        // SERVICIOS - Acceso a la base de datos
        // Se inyectan por el constructor (Dependency Injection)
        // ═══════════════════════════════════════════════════
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;

        // ═══════════════════════════════════════════════════
        // CAMPOS PRIVADOS - Solo accesibles dentro de esta clase
        // Cada uno tiene una propiedad pública abajo que notifica cambios a la UI
        // ═══════════════════════════════════════════════════
        private ObservableCollection<Producto> _productos;
        private ObservableCollection<Producto> _productosFiltrados;
        private ObservableCollection<Categoria> _categorias;
        private Producto? _productoSeleccionado;
        private string _filtro = string.Empty;
        private bool _mostrarSoloEnStock;
        private bool _mostrarFormulario;
        private bool _esEdicion;
        private string _mensajeEstado = string.Empty;

        // Campos del formulario (lo que el usuario escribe al crear/editar)
        private string _formNombre = string.Empty;
        private string _formDescripcion = string.Empty;
        private decimal _formPrecioVenta;
        private decimal _formCostoCompra;
        private decimal _formCantidadStock;
        private UnidadMedida _formUnidad = UnidadMedida.Unidad;
        private Categoria? _formCategoriaSeleccionada;

        #region Propiedades de Datos
        // ═══════════════════════════════════════════════════
        // PROPIEDADES - La UI se conecta aquí con {Binding NombrePropiedad}
        // SetProperty notifica a la UI cuando el valor cambia
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Lista completa de productos (sin filtrar).
        /// Se carga una vez desde la BD y se refresca cuando hay cambios.
        /// </summary>
        public ObservableCollection<Producto> Productos
        {
            get => _productos;
            set => SetProperty(ref _productos, value);
        }

        /// <summary>
        /// Lista de productos filtrada - ES LA QUE SE MUESTRA EN EL DATAGRID.
        /// Cuando el usuario escribe en el buscador, esta lista se actualiza.
        /// </summary>
        public ObservableCollection<Producto> ProductosFiltrados
        {
            get => _productosFiltrados;
            set => SetProperty(ref _productosFiltrados, value);
        }

        /// <summary>
        /// Lista de categorías para el ComboBox del formulario.
        /// </summary>
        public ObservableCollection<Categoria> Categorias
        {
            get => _categorias;
            set => SetProperty(ref _categorias, value);
        }

        /// <summary>
        /// El producto que el usuario seleccionó en el DataGrid.
        /// Se usa para saber cuál editar/eliminar.
        /// </summary>
        public Producto? ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set => SetProperty(ref _productoSeleccionado, value);
        }

        /// <summary>
        /// Texto del buscador. Cada vez que cambia, se re-filtra la lista.
        /// El tercer parámetro "FiltrarProductos" se ejecuta automáticamente al cambiar.
        /// </summary>
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

        /// <summary>
        /// Controla la visibilidad del panel de formulario.
        /// true = se muestra el formulario, false = se oculta.
        /// </summary>
        public bool MostrarFormulario
        {
            get => _mostrarFormulario;
            set => SetProperty(ref _mostrarFormulario, value);
        }

        /// <summary>
        /// true si estamos editando un producto existente,
        /// false si estamos creando uno nuevo.
        /// Cambia el título del formulario ("Nuevo Producto" vs "Editar Producto").
        /// </summary>
        public bool EsEdicion
        {
            get => _esEdicion;
            set => SetProperty(ref _esEdicion, value);
        }

        /// <summary>
        /// Mensaje que aparece abajo ("Producto guardado", "Error al guardar", etc.)
        /// </summary>
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        /// <summary>
        /// Valores posibles de UnidadMedida para el ComboBox del formulario.
        /// Enum.GetValues toma todos los valores del enum automáticamente.
        /// </summary>
        public Array UnidadesMedida => Enum.GetValues(typeof(UnidadMedida));

        /// <summary>
        /// Título dinámico del formulario.
        /// </summary>
        public string TituloFormulario => EsEdicion ? "✏️ Editar Producto" : "➕ Nuevo Producto";

        #endregion

        #region Propiedades del Formulario
        // ═══════════════════════════════════════════════════
        // FORMULARIO - Cada campo del formulario es una propiedad
        // La UI hace Binding a estas propiedades con TextBox, ComboBox, etc.
        // ═══════════════════════════════════════════════════

        public string FormNombre
        {
            get => _formNombre;
            set => SetProperty(ref _formNombre, value);
        }

        public string FormDescripcion
        {
            get => _formDescripcion;
            set => SetProperty(ref _formDescripcion, value);
        }

        public decimal FormPrecioVenta
        {
            get => _formPrecioVenta;
            set => SetProperty(ref _formPrecioVenta, value);
        }

        public decimal FormCostoCompra
        {
            get => _formCostoCompra;
            set => SetProperty(ref _formCostoCompra, value);
        }

        public decimal FormCantidadStock
        {
            get => _formCantidadStock;
            set => SetProperty(ref _formCantidadStock, value);
        }

        public UnidadMedida FormUnidad
        {
            get => _formUnidad;
            set => SetProperty(ref _formUnidad, value);
        }

        public Categoria? FormCategoriaSeleccionada
        {
            get => _formCategoriaSeleccionada;
            set => SetProperty(ref _formCategoriaSeleccionada, value);
        }

        #endregion

        #region Comandos
        // ═══════════════════════════════════════════════════
        // COMANDOS - Acciones que se disparan desde botones en la UI.
        // Cada botón en XAML tiene: Command="{Binding NombreCommand}"
        // RelayCommand recibe: (acción, condición para estar habilitado)
        // ═══════════════════════════════════════════════════

        /// <summary>Abre el formulario en modo "nuevo producto".</summary>
        public ICommand NuevoProductoCommand { get; }

        /// <summary>Abre el formulario en modo "editar" con los datos del producto seleccionado.</summary>
        public ICommand EditarProductoCommand { get; }

        /// <summary>Elimina el producto seleccionado (previa confirmación).</summary>
        public ICommand EliminarProductoCommand { get; }

        /// <summary>Guarda el producto (crea nuevo o actualiza existente).</summary>
        public ICommand GuardarProductoCommand { get; }

        /// <summary>Cierra el formulario sin guardar.</summary>
        public ICommand CancelarCommand { get; }

        /// <summary>Recarga la lista de productos desde la BD.</summary>
        public ICommand RefrescarCommand { get; }

        #endregion

        // ═══════════════════════════════════════════════════
        // CONSTRUCTOR
        // Aquí se reciben los servicios por inyección de dependencias.
        // También se crean los comandos y se conectan a sus métodos.
        // ═══════════════════════════════════════════════════
        public InventarioViewModel(IProductoService productoService, ICategoriaService categoriaService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;

            _productos = new ObservableCollection<Producto>();
            _productosFiltrados = new ObservableCollection<Producto>();
            _categorias = new ObservableCollection<Categoria>();

            // Crear comandos y conectarlos a los métodos de abajo
            NuevoProductoCommand = new RelayCommand(_ => PrepararNuevoProducto());
            EditarProductoCommand = new RelayCommand(_ => PrepararEdicion(), _ => ProductoSeleccionado != null);
            EliminarProductoCommand = new RelayCommand(async _ => await EliminarProductoAsync(), _ => ProductoSeleccionado != null);
            GuardarProductoCommand = new RelayCommand(async _ => await GuardarProductoAsync(), _ => PuedeGuardar());
            CancelarCommand = new RelayCommand(_ => CerrarFormulario());
            RefrescarCommand = new RelayCommand(async _ => await CargarDatosAsync());
        }

        // ═══════════════════════════════════════════════════
        // CICLO DE VIDA
        // OnNavigatedTo se llama automáticamente cuando el usuario
        // navega a esta vista (hace clic en "Inventario" en el menú).
        // ═══════════════════════════════════════════════════
        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            // Disparar la carga de datos (async void porque es un evento)
            _ = CargarDatosAsync();
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DE DATOS
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Carga productos y categorías desde la base de datos.
        /// Se llama al entrar a la vista y al refrescar.
        /// </summary>
        private async Task CargarDatosAsync()
        {
            try
            {
                // 1. Pedir los datos al servicio (que consulta la BD)
                var productos = await _productoService.ObtenerTodosAsync();
                var categorias = await _categoriaService.ObtenerActivasAsync();

                // 2. Actualizar las colecciones observables
                //    ObservableCollection notifica a la UI automáticamente
                Productos = new ObservableCollection<Producto>(productos);
                Categorias = new ObservableCollection<Categoria>(categorias);

                // 3. Aplicar el filtro actual
                FiltrarProductos();

                MensajeEstado = $"{productos.Count} producto(s) cargados";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar: {ex.Message}";
            }
        }

        /// <summary>
        /// Filtra la lista de productos según el texto de búsqueda y el checkbox "solo en stock".
        /// Se ejecuta automáticamente cada vez que cambia Filtro o MostrarSoloEnStock.
        /// 
        /// NO consulta la BD — filtra en memoria la lista que ya se cargó.
        /// </summary>
        private void FiltrarProductos()
        {
            // Empezar con todos los productos
            var filtrados = Productos.AsEnumerable();

            // Si hay texto en el buscador, filtrar por nombre
            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                filtrados = filtrados.Where(p =>
                    p.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                    (p.Descripcion?.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Categoria?.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Si el checkbox está marcado, solo mostrar productos con stock
            if (MostrarSoloEnStock)
            {
                filtrados = filtrados.Where(p => p.EnStock);
            }

            // Actualizar la lista filtrada (la UI se actualiza sola)
            ProductosFiltrados = new ObservableCollection<Producto>(filtrados);
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DEL FORMULARIO
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Prepara el formulario para CREAR un producto nuevo.
        /// Limpia todos los campos y muestra el panel.
        /// </summary>
        private void PrepararNuevoProducto()
        {
            EsEdicion = false;
            LimpiarFormulario();
            MostrarFormulario = true;
            // Forzar actualización del título
            OnPropertyChanged(nameof(TituloFormulario));
        }

        /// <summary>
        /// Prepara el formulario para EDITAR el producto seleccionado.
        /// Copia los datos del producto a los campos del formulario.
        /// </summary>
        private void PrepararEdicion()
        {
            if (ProductoSeleccionado == null) return;

            EsEdicion = true;

            // Copiar datos del producto seleccionado al formulario
            FormNombre = ProductoSeleccionado.Nombre;
            FormDescripcion = ProductoSeleccionado.Descripcion ?? string.Empty;
            FormPrecioVenta = ProductoSeleccionado.PrecioVenta;
            FormCostoCompra = ProductoSeleccionado.CostoCompra;
            FormCantidadStock = ProductoSeleccionado.CantidadStock;
            FormUnidad = ProductoSeleccionado.Unidad;
            FormCategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == ProductoSeleccionado.CategoriaId);

            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        /// <summary>
        /// Guarda el producto en la base de datos.
        /// Si EsEdicion=true, actualiza el existente. Si no, crea uno nuevo.
        /// </summary>
        private async Task GuardarProductoAsync()
        {
            try
            {
                if (EsEdicion && ProductoSeleccionado != null)
                {
                    // EDITAR: actualizar las propiedades del producto existente
                    ProductoSeleccionado.Nombre = FormNombre.Trim();
                    ProductoSeleccionado.Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim();
                    ProductoSeleccionado.PrecioVenta = FormPrecioVenta;
                    ProductoSeleccionado.CostoCompra = FormCostoCompra;
                    ProductoSeleccionado.CantidadStock = FormCantidadStock;
                    ProductoSeleccionado.EnStock = FormCantidadStock > 0;
                    ProductoSeleccionado.Unidad = FormUnidad;
                    ProductoSeleccionado.CategoriaId = FormCategoriaSeleccionada?.Id;

                    await _productoService.ActualizarAsync(ProductoSeleccionado);
                    MensajeEstado = $"✅ Producto \"{FormNombre}\" actualizado";
                }
                else
                {
                    // CREAR: construir un nuevo objeto Producto con los datos del formulario
                    var nuevoProducto = new Producto
                    {
                        Nombre = FormNombre.Trim(),
                        Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim(),
                        PrecioVenta = FormPrecioVenta,
                        CostoCompra = FormCostoCompra,
                        CantidadStock = FormCantidadStock,
                        EnStock = FormCantidadStock > 0,
                        Unidad = FormUnidad,
                        CategoriaId = FormCategoriaSeleccionada?.Id
                    };

                    await _productoService.CrearAsync(nuevoProducto);
                    MensajeEstado = $"✅ Producto \"{FormNombre}\" creado";
                }

                // Cerrar formulario y recargar la lista
                CerrarFormulario();
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al guardar: {ex.Message}";
            }
        }

        /// <summary>
        /// Elimina el producto seleccionado (soft delete: lo marca como inactivo).
        /// Muestra confirmación antes de eliminar.
        /// </summary>
        private async Task EliminarProductoAsync()
        {
            if (ProductoSeleccionado == null) return;

            // Confirmar con el usuario
            var resultado = MessageBox.Show(
                $"¿Eliminar el producto \"{ProductoSeleccionado.Nombre}\"?\n\nEsta acción lo desactivará del inventario.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                var nombre = ProductoSeleccionado.Nombre;
                await _productoService.EliminarAsync(ProductoSeleccionado.Id);
                MensajeEstado = $"🗑️ Producto \"{nombre}\" eliminado";
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al eliminar: {ex.Message}";
            }
        }

        /// <summary>
        /// Valida que el formulario tenga datos suficientes para guardar.
        /// Si retorna false, el botón "Guardar" se deshabilita.
        /// </summary>
        private bool PuedeGuardar()
        {
            return !string.IsNullOrWhiteSpace(FormNombre) && FormPrecioVenta >= 0;
        }

        /// <summary>
        /// Cierra el formulario y limpia los campos.
        /// </summary>
        private void CerrarFormulario()
        {
            MostrarFormulario = false;
            LimpiarFormulario();
        }

        /// <summary>
        /// Resetea todos los campos del formulario a sus valores por defecto.
        /// </summary>
        private void LimpiarFormulario()
        {
            FormNombre = string.Empty;
            FormDescripcion = string.Empty;
            FormPrecioVenta = 0;
            FormCostoCompra = 0;
            FormCantidadStock = 0;
            FormUnidad = UnidadMedida.Unidad;
            FormCategoriaSeleccionada = null;
            ProductoSeleccionado = null;
        }
    }
}
