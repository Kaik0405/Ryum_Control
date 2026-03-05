using System.Collections.ObjectModel;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para el Dashboard principal.
    /// Muestra resumen financiero, alertas de inventario, entregas urgentes y actividad reciente.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IProductoService _productoService;
        private readonly IEntregaService _entregaService;
        private readonly IMovimientoService _movimientoService;
        private readonly IFichaCostoService _fichaCostoService;
        private readonly IConfiguracionService _configuracionService;

        #region Fields

        private decimal _balanceActual;
        private decimal _ingresosDelMes;
        private decimal _egresosDelMes;
        private int _entregasPendientesCount;
        private int _remesasPendientesCount;
        private decimal _remesasPendientesTotal;
        private int _totalProductos;
        private int _productosSinStock;
        private int _ventasDelMes;
        private string _mesActual = string.Empty;
        private string _saludo = string.Empty;
        private bool _cargando;

        private ObservableCollection<AlertaStockItem> _alertasStock = new();
        private ObservableCollection<EntregaResumen> _entregasUrgentes = new();
        private ObservableCollection<Movimiento> _movimientosRecientes = new();
        private ObservableCollection<DestinoPendiente> _destinosPendientes = new();
        private ObservableCollection<EntregaResumen> _remesasPendientes = new();

        #endregion

        #region Properties — KPI

        public decimal BalanceActual
        {
            get => _balanceActual;
            set => SetProperty(ref _balanceActual, value);
        }

        public decimal IngresosDelMes
        {
            get => _ingresosDelMes;
            set => SetProperty(ref _ingresosDelMes, value);
        }

        public decimal EgresosDelMes
        {
            get => _egresosDelMes;
            set => SetProperty(ref _egresosDelMes, value);
        }

        public int EntregasPendientesCount
        {
            get => _entregasPendientesCount;
            set => SetProperty(ref _entregasPendientesCount, value);
        }

        public int RemesasPendientesCount
        {
            get => _remesasPendientesCount;
            set => SetProperty(ref _remesasPendientesCount, value);
        }

        public decimal RemesasPendientesTotal
        {
            get => _remesasPendientesTotal;
            set => SetProperty(ref _remesasPendientesTotal, value);
        }

        public int TotalProductos
        {
            get => _totalProductos;
            set => SetProperty(ref _totalProductos, value);
        }

        public int ProductosSinStock
        {
            get => _productosSinStock;
            set => SetProperty(ref _productosSinStock, value);
        }

        public int VentasDelMes
        {
            get => _ventasDelMes;
            set => SetProperty(ref _ventasDelMes, value);
        }

        public string MesActual
        {
            get => _mesActual;
            set => SetProperty(ref _mesActual, value);
        }

        public string Saludo
        {
            get => _saludo;
            set => SetProperty(ref _saludo, value);
        }

        public bool Cargando
        {
            get => _cargando;
            set => SetProperty(ref _cargando, value);
        }

        #endregion

        #region Properties — Collections

        public ObservableCollection<AlertaStockItem> AlertasStock
        {
            get => _alertasStock;
            set => SetProperty(ref _alertasStock, value);
        }

        public ObservableCollection<EntregaResumen> EntregasUrgentes
        {
            get => _entregasUrgentes;
            set => SetProperty(ref _entregasUrgentes, value);
        }

        public ObservableCollection<Movimiento> MovimientosRecientes
        {
            get => _movimientosRecientes;
            set => SetProperty(ref _movimientosRecientes, value);
        }

        public ObservableCollection<DestinoPendiente> DestinosPendientes
        {
            get => _destinosPendientes;
            set => SetProperty(ref _destinosPendientes, value);
        }

        public ObservableCollection<EntregaResumen> RemesasPendientes
        {
            get => _remesasPendientes;
            set => SetProperty(ref _remesasPendientes, value);
        }

        #endregion

        #region Commands

        public ICommand RefrescarCommand { get; }

        #endregion

        public DashboardViewModel(
            IProductoService productoService,
            IEntregaService entregaService,
            IMovimientoService movimientoService,
            IFichaCostoService fichaCostoService,
            IConfiguracionService configuracionService)
        {
            _productoService = productoService;
            _entregaService = entregaService;
            _movimientoService = movimientoService;
            _fichaCostoService = fichaCostoService;
            _configuracionService = configuracionService;

            RefrescarCommand = new RelayCommand(async _ => await CargarDatosAsync());

            ActualizarSaludo();
        }

        public override void OnNavigatedTo(object? parameter = null)
        {
            base.OnNavigatedTo(parameter);
            ActualizarSaludo();
            _ = CargarDatosAsync();
        }

        private void ActualizarSaludo(string? nombre = null)
        {
            var hora = DateTime.Now.Hour;
            var periodo = hora switch
            {
                < 12 => "Buenos días",
                < 18 => "Buenas tardes",
                _ => "Buenas noches"
            };

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var primerNombre = nombre.Trim().Split(' ')[0];
                Saludo = $"{periodo}, {primerNombre} 👋";
            }
            else
            {
                Saludo = $"{periodo} 👋";
            }
            MesActual = DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("es-ES"));
        }

        /// <summary>
        /// Carga todos los datos del dashboard desde los servicios.
        /// </summary>
        private async Task CargarDatosAsync()
        {
            Cargando = true;
            try
            {
                var ahora = DateTime.Now;
                var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                // Cargar en paralelo los datos independientes
                // Cargar nombre del distribuidor para el saludo
                var configTask = _configuracionService.ObtenerConfiguracionAsync();
                var balanceTask = _movimientoService.ObtenerBalanceActualAsync();
                var resumenTask = _movimientoService.ObtenerResumenRangoAsync(inicioMes, finMes);
                var productosTask = _productoService.ObtenerTodosAsync();
                var entregasTask = _entregaService.ObtenerTodosAsync();
                var movimientosTask = _movimientoService.ObtenerPorRangoFechaAsync(inicioMes, finMes);
                var fichasTask = _fichaCostoService.ObtenerPorRangoFechaAsync(inicioMes, finMes);

                await Task.WhenAll(configTask, balanceTask, resumenTask, productosTask, entregasTask, movimientosTask, fichasTask);

                // Actualizar saludo con nombre
                var config = await configTask;
                ActualizarSaludo(config.Nombre);

                // ═══ KPIs Financieros ═══
                BalanceActual = await balanceTask;
                var (ingresos, egresos) = await resumenTask;
                IngresosDelMes = ingresos;
                EgresosDelMes = egresos;

                // ═══ Fichas del mes ═══
                var fichas = await fichasTask;
                VentasDelMes = fichas.Count;

                // ═══ Productos ═══
                var productos = await productosTask;
                TotalProductos = productos.Count;
                ProductosSinStock = productos.Count(p => !p.EnStock || p.CantidadStock <= 0);

                // Alertas de stock: sin stock + bajo stock (≤ 5 unidades)
                var alertas = new List<AlertaStockItem>();
                foreach (var p in productos.Where(p => p.Activo).OrderBy(p => p.CantidadStock))
                {
                    if (p.CantidadStock <= 0)
                    {
                        alertas.Add(new AlertaStockItem
                        {
                            Nombre = p.Nombre,
                            CantidadStock = p.CantidadStock,
                            Unidad = p.Unidad.ToString(),
                            EsCritico = true,
                            Icono = "🔴",
                            ColorFondo = "#FFEBEE",
                            ColorTexto = "#C62828"
                        });
                    }
                    else if (p.CantidadStock <= 5)
                    {
                        alertas.Add(new AlertaStockItem
                        {
                            Nombre = p.Nombre,
                            CantidadStock = p.CantidadStock,
                            Unidad = p.Unidad.ToString(),
                            EsCritico = false,
                            Icono = "🟡",
                            ColorFondo = "#FFF3E0",
                            ColorTexto = "#E65100"
                        });
                    }
                }
                AlertasStock = new ObservableCollection<AlertaStockItem>(alertas);

                // ═══ Entregas ═══
                var entregas = await entregasTask;
                var pendientes = entregas.Where(e => !e.Entregada).ToList();
                var entregasCombos = pendientes.Where(e => !e.EsRemesa).ToList();
                var remesas = pendientes.Where(e => e.EsRemesa).ToList();

                EntregasPendientesCount = entregasCombos.Count;
                RemesasPendientesCount = remesas.Count;
                RemesasPendientesTotal = remesas.Sum(r => r.MontoRemesa);

                // Entregas urgentes (más urgentes primero, máximo 8)
                var urgentes = entregasCombos
                    .OrderBy(e => e.DiasRestantes)
                    .Take(8)
                    .Select(e => new EntregaResumen
                    {
                        NumeroOrden = e.NumeroOrden,
                        NombreReceptor = e.NombreReceptor,
                        Direccion = e.DireccionResumida,
                        DiasRestantes = e.DiasRestantes,
                        Agencia = e.Agencia,
                        EsRemesa = false,
                        ColorFondo = e.Vencida ? "#FFEBEE" : e.Urgente ? "#FFF3E0" : "#E3F2FD",
                        ColorTexto = e.Vencida ? "#C62828" : e.Urgente ? "#E65100" : "#1565C0",
                        Icono = e.Vencida ? "🔴" : e.Urgente ? "🟠" : "🔵",
                        EstadoTexto = e.Vencida ? "VENCIDA" : e.Urgente ? $"{e.DiasRestantes}d — Urgente" : $"{e.DiasRestantes} días"
                    })
                    .ToList();
                EntregasUrgentes = new ObservableCollection<EntregaResumen>(urgentes);

                // Remesas pendientes
                var remesasResumen = remesas
                    .OrderBy(e => e.DiasRestantes)
                    .Take(6)
                    .Select(e => new EntregaResumen
                    {
                        NumeroOrden = e.NumeroOrden,
                        NombreReceptor = e.NombreReceptor,
                        Direccion = e.DireccionResumida,
                        DiasRestantes = e.DiasRestantes,
                        Agencia = e.Agencia,
                        EsRemesa = true,
                        MontoRemesa = e.MontoRemesa,
                        ColorFondo = "#F3E5F5",
                        ColorTexto = "#6A1B9A",
                        Icono = "💵",
                        EstadoTexto = $"{e.MontoRemesa:N0} CUP"
                    })
                    .ToList();
                RemesasPendientes = new ObservableCollection<EntregaResumen>(remesasResumen);

                // ═══ Destinos Pendientes (agrupados por provincia) ═══
                var destinos = pendientes
                    .Where(e => !string.IsNullOrWhiteSpace(e.Provincia))
                    .GroupBy(e => e.Provincia)
                    .Select(g => new DestinoPendiente
                    {
                        Provincia = g.Key,
                        Municipios = string.Join(", ", g.Select(e => e.Municipio).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().Take(3)),
                        Cantidad = g.Count(),
                        TieneUrgente = g.Any(e => e.Vencida || e.Urgente)
                    })
                    .OrderByDescending(d => d.TieneUrgente)
                    .ThenByDescending(d => d.Cantidad)
                    .ToList();
                DestinosPendientes = new ObservableCollection<DestinoPendiente>(destinos);

                // ═══ Movimientos Recientes (últimos 8 del mes) ═══
                var movimientos = await movimientosTask;
                var recientes = movimientos
                    .OrderByDescending(m => m.Fecha)
                    .Take(8)
                    .ToList();
                MovimientosRecientes = new ObservableCollection<Movimiento>(recientes);
            }
            catch
            {
                // Silenciar errores de carga inicial
            }
            finally
            {
                Cargando = false;
            }
        }
    }

    // ═══════════════════════════════════════════════════
    // MODELOS AUXILIARES DEL DASHBOARD
    // ═══════════════════════════════════════════════════

    /// <summary>
    /// Alerta de producto con stock bajo o sin stock.
    /// </summary>
    public class AlertaStockItem
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal CantidadStock { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public bool EsCritico { get; set; }
        public string Icono { get; set; } = string.Empty;
        public string ColorFondo { get; set; } = string.Empty;
        public string ColorTexto { get; set; } = string.Empty;

        public string Detalle => EsCritico
            ? "Sin stock"
            : $"{CantidadStock:G} {Unidad} restantes";
    }

    /// <summary>
    /// Resumen de una entrega o remesa pendiente.
    /// </summary>
    public class EntregaResumen
    {
        public string NumeroOrden { get; set; } = string.Empty;
        public string NombreReceptor { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int DiasRestantes { get; set; }
        public string Agencia { get; set; } = string.Empty;
        public bool EsRemesa { get; set; }
        public decimal MontoRemesa { get; set; }
        public string ColorFondo { get; set; } = string.Empty;
        public string ColorTexto { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string EstadoTexto { get; set; } = string.Empty;
    }

    /// <summary>
    /// Destino de entrega agrupado por provincia.
    /// </summary>
    public class DestinoPendiente
    {
        public string Provincia { get; set; } = string.Empty;
        public string Municipios { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public bool TieneUrgente { get; set; }
    }
}
