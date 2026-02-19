using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de fichas de costo.
    /// Catálogo horizontal de tarjetas con datos de envíos y costos.
    /// </summary>
    public class FichasCostoViewModel : BaseViewModel
    {
        private readonly IFichaCostoService _fichaCostoService;
        private readonly IConfiguracionService _configuracionService;
        private readonly IEntregaService _entregaService;
        private readonly IProductoService _productoService;
        private readonly IComboService _comboService;

        private ObservableCollection<FichaCosto> _fichas = new();
        private ObservableCollection<FichaCosto> _fichasFiltradas = new();
        private FichaCosto? _fichaSeleccionada;
        private DateTime _fechaDesde;
        private DateTime _fechaHasta;
        private string _filtro = string.Empty;
        private bool _mostrarDetalle;

        // Productos del inventario disponibles para asignar
        private ObservableCollection<Producto> _productosInventario = new();

        /// <summary>
        /// Wrappers para los productos de la ficha que permiten seleccionar
        /// el producto del inventario desde un ComboBox en la UI.
        /// </summary>
        private ObservableCollection<FichaProductoWrapper> _productosDetalle = new();

        #region Properties

        public ObservableCollection<FichaCosto> Fichas
        {
            get => _fichas;
            set => SetProperty(ref _fichas, value);
        }

        public ObservableCollection<FichaCosto> FichasFiltradas
        {
            get => _fichasFiltradas;
            set => SetProperty(ref _fichasFiltradas, value);
        }

        public FichaCosto? FichaSeleccionada
        {
            get => _fichaSeleccionada;
            set
            {
                SetProperty(ref _fichaSeleccionada, value);
                OnPropertyChanged(nameof(TieneSeleccion));
            }
        }

        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set => SetProperty(ref _fechaDesde, value, () => _ = CargarFichasAsync());
        }

        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set => SetProperty(ref _fechaHasta, value, () => _ = CargarFichasAsync());
        }

        public string Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value, FiltrarFichas);
        }

        public bool MostrarDetalle
        {
            get => _mostrarDetalle;
            set => SetProperty(ref _mostrarDetalle, value);
        }

        public bool TieneSeleccion => FichaSeleccionada != null;

        /// <summary>
        /// Total de fichas en el período filtrado.
        /// </summary>
        public int TotalFichas => FichasFiltradas?.Count ?? 0;

        /// <summary>
        /// Suma de ventas (precio USD) en el período.
        /// </summary>
        public decimal TotalVentas => FichasFiltradas?.Sum(f => f.PrecioVentaUSD) ?? 0;

        /// <summary>
        /// Suma de costos totales en el período.
        /// </summary>
        public decimal TotalCostos => FichasFiltradas?.Sum(f => f.CostoTotal) ?? 0;

        /// <summary>
        /// Ganancia total en el período.
        /// </summary>
        public decimal TotalGanancia => TotalVentas - TotalCostos;

        private string _mensajeEstado = string.Empty;
        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        public ObservableCollection<Producto> ProductosInventario
        {
            get => _productosInventario;
            set => SetProperty(ref _productosInventario, value);
        }

        public ObservableCollection<FichaProductoWrapper> ProductosDetalle
        {
            get => _productosDetalle;
            set => SetProperty(ref _productosDetalle, value);
        }

        #endregion

        #region Commands

        public ICommand VerDetalleCommand { get; }
        public ICommand CerrarDetalleCommand { get; }
        public ICommand GuardarCostosCommand { get; }
        public ICommand ConfirmarInventarioCommand { get; }
        public ICommand DividirProductoCommand { get; }
        public ICommand EliminarFichaCommand { get; }
        public ICommand RefrescarCommand { get; }

        #endregion

        public FichasCostoViewModel(
            IFichaCostoService fichaCostoService, 
            IConfiguracionService configuracionService,
            IEntregaService entregaService,
            IProductoService productoService,
            IComboService comboService)
        {
            _fichaCostoService = fichaCostoService;
            _configuracionService = configuracionService;
            _entregaService = entregaService;
            _productoService = productoService;
            _comboService = comboService;

            // Rango por defecto: mes actual
            _fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _fechaHasta = _fechaDesde.AddMonths(1).AddDays(-1);

            VerDetalleCommand = new RelayCommand(async param => await AbrirDetalleAsync(param as FichaCosto));
            CerrarDetalleCommand = new RelayCommand(_ => CerrarDetalle());
            GuardarCostosCommand = new RelayCommand(async _ => await GuardarCostosAsync());
            ConfirmarInventarioCommand = new RelayCommand(async _ => await ConfirmarDescontarInventarioAsync());
            DividirProductoCommand = new RelayCommand(param => DividirProducto(param as FichaProductoWrapper));
            EliminarFichaCommand = new RelayCommand(async param => await EliminarFichaAsync(param as FichaCosto));
            RefrescarCommand = new RelayCommand(async _ => await CargarFichasAsync());
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            _ = CargarFichasAsync();
        }

        // ═══════════════════════════════════════════════════
        // CARGA DE DATOS
        // ═══════════════════════════════════════════════════

        private async Task CargarFichasAsync()
        {
            try
            {
                var fichas = await _fichaCostoService.ObtenerPorRangoFechaAsync(FechaDesde, FechaHasta);
                Fichas = new ObservableCollection<FichaCosto>(fichas);
                FiltrarFichas();
                MensajeEstado = $"{fichas.Count} ficha(s) en el período";
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar fichas: {ex.Message}";
            }
        }

        private void FiltrarFichas()
        {
            if (string.IsNullOrWhiteSpace(Filtro))
            {
                FichasFiltradas = new ObservableCollection<FichaCosto>(Fichas);
            }
            else
            {
                var filtroLower = Filtro.ToLower();
                var filtradas = Fichas.Where(f =>
                    f.NumeroFicha.Contains(filtroLower, StringComparison.OrdinalIgnoreCase) ||
                    f.NombreReceptor.Contains(filtroLower, StringComparison.OrdinalIgnoreCase) ||
                    f.Remitente.Contains(filtroLower, StringComparison.OrdinalIgnoreCase) ||
                    f.NombreAgencia.Contains(filtroLower, StringComparison.OrdinalIgnoreCase) ||
                    f.Distribuidor.Contains(filtroLower, StringComparison.OrdinalIgnoreCase)
                );
                FichasFiltradas = new ObservableCollection<FichaCosto>(filtradas);
            }

            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            OnPropertyChanged(nameof(TotalFichas));
            OnPropertyChanged(nameof(TotalVentas));
            OnPropertyChanged(nameof(TotalCostos));
            OnPropertyChanged(nameof(TotalGanancia));
        }

        // ═══════════════════════════════════════════════════
        // DETALLE
        // ═══════════════════════════════════════════════════

        private async Task AbrirDetalleAsync(FichaCosto? ficha)
        {
            if (ficha == null) return;
            FichaSeleccionada = ficha;

            // Cargar productos del inventario para los ComboBox
            try
            {
                var productos = await _productoService.ObtenerTodosAsync();
                ProductosInventario = new ObservableCollection<Producto>(productos);

                // Crear wrappers con la lista de productos vinculados disponibles
                var wrappers = new ObservableCollection<FichaProductoWrapper>();
                Combo? combo = null;
                if (ficha.ComboId.HasValue)
                {
                    combo = await _comboService.ObtenerPorIdAsync(ficha.ComboId.Value);
                }

                foreach (var fp in ficha.Productos)
                {
                    var wrapper = new FichaProductoWrapper(fp, productos);

                    // Filtrar los productos vinculados desde el combo
                    if (combo != null)
                    {
                        var comboProducto = combo.Productos
                            .FirstOrDefault(cp => cp.NombreProducto.Equals(fp.NombreProducto, StringComparison.OrdinalIgnoreCase));

                        if (comboProducto?.ProductosInventario != null)
                        {
                            var vinculadosIds = comboProducto.ProductosInventario
                                .Select(v => v.ProductoId).ToList();
                            var vinculados = productos
                                .Where(p => vinculadosIds.Contains(p.Id)).ToList();
                            wrapper.ProductosDisponibles = new ObservableCollection<Producto>(vinculados);
                        }
                    }

                    // Si no hay vinculados del combo, mostrar todos
                    if (wrapper.ProductosDisponibles.Count == 0)
                    {
                        wrapper.ProductosDisponibles = new ObservableCollection<Producto>(
                            productos.Where(p => p.Nombre.Contains(fp.NombreProducto, StringComparison.OrdinalIgnoreCase)
                                || fp.NombreProducto.Contains(p.Nombre, StringComparison.OrdinalIgnoreCase))
                        );
                        // Si aún no hay match por nombre, poner todos
                        if (wrapper.ProductosDisponibles.Count == 0)
                        {
                            wrapper.ProductosDisponibles = new ObservableCollection<Producto>(productos);
                        }
                    }

                    // Pre-seleccionar si ya tiene ProductoId
                    if (fp.ProductoId.HasValue)
                    {
                        wrapper.ProductoSeleccionado = productos.FirstOrDefault(p => p.Id == fp.ProductoId.Value);
                    }

                    wrappers.Add(wrapper);
                }

                ProductosDetalle = wrappers;
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar detalle: {ex.Message}";
            }

            MostrarDetalle = true;
        }

        private void CerrarDetalle()
        {
            MostrarDetalle = false;
            FichaSeleccionada = null;
            ProductosDetalle = new ObservableCollection<FichaProductoWrapper>();
        }

        // ═════════════════════════════════════════════════
        // GUARDAR COSTOS
        // ═════════════════════════════════════════════════

        private async Task GuardarCostosAsync()
        {
            if (FichaSeleccionada == null) return;

            try
            {
                // Sincronizar selecciones de inventario desde los wrappers
                foreach (var wrapper in ProductosDetalle)
                {
                    if (wrapper.ProductoSeleccionado != null)
                    {
                        wrapper.FichaProducto.ProductoId = wrapper.ProductoSeleccionado.Id;
                        wrapper.FichaProducto.CostoUnitario = wrapper.ProductoSeleccionado.CostoCompra;
                    }
                }

                await _fichaCostoService.ActualizarAsync(FichaSeleccionada);
                MensajeEstado = $"✅ Costos actualizados — {FichaSeleccionada.NumeroFicha}";
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al guardar: {ex.Message}";
            }
        }

        // ═════════════════════════════════════════════════
        // CONFIRMAR Y DESCONTAR INVENTARIO
        // ═════════════════════════════════════════════════

        private async Task ConfirmarDescontarInventarioAsync()
        {
            if (FichaSeleccionada == null) return;

            if (FichaSeleccionada.InventarioDescontado)
            {
                MensajeEstado = "ℹ️ El inventario ya fue descontado para esta ficha";
                return;
            }

            // Verificar que todos los productos tengan inventario asignado
            var sinAsignar = ProductosDetalle.Where(w => w.ProductoSeleccionado == null).ToList();
            if (sinAsignar.Any())
            {
                var nombres = string.Join(", ", sinAsignar.Select(w => w.FichaProducto.NombreProducto));
                MensajeEstado = $"⚠️ Asigna producto de inventario a: {nombres}";
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Confirmar y descontar del inventario?\n\nSe descontarán las cantidades de los productos seleccionados.\n" +
                $"Ficha: {FichaSeleccionada.NumeroFicha}",
                "Confirmar descuento de inventario",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                // Primero guardar las selecciones
                foreach (var wrapper in ProductosDetalle)
                {
                    if (wrapper.ProductoSeleccionado != null)
                    {
                        wrapper.FichaProducto.ProductoId = wrapper.ProductoSeleccionado.Id;
                        wrapper.FichaProducto.CostoUnitario = wrapper.ProductoSeleccionado.CostoCompra;
                    }
                }

                await _fichaCostoService.ActualizarAsync(FichaSeleccionada);

                // Descontar inventario
                await _fichaCostoService.DescontarInventarioAsync(FichaSeleccionada);

                MensajeEstado = $"✅ Inventario descontado — {FichaSeleccionada.NumeroFicha}";
                OnPropertyChanged(nameof(FichaSeleccionada));
                ActualizarTotales();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al descontar: {ex.Message}";
            }
        }

        // ═════════════════════════════════════════════════
        // DIVIDIR PRODUCTO (usar múltiples fuentes de inventario)
        // ═════════════════════════════════════════════════

        private void DividirProducto(FichaProductoWrapper? wrapper)
        {
            if (wrapper == null || FichaSeleccionada == null) return;

            var original = wrapper.FichaProducto;
            if (original.Cantidad <= 1)
            {
                MensajeEstado = "⚠️ La cantidad debe ser mayor que 1 para dividir";
                return;
            }

            // Dividir en 2: mitad en el original, mitad en un nuevo producto
            decimal mitad = Math.Floor(original.Cantidad / 2);
            decimal resto = original.Cantidad - mitad;

            original.Cantidad = mitad;

            var nuevo = new FichaCostoProducto
            {
                FichaCostoId = original.FichaCostoId,
                NombreProducto = original.NombreProducto,
                Unidad = original.Unidad,
                Cantidad = resto,
                CostoUnitario = 0, // Sin asignar aún
                ProductoId = null
            };

            FichaSeleccionada.Productos.Add(nuevo);

            // Recrear wrappers
            var wrappers = new ObservableCollection<FichaProductoWrapper>();
            foreach (var fp in FichaSeleccionada.Productos)
            {
                var w = new FichaProductoWrapper(fp, ProductosInventario.ToList());
                // Copiar disponibles del wrapper original si tiene mismo nombre
                var existente = ProductosDetalle.FirstOrDefault(pd => pd.FichaProducto.NombreProducto == fp.NombreProducto);
                if (existente != null)
                {
                    w.ProductosDisponibles = existente.ProductosDisponibles;
                }
                if (fp.ProductoId.HasValue)
                {
                    w.ProductoSeleccionado = ProductosInventario.FirstOrDefault(p => p.Id == fp.ProductoId.Value);
                }
                wrappers.Add(w);
            }
            ProductosDetalle = wrappers;

            MensajeEstado = $"✂️ Producto dividido: {mitad} + {resto} {original.Unidad}";
        }

        // ═══════════════════════════════════════════════════
        // ELIMINAR
        // ═══════════════════════════════════════════════════

        private async Task EliminarFichaAsync(FichaCosto? ficha)
        {
            if (ficha == null) return;

            var resultado = MessageBox.Show(
                $"¿Eliminar la ficha {ficha.NumeroFicha}?\n\nSe restaurará el inventario descontado y la entrega asociada quedará sin ficha.\nEsta acción no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                // Restaurar inventario si fue descontado
                await _fichaCostoService.RestaurarInventarioAsync(ficha);

                // Resetear la entrega asociada
                if (ficha.EntregaId.HasValue)
                {
                    var entrega = await _entregaService.ObtenerPorIdAsync(ficha.EntregaId.Value);
                    if (entrega != null)
                    {
                        entrega.TieneFichaCosto = false;
                        await _entregaService.ActualizarAsync(entrega);
                    }
                }

                await _fichaCostoService.EliminarAsync(ficha.Id);
                MensajeEstado = $"🗑 Ficha {ficha.NumeroFicha} eliminada — inventario restaurado";
                CerrarDetalle();
                await CargarFichasAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al eliminar: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Wrapper para un FichaCostoProducto que permite seleccionar
    /// el producto del inventario desde un ComboBox.
    /// Maneja múltiples productos vinculados (ej: cerdo a $40 y cerdo a $50).
    /// </summary>
    public class FichaProductoWrapper : BaseViewModel
    {
        public FichaCostoProducto FichaProducto { get; }

        private ObservableCollection<Producto> _productosDisponibles = new();
        private Producto? _productoSeleccionado;

        public ObservableCollection<Producto> ProductosDisponibles
        {
            get => _productosDisponibles;
            set => SetProperty(ref _productosDisponibles, value);
        }

        public Producto? ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set
            {
                SetProperty(ref _productoSeleccionado, value);
                if (value != null)
                {
                    FichaProducto.ProductoId = value.Id;
                    FichaProducto.CostoUnitario = value.CostoCompra;
                    OnPropertyChanged(nameof(CostoUnitario));
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(InfoStock));
                }
            }
        }

        public string NombreProducto => FichaProducto.NombreProducto;
        public UnidadMedida Unidad => FichaProducto.Unidad;

        public decimal Cantidad
        {
            get => FichaProducto.Cantidad;
            set
            {
                FichaProducto.Cantidad = value;
                OnPropertyChanged(nameof(Cantidad));
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal CostoUnitario => FichaProducto.CostoUnitario;
        public decimal Total => FichaProducto.Total;

        public string InfoStock => ProductoSeleccionado != null
            ? $"Stock: {ProductoSeleccionado.CantidadStock:G} {ProductoSeleccionado.Unidad} — ${ProductoSeleccionado.CostoCompra:N2}"
            : "Sin asignar";

        public FichaProductoWrapper(FichaCostoProducto fichaProducto, List<Producto> todosProductos)
        {
            FichaProducto = fichaProducto;
            ProductosDisponibles = new ObservableCollection<Producto>(todosProductos);
        }
    }
}
