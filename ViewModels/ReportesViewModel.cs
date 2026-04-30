using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GestionApp.Helpers;
using GestionApp.Models;
using GestionApp.Services;

namespace GestionApp.ViewModels
{
    /// <summary>
    /// ViewModel para la generación de reportes PDF.
    /// Soporta rango de fechas personalizado y tipos de reporte.
    /// </summary>
    public class ReportesViewModel : BaseViewModel
    {
        private readonly IFichaCostoService _fichaCostoService;
        private readonly IConfiguracionService _configuracionService;
        private readonly IMovimientoService _movimientoService;
        private readonly IPeriodoInventarioService _periodoService;
        private readonly IProductoService _productoService;
        private readonly ReportesPdfService _pdfService;

        private DateTime _fechaDesde;
        private DateTime _fechaHasta;
        private bool _generandoReporte;
        private string _mensajeEstado = string.Empty;
        private string? _ultimoArchivo;

        #region Properties

        public DateTime FechaDesde
        {
            get => _fechaDesde;
            set { SetProperty(ref _fechaDesde, value); ActualizarResumen(); }
        }

        public DateTime FechaHasta
        {
            get => _fechaHasta;
            set { SetProperty(ref _fechaHasta, value); ActualizarResumen(); }
        }

        public bool GenerandoReporte
        {
            get => _generandoReporte;
            set => SetProperty(ref _generandoReporte, value);
        }

        public string MensajeEstado
        {
            get => _mensajeEstado;
            set => SetProperty(ref _mensajeEstado, value);
        }

        // Resumen previo
        private int _totalFichas;
        public int TotalFichas { get => _totalFichas; set => SetProperty(ref _totalFichas, value); }

        private decimal _totalVentas;
        public decimal TotalVentas { get => _totalVentas; set => SetProperty(ref _totalVentas, value); }

        private decimal _totalGastoProductos;
        public decimal TotalGastoProductos { get => _totalGastoProductos; set => SetProperty(ref _totalGastoProductos, value); }

        private decimal _totalTransporte;
        public decimal TotalTransporte { get => _totalTransporte; set => SetProperty(ref _totalTransporte, value); }

        // Atajos de período
        public ObservableCollection<KeyValuePair<string, string>> AtajosPeriodo { get; } = new()
        {
            new("mes_actual", "📅 Mes actual"),
            new("mes_anterior", "📅 Mes anterior"),
            new("ultimos_3", "📅 Últimos 3 meses"),
            new("año_actual", "📅 Año actual"),
        };

        private string _atajoSeleccionado = "mes_actual";
        public string AtajoSeleccionado
        {
            get => _atajoSeleccionado;
            set
            {
                if (SetProperty(ref _atajoSeleccionado, value))
                    AplicarAtajo(value);
            }
        }

        #endregion

        #region Commands

        public ICommand GenerarEntregasPDFCommand { get; }
        public ICommand GenerarFichasCostoPDFCommand { get; }
        public ICommand GenerarResumenMensualPDFCommand { get; }
        public ICommand AbrirUltimoArchivoCommand { get; }
        public ICommand AbrirCarpetaReportesCommand { get; }
        public ICommand ActualizarResumenCommand { get; }

        #endregion

        public ReportesViewModel(
            IFichaCostoService fichaCostoService,
            IConfiguracionService configuracionService,
            IMovimientoService movimientoService,
            IPeriodoInventarioService periodoService,
            IProductoService productoService,
            ReportesPdfService pdfService)
        {
            _fichaCostoService = fichaCostoService;
            _configuracionService = configuracionService;
            _movimientoService = movimientoService;
            _periodoService = periodoService;
            _productoService = productoService;
            _pdfService = pdfService;

            // Período por defecto: mes actual
            var hoy = DateTime.Now;
            _fechaDesde = new DateTime(hoy.Year, hoy.Month, 1);
            _fechaHasta = hoy;

            GenerarEntregasPDFCommand = new RelayCommand(_ => GenerarEntregasPDF(), _ => !GenerandoReporte);
            GenerarFichasCostoPDFCommand = new RelayCommand(_ => GenerarFichasCostoPDF(), _ => !GenerandoReporte);
            GenerarResumenMensualPDFCommand = new RelayCommand(_ => GenerarResumenMensualPDF(), _ => !GenerandoReporte);
            AbrirUltimoArchivoCommand = new RelayCommand(_ => AbrirUltimoArchivo(), _ => _ultimoArchivo != null);
            AbrirCarpetaReportesCommand = new RelayCommand(_ => AbrirCarpetaReportes());
            ActualizarResumenCommand = new RelayCommand(_ => ActualizarResumen());

            // Cargar resumen inicial
            ActualizarResumen();
        }

