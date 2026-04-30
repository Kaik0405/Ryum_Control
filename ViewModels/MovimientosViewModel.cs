using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel completo para el módulo de Finanzas.
    /// Gestiona ingresos, egresos, balance, ganancia y análisis por ficha.
    /// </summary>
    public class MovimientosViewModel : BaseViewModel
    {
        private readonly IMovimientoService _movimientoService;
        private readonly IFichaCostoService _fichaCostoService;
        private readonly IConfiguracionService _configuracionService;

        #region Backing fields

        private ObservableCollection<Movimiento> _movimientos = new();
        private ObservableCollection<Movimiento> _movimientosFiltrados = new();
        private List<Movimiento> _todosMovimientos = new();

        private Movimiento? _movimientoSeleccionado;
        private DateTime _fechaDesde;
        private DateTime _fechaHasta;
        private TipoMovimiento? _filtroTipo;
        private CategoriaMovimiento? _filtroCategoria;
        private bool _filtroOtros;
        private string _busqueda = string.Empty;
        private string _categoriaFiltroActiva = string.Empty;

        // KPIs
        private decimal _balanceGlobal;
        private decimal _totalIngresos;
        private decimal _totalEgresos;
        private decimal _totalInvertido;
        private decimal _gananciaAcumulada;
        private decimal _gananciaDistribuidor;
        private decimal _gastoProductos;
        private decimal _gastoTransporte;
        private decimal _gastoRemesas;
        private decimal _gastoRebaja;
        private decimal _gastoAdicional;
        private decimal _gastoOtros;
        private bool _hayPerdida;
        private decimal _tasaCambio = 300m;

        // Form
        private bool _mostrarFormulario;
        private bool _editando;
        private int _editandoId;
        private string _formConcepto = string.Empty;
        private string _formDescripcion = string.Empty;
        private decimal _formMonto;
        private DateTime _formFecha = DateTime.Now;
        private TipoMovimiento _formTipo = TipoMovimiento.Ingreso;
        private CategoriaMovimiento _formCategoria = CategoriaMovimiento.Otro;
        private string _mensajeEstado = string.Empty;

        // Fichas
        private ObservableCollection<FichaResumenFinanciero> _fichasResumen = new();

        #endregion

        #region Properties — KPIs

        public decimal BalanceGlobal
        {
            get => _balanceGlobal;
            set => SetProperty(ref _balanceGlobal, value);
        }

        public decimal TotalIngresos
        {
            get => _totalIngresos;
            set => SetProperty(ref _totalIngresos, value);
        }

        public decimal TotalEgresos
        {
            get => _totalEgresos;
            set => SetProperty(ref _totalEgresos, value);
        }

        public decimal TotalInvertido
        {
            get => _totalInvertido;
            set => SetProperty(ref _totalInvertido, value);
        }

        public decimal GananciaAcumulada
        {
            get => _gananciaAcumulada;
            set => SetProperty(ref _gananciaAcumulada, value);
        }

        public decimal GananciaDistribuidor
        {
            get => _gananciaDistribuidor;
            set => SetProperty(ref _gananciaDistribuidor, value);
        }

        public decimal GastoProductos
        {
            get => _gastoProductos;
            set => SetProperty(ref _gastoProductos, value);
        }

        public decimal GastoTransporte
        {
            get => _gastoTransporte;
            set => SetProperty(ref _gastoTransporte, value);
        }

        public decimal GastoRemesas
        {
            get => _gastoRemesas;
            set => SetProperty(ref _gastoRemesas, value);
        }

        public decimal GastoRebaja
        {
            get => _gastoRebaja;
            set => SetProperty(ref _gastoRebaja, value);
        }

        public decimal GastoAdicional
        {
            get => _gastoAdicional;
            set => SetProperty(ref _gastoAdicional, value);
        }

        public decimal GastoOtros
        {
            get => _gastoOtros;
            set => SetProperty(ref _gastoOtros, value);
        }

        public bool HayPerdida
        {
            get => _hayPerdida;
            set => SetProperty(ref _hayPerdida, value);
        }

        /// <summary>
        /// Tasa de cambio actual: 1 USD = X CUP.
        /// </summary>
        public decimal TasaCambio
        {
            get => _tasaCambio;
            set => SetProperty(ref _tasaCambio, value);
        }

        #endregion

        #region Properties — Collections & Selection

        public ObservableCollection<Movimiento> Movimientos
        {
            get => _movimientosFiltrados;
            set => SetProperty(ref _movimientosFiltrados, value);
        }

        public Movimiento? MovimientoSeleccionado
        {
            get => _movimientoSeleccionado;
            set
            {
                SetProperty(ref _movimientoSeleccionado, value);
                OnPropertyChanged(nameof(HaySeleccion));
            }
        }

        public bool HaySeleccion => MovimientoSeleccionado != null;

        public ObservableCollection<FichaResumenFinanciero> FichasResumen
        {
            get => _fichasResumen;
            set => SetProperty(ref _fichasResumen, value);
        }

        #endregion

        #region Properties — Filters

        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set { if (SetProperty(ref _fechaDesde, value)) _ = CargarMovimientosAsync(); }
        }

        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set { if (SetProperty(ref _fechaHasta, value)) _ = CargarMovimientosAsync(); }
        }

        public TipoMovimiento? FiltroTipo
        {
            get => _filtroTipo;
            set { SetProperty(ref _filtroTipo, value); FiltrarMovimientos(); }
        }

        public CategoriaMovimiento? FiltroCategoria
        {
            get => _filtroCategoria;
            set { SetProperty(ref _filtroCategoria, value); FiltrarMovimientos(); }
        }

        public string CategoriaFiltroActiva
        {
            get => _categoriaFiltroActiva;
            set => SetProperty(ref _categoriaFiltroActiva, value);
        }

        public string Busqueda
        {
            get => _busqueda;
            set { SetProperty(ref _busqueda, value); FiltrarMovimientos(); }
        }

        public Array TiposMovimiento => Enum.GetValues(typeof(TipoMovimiento));
        public Array Categorias => Enum.GetValues(typeof(CategoriaMovimiento));

        #endregion

        #region Properties — Form

        public bool MostrarFormulario
        {
            get => _mostrarFormulario;
            set => SetProperty(ref _mostrarFormulario, value);
        }

        public bool Editando
        {
            get => _editando;
            set => SetProperty(ref _editando, value);
        }

        public string TituloFormulario => Editando ? "Editar Movimiento"
            : FormCategoria == CategoriaMovimiento.GastoAdicional ? "Gasto Adicional"
            : (FormTipo == TipoMovimiento.Ingreso ? "Nuevo Ingreso" : "Nuevo Egreso");

        public string FormConcepto
        {
            get => _formConcepto;
            set => SetProperty(ref _formConcepto, value);
        }

        public string FormDescripcion
        {
            get => _formDescripcion;
            set => SetProperty(ref _formDescripcion, value);
        }

        public decimal FormMonto
        {
            get => _formMonto;
            set => SetProperty(ref _formMonto, value);
        }

        public DateTime FormFecha
        {
            get => _formFecha;
            set => SetProperty(ref _formFecha, value);
        }

        public TipoMovimiento FormTipo
        {
            get => _formTipo;
            set { SetProperty(ref _formTipo, value); OnPropertyChanged(nameof(TituloFormulario)); }
        }

        public CategoriaMovimiento FormCategoria
        {
            get => _formCategoria;
            set => SetProperty(ref _formCategoria, value);
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        #endregion

        #region Commands

        public ICommand NuevoIngresoCommand { get; }
        public ICommand NuevoEgresoCommand { get; }
        public ICommand NuevoGastoAdicionalCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand LimpiarFiltrosCommand { get; }
        public ICommand FiltrarPorCategoriaCommand { get; }

        #endregion

        public MovimientosViewModel(
            IMovimientoService movimientoService,
            IFichaCostoService fichaCostoService,
            IConfiguracionService configuracionService)
        {
            _movimientoService = movimientoService;
            _fichaCostoService = fichaCostoService;
            _configuracionService = configuracionService;

            _fechaDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            _fechaHasta = DateTime.Now.Date;

            NuevoIngresoCommand = new RelayCommand(_ => PrepararNuevo(TipoMovimiento.Ingreso));
            NuevoEgresoCommand = new RelayCommand(_ => PrepararNuevo(TipoMovimiento.Egreso));
            NuevoGastoAdicionalCommand = new RelayCommand(_ => PrepararGastoAdicional());
            GuardarCommand = new RelayCommand(async _ => await GuardarAsync(),
                _ => !string.IsNullOrWhiteSpace(FormDescripcion) && FormMonto > 0);
            CancelarCommand = new RelayCommand(_ => CerrarFormulario());
            EditarCommand = new RelayCommand(_ => PrepararEdicion(), _ => MovimientoSeleccionado != null);
            EliminarCommand = new RelayCommand(async _ => await EliminarAsync(), _ => MovimientoSeleccionado != null);
            LimpiarFiltrosCommand = new RelayCommand(_ => LimpiarFiltros());
            FiltrarPorCategoriaCommand = new RelayCommand(param => FiltrarPorCategoria(param as string));
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            _ = CargarMovimientosAsync();
        }

        #region Core data loading

        private async Task CargarMovimientosAsync()
        {
            try
            {
                _todosMovimientos = await _movimientoService.ObtenerPorRangoFechaAsync(FechaDesde, FechaHasta);
                FiltrarMovimientos();
                await ActualizarKPIsAsync();
                await CargarFichasResumenAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error al cargar: {ex.Message}";
            }
        }

        private void FiltrarMovimientos()
        {
            var filtrados = _todosMovimientos.AsEnumerable();

            if (FiltroTipo.HasValue)
                filtrados = filtrados.Where(m => m.Tipo == FiltroTipo.Value);

            if (FiltroCategoria.HasValue)
                filtrados = filtrados.Where(m => m.Categoria == FiltroCategoria.Value);

            if (_filtroOtros)
            {
                var categoriasConocidas = new[] { CategoriaMovimiento.CompraProducto, CategoriaMovimiento.Transporte, CategoriaMovimiento.Remesa, CategoriaMovimiento.DescontarEntrega, CategoriaMovimiento.GastoAdicional };
                filtrados = filtrados.Where(m => m.Tipo == TipoMovimiento.Egreso && !categoriasConocidas.Contains(m.Categoria));
            }

            if (!string.IsNullOrWhiteSpace(Busqueda))
            {
                var q = Busqueda.ToLowerInvariant();
                filtrados = filtrados.Where(m =>
                    (m.Concepto?.ToLowerInvariant().Contains(q) ?? false) ||
                    (m.Descripcion?.ToLowerInvariant().Contains(q) ?? false));
            }

            Movimientos = new ObservableCollection<Movimiento>(filtrados);
        }

        private async Task ActualizarKPIsAsync()
        {
            try
            {
                // Cargar tasa de cambio
                var config = await _configuracionService.ObtenerConfiguracionAsync();
                TasaCambio = config.TasaCambioCUP > 0 ? config.TasaCambioCUP : 300m;

                // Balance global en CUP (todas las fechas)
                BalanceGlobal = await _movimientoService.ObtenerBalanceActualAsync();
                TotalInvertido = await _movimientoService.ObtenerTotalInvertidoAsync();

                // Resumen del rango filtrado (en CUP)
                var (ingresos, egresos) = await _movimientoService.ObtenerResumenRangoAsync(FechaDesde, FechaHasta);
                TotalIngresos = ingresos;
                TotalEgresos = egresos;

                // Desglose por categoría en rango (CUP)
                var movRango = _todosMovimientos;
                GastoProductos = movRango.Where(m => m.Categoria == CategoriaMovimiento.CompraProducto && m.Tipo == TipoMovimiento.Egreso).Sum(m => m.Monto);
                GastoTransporte = movRango.Where(m => m.Categoria == CategoriaMovimiento.Transporte && m.Tipo == TipoMovimiento.Egreso).Sum(m => m.Monto);
                GastoRemesas = movRango.Where(m => m.Categoria == CategoriaMovimiento.Remesa && m.Tipo == TipoMovimiento.Egreso).Sum(m => m.Monto);
                GastoRebaja = movRango.Where(m => m.Categoria == CategoriaMovimiento.DescontarEntrega && m.Tipo == TipoMovimiento.Egreso).Sum(m => m.Monto);
                GastoAdicional = movRango.Where(m => m.Categoria == CategoriaMovimiento.GastoAdicional && m.Tipo == TipoMovimiento.Egreso).Sum(m => m.Monto);
                // Otros = todo egreso que NO sea Productos, Transporte, Rebaja, Remesas ni GastoAdicional.
                // Así la suma de las 6 tarjetas = TotalEgresos exactamente.
                var categoriasConocidas = new[] { CategoriaMovimiento.CompraProducto, CategoriaMovimiento.Transporte, CategoriaMovimiento.Remesa, CategoriaMovimiento.DescontarEntrega, CategoriaMovimiento.GastoAdicional };
                GastoOtros = movRango.Where(m => m.Tipo == TipoMovimiento.Egreso && !categoriasConocidas.Contains(m.Categoria)).Sum(m => m.Monto);

                // Ganancia acumulada en USD:
                // Por cada ficha: (PrecioVentaUSD × TasaCambio − CostoProductosCUP − TransporteCUP) / TasaCambio
                var fichas = await _fichaCostoService.ObtenerTodosAsync();
                decimal gananciaTotal = 0;
                foreach (var f in fichas)
                {
                    var costoCUP = (f.Productos?.Sum(p => p.Total) ?? 0) + f.CostoTransportacion;
                    var ventaCUP = f.PrecioVentaUSD * TasaCambio;
                    var gananciaUSD = (ventaCUP - costoCUP) / TasaCambio;
                    gananciaTotal += gananciaUSD;
                }
                GananciaAcumulada = gananciaTotal;

                // Ganancia distribuidor: 12% del precio del combo (USD) de cada ficha
                GananciaDistribuidor = fichas.Sum(f => f.PrecioVentaUSD * 0.12m);

                HayPerdida = GananciaAcumulada < 0;
            }
            catch (Exception ex)
            {
                MensajeEstado = $"Error KPIs: {ex.Message}";
            }
        }

        private async Task CargarFichasResumenAsync()
        {
            try
            {
                var fichas = await _fichaCostoService.ObtenerTodosAsync();
                var tasa = TasaCambio > 0 ? TasaCambio : 300m;
                var resumen = fichas
                    .Where(f => f.FechaEnvio >= FechaDesde && f.FechaEnvio <= FechaHasta.AddDays(1))
                    .Select(f =>
                    {
                        var costoProductosCUP = f.Productos?.Sum(p => p.Total) ?? 0;
                        var costoTransporteCUP = f.CostoTransportacion;
                        var costoTotalCUP = costoProductosCUP + costoTransporteCUP;
                        var ventaCUP = f.PrecioVentaUSD * tasa;
                        var gananciaCUP = ventaCUP - costoTotalCUP;
                        var gananciaUSD = gananciaCUP / tasa;

                        return new FichaResumenFinanciero
                        {
                            NumeroFicha = f.NumeroFicha,
                            Receptor = f.NombreReceptor,
                            Agencia = f.NombreAgencia,
                            FechaEnvio = f.FechaEnvio,
                            PrecioVentaUSD = f.PrecioVentaUSD,
                            PrecioVentaCUP = ventaCUP,
                            CostoProductosCUP = costoProductosCUP,
                            CostoTransporteCUP = costoTransporteCUP,
                            CostoTotalCUP = costoTotalCUP,
                            GananciaCUP = gananciaCUP,
                            GananciaUSD = gananciaUSD,
                            GananciaDistribuidor = f.PrecioVentaUSD * 0.12m,
                            HayPerdida = gananciaUSD < 0
                        };
                    })
                    .OrderByDescending(f => f.FechaEnvio)
                    .ToList();

                FichasResumen = new ObservableCollection<FichaResumenFinanciero>(resumen);
            }
            catch { }
        }

        #endregion

        #region Form actions

        private void PrepararNuevo(TipoMovimiento tipo)
        {
            Editando = false;
            _editandoId = 0;
            FormConcepto = string.Empty;
            FormDescripcion = string.Empty;
            FormMonto = 0;
            FormFecha = DateTime.Now;
            FormTipo = tipo;
            FormCategoria = CategoriaMovimiento.Otro;
            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void PrepararGastoAdicional()
        {
            Editando = false;
            _editandoId = 0;
            FormConcepto = string.Empty;
            FormDescripcion = string.Empty;
            FormMonto = 0;
            FormFecha = DateTime.Now;
            FormTipo = TipoMovimiento.Egreso;
            FormCategoria = CategoriaMovimiento.GastoAdicional;
            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private void PrepararEdicion()
        {
            if (MovimientoSeleccionado == null) return;
            Editando = true;
            _editandoId = MovimientoSeleccionado.Id;
            FormDescripcion = MovimientoSeleccionado.Descripcion ?? MovimientoSeleccionado.Concepto;
            FormMonto = MovimientoSeleccionado.Monto;
            FormFecha = MovimientoSeleccionado.Fecha;
            FormTipo = MovimientoSeleccionado.Tipo;
            MostrarFormulario = true;
            OnPropertyChanged(nameof(TituloFormulario));
        }

        private async Task GuardarAsync()
        {
            try
            {
                if (Editando)
                {
                    var mov = await _movimientoService.ObtenerPorIdAsync(_editandoId);
                    if (mov == null) return;
                    mov.Concepto = FormDescripcion.Trim();
                    mov.Descripcion = FormDescripcion.Trim();
                    mov.Monto = FormMonto;
                    mov.Fecha = FormFecha;
                    mov.Tipo = FormTipo;
                    await _movimientoService.ActualizarAsync(mov);
                    MensajeEstado = $"✅ Movimiento actualizado — {FormMonto:N0} CUP";
                }
                else
                {
                    var mov = new Movimiento
                    {
                        Concepto = FormDescripcion.Trim(),
                        Descripcion = FormDescripcion.Trim(),
                        Monto = FormMonto,
                        Fecha = FormFecha,
                        Tipo = FormTipo,
                        Categoria = FormCategoria
                    };
                    await _movimientoService.CrearAsync(mov);
                    var icono = FormTipo == TipoMovimiento.Ingreso ? "📥" : "📤";
                    MensajeEstado = $"✅ {icono} {FormTipo} registrado — {FormMonto:N0} CUP";
                }

                CerrarFormulario();
                await CargarMovimientosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        private async Task EliminarAsync()
        {
            if (MovimientoSeleccionado == null) return;

            var mov = MovimientoSeleccionado;
            var esAutomatico = mov.Categoria != CategoriaMovimiento.Otro
                            && mov.Categoria != CategoriaMovimiento.FondoInicial;

            // Advertir si es un movimiento generado automáticamente
            string mensaje;
            if (esAutomatico)
            {
                mensaje = $"⚠️ Este movimiento fue generado automáticamente ({mov.Categoria}).\n\n" +
                          $"¿Eliminar \"{mov.Concepto}\" por {mov.Monto:N0} CUP?\n\n" +
                          $"El efecto financiero se revertirá pero el inventario NO se restaurará.\n" +
                          $"Para restaurar inventario, elimine la entrega o ficha asociada.";
            }
            else
            {
                mensaje = $"¿Eliminar movimiento \"{mov.Concepto}\" por {mov.Monto:N0} CUP?";
            }

            var result = MessageBox.Show(
                mensaje,
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _movimientoService.EliminarAsync(mov.Id);
                MensajeEstado = "🗑️ Movimiento eliminado";
                MovimientoSeleccionado = null;
                await CargarMovimientosAsync();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
        }

        private void CerrarFormulario()
        {
            MostrarFormulario = false;
            FormConcepto = string.Empty;
            FormDescripcion = string.Empty;
            FormMonto = 0;
        }

        private void LimpiarFiltros()
        {
            _filtroTipo = null;
            _filtroCategoria = null;
            _filtroOtros = false;
            _busqueda = string.Empty;
            CategoriaFiltroActiva = string.Empty;
            OnPropertyChanged(nameof(FiltroTipo));
            OnPropertyChanged(nameof(FiltroCategoria));
            OnPropertyChanged(nameof(Busqueda));
            FiltrarMovimientos();
        }

        /// <summary>
        /// Filtra la tabla de movimientos por categoría al hacer clic en una KPI card.
        /// Un segundo clic en la misma categoría limpia el filtro.
        /// </summary>
        private void FiltrarPorCategoria(string? categoria)
        {
            if (string.IsNullOrEmpty(categoria)) return;

            CategoriaMovimiento? cat = categoria switch
            {
                "Ingresos" => null,   // caso especial: filtra por tipo Ingreso
                "Egresos" => null,    // caso especial: filtra por tipo Egreso
                "Productos" => CategoriaMovimiento.CompraProducto,
                "Transporte" => CategoriaMovimiento.Transporte,
                "Rebaja" => CategoriaMovimiento.DescontarEntrega,
                "Remesas" => CategoriaMovimiento.Remesa,
                "GastoAdicional" => CategoriaMovimiento.GastoAdicional,
                "Otros" => null,      // caso especial: filtro residual
                _ => null
            };

            // Casos especiales para Ingresos/Egresos (filtra por tipo, no por categoría)
            if (categoria == "Ingresos")
            {
                _filtroOtros = false;
                if (FiltroTipo == TipoMovimiento.Ingreso)
                {
                    FiltroTipo = null; // Toggle off
                    CategoriaFiltroActiva = string.Empty;
                }
                else
                {
                    FiltroCategoria = null;
                    FiltroTipo = TipoMovimiento.Ingreso;
                    CategoriaFiltroActiva = categoria;
                }
                return;
            }
            if (categoria == "Egresos")
            {
                _filtroOtros = false;
                if (FiltroTipo == TipoMovimiento.Egreso)
                {
                    FiltroTipo = null;
                    CategoriaFiltroActiva = string.Empty;
                }
                else
                {
                    FiltroCategoria = null;
                    FiltroTipo = TipoMovimiento.Egreso;
                    CategoriaFiltroActiva = categoria;
                }
                return;
            }

            // Caso especial: "Otros" = filtro residual (todo egreso que no es las 4 categorías conocidas)
            if (categoria == "Otros")
            {
                if (_filtroOtros)
                {
                    _filtroOtros = false;
                    FiltroTipo = null;
                    CategoriaFiltroActiva = string.Empty;
                }
                else
                {
                    FiltroCategoria = null;
                    _filtroOtros = true;
                    FiltroTipo = null; // el filtro residual ya filtra por Egreso internamente
                    CategoriaFiltroActiva = categoria;
                    FiltrarMovimientos();
                }
                return;
            }

            // Toggle: si ya está filtrado por esta categoría, limpiar filtro
            _filtroOtros = false; // limpiar filtro residual si se selecciona otra categoría
            if (FiltroCategoria == cat)
            {
                FiltroCategoria = null;
                FiltroTipo = null;
                CategoriaFiltroActiva = string.Empty;
            }
            else
            {
                FiltroTipo = TipoMovimiento.Egreso; // todas estas categorías son egresos
                FiltroCategoria = cat;
                CategoriaFiltroActiva = categoria;
            }
        }

        #endregion
    }

    /// <summary>
    /// DTO para resumen financiero por ficha de costo.
    /// </summary>
    public class FichaResumenFinanciero
    {
        public string NumeroFicha { get; set; } = string.Empty;
        public string Receptor { get; set; } = string.Empty;
        public string Agencia { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }

        /// <summary>Precio de venta en USD.</summary>
        public decimal PrecioVentaUSD { get; set; }

        /// <summary>Precio de venta convertido a CUP (PrecioVentaUSD × TasaCambio).</summary>
        public decimal PrecioVentaCUP { get; set; }

        /// <summary>Costo de productos en CUP.</summary>
        public decimal CostoProductosCUP { get; set; }

        /// <summary>Costo de transporte en CUP.</summary>
        public decimal CostoTransporteCUP { get; set; }

        /// <summary>Costo total en CUP (productos + transporte).</summary>
        public decimal CostoTotalCUP { get; set; }

        /// <summary>Ganancia en CUP (VentaCUP - CostoTotalCUP).</summary>
        public decimal GananciaCUP { get; set; }

        /// <summary>Ganancia real en USD (GananciaCUP / TasaCambio).</summary>
        public decimal GananciaUSD { get; set; }

        /// <summary>Ganancia del distribuidor: 12% del precio USD.</summary>
        public decimal GananciaDistribuidor { get; set; }

        public bool HayPerdida { get; set; }
    }
}
