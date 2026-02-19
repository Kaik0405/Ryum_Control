using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;
using Microsoft.Win32;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de Entregas (órdenes de envío).
    /// 
    /// FLUJO:
    /// 1. Usuario ve la lista de entregas con indicadores de urgencia.
    /// 2. Crea nueva entrega → selecciona combo → datos del receptor.
    /// 3. La fecha se pone automáticamente (editable).
    /// 4. Las entregas próximas a vencer (≤2 días) se marcan urgentes (naranja).
    /// 5. Las entregas vencidas se marcan en rojo.
    /// 6. Puede marcar una entrega como "entregada".
    /// </summary>
    public class EntregasViewModel : BaseViewModel
    {
        private readonly IEntregaService _entregaService;
        private readonly IComboService _comboService;
        private readonly IConfiguracionService _configuracionService;
        private readonly IFichaCostoService _fichaCostoService;

        // Listas principales
        private ObservableCollection<Entrega> _entregas = new();
        private ObservableCollection<Entrega> _entregasFiltradas = new();
        private ObservableCollection<Combo> _combosDisponibles = new();

        // Selección
        private Entrega? _entregaSeleccionada;

        // Detalle
        private bool _mostrarDetalle;
        private Entrega? _entregaDetalle;

        // Filtro
        private string _filtro = string.Empty;
        private string _filtroEstado = "Todas"; // Todas, Pendientes, Urgentes, Entregadas

        // Formulario
        private bool _mostrarFormulario;
        private bool _esEdicion;
        private Combo? _formComboSeleccionado;
        private string _formReceptor = string.Empty;
        private string _formDireccion = string.Empty;
        private string _formTelefonoMovil = string.Empty;
        private string _formTelefonoFijo = string.Empty;
        private string _formRemitente = string.Empty;
        private string _formAgencia = string.Empty;
        private DateTime _formFechaOrden = DateTime.Now;
        private string _formObservaciones = string.Empty;

        // Disponibilidad de inventario
        private ObservableCollection<ProductoDisponibilidad> _productosDisponibilidad = new();
        private string _disponibilidadGeneral = string.Empty;
        private bool _mostrarDisponibilidad;

        // Contadores
        private int _totalPendientes;
        private int _totalUrgentes;
        private int _totalVencidas;
        private int _totalEntregadas;

        // Estado
        private string _mensajeEstado = string.Empty;

        // Modelo de conformidad
        private bool _mostrarConformidad;
        private Entrega? _entregaConformidad;
        private string _nombreDistribuidor = string.Empty;

        #region Propiedades de Datos

        public ObservableCollection<Entrega> Entregas
        {
            get => _entregas;
            set => SetProperty(ref _entregas, value);
        }

        public ObservableCollection<Entrega> EntregasFiltradas
        {
            get => _entregasFiltradas;
            set => SetProperty(ref _entregasFiltradas, value);
        }

        public ObservableCollection<Combo> CombosDisponibles
        {
            get => _combosDisponibles;
            set => SetProperty(ref _combosDisponibles, value);
        }

        public Entrega? EntregaSeleccionada
        {
            get => _entregaSeleccionada;
            set => SetProperty(ref _entregaSeleccionada, value);
        }

        public bool MostrarDetalle
        {
            get => _mostrarDetalle;
            set => SetProperty(ref _mostrarDetalle, value);
        }

        public Entrega? EntregaDetalle
        {
            get => _entregaDetalle;
            set => SetProperty(ref _entregaDetalle, value);
        }

        public string Filtro
        {
            get => _filtro;
            set => SetProperty(ref _filtro, value, FiltrarEntregas);
        }

        public string FiltroEstado
        {
            get => _filtroEstado;
            set => SetProperty(ref _filtroEstado, value, FiltrarEntregas);
        }

        public string[] OpcionesEstado => new[] { "Todas", "Pendientes", "Urgentes", "Entregadas" };

        public int TotalPendientes
        {
            get => _totalPendientes;
            set => SetProperty(ref _totalPendientes, value);
        }

        public int TotalUrgentes
        {
            get => _totalUrgentes;
            set => SetProperty(ref _totalUrgentes, value);
        }

        public int TotalVencidas
        {
            get => _totalVencidas;
            set => SetProperty(ref _totalVencidas, value);
        }

        public int TotalEntregadas
        {
            get => _totalEntregadas;
            set => SetProperty(ref _totalEntregadas, value);
        }

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

        public string TituloFormulario => EsEdicion ? "✏️ Editar Entrega" : "📦 Nueva Entrega";

        public Combo? FormComboSeleccionado
        {
            get => _formComboSeleccionado;
            set
            {
                if (SetProperty(ref _formComboSeleccionado, value))
                    CalcularDisponibilidad(value);
            }
        }

        public string FormReceptor
        {
            get => _formReceptor;
            set => SetProperty(ref _formReceptor, value);
        }

        public string FormDireccion
        {
            get => _formDireccion;
            set => SetProperty(ref _formDireccion, value);
        }

        public string FormTelefonoMovil
        {
            get => _formTelefonoMovil;
            set => SetProperty(ref _formTelefonoMovil, value);
        }

        public string FormTelefonoFijo
        {
            get => _formTelefonoFijo;
            set => SetProperty(ref _formTelefonoFijo, value);
        }

        public string FormRemitente
        {
            get => _formRemitente;
            set => SetProperty(ref _formRemitente, value);
        }

        public string FormAgencia
        {
            get => _formAgencia;
            set => SetProperty(ref _formAgencia, value);
        }

        public DateTime FormFechaOrden
        {
            get => _formFechaOrden;
            set => SetProperty(ref _formFechaOrden, value);
        }

        public string FormObservaciones
        {
            get => _formObservaciones;
            set => SetProperty(ref _formObservaciones, value);
        }

        #endregion

        #region Propiedades de Disponibilidad

        public ObservableCollection<ProductoDisponibilidad> ProductosDisponibilidad
        {
            get => _productosDisponibilidad;
            set => SetProperty(ref _productosDisponibilidad, value);
        }

        public string DisponibilidadGeneral
        {
            get => _disponibilidadGeneral;
            set => SetProperty(ref _disponibilidadGeneral, value);
        }

        public bool MostrarDisponibilidad
        {
            get => _mostrarDisponibilidad;
            set => SetProperty(ref _mostrarDisponibilidad, value);
        }

        #endregion

        #region Comandos

        public ICommand CrearEntregaCommand { get; }
        public ICommand EditarEntregaCommand { get; }
        public ICommand EliminarEntregaCommand { get; }
        public ICommand MarcarEntregadaCommand { get; }
        public ICommand VerDetalleCommand { get; }
        public ICommand CerrarDetalleCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand RefrescarCommand { get; }
        public ICommand GenerarConformidadCommand { get; }
        public ICommand ExportarConformidadImagenCommand { get; }
        public ICommand ImprimirConformidadCommand { get; }
        public ICommand CerrarConformidadCommand { get; }

        #endregion

        #region Propiedades Conformidad

        public bool MostrarConformidad
        {
            get => _mostrarConformidad;
            set => SetProperty(ref _mostrarConformidad, value);
        }

        public Entrega? EntregaConformidad
        {
            get => _entregaConformidad;
            set => SetProperty(ref _entregaConformidad, value);
        }

        public string NombreDistribuidor
        {
            get => _nombreDistribuidor;
            set => SetProperty(ref _nombreDistribuidor, value);
        }

        #endregion

        public EntregasViewModel(IEntregaService entregaService, IComboService comboService, IConfiguracionService configuracionService, IFichaCostoService fichaCostoService)
        {
            _entregaService = entregaService;
            _comboService = comboService;
            _configuracionService = configuracionService;
            _fichaCostoService = fichaCostoService;

            CrearEntregaCommand = new RelayCommand(_ => PrepararNueva());
            EditarEntregaCommand = new RelayCommand(param => PrepararEdicion(param as Entrega));
            EliminarEntregaCommand = new RelayCommand(async param => await EliminarAsync(param as Entrega));
            MarcarEntregadaCommand = new RelayCommand(async param => await MarcarEntregadaAsync(param as Entrega));
            VerDetalleCommand = new RelayCommand(param => VerDetalle(param as Entrega));
            CerrarDetalleCommand = new RelayCommand(_ => { MostrarDetalle = false; EntregaDetalle = null; });
            GuardarCommand = new RelayCommand(
                async _ => await GuardarAsync(),
                _ => !string.IsNullOrWhiteSpace(FormReceptor) && FormComboSeleccionado != null
                     && !string.IsNullOrWhiteSpace(FormRemitente));
            CancelarCommand = new RelayCommand(_ => CerrarFormulario());
            RefrescarCommand = new RelayCommand(async _ => await CargarDatosAsync());
            GenerarConformidadCommand = new RelayCommand(param => AbrirConformidad(param as Entrega));
            ExportarConformidadImagenCommand = new RelayCommand(_ => { /* se invoca desde code-behind */ });
            ImprimirConformidadCommand = new RelayCommand(_ => { /* se invoca desde code-behind */ });
            CerrarConformidadCommand = new RelayCommand(_ => CerrarConformidad());
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
                var entregas = await _entregaService.ObtenerTodosAsync();
                Entregas = new ObservableCollection<Entrega>(entregas);
                FiltrarEntregas();
                ActualizarContadores();
                MensajeEstado = $"{entregas.Count} entrega(s) registrada(s)";

                // Cargar nombre del distribuidor
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                NombreDistribuidor = config.Nombre;
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar entregas: {ex.Message}";
            }

            await CargarCombosAsync();
        }

        private async Task CargarCombosAsync()
        {
            try
            {
                var combos = await _comboService.ObtenerTodosAsync();
                CombosDisponibles = new ObservableCollection<Combo>(combos);
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar combos: {ex.Message}";
            }
        }

        private void FiltrarEntregas()
        {
            var filtradas = Entregas.AsEnumerable();

            // Filtro por estado
            switch (FiltroEstado)
            {
                case "Pendientes":
                    filtradas = filtradas.Where(e => !e.Entregada);
                    break;
                case "Urgentes":
                    filtradas = filtradas.Where(e => e.Urgente || e.Vencida);
                    break;
                case "Entregadas":
                    filtradas = filtradas.Where(e => e.Entregada);
                    break;
            }

            // Filtro por texto
            if (!string.IsNullOrWhiteSpace(Filtro))
            {
                filtradas = filtradas.Where(e =>
                    e.NombreReceptor.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                    e.NumeroOrden.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                    e.NombreRemitente.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                    e.Agencia.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ||
                    (e.Combo?.Nombre.Contains(Filtro, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (e.Combo?.Numero.ToString().Contains(Filtro) ?? false));
            }

            EntregasFiltradas = new ObservableCollection<Entrega>(filtradas);
        }

        private void ActualizarContadores()
        {
            var pendientes = Entregas.Where(e => !e.Entregada).ToList();
            TotalPendientes = pendientes.Count;
            TotalUrgentes = pendientes.Count(e => e.Urgente);
            TotalVencidas = pendientes.Count(e => e.Vencida);
            TotalEntregadas = Entregas.Count(e => e.Entregada);
        }

        // ═══════════════════════════════════════════════════
        // MÉTODOS DEL FORMULARIO
        // ═══════════════════════════════════════════════════

        private async void PrepararNueva()
        {
            EsEdicion = false;
            LimpiarFormulario();
            FormFechaOrden = DateTime.Now; // Fecha automática
            await CargarCombosAsync();
            MostrarFormulario = true;
            MostrarDetalle = false;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void PrepararEdicion(Entrega? entrega)
        {
            if (entrega == null) return;
            EsEdicion = true;
            EntregaSeleccionada = entrega;

            FormComboSeleccionado = CombosDisponibles.FirstOrDefault(c => c.Id == entrega.ComboId);
            FormReceptor = entrega.NombreReceptor;
            FormDireccion = entrega.DireccionReceptor;
            FormTelefonoMovil = entrega.TelefonoMovil;
            FormTelefonoFijo = entrega.TelefonoFijo;
            FormRemitente = entrega.NombreRemitente;
            FormAgencia = entrega.Agencia;
            FormFechaOrden = entrega.FechaOrden;
            FormObservaciones = entrega.Observaciones ?? string.Empty;

            MostrarFormulario = true;
            MostrarDetalle = false;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private async Task GuardarAsync()
        {
            try
            {
                if (FormComboSeleccionado == null)
                {
                    MensajeEstado = "❌ Debe seleccionar un combo";
                    return;
                }

                if (string.IsNullOrWhiteSpace(FormReceptor))
                {
                    MensajeEstado = "❌ El nombre del receptor es obligatorio";
                    return;
                }

                if (string.IsNullOrWhiteSpace(FormRemitente))
                {
                    MensajeEstado = "❌ El nombre de quien envía es obligatorio";
                    return;
                }

                if (EsEdicion && EntregaSeleccionada != null)
                {
                    EntregaSeleccionada.NombreReceptor = FormReceptor.Trim();
                    EntregaSeleccionada.DireccionReceptor = FormDireccion.Trim();
                    EntregaSeleccionada.TelefonoMovil = FormTelefonoMovil.Trim();
                    EntregaSeleccionada.TelefonoFijo = FormTelefonoFijo.Trim();
                    EntregaSeleccionada.NombreRemitente = FormRemitente.Trim();
                    EntregaSeleccionada.Agencia = FormAgencia.Trim();
                    EntregaSeleccionada.FechaOrden = FormFechaOrden;
                    EntregaSeleccionada.Observaciones = string.IsNullOrWhiteSpace(FormObservaciones) ? null : FormObservaciones.Trim();

                    await _entregaService.ActualizarAsync(EntregaSeleccionada);
                    MensajeEstado = $"✅ Entrega {EntregaSeleccionada.NumeroOrden} actualizada";
                }
                else
                {
                    var nueva = await _entregaService.CrearDesdeComboAsync(
                        FormComboSeleccionado.Id,
                        FormReceptor.Trim(),
                        FormDireccion.Trim(),
                        FormTelefonoMovil.Trim(),
                        FormTelefonoFijo.Trim(),
                        FormRemitente.Trim(),
                        FormAgencia.Trim());

                    // Actualizar fecha si fue modificada (no es la de creación)
                    if (FormFechaOrden.Date != DateTime.Now.Date)
                    {
                        nueva.FechaOrden = FormFechaOrden;
                    }
                    if (!string.IsNullOrWhiteSpace(FormObservaciones))
                    {
                        nueva.Observaciones = FormObservaciones.Trim();
                    }
                    await _entregaService.ActualizarAsync(nueva);

                    MensajeEstado = $"✅ Entrega {nueva.NumeroOrden} creada — plazo: 5 días";
                }

                CerrarFormulario();
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al guardar: {ex.Message}";
            }
        }

        private async Task EliminarAsync(Entrega? entrega)
        {
            if (entrega == null) return;

            var mensaje = entrega.TieneFichaCosto
                ? $"¿Eliminar la entrega {entrega.NumeroOrden}?\n({entrega.NombreReceptor})\n\n⚠️ También se eliminará la ficha de costo asociada y se restaurará el inventario."
                : $"¿Eliminar la entrega {entrega.NumeroOrden}?\n({entrega.NombreReceptor})";

            var resultado = MessageBox.Show(
                mensaje,
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            try
            {
                // Si tiene ficha de costo, eliminarla primero (restaura inventario)
                if (entrega.TieneFichaCosto)
                {
                    var ficha = await _fichaCostoService.ObtenerPorEntregaIdAsync(entrega.Id);
                    if (ficha != null)
                    {
                        await _fichaCostoService.RestaurarInventarioAsync(ficha);
                        await _fichaCostoService.EliminarAsync(ficha.Id);
                    }
                }

                await _entregaService.EliminarAsync(entrega.Id);
                MensajeEstado = $"🗑️ Entrega {entrega.NumeroOrden} eliminada";
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error al eliminar: {ex.Message}";
            }
        }

        private async Task MarcarEntregadaAsync(Entrega? entrega)
        {
            if (entrega == null) return;

            if (entrega.Entregada)
            {
                MensajeEstado = "ℹ️ Ya fue marcada como entregada";
                return;
            }

            try
            {
                await _entregaService.MarcarEntregadaAsync(entrega.Id);

                // Auto-crear Ficha de Costo
                if (!entrega.TieneFichaCosto)
                {
                    await CrearFichaCostoDesdeEntregaAsync(entrega);
                }

                MensajeEstado = $"✅ Entrega {entrega.NumeroOrden} entregada — Ficha de costo generada";
                await CargarDatosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Crea automáticamente una Ficha de Costo a partir de una entrega.
        /// Toma los datos del receptor, remitente, combo y productos.
        /// Los costos se asignan desde los productos de inventario vinculados al combo.
        /// </summary>
        private async Task CrearFichaCostoDesdeEntregaAsync(Entrega entrega)
        {
            // Obtener el combo completo con productos e inventario vinculado
            var combo = CombosDisponibles.FirstOrDefault(c => c.Id == entrega.ComboId);

            // Si el combo no se encontró en memoria, cargar desde DB con includes completos
            if (combo == null && entrega.ComboId > 0)
            {
                combo = await _comboService.ObtenerPorIdAsync(entrega.ComboId);
            }

            // Obtener nombre del distribuidor
            var config = await _configuracionService.ObtenerConfiguracionAsync();

            // Crear los productos de la ficha con costos del inventario vinculado
            var productosFicha = new List<FichaCostoProducto>();
            foreach (var ep in entrega.Productos)
            {
                decimal costoUnitario = 0;
                int? productoInventarioId = null;

                // Buscar el ComboProducto que corresponde a este EntregaProducto por nombre
                var comboProducto = combo?.Productos
                    .FirstOrDefault(cp => cp.NombreProducto.Equals(ep.NombreProducto, StringComparison.OrdinalIgnoreCase));

                if (comboProducto?.ProductosInventario != null && comboProducto.ProductosInventario.Any())
                {
                    // Tomar el primer producto vinculado del inventario para obtener el costo
                    var vinculo = comboProducto.ProductosInventario.FirstOrDefault();
                    if (vinculo?.Producto != null)
                    {
                        costoUnitario = vinculo.Producto.CostoCompra;
                        productoInventarioId = vinculo.ProductoId;
                    }
                }

                productosFicha.Add(new FichaCostoProducto
                {
                    NombreProducto = ep.NombreProducto,
                    Unidad = ep.Unidad,
                    Cantidad = ep.Cantidad,
                    CostoUnitario = costoUnitario,
                    ProductoId = productoInventarioId
                });
            }

            var ficha = new FichaCosto
            {
                ComboId = entrega.ComboId,
                TipoPedido = combo?.Tipo ?? TipoCombo.Combo,
                Distribuidor = config.Nombre,
                Remitente = entrega.NombreRemitente,
                NombreReceptor = entrega.NombreReceptor,
                DireccionReceptor = entrega.DireccionReceptor,
                TelefonoReceptor = entrega.TelefonoMovil,
                NombreAgencia = entrega.Agencia,
                FechaEnvio = entrega.FechaOrden,
                FechaEntrega = DateTime.Now,
                PrecioVentaUSD = combo?.PrecioVenta ?? 0,
                EntregaId = entrega.Id,
                Notas = entrega.Observaciones,
                Productos = productosFicha
            };

            await _fichaCostoService.CrearAsync(ficha);

            // Marcar que la entrega ya tiene ficha (recargar desde DB para evitar conflicto de tracking)
            var entregaDb = await _entregaService.ObtenerPorIdAsync(entrega.Id);
            if (entregaDb != null)
            {
                entregaDb.TieneFichaCosto = true;
                await _entregaService.ActualizarAsync(entregaDb);
            }
        }

        private void VerDetalle(Entrega? entrega)
        {
            if (entrega == null) return;
            EntregaDetalle = entrega;
            MostrarDetalle = true;
            MostrarFormulario = false;

            // Calcular disponibilidad usando el combo completo de CombosDisponibles
            var comboCompleto = CombosDisponibles.FirstOrDefault(c => c.Id == entrega.ComboId);
            CalcularDisponibilidad(comboCompleto);
        }

        private void CerrarFormulario()
        {
            MostrarFormulario = false;
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            FormComboSeleccionado = null;
            FormReceptor = string.Empty;
            FormDireccion = string.Empty;
            FormTelefonoMovil = string.Empty;
            FormTelefonoFijo = string.Empty;
            FormRemitente = string.Empty;
            FormAgencia = string.Empty;
            FormFechaOrden = DateTime.Now;
            FormObservaciones = string.Empty;
            EntregaSeleccionada = null;
            ProductosDisponibilidad.Clear();
            MostrarDisponibilidad = false;
            DisponibilidadGeneral = string.Empty;
        }

        // ═══════════════════════════════════════════════════
        // DISPONIBILIDAD DE INVENTARIO
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Calcula la disponibilidad de stock para cada producto del combo
        /// basándose en las vinculaciones con productos de inventario.
        /// </summary>
        private void CalcularDisponibilidad(Combo? combo)
        {
            if (combo == null || combo.Productos == null || !combo.Productos.Any())
            {
                ProductosDisponibilidad = new ObservableCollection<ProductoDisponibilidad>();
                MostrarDisponibilidad = false;
                DisponibilidadGeneral = string.Empty;
                return;
            }

            var disponibilidad = new ObservableCollection<ProductoDisponibilidad>();

            foreach (var cp in combo.Productos)
            {
                var vinculaciones = cp.ProductosInventario;
                
                if (vinculaciones == null || !vinculaciones.Any())
                {
                    disponibilidad.Add(new ProductoDisponibilidad
                    {
                        NombreProducto = cp.NombreProducto,
                        CantidadRequerida = cp.Cantidad,
                        Unidad = cp.Unidad.ToString(),
                        StockDisponible = 0,
                        TieneVinculacion = false,
                        Estado = "Sin vincular",
                        Icono = "⚪",
                        ColorFondo = "#F5F5F5",
                        ColorTexto = "#999",
                        Detalle = "No vinculado al inventario"
                    });
                }
                else
                {
                    var stockTotal = vinculaciones.Sum(v => v.Producto?.CantidadStock ?? 0);
                    var suficiente = stockTotal >= cp.Cantidad;
                    var hayAlgo = stockTotal > 0;
                    var faltante = cp.Cantidad - stockTotal;

                    string detalle;
                    if (suficiente)
                    {
                        detalle = $"Tenés {stockTotal:G} {cp.Unidad} — necesitás {cp.Cantidad:G} {cp.Unidad} ✓";
                    }
                    else if (hayAlgo)
                    {
                        detalle = $"Tenés {stockTotal:G} de {cp.Cantidad:G} {cp.Unidad} — faltan {faltante:G} {cp.Unidad}";
                    }
                    else
                    {
                        detalle = $"Necesitás {cp.Cantidad:G} {cp.Unidad} — no hay stock";
                    }

                    disponibilidad.Add(new ProductoDisponibilidad
                    {
                        NombreProducto = cp.NombreProducto,
                        CantidadRequerida = cp.Cantidad,
                        Unidad = cp.Unidad.ToString(),
                        StockDisponible = stockTotal,
                        TieneVinculacion = true,
                        Estado = suficiente ? "Disponible" : (hayAlgo ? $"Faltan {faltante:G} {cp.Unidad}" : "Agotado"),
                        Icono = suficiente ? "✅" : (hayAlgo ? "🟡" : "🔴"),
                        ColorFondo = suficiente ? "#E8F5E9" : (hayAlgo ? "#FFF8E1" : "#FFEBEE"),
                        ColorTexto = suficiente ? "#2E7D32" : (hayAlgo ? "#F57F17" : "#C62828"),
                        Detalle = detalle
                    });
                }
            }

            ProductosDisponibilidad = disponibilidad;
            MostrarDisponibilidad = true;

            var vinculados = disponibilidad.Where(d => d.TieneVinculacion).ToList();
            if (!vinculados.Any())
            {
                DisponibilidadGeneral = "⚪ Sin vinculaciones de inventario";
            }
            else if (vinculados.All(d => d.Estado == "Disponible"))
            {
                DisponibilidadGeneral = "✅ Todo disponible en inventario";
            }
            else
            {
                var agotados = vinculados.Count(d => d.StockDisponible == 0);
                var parciales = vinculados.Count(d => d.StockDisponible > 0 && d.StockDisponible < d.CantidadRequerida);
                var partes = new List<string>();
                if (agotados > 0) partes.Add($"{agotados} agotado(s)");
                if (parciales > 0) partes.Add($"{parciales} con stock parcial");
                DisponibilidadGeneral = (agotados > 0 ? "🔴" : "🟡") + $" {string.Join(", ", partes)}";
            }
        }

        // ═══════════════════════════════════════════════════
        // MODELO DE CONFORMIDAD
        // ═══════════════════════════════════════════════════

        private void AbrirConformidad(Entrega? entrega)
        {
            if (entrega == null) return;
            EntregaConformidad = entrega;
            MostrarConformidad = true;
            MostrarDetalle = false;
            MostrarFormulario = false;
        }

        private void CerrarConformidad()
        {
            MostrarConformidad = false;
            EntregaConformidad = null;
        }
    }
}