        private void AplicarAtajo(string atajo)
        {
            var hoy = DateTime.Now;
            switch (atajo)
            {
                case "mes_actual":
                    _fechaDesde = new DateTime(hoy.Year, hoy.Month, 1);
                    _fechaHasta = hoy;
                    break;
                case "mes_anterior":
                    var mesAnt = hoy.AddMonths(-1);
                    _fechaDesde = new DateTime(mesAnt.Year, mesAnt.Month, 1);
                    _fechaHasta = new DateTime(mesAnt.Year, mesAnt.Month, DateTime.DaysInMonth(mesAnt.Year, mesAnt.Month));
                    break;
                case "ultimos_3":
                    _fechaDesde = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-2);
                    _fechaHasta = hoy;
                    break;
                case "año_actual":
                    _fechaDesde = new DateTime(hoy.Year, 1, 1);
                    _fechaHasta = hoy;
                    break;
            }
            OnPropertyChanged(nameof(FechaDesde));
            OnPropertyChanged(nameof(FechaHasta));
            ActualizarResumen();
        }

        private async void ActualizarResumen()
        {
            try
            {
                var fichas = await _fichaCostoService.ObtenerPorRangoFechaAsync(FechaDesde, FechaHasta);
                TotalFichas = fichas.Count;
                TotalVentas = fichas.Sum(f => f.PrecioVentaUSD);
                TotalGastoProductos = fichas.Sum(f => f.Productos?.Sum(p => p.Total) ?? 0);
                TotalTransporte = fichas.Sum(f => f.CostoTransportacion);
            }
            catch { }
        }

