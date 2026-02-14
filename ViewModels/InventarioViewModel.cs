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
        private readonly IMovimientoService _movimientoService;
        private readonly IPeriodoInventarioService _periodoService;

        // ═══════════════════════════════════════════════════
        // CAMPOS PRIVADOS - Solo accesibles dentro de esta clase
        // Cada uno tiene una propiedad pública abajo que notifica cambios a la UI
        // ═══════════════════════════════════════════════════
        private ObservableCollection<Producto> _productos;
        private ObservableCollection<Producto> _productosFiltrados;
        private Producto? _productoSeleccionado;
        private string _filtro = string.Empty;
        private bool _mostrarSoloEnStock;
        private bool _mostrarFormulario;
        private bool _esEdicion;
        private string _mensajeEstado = string.Empty;

        // Campos del formulario — usamos string para los números
        // para evitar problemas de formato con el punto decimal
        private string _formNombre = string.Empty;
        private string _formDescripcion = string.Empty;
        private string _formCostoCompra = string.Empty;
        private string _formCantidadStock = string.Empty;
        private UnidadMedida _formUnidad = UnidadMedida.Unidad;

        // Campos para ajuste de stock
        private bool _modoAjusteStock;
        private string _ajusteCantidad = string.Empty;
        private string _ajusteMotivo = string.Empty;
        private bool _ajusteEsIngreso;

        // Campos para historial de producto
        private bool _modoHistorial;
        private ObservableCollection<Movimiento> _historialProducto = new();
        private Producto? _productoHistorial;

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
            set
            {
                SetProperty(ref _productosFiltrados, value);
                // Recalcular el total cada vez que cambia la lista filtrada
                OnPropertyChanged(nameof(CostoTotalInventario));
            }
        }

        /// <summary>
        /// Suma del CostoTotal (Costo × Stock) de todos los productos filtrados.
        /// Se muestra en el pie de página.
        /// </summary>
        public decimal CostoTotalInventario => ProductosFiltrados?.Sum(p => p.CostoTotal) ?? 0;

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

        public string FormCostoCompra
        {
            get => _formCostoCompra;
            set => SetProperty(ref _formCostoCompra, value);
        }

        public string FormCantidadStock
        {
            get => _formCantidadStock;
            set => SetProperty(ref _formCantidadStock, value);
        }

        public UnidadMedida FormUnidad
        {
            get => _formUnidad;
            set => SetProperty(ref _formUnidad, value);
        }

        #endregion

        #region Propiedades de Ajuste de Stock

        public bool ModoAjusteStock
        {
            get => _modoAjusteStock;
            set
            {
                SetProperty(ref _modoAjusteStock, value);
                OnPropertyChanged(nameof(TituloAjuste));
            }
        }

        public string AjusteCantidad
        {
            get => _ajusteCantidad;
            set => SetProperty(ref _ajusteCantidad, value);
        }

        public string AjusteMotivo
        {
            get => _ajusteMotivo;
            set => SetProperty(ref _ajusteMotivo, value);
        }

        public bool AjusteEsIngreso
        {
            get => _ajusteEsIngreso;
            set
            {
                SetProperty(ref _ajusteEsIngreso, value);
                OnPropertyChanged(nameof(TituloAjuste));
            }
        }

        public string TituloAjuste => AjusteEsIngreso ? "📥 Ingresar Stock" : "📤 Dar Salida";

        #endregion

        #region Propiedades de Historial

        public bool ModoHistorial
        {
            get => _modoHistorial;
            set => SetProperty(ref _modoHistorial, value);
        }

        public ObservableCollection<Movimiento> HistorialProducto
        {
            get => _historialProducto;
            set => SetProperty(ref _historialProducto, value);
        }

        public Producto? ProductoHistorial
        {
            get => _productoHistorial;
            set => SetProperty(ref _productoHistorial, value);
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

        /// <summary>Abre panel de ingreso de stock para un producto.</summary>
        public ICommand IngresarStockCommand { get; }

        /// <summary>Abre panel de salida de stock para un producto.</summary>
        public ICommand SacarStockCommand { get; }

        /// <summary>Confirma el ajuste de stock.</summary>
        public ICommand ConfirmarAjusteCommand { get; }

        /// <summary>Cancela el ajuste de stock.</summary>
        public ICommand CancelarAjusteCommand { get; }

        /// <summary>Abre el historial de movimientos de un producto.</summary>
        public ICommand VerHistorialCommand { get; }

        /// <summary>Cierra el panel de historial.</summary>
        public ICommand CerrarHistorialCommand { get; }

        #endregion

        // ═══════════════════════════════════════════════════
        // CONSTRUCTOR
        // Aquí se reciben los servicios por inyección de dependencias.
        // También se crean los comandos y se conectan a sus métodos.
        // ═══════════════════════════════════════════════════
        public InventarioViewModel(IProductoService productoService, IMovimientoService movimientoService, IPeriodoInventarioService periodoService)
        {
            _productoService = productoService;
            _movimientoService = movimientoService;
            _periodoService = periodoService;

            _productos = new ObservableCollection<Producto>();
            _productosFiltrados = new ObservableCollection<Producto>();

            // Crear comandos y conectarlos a los métodos de abajo
            NuevoProductoCommand = new RelayCommand(_ => PrepararNuevoProducto());
            EditarProductoCommand = new RelayCommand(_ => PrepararEdicion(), _ => ProductoSeleccionado != null);
            EliminarProductoCommand = new RelayCommand(async _ => await EliminarProductoAsync(), _ => ProductoSeleccionado != null);
            GuardarProductoCommand = new RelayCommand(async _ => await GuardarProductoAsync(), _ => PuedeGuardar());
            CancelarCommand = new RelayCommand(_ => CerrarFormulario());
            RefrescarCommand = new RelayCommand(async _ => await CargarDatosAsync());

            // Comandos de ajuste de stock
            IngresarStockCommand = new RelayCommand(param => PrepararAjusteStock(param as Producto, true));
            SacarStockCommand = new RelayCommand(param => PrepararAjusteStock(param as Producto, false));
            ConfirmarAjusteCommand = new RelayCommand(async _ => await ConfirmarAjusteStockAsync(), _ => !string.IsNullOrWhiteSpace(AjusteCantidad));
            CancelarAjusteCommand = new RelayCommand(_ => CerrarAjusteStock());

            // Comandos de historial
            VerHistorialCommand = new RelayCommand(async param => await MostrarHistorialAsync(param as Producto));
            CerrarHistorialCommand = new RelayCommand(_ => CerrarHistorial());
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

                // 2. Actualizar las colecciones observables
                //    ObservableCollection notifica a la UI automáticamente
                Productos = new ObservableCollection<Producto>(productos);

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
                    (p.Descripcion?.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ?? false));
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
            ModoAjusteStock = false;
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

            ModoAjusteStock = false;
            EsEdicion = true;

            // Copiar datos del producto seleccionado al formulario
            FormNombre = ProductoSeleccionado.Nombre;
            FormDescripcion = ProductoSeleccionado.Descripcion ?? string.Empty;
            FormCostoCompra = ProductoSeleccionado.CostoCompra.ToString();
            FormCantidadStock = ProductoSeleccionado.CantidadStock.ToString();
            FormUnidad = ProductoSeleccionado.Unidad;

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
                // Convertir los textos a números
                if (!decimal.TryParse(FormCostoCompra.Replace('.', ','), out var costoCompra) &&
                    !decimal.TryParse(FormCostoCompra, out costoCompra))
                {
                    MensajeEstado = "❌ El costo de compra no es un número válido";
                    return;
                }

                if (!decimal.TryParse(FormCantidadStock.Replace('.', ','), out var cantidadStock) &&
                    !decimal.TryParse(FormCantidadStock, out cantidadStock))
                {
                    MensajeEstado = "❌ La cantidad no es un número válido";
                    return;
                }

                if (EsEdicion && ProductoSeleccionado != null)
                {
                    // EDITAR: actualizar las propiedades del producto existente
                    ProductoSeleccionado.Nombre = FormNombre.Trim();
                    ProductoSeleccionado.Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim();
                    ProductoSeleccionado.CostoCompra = costoCompra;
                    ProductoSeleccionado.CantidadStock = cantidadStock;
                    ProductoSeleccionado.EnStock = cantidadStock > 0;
                    ProductoSeleccionado.Unidad = FormUnidad;

                    await _productoService.ActualizarAsync(ProductoSeleccionado);
                    MensajeEstado = $"✅ Producto \"{FormNombre}\" actualizado";
                }
                else
                {
                    // CREAR: construir un nuevo objeto Producto
                    var nuevoProducto = new Producto
                    {
                        Nombre = FormNombre.Trim(),
                        Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim(),
                        CostoCompra = costoCompra,
                        CantidadStock = cantidadStock,
                        EnStock = cantidadStock > 0,
                        Unidad = FormUnidad,
                        FechaIngreso = DateTime.Now
                    };

                    await _productoService.CrearAsync(nuevoProducto);

                    // Registrar como egreso (compra de producto) en los movimientos
                    // Solo si tiene costo > 0
                    if (costoCompra > 0 && cantidadStock > 0)
                    {
                        var periodo = await _periodoService.ObtenerPeriodoActualAsync();
                        var movimiento = new Movimiento
                        {
                            Concepto = $"Compra - {nuevoProducto.Nombre}",
                            Descripcion = $"Ingreso de {cantidadStock} {FormUnidad} al inventario",
                            Monto = costoCompra * cantidadStock,
                            Tipo = TipoMovimiento.Egreso,
                            Categoria = CategoriaMovimiento.CompraProducto,
                            ProductoId = nuevoProducto.Id,
                            PeriodoInventarioId = periodo?.Id,
                            Fecha = DateTime.Now
                        };
                        await _movimientoService.CrearAsync(movimiento);
                    }

                    MensajeEstado = $"✅ Producto \"{FormNombre}\" creado (egreso registrado: ${costoCompra * cantidadStock:N2})";
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
            return !string.IsNullOrWhiteSpace(FormNombre);
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
            FormCostoCompra = string.Empty;
            FormCantidadStock = string.Empty;
            FormUnidad = UnidadMedida.Unidad;
            ProductoSeleccionado = null;
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DE AJUSTE DE STOCK
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Prepara el panel lateral para ajustar stock de un producto.
        /// </summary>
        private void PrepararAjusteStock(Producto? producto, bool esIngreso)
        {
            if (producto == null) return;
            ProductoSeleccionado = producto;
            AjusteEsIngreso = esIngreso;
            AjusteCantidad = string.Empty;
            AjusteMotivo = string.Empty;
            MostrarFormulario = false;
            ModoAjusteStock = true;
        }

        /// <summary>
        /// Cierra el panel de ajuste de stock.
        /// </summary>
        private void CerrarAjusteStock()
        {
            ModoAjusteStock = false;
            AjusteCantidad = string.Empty;
            AjusteMotivo = string.Empty;
        }

        /// <summary>
        /// Confirma el ajuste: suma o resta stock, actualiza BD y registra movimiento.
        /// </summary>
        private async Task ConfirmarAjusteStockAsync()
        {
            if (ProductoSeleccionado == null) return;

            if (!decimal.TryParse(AjusteCantidad.Replace('.', ','), out var cantidad) &&
                !decimal.TryParse(AjusteCantidad, out cantidad))
            {
                MensajeEstado = "❌ La cantidad no es un número válido";
                return;
            }

            if (cantidad <= 0)
            {
                MensajeEstado = "❌ La cantidad debe ser mayor a 0";
                return;
            }

            try
            {
                var nombreProducto = ProductoSeleccionado.Nombre;

                if (AjusteEsIngreso)
                {
                    ProductoSeleccionado.CantidadStock += cantidad;
                }
                else
                {
                    if (cantidad > ProductoSeleccionado.CantidadStock)
                    {
                        MensajeEstado = $"❌ Stock insuficiente (disponible: {ProductoSeleccionado.CantidadStock} {ProductoSeleccionado.Unidad})";
                        return;
                    }
                    ProductoSeleccionado.CantidadStock -= cantidad;
                }

                ProductoSeleccionado.EnStock = ProductoSeleccionado.CantidadStock > 0;
                await _productoService.ActualizarAsync(ProductoSeleccionado);

                // Registrar movimiento financiero
                var periodo = await _periodoService.ObtenerPeriodoActualAsync();
                var motivo = string.IsNullOrWhiteSpace(AjusteMotivo) ? "" : $" — {AjusteMotivo.Trim()}";

                var movimiento = new Movimiento
                {
                    Concepto = AjusteEsIngreso
                        ? $"Ingreso inventario — {nombreProducto}"
                        : $"Salida inventario — {nombreProducto}",
                    Descripcion = $"{(AjusteEsIngreso ? "Ingreso" : "Salida")} de {cantidad} {ProductoSeleccionado.Unidad}{motivo}",
                    Monto = ProductoSeleccionado.CostoCompra * cantidad,
                    Tipo = TipoMovimiento.Egreso,
                    Categoria = AjusteEsIngreso ? CategoriaMovimiento.CompraProducto : CategoriaMovimiento.Otro,
                    ProductoId = ProductoSeleccionado.Id,
                    PeriodoInventarioId = periodo?.Id,
                    Fecha = DateTime.Now
                };
                await _movimientoService.CrearAsync(movimiento);

                var accion = AjusteEsIngreso ? "ingresadas" : "retiradas";
                MensajeEstado = $"✅ {cantidad} {ProductoSeleccionado.Unidad} {accion} — \"{nombreProducto}\"";

                CerrarAjusteStock();
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DE HISTORIAL
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Carga y muestra el historial de movimientos de un producto.
        /// </summary>
        private async Task MostrarHistorialAsync(Producto? producto)
        {
            if (producto == null) return;

            try
            {
                MostrarFormulario = false;
                ModoAjusteStock = false;
                ProductoHistorial = producto;

                var movimientos = await _movimientoService.ObtenerPorProductoAsync(producto.Id);
                HistorialProducto = new ObservableCollection<Movimiento>(movimientos);
                ModoHistorial = true;

                MensajeEstado = $"📋 Historial de \"{producto.Nombre}\" — {movimientos.Count} registro(s)";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al cargar historial: {ex.Message}";
            }
        }

        /// <summary>
        /// Cierra el panel de historial.
        /// </summary>
        private void CerrarHistorial()
        {
            ModoHistorial = false;
            HistorialProducto.Clear();
            ProductoHistorial = null;
        }
    }
}
