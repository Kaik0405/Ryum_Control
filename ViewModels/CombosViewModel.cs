using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de combos.
    /// Permite crear, editar, duplicar y eliminar combos de productos.
    /// 
    /// FLUJO:
    /// 1. Usuario ve la lista de combos (tarjetas).
    /// 2. Crea un combo → se abre panel lateral con formulario.
    /// 3. Agrega productos del inventario al combo, asignando cantidad.
    /// 4. Guarda → combo se guarda en BD con sus ComboProducto.
    /// </summary>
    public class CombosViewModel : BaseViewModel
    {
        private readonly IComboService _comboService;
        private readonly IProductoService _productoService;

        // Listas principales
        private ObservableCollection<Combo> _combos = new();
        private ObservableCollection<Combo> _combosFiltrados = new();
        private ObservableCollection<Producto> _productosDisponibles = new();
        private ObservableCollection<ComboProducto> _productosDelCombo = new();

        // Selección
        private Combo? _comboSeleccionado;
        private Producto? _productoParaAgregar;

        // Filtros
        private string _filtro = string.Empty;
        private TipoCombo? _filtroTipo;

        // Formulario
        private bool _mostrarFormulario;
        private bool _esEdicion;
        private string _formNombre = string.Empty;
        private string _formDescripcion = string.Empty;
        private TipoCombo _formTipo = TipoCombo.Combo;
        private string _formPrecioVenta = string.Empty;
        private string _formCantidadProducto = string.Empty;

        // Detalle
        private bool _mostrarDetalle;
        private Combo? _comboDetalle;

        // Estado
        private string _mensajeEstado = string.Empty;

        #region Propiedades de Datos

        public ObservableCollection<Combo> Combos
        {
            get => _combos;
            set => SetProperty(ref _combos, value);
        }

        public ObservableCollection<Combo> CombosFiltrados
        {
            get => _combosFiltrados;
            set => SetProperty(ref _combosFiltrados, value);
        }

        /// <summary>
        /// Productos del inventario disponibles para agregar al combo.
        /// </summary>
        public ObservableCollection<Producto> ProductosDisponibles
        {
            get => _productosDisponibles;
            set => SetProperty(ref _productosDisponibles, value);
        }

        /// <summary>
        /// Productos que componen el combo que se está creando/editando.
        /// </summary>
        public ObservableCollection<ComboProducto> ProductosDelCombo
        {
            get => _productosDelCombo;
            set
            {
                SetProperty(ref _productosDelCombo, value);
                OnPropertyChanged(nameof(CostoTotalCombo));
            }
        }

        /// <summary>
        /// Costo total calculado de los productos del combo en edición.
        /// </summary>
        public decimal CostoTotalCombo => ProductosDelCombo?.Sum(p => p.Total) ?? 0;

        public Combo? ComboSeleccionado
        {
            get => _comboSeleccionado;
            set => SetProperty(ref _comboSeleccionado, value);
        }

        public Producto? ProductoParaAgregar
        {
            get => _productoParaAgregar;
            set => SetProperty(ref _productoParaAgregar, value);
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

        public Array TiposCombo => Enum.GetValues(typeof(TipoCombo));

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        #endregion

        #region Propiedades del Formulario

        public bool MostrarFormulario
        {
            get => _mostrarFormulario;
            set => SetProperty(ref _mostrarFormulario, value);
        }

        public bool EsEdicion
        {
            get => _esEdicion;
            set => SetProperty(ref _esEdicion, value);
        }

        public string TituloFormulario => EsEdicion ? "✏️ Editar Combo" : "➕ Nuevo Combo";

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

        public TipoCombo FormTipo
        {
            get => _formTipo;
            set => SetProperty(ref _formTipo, value);
        }

        public string FormPrecioVenta
        {
            get => _formPrecioVenta;
            set => SetProperty(ref _formPrecioVenta, value);
        }

        public string FormCantidadProducto
        {
            get => _formCantidadProducto;
            set => SetProperty(ref _formCantidadProducto, value);
        }

        #endregion

        #region Propiedades de Detalle

        public bool MostrarDetalle
        {
            get => _mostrarDetalle;
            set => SetProperty(ref _mostrarDetalle, value);
        }

        public Combo? ComboDetalle
        {
            get => _comboDetalle;
            set => SetProperty(ref _comboDetalle, value);
        }

        #endregion

        #region Comandos

        public ICommand CrearComboCommand { get; }
        public ICommand EditarComboCommand { get; }
        public ICommand DuplicarComboCommand { get; }
        public ICommand EliminarComboCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand CerrarDetalleCommand { get; }
        public ICommand GuardarComboCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand AgregarProductoCommand { get; }
        public ICommand QuitarProductoCommand { get; }
        public ICommand LimpiarFiltroTipoCommand { get; }
        public ICommand RefrescarCommand { get; }

        #endregion

        public CombosViewModel(IComboService comboService, IProductoService productoService)
        {
            _comboService = comboService;
            _productoService = productoService;

            CrearComboCommand = new RelayCommand(_ => PrepararNuevoCombo());
            EditarComboCommand = new RelayCommand(param => PrepararEdicion(param as Combo));
            DuplicarComboCommand = new RelayCommand(async param => await DuplicarComboAsync(param as Combo));
            EliminarComboCommand = new RelayCommand(async param => await EliminarComboAsync(param as Combo));
            VerDetalleCommand = new RelayCommand(param => MostrarDetalleCombo(param as Combo));
            CerrarDetalleCommand = new RelayCommand(_ => CerrarDetalle());
            GuardarComboCommand = new RelayCommand(async _ => await GuardarComboAsync(), _ => PuedeGuardar());
            CancelarCommand = new RelayCommand(_ => CerrarFormulario());
            AgregarProductoCommand = new RelayCommand(_ => AgregarProductoAlCombo(), _ => ProductoParaAgregar != null);
            QuitarProductoCommand = new RelayCommand(param => QuitarProductoDelCombo(param as ComboProducto));
            LimpiarFiltroTipoCommand = new RelayCommand(_ => { FiltroTipo = null; });
            RefrescarCommand = new RelayCommand(async _ => await CargarDatosAsync());
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            _ = CargarDatosAsync();
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DE DATOS
        // ═══════════════════════════════════════════════════

        private async Task CargarDatosAsync()
        {
            try
            {
                var combos = await _comboService.ObtenerTodosAsync();
                Combos = new ObservableCollection<Combo>(combos);
                FiltrarCombos();

                var productos = await _productoService.ObtenerEnStockAsync();
                ProductosDisponibles = new ObservableCollection<Producto>(productos);

                MensajeEstado = $"{combos.Count} combo(s) cargados";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar: {ex.Message}";
            }
        }

        private void FiltrarCombos()
        {
            var filtrados = Combos.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                filtrados = filtrados.Where(c =>
                    c.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                    (c.Descripcion?.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (FiltroTipo.HasValue)
            {
                filtrados = filtrados.Where(c => c.Tipo == FiltroTipo.Value);
            }

            CombosFiltrados = new ObservableCollection<Combo>(filtrados);
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DEL FORMULARIO
        // ═══════════════════════════════════════════════════

        private void PrepararNuevoCombo()
        {
            MostrarDetalle = false;
            EsEdicion = false;
            LimpiarFormulario();
            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void PrepararEdicion(Combo? combo)
        {
            if (combo == null) return;
            MostrarDetalle = false;
            EsEdicion = true;
            ComboSeleccionado = combo;

            FormNombre = combo.Nombre;
            FormDescripcion = combo.Descripcion ?? string.Empty;
            FormTipo = combo.Tipo;
            FormPrecioVenta = combo.PrecioVenta.ToString();

            // Cargar productos del combo
            ProductosDelCombo = new ObservableCollection<ComboProducto>(
                combo.Productos.Select(p => new ComboProducto
                {
                    ProductoId = p.ProductoId,
                    Producto = p.Producto,
                    Cantidad = p.Cantidad,
                    Unidad = p.Unidad,
                    CostoUnitario = p.CostoUnitario
                }));

            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
            OnPropertyChanged(nameof(CostoTotalCombo));
        }

        private void AgregarProductoAlCombo()
        {
            if (ProductoParaAgregar == null) return;

            // Parsear cantidad
            if (!decimal.TryParse(FormCantidadProducto.Replace('.', ','), out var cantidad) &&
                !decimal.TryParse(FormCantidadProducto, out cantidad))
            {
                cantidad = 1;
            }
            if (cantidad <= 0) cantidad = 1;

            // Verificar si ya está en el combo
            var existente = ProductosDelCombo.FirstOrDefault(p => p.ProductoId == ProductoParaAgregar.Id);
            if (existente != null)
            {
                existente.Cantidad += cantidad;
                // Forzar refresh
                var temp = new ObservableCollection<ComboProducto>(ProductosDelCombo);
                ProductosDelCombo = temp;
            }
            else
            {
                ProductosDelCombo.Add(new ComboProducto
                {
                    ProductoId = ProductoParaAgregar.Id,
                    Producto = ProductoParaAgregar,
                    Cantidad = cantidad,
                    Unidad = ProductoParaAgregar.Unidad,
                    CostoUnitario = ProductoParaAgregar.CostoCompra
                });
            }

            FormCantidadProducto = string.Empty;
            ProductoParaAgregar = null;
            OnPropertyChanged(nameof(CostoTotalCombo));
            MensajeEstado = "Producto agregado al combo";
        }

        private void QuitarProductoDelCombo(ComboProducto? item)
        {
            if (item == null) return;
            ProductosDelCombo.Remove(item);
            OnPropertyChanged(nameof(CostoTotalCombo));
        }

        private async Task GuardarComboAsync()
        {
            try
            {
                if (!decimal.TryParse(FormPrecioVenta.Replace('.', ','), out var precioVenta) &&
                    !decimal.TryParse(FormPrecioVenta, out precioVenta))
                {
                    precioVenta = 0;
                }

                if (EsEdicion && ComboSeleccionado != null)
                {
                    ComboSeleccionado.Nombre = FormNombre.Trim();
                    ComboSeleccionado.Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim();
                    ComboSeleccionado.Tipo = FormTipo;
                    ComboSeleccionado.PrecioVenta = precioVenta;

                    // Reemplazar productos
                    ComboSeleccionado.Productos.Clear();
                    foreach (var p in ProductosDelCombo)
                    {
                        ComboSeleccionado.Productos.Add(new ComboProducto
                        {
                            ComboId = ComboSeleccionado.Id,
                            ProductoId = p.ProductoId,
                            Cantidad = p.Cantidad,
                            Unidad = p.Unidad,
                            CostoUnitario = p.CostoUnitario
                        });
                    }

                    await _comboService.ActualizarAsync(ComboSeleccionado);
                    MensajeEstado = $"✅ Combo \"{FormNombre}\" actualizado";
                }
                else
                {
                    var nuevoCombo = new Combo
                    {
                        Nombre = FormNombre.Trim(),
                        Descripcion = string.IsNullOrWhiteSpace(FormDescripcion) ? null : FormDescripcion.Trim(),
                        Tipo = FormTipo,
                        PrecioVenta = precioVenta
                    };

                    foreach (var p in ProductosDelCombo)
                    {
                        nuevoCombo.Productos.Add(new ComboProducto
                        {
                            ProductoId = p.ProductoId,
                            Cantidad = p.Cantidad,
                            Unidad = p.Unidad,
                            CostoUnitario = p.CostoUnitario
                        });
                    }

                    await _comboService.CrearAsync(nuevoCombo);
                    MensajeEstado = $"✅ Combo \"{FormNombre}\" creado con {ProductosDelCombo.Count} productos";
                }

                CerrarFormulario();
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al guardar: {ex.Message}";
            }
        }

        private async Task DuplicarComboAsync(Combo? combo)
        {
            if (combo == null) return;

            try
            {
                var nuevoNombre = $"{combo.Nombre} (copia)";
                await _comboService.DuplicarComboAsync(combo.Id, nuevoNombre);
                MensajeEstado = $"✅ Combo duplicado como \"{nuevoNombre}\"";
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al duplicar: {ex.Message}";
            }
        }

        private async Task EliminarComboAsync(Combo? combo)
        {
            if (combo == null) return;

            var resultado = MessageBox.Show(
                $"¿Eliminar el combo \"{combo.Nombre}\"?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                var nombre = combo.Nombre;
                await _comboService.EliminarAsync(combo.Id);
                MensajeEstado = $"🗑️ Combo \"{nombre}\" eliminado";
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al eliminar: {ex.Message}";
            }
        }

        private void MostrarDetalleCombo(Combo? combo)
        {
            if (combo == null) return;
            MostrarFormulario = false;
            ComboDetalle = combo;
            MostrarDetalle = true;
        }

        private void CerrarDetalle()
        {
            MostrarDetalle = false;
            ComboDetalle = null;
        }

        private bool PuedeGuardar()
        {
            return !string.IsNullOrWhiteSpace(FormNombre) && ProductosDelCombo.Count > 0;
        }

        private void CerrarFormulario()
        {
            MostrarFormulario = false;
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            FormNombre = string.Empty;
            FormDescripcion = string.Empty;
            FormTipo = TipoCombo.Combo;
            FormPrecioVenta = string.Empty;
            FormCantidadProducto = string.Empty;
            ProductoParaAgregar = null;
            ProductosDelCombo = new ObservableCollection<ComboProducto>();
            ComboSeleccionado = null;
        }
    }
}