        private async void GenerarEntregasPDF()
        {
            GenerandoReporte = true;
            MensajeEstado = "Generando reporte de entregas...";
            try
            {
                var fichas = await _fichaCostoService.ObtenerPorRangoFechaAsync(FechaDesde, FechaHasta);
                if (fichas.Count == 0)
                {
                    MensajeEstado = "⚠️ No hay fichas de costo en el período seleccionado.";
                    return;
                }

                var config = await _configuracionService.ObtenerConfiguracionAsync();
                var archivo = _pdfService.GenerarReporteEntregas(fichas, FechaDesde, FechaHasta, config.Nombre, config.RutaReportes);
                _ultimoArchivo = archivo;
                MensajeEstado = $"✅ Reporte generado: {Path.GetFileName(archivo)}";

                // Abrir el PDF automáticamente
                AbrirUltimoArchivo();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
            finally
            {
                GenerandoReporte = false;
            }
        }

        private async void GenerarFichasCostoPDF()
        {
            GenerandoReporte = true;
            MensajeEstado = "Generando fichas de costo...";
            try
            {
                var fichas = await _fichaCostoService.ObtenerPorRangoFechaAsync(FechaDesde, FechaHasta);
                if (fichas.Count == 0)
                {
                    MensajeEstado = "⚠️ No hay fichas de costo en el período seleccionado.";
                    return;
                }

                var config = await _configuracionService.ObtenerConfiguracionAsync();
                var archivo = _pdfService.GenerarReporteFichasCosto(fichas, FechaDesde, FechaHasta, config.Nombre, config.RutaReportes);
                _ultimoArchivo = archivo;
                MensajeEstado = $"✅ Fichas generadas: {Path.GetFileName(archivo)}";

                AbrirUltimoArchivo();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
            finally
            {
                GenerandoReporte = false;
            }
        }

        private async void GenerarResumenMensualPDF()
        {
            GenerandoReporte = true;
            MensajeEstado = "Generando resumen mensual...";
            try
            {
                var config = await _configuracionService.ObtenerConfiguracionAsync();

                // 1. Obtener movimientos del período
                var movimientos = await _movimientoService.ObtenerPorRangoFechaAsync(FechaDesde, FechaHasta);

                // 2. Obtener fichas (para salidas de productos)
                var fichas = await _fichaCostoService.ObtenerPorRangoFechaAsync(FechaDesde, FechaHasta);

                // 3. Obtener compras (para entradas de productos)
                var compras = await _productoService.ObtenerComprasPorRangoAsync(FechaDesde, FechaHasta);

                // 4. Obtener período de inventario (para snapshots)
                var periodo = await _periodoService.ObtenerPorMesAsync(FechaDesde.Year, FechaDesde.Month);

                // 5. Productos actuales (para inventario final si no hay snapshot)
                var productos = await _productoService.ObtenerTodosAsync();

                // ── Calcular financiero ──
                var efectivoInicial = movimientos
                    .Where(m => m.Categoria == CategoriaMovimiento.FondoInicial && m.Tipo == TipoMovimiento.Ingreso)
                    .Sum(m => m.Monto);

                if (periodo != null && periodo.FondosIniciales > 0)
                    efectivoInicial = periodo.FondosIniciales;

                var ingresosVentas = movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Ingreso && m.Categoria == CategoriaMovimiento.Venta)
                    .Sum(m => m.Monto);
                var ingresosOtros = movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Ingreso
                        && m.Categoria != CategoriaMovimiento.FondoInicial
                        && m.Categoria != CategoriaMovimiento.Venta)
                    .Sum(m => m.Monto);
                var totalIngresos = ingresosVentas + ingresosOtros;

                var gastoProductos = movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Categoria == CategoriaMovimiento.CompraProducto)
                    .Sum(m => m.Monto);
                var gastoTransporte = movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Categoria == CategoriaMovimiento.Transporte)
                    .Sum(m => m.Monto);
                var gastoRemesas = movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Categoria == CategoriaMovimiento.Remesa)
                    .Sum(m => m.Monto);
                var gastoRebaja = movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Categoria == CategoriaMovimiento.DescontarEntrega)
                    .Sum(m => m.Monto);
                var gastoAdicional = movimientos
                    .Where(m => m.Tipo == TipoMovimiento.Egreso && m.Categoria == CategoriaMovimiento.GastoAdicional)
                    .Sum(m => m.Monto);

                var totalEgresos = gastoProductos + gastoTransporte + gastoRemesas + gastoRebaja + gastoAdicional;
                var efectivoFinal = efectivoInicial + totalIngresos - totalEgresos;

                // ── Inventario Inicial ──
                var inventarioInicial = new List<LineaInventario>();
                if (periodo?.InventarioInicial != null)
                {
                    var snapshots = periodo.InventarioInicial
                        .Where(s => s.Tipo == TipoSnapshot.Inicial && s.Cantidad > 0)
                        .ToList();

                    if (snapshots.Count > 0)
                    {
                        inventarioInicial = snapshots.Select(s => new LineaInventario
                        {
                            Producto = s.NombreProducto,
                            Cantidad = s.Cantidad,
                            Unidad = "",
                            CostoUnitario = s.CostoUnitario,
                            ValorTotal = s.ValorTotal
                        }).ToList();
                    }
                }
                // Fallback: si no hay snapshot inicial, usar stock actual como referencia
                if (inventarioInicial.Count == 0)
                {
                    inventarioInicial = productos
                        .Where(p => p.Activo && (p.CantidadStock > 0 || p.Compras.Any()))
                        .Select(p => new LineaInventario
                        {
                            Producto = p.Nombre,
                            Cantidad = p.CantidadStock,
                            Unidad = p.Unidad.ToString(),
                            CostoUnitario = p.CostoCompra,
                            ValorTotal = p.CostoTotal
                        })
                        .Where(l => l.Cantidad > 0)
                        .ToList();
                }

                // ── Inventario Final ──
                List<LineaInventario> inventarioFinal;
                var snapshotsFinales = periodo?.InventarioInicial?
                    .Where(s => s.Tipo == TipoSnapshot.Final && s.Cantidad > 0)
                    .ToList();

                if (snapshotsFinales != null && snapshotsFinales.Count > 0)
                {
                    inventarioFinal = snapshotsFinales.Select(s => new LineaInventario
                    {
                        Producto = s.NombreProducto,
                        Cantidad = s.Cantidad,
                        Unidad = "",
                        CostoUnitario = s.CostoUnitario,
                        ValorTotal = s.ValorTotal
                    }).ToList();
                }
                else
                {
                    inventarioFinal = productos
                        .Where(p => p.Activo && p.CantidadStock > 0)
                        .Select(p => new LineaInventario
                        {
                            Producto = p.Nombre,
                            Cantidad = p.CantidadStock,
                            Unidad = p.Unidad.ToString(),
                            CostoUnitario = p.CostoCompra,
                            ValorTotal = p.CostoTotal
                        }).ToList();
                }

                // ── Entradas de Productos (Compras) ──
                var entradas = compras.Select(c => new LineaMovimientoProducto
                {
                    Producto = c.Producto?.Nombre ?? "Producto eliminado",
                    Cantidad = c.Cantidad,
                    Unidad = c.Producto?.Unidad.ToString() ?? "",
                    CostoUnitario = c.CostoUnitario,
                    Total = c.Total,
                    Fecha = c.Fecha,
                    Detalle = c.Proveedor ?? ""
                }).ToList();

                // ── Salidas de Productos agrupadas por producto ──
                var salidasAgrupadas = fichas
                    .Where(f => f.InventarioDescontado)
                    .SelectMany(f => (f.Productos ?? Enumerable.Empty<FichaCostoProducto>()).Select(p => new
                    {
                        p.NombreProducto,
                        p.Cantidad,
                        p.Unidad,
                        p.CostoUnitario,
                        p.Total,
                        FichaId = f.Id
                    }))
                    .GroupBy(x => x.NombreProducto)
                    .Select(g => new LineaProductoAgrupado
                    {
                        Producto = g.Key,
                        CantidadTotal = g.Sum(x => x.Cantidad),
                        Unidad = g.First().Unidad.ToString(),
                        CostoPromedio = g.Sum(x => x.Total) / (g.Sum(x => x.Cantidad) == 0 ? 1 : g.Sum(x => x.Cantidad)),
                        ValorTotal = g.Sum(x => x.Total),
                        CantidadFichas = g.Select(x => x.FichaId).Distinct().Count()
                    })
                    .OrderByDescending(x => x.ValorTotal)
                    .ToList();

                // ── Nombre del período ──
                var nombrePeriodo = periodo?.Nombre
                    ?? new DateTime(FechaDesde.Year, FechaDesde.Month, 1).ToString("MMMM yyyy");

                var totalVentasUSD = fichas.Sum(f => f.PrecioVentaUSD);

                var datos = new DatosResumenMensual
                {
                    NombrePeriodo = char.ToUpper(nombrePeriodo[0]) + nombrePeriodo[1..],
                    FechaDesde = FechaDesde,
                    FechaHasta = FechaHasta,
                    EfectivoInicial = efectivoInicial,
                    IngresosVentas = ingresosVentas,
                    IngresosOtros = ingresosOtros,
                    TotalIngresos = totalIngresos,
                    TotalEgresos = totalEgresos,
                    EfectivoFinal = efectivoFinal,
                    GastoProductos = gastoProductos,
                    GastoTransporte = gastoTransporte,
                    GastoRemesas = gastoRemesas,
                    GastoRebaja = gastoRebaja,
                    GastoAdicional = gastoAdicional,
                    TotalFichas = fichas.Count,
                    TotalVentasUSD = totalVentasUSD,
                    TasaCambio = config.TasaCambioCUP,
                    ValorInventarioInicial = inventarioInicial.Sum(l => l.ValorTotal),
                    ValorInventarioFinal = inventarioFinal.Sum(l => l.ValorTotal),
                    InventarioInicial = inventarioInicial,
                    InventarioFinal = inventarioFinal,
                    EntradasProductos = entradas,
                    SalidasPorProducto = salidasAgrupadas
                };

                var archivo = _pdfService.GenerarResumenMensual(datos, config.Nombre, config.RutaReportes);
                _ultimoArchivo = archivo;
                MensajeEstado = $"✅ Resumen generado: {Path.GetFileName(archivo)}";
                AbrirUltimoArchivo();
            }
            catch (Exception ex)
            {
                MensajeEstado = $"❌ Error: {ex.Message}";
            }
            finally
            {
                GenerandoReporte = false;
            }
        }

        private void AbrirUltimoArchivo()
        {
            if (_ultimoArchivo != null && File.Exists(_ultimoArchivo))
            {
                Process.Start(new ProcessStartInfo(_ultimoArchivo) { UseShellExecute = true });
            }
        }

        private async void AbrirCarpetaReportes()
        {
            var config = await _configuracionService.ObtenerConfiguracionAsync();
            var carpeta = !string.IsNullOrWhiteSpace(config.RutaReportes)
                ? config.RutaReportes
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GestionApp", "Reportes");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);
            Process.Start(new ProcessStartInfo(carpeta) { UseShellExecute = true });
        }
    }
}
