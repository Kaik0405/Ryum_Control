using System.IO;
using GestionApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GestionApp.Services
{
    // ════════════════════════════════════════════════════
    // DTOs para el Resumen Mensual
    // ════════════════════════════════════════════════════

    public class LineaInventario
    {
        public string Producto { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; } = "";
        public decimal CostoUnitario { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class LineaMovimientoProducto
    {
        public string Producto { get; set; } = "";
        public decimal Cantidad { get; set; }
        public string Unidad { get; set; } = "";
        public decimal CostoUnitario { get; set; }
        public decimal Total { get; set; }
        public DateTime Fecha { get; set; }
        public string? Detalle { get; set; }
    }

    /// <summary>
    /// Producto agrupado: acumula toda la cantidad usada en fichas de un mismo producto.
    /// </summary>
    public class LineaProductoAgrupado
    {
        public string Producto { get; set; } = "";
        public decimal CantidadTotal { get; set; }
        public string Unidad { get; set; } = "";
        public decimal CostoPromedio { get; set; }
        public decimal ValorTotal { get; set; }
        public int CantidadFichas { get; set; }
    }

    public class DatosResumenMensual
    {
        public string NombrePeriodo { get; set; } = "";
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }

        // Efectivo
        public decimal EfectivoInicial { get; set; }
        public decimal IngresosVentas { get; set; }
        public decimal IngresosOtros { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal EfectivoFinal { get; set; }

        // Desglose egresos
        public decimal GastoProductos { get; set; }
        public decimal GastoTransporte { get; set; }
        public decimal GastoRemesas { get; set; }
        public decimal GastoRebaja { get; set; }
        public decimal GastoAdicional { get; set; }

        // Indicadores
        public int TotalFichas { get; set; }
        public decimal TotalVentasUSD { get; set; }
        public decimal TasaCambio { get; set; }
        public decimal ValorInventarioInicial { get; set; }
        public decimal ValorInventarioFinal { get; set; }

        // Inventario
        public List<LineaInventario> InventarioInicial { get; set; } = new();
        public List<LineaInventario> InventarioFinal { get; set; } = new();

        // Movimientos de productos
        public List<LineaMovimientoProducto> EntradasProductos { get; set; } = new();
        public List<LineaProductoAgrupado> SalidasPorProducto { get; set; } = new();
    }

    /// <summary>
    /// Servicio para generar reportes PDF con diseño profesional.
    /// Usa QuestPDF (licencia Community para uso personal).
    /// </summary>
    public class ReportesPdfService
    {
        // Colores de la app
        private static readonly string AzulOscuro = "#1A237E";
        private static readonly string AzulPrimario = "#1565C0";
        private static readonly string Verde = "#2E7D32";
        private static readonly string Rojo = "#C62828";
        private static readonly string GrisFondo = "#F5F5F5";
        private static readonly string GrisBorde = "#E0E0E0";
        private static readonly string GrisTexto = "#666666";

        private readonly string _logosPath;

        public ReportesPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
            _logosPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GestionApp", "Logos");
        }

        /// <summary>
        /// Genera PDF con el resumen de entregas del período.
        /// Columnas: Receptor, Agencia, Precio Venta USD, Gasto Productos, Gasto Transporte.
        /// </summary>
        public string GenerarReporteEntregas(
            List<FichaCosto> fichas, DateTime desde, DateTime hasta, string nombreDistribuidor, string? rutaDestino = null)
        {
            var carpeta = ObtenerCarpetaReportes(rutaDestino);
            var archivo = Path.Combine(carpeta,
                $"Entregas_{desde:yyyy-MM-dd}_a_{hasta:yyyy-MM-dd}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter.Landscape());
                    page.MarginVertical(30);
                    page.MarginHorizontal(35);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Header().Element(c => EncabezadoReporte(c,
                        "Reporte de Entregas",
                        $"{desde:dd/MM/yyyy} — {hasta:dd/MM/yyyy}",
                        nombreDistribuidor));

                    page.Content().Element(c => ContenidoEntregas(c, fichas));

                    page.Footer().Element(PieReporte);
                });
            }).GeneratePdf(archivo);

            return archivo;
        }

        /// <summary>
        /// Genera PDF con fichas de costo individuales (1 por página).
        /// Muestra todos los datos EXCEPTO la ganancia.
        /// </summary>
        public string GenerarReporteFichasCosto(
            List<FichaCosto> fichas, DateTime desde, DateTime hasta, string nombreDistribuidor, string? rutaDestino = null)
        {
            var carpeta = ObtenerCarpetaReportes(rutaDestino);
            var archivo = Path.Combine(carpeta,
                $"FichasCosto_{desde:yyyy-MM-dd}_a_{hasta:yyyy-MM-dd}.pdf");

            Document.Create(container =>
            {
                foreach (var ficha in fichas)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.MarginVertical(30);
                        page.MarginHorizontal(35);
                        page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                        page.Header().Element(c => EncabezadoFicha(c, ficha));

                        page.Content().Element(c => ContenidoFicha(c, ficha));

                        page.Footer().Element(PieReporte);
                    });
                }
            }).GeneratePdf(archivo);

            return archivo;
        }

        // ═══════════════════════════════════════════════════════════
        // RESUMEN MENSUAL
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Genera PDF con el resumen mensual completo: financiero, inventario y movimientos de productos.
        /// </summary>
        public string GenerarResumenMensual(
            DatosResumenMensual datos, string nombreDistribuidor, string? rutaDestino = null)
        {
            var carpeta = ObtenerCarpetaReportes(rutaDestino);
            var archivo = Path.Combine(carpeta,
                $"ResumenMensual_{datos.FechaDesde:yyyy-MM}.pdf");

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.MarginVertical(30);
                    page.MarginHorizontal(35);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Header().Element(c => EncabezadoReporte(c,
                        $"Resumen Mensual — {datos.NombrePeriodo}",
                        $"{datos.FechaDesde:dd/MM/yyyy} — {datos.FechaHasta:dd/MM/yyyy}",
                        nombreDistribuidor));

                    page.Content().Element(c => ContenidoResumenMensual(c, datos));

                    page.Footer().Element(PieReporte);
                });
            }).GeneratePdf(archivo);

            return archivo;
        }

        private void ContenidoResumenMensual(IContainer container, DatosResumenMensual datos)
        {
            container.PaddingTop(12).Column(col =>
            {
                // ── SECCIÓN 1: INDICADORES CLAVE ──
                col.Item().Element(c => SeccionIndicadores(c, datos));

                col.Item().PaddingTop(14);

                // ── SECCIÓN 2: RESUMEN FINANCIERO ──
                col.Item().Element(c => SeccionResumenFinanciero(c, datos));

                col.Item().PaddingTop(14);

                // ── SECCIÓN 3: INVENTARIO INICIAL ──
                col.Item().Element(c => SeccionTablaInventario(c,
                    $"📦 Inventario de Productos — Inicio de Mes ({datos.FechaDesde:dd/MM/yyyy})",
                    datos.InventarioInicial));

                col.Item().PaddingTop(14);

                // ── SECCIÓN 4: INVENTARIO FINAL ──
                col.Item().Element(c => SeccionTablaInventario(c,
                    $"📦 Inventario de Productos — Fin de Mes ({datos.FechaHasta:dd/MM/yyyy})",
                    datos.InventarioFinal));

                col.Item().PaddingTop(14);

                // ── SECCIÓN 5: ENTRADAS DE PRODUCTOS ──
                if (datos.EntradasProductos.Count > 0)
                {
                    col.Item().Element(c => SeccionEntradas(c, datos.EntradasProductos));
                    col.Item().PaddingTop(14);
                }

                // ── SECCIÓN 6: SALIDAS DE PRODUCTOS (agrupadas) ──
                if (datos.SalidasPorProducto.Count > 0)
                {
                    col.Item().Element(c => SeccionSalidasAgrupadas(c, datos.SalidasPorProducto));
                }
            });
        }

        private void SeccionIndicadores(IContainer container, DatosResumenMensual datos)
        {
            container.Row(row =>
            {
                void MiniCard(RowDescriptor r, string label, string valor, string bg, string colorTexto, bool last = false)
                {
                    var item = last ? r.RelativeItem() : r.RelativeItem();
                    item.Background(bg).Padding(10).Column(c =>
                    {
                        c.Item().Text(label).FontSize(8).FontColor(GrisTexto);
                        c.Item().PaddingTop(3).Text(valor).FontSize(14).Bold().FontColor(colorTexto);
                    });
                    if (!last) r.ConstantItem(6);
                }

                MiniCard(row, "Fichas generadas", $"{datos.TotalFichas}", "#E3F2FD", AzulPrimario);
                MiniCard(row, "Ventas (USD)", $"${datos.TotalVentasUSD:N2}", "#E8F5E9", Verde);
                MiniCard(row, "Tasa de cambio", $"1 USD = {datos.TasaCambio:N0} CUP", "#FFF3E0", "#E65100");
                MiniCard(row, "Valor inventario inicio", $"{datos.ValorInventarioInicial:N0} CUP", "#F3E5F5", "#7B1FA2");
                MiniCard(row, "Valor inventario fin", $"{datos.ValorInventarioFinal:N0} CUP", "#FCE4EC", Rojo, true);
            });
        }

        private void SeccionResumenFinanciero(IContainer container, DatosResumenMensual datos)
        {
            container.Row(row =>
            {
                // ── Columna izquierda: Resumen de Efectivo ──
                row.RelativeItem().Border(1).BorderColor(AzulPrimario).Background("#E3F2FD")
                    .Padding(14).Column(col =>
                {
                    col.Item().Text("💰 Resumen de Efectivo").FontSize(13).Bold().FontColor(AzulOscuro);
                    col.Item().PaddingTop(10);

                    FilaResumen(col, "Efectivo al inicio del mes", datos.EfectivoInicial, AzulOscuro);
                    col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor("#90CAF9");
                    FilaResumen(col, "(+) Ingresos por ventas", datos.IngresosVentas, Verde);
                    if (datos.IngresosOtros > 0)
                        FilaResumen(col, "(+) Otros ingresos", datos.IngresosOtros, Verde);
                    FilaResumen(col, "(+) Total ingresos", datos.TotalIngresos, Verde);
                    col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor("#90CAF9");
                    FilaResumen(col, "(−) Total de egresos", datos.TotalEgresos, Rojo);

                    col.Item().PaddingVertical(6).LineHorizontal(1.5f).LineColor(AzulPrimario);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Efectivo al final del mes").FontSize(12).Bold();
                        r.ConstantItem(130).AlignRight()
                            .Text($"{datos.EfectivoFinal:N0} CUP").FontSize(14).Bold()
                            .FontColor(datos.EfectivoFinal >= 0 ? Verde : Rojo);
                    });
                });

                row.ConstantItem(12);

                // ── Columna derecha: Desglose de Egresos ──
                row.RelativeItem().Border(1).BorderColor(Rojo).Background("#FFEBEE")
                    .Padding(14).Column(col =>
                {
                    col.Item().Text("📊 Desglose de Egresos").FontSize(13).Bold().FontColor(AzulOscuro);
                    col.Item().PaddingTop(10);

                    FilaResumenDetalle(col, "🛒 Compra de productos", datos.GastoProductos, Rojo);
                    FilaResumenDetalle(col, "🚚 Transporte", datos.GastoTransporte, Rojo);
                    FilaResumenDetalle(col, "💵 Remesas enviadas", datos.GastoRemesas, "#00695C");
                    FilaResumenDetalle(col, "📉 Rebaja de entregas", datos.GastoRebaja, "#E65100");
                    if (datos.GastoAdicional > 0)
                        FilaResumenDetalle(col, "💸 Gasto adicional", datos.GastoAdicional, "#7B1FA2");

                    col.Item().PaddingVertical(6).LineHorizontal(1.5f).LineColor(Rojo);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Total Egresos").FontSize(12).Bold();
                        r.ConstantItem(130).AlignRight()
                            .Text($"{datos.TotalEgresos:N0} CUP").FontSize(14).Bold().FontColor(Rojo);
                    });
                });
            });
        }

        private static void FilaResumen(ColumnDescriptor col, string label, decimal valor, string color)
        {
            col.Item().PaddingBottom(5).Row(r =>
            {
                r.RelativeItem().Text(label).FontSize(10);
                r.ConstantItem(130).AlignRight()
                    .Text($"{valor:N0} CUP").FontSize(10).Bold().FontColor(color);
            });
        }

        private static void FilaResumenDetalle(ColumnDescriptor col, string label, decimal valor, string color)
        {
            col.Item().PaddingBottom(6).Row(r =>
            {
                r.RelativeItem().Text(label).FontSize(10);
                r.ConstantItem(130).AlignRight()
                    .Text($"{valor:N0} CUP").FontSize(11).Bold().FontColor(color);
            });
        }

        private void SeccionTablaInventario(IContainer container, string titulo, List<LineaInventario> lineas)
        {
            container.Column(col =>
            {
                col.Item().Text(titulo).FontSize(13).Bold().FontColor(AzulOscuro);
                col.Item().PaddingTop(6);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);   // #
                        cols.RelativeColumn(3);    // Producto
                        cols.ConstantColumn(70);   // Cantidad
                        cols.ConstantColumn(65);   // Unidad
                        cols.ConstantColumn(85);   // Costo Unit.
                        cols.ConstantColumn(85);   // Valor Total
                    });

                    table.Header(header =>
                    {
                        CeldaEncabezado(header.Cell(), "#");
                        CeldaEncabezado(header.Cell(), "Producto");
                        CeldaEncabezadoDerecha(header.Cell(), "Cantidad");
                        CeldaEncabezado(header.Cell(), "Unidad");
                        CeldaEncabezadoDerecha(header.Cell(), "Costo Unit.");
                        CeldaEncabezadoDerecha(header.Cell(), "Valor Total");
                    });

                    for (var i = 0; i < lineas.Count; i++)
                    {
                        var l = lineas[i];
                        var bg = (i + 1) % 2 == 0 ? GrisFondo : "#FFFFFF";

                        CeldaDato(table.Cell(), (i + 1).ToString(), bg);
                        CeldaDato(table.Cell(), l.Producto, bg);
                        CeldaDatoMoneda(table.Cell(), $"{l.Cantidad:G}", bg, null);
                        CeldaDato(table.Cell(), l.Unidad, bg);
                        CeldaDatoMoneda(table.Cell(), $"{l.CostoUnitario:N2}", bg, null);
                        CeldaDatoMoneda(table.Cell(), $"{l.ValorTotal:N2}", bg, AzulPrimario);
                    }
                });

                // Total
                var totalValor = lineas.Sum(l => l.ValorTotal);
                col.Item().PaddingTop(6).AlignRight()
                    .Text($"Total: {totalValor:N2} CUP")
                    .FontSize(11).Bold().FontColor(AzulOscuro);
            });
        }

        private void SeccionEntradas(IContainer container, List<LineaMovimientoProducto> lineas)
        {
            container.Column(col =>
            {
                col.Item().Text("📥 Entradas de Productos (Compras)").FontSize(13).Bold().FontColor(AzulOscuro);
                col.Item().PaddingTop(6);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);   // #
                        cols.RelativeColumn(2.5f); // Producto
                        cols.ConstantColumn(65);   // Cantidad
                        cols.ConstantColumn(55);   // Unidad
                        cols.ConstantColumn(80);   // Costo Unit.
                        cols.ConstantColumn(80);   // Total
                        cols.ConstantColumn(70);   // Fecha
                        cols.RelativeColumn(1.5f); // Proveedor
                    });

                    table.Header(header =>
                    {
                        CeldaEncabezado(header.Cell(), "#");
                        CeldaEncabezado(header.Cell(), "Producto");
                        CeldaEncabezadoDerecha(header.Cell(), "Cant.");
                        CeldaEncabezado(header.Cell(), "Unidad");
                        CeldaEncabezadoDerecha(header.Cell(), "Costo U.");
                        CeldaEncabezadoDerecha(header.Cell(), "Total");
                        CeldaEncabezado(header.Cell(), "Fecha");
                        CeldaEncabezado(header.Cell(), "Proveedor");
                    });

                    for (var i = 0; i < lineas.Count; i++)
                    {
                        var l = lineas[i];
                        var bg = (i + 1) % 2 == 0 ? GrisFondo : "#FFFFFF";

                        CeldaDato(table.Cell(), (i + 1).ToString(), bg);
                        CeldaDato(table.Cell(), l.Producto, bg);
                        CeldaDatoMoneda(table.Cell(), $"{l.Cantidad:G}", bg, null);
                        CeldaDato(table.Cell(), l.Unidad, bg);
                        CeldaDatoMoneda(table.Cell(), $"{l.CostoUnitario:N2}", bg, null);
                        CeldaDatoMoneda(table.Cell(), $"{l.Total:N2}", bg, Verde);
                        CeldaDato(table.Cell(), l.Fecha.ToString("dd/MM"), bg);
                        CeldaDato(table.Cell(), l.Detalle ?? "—", bg);
                    }
                });

                var total = lineas.Sum(l => l.Total);
                col.Item().PaddingTop(6).AlignRight()
                    .Text($"Total compras: {total:N2} CUP")
                    .FontSize(11).Bold().FontColor(Verde);
            });
        }

        private void SeccionSalidasAgrupadas(IContainer container, List<LineaProductoAgrupado> lineas)
        {
            container.Column(col =>
            {
                col.Item().Text("📤 Productos Rebajados del Inventario").FontSize(13).Bold().FontColor(AzulOscuro);
                col.Item().PaddingTop(2).Text("Resumen de productos usados en entregas del período")
                    .FontSize(9).FontColor(GrisTexto);
                col.Item().PaddingTop(6);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);   // #
                        cols.RelativeColumn(3);    // Producto
                        cols.ConstantColumn(70);   // Cant. Total
                        cols.ConstantColumn(60);   // Unidad
                        cols.ConstantColumn(80);   // Costo Prom.
                        cols.ConstantColumn(90);   // Valor Total
                        cols.ConstantColumn(55);   // # Fichas
                    });

                    table.Header(header =>
                    {
                        CeldaEncabezado(header.Cell(), "#");
                        CeldaEncabezado(header.Cell(), "Producto");
                        CeldaEncabezadoDerecha(header.Cell(), "Cant. Total");
                        CeldaEncabezado(header.Cell(), "Unidad");
                        CeldaEncabezadoDerecha(header.Cell(), "Costo Prom.");
                        CeldaEncabezadoDerecha(header.Cell(), "Valor Total");
                        CeldaEncabezadoDerecha(header.Cell(), "Fichas");
                    });

                    for (var i = 0; i < lineas.Count; i++)
                    {
                        var l = lineas[i];
                        var bg = (i + 1) % 2 == 0 ? GrisFondo : "#FFFFFF";

                        CeldaDato(table.Cell(), (i + 1).ToString(), bg);
                        CeldaDato(table.Cell(), l.Producto, bg);
                        CeldaDatoMoneda(table.Cell(), $"{l.CantidadTotal:G}", bg, null);
                        CeldaDato(table.Cell(), l.Unidad, bg);
                        CeldaDatoMoneda(table.Cell(), $"{l.CostoPromedio:N2}", bg, null);
                        CeldaDatoMoneda(table.Cell(), $"{l.ValorTotal:N2}", bg, Rojo);
                        CeldaDatoMoneda(table.Cell(), $"{l.CantidadFichas}", bg, AzulPrimario);
                    }
                });

                var total = lineas.Sum(l => l.ValorTotal);
                var totalCantFichas = lineas.Max(l => l.CantidadFichas); // Fichas are shared
                col.Item().PaddingTop(6).AlignRight()
                    .Text($"Total rebajado: {total:N2} CUP")
                    .FontSize(11).Bold().FontColor(Rojo);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // ENCABEZADO COMÚN PARA REPORTE DE ENTREGAS
        // ═══════════════════════════════════════════════════════════

        private void EncabezadoReporte(IContainer container, string titulo, string periodo, string distribuidor)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Logos de agencias
                    row.ConstantItem(90).AlignCenter().AlignMiddle().Element(c => InsertarLogo(c, "rios"));
                    
                    row.RelativeItem().PaddingHorizontal(15).AlignMiddle().Column(inner =>
                    {
                        inner.Item().Text(titulo)
                            .FontSize(20).Bold().FontColor(AzulOscuro);
                        inner.Item().Text(distribuidor)
                            .FontSize(11).FontColor(GrisTexto);
                        inner.Item().Text(periodo)
                            .FontSize(11).FontColor(AzulPrimario);
                    });

                    row.ConstantItem(90).AlignCenter().AlignMiddle().Element(c => InsertarLogo(c, "yumury"));
                });

                col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(AzulPrimario);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // TABLA DE ENTREGAS
        // ═══════════════════════════════════════════════════════════

        private void ContenidoEntregas(IContainer container, List<FichaCosto> fichas)
        {
            container.PaddingTop(15).Column(col =>
            {
                // Tabla principal
                col.Item().Table(table =>
                {
                    // Definir columnas
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(35);   // #
                        cols.RelativeColumn(2.5f); // Receptor
                        cols.RelativeColumn(1.5f); // Agencia
                        cols.RelativeColumn(1.3f); // Pedido
                        cols.ConstantColumn(85);   // Precio Venta USD
                        cols.ConstantColumn(85);   // Gasto Productos
                        cols.ConstantColumn(85);   // Gasto Transporte
                    });

                    // Encabezado de tabla
                    table.Header(header =>
                    {
                        CeldaEncabezado(header.Cell(), "#");
                        CeldaEncabezado(header.Cell(), "Receptor");
                        CeldaEncabezado(header.Cell(), "Agencia");
                        CeldaEncabezado(header.Cell(), "Pedido");
                        CeldaEncabezadoDerecha(header.Cell(), "Venta USD");
                        CeldaEncabezadoDerecha(header.Cell(), "Gasto Prod.");
                        CeldaEncabezadoDerecha(header.Cell(), "Transporte");
                    });

                    // Filas de datos
                    var i = 0;
                    foreach (var f in fichas)
                    {
                        i++;
                        var bgColor = i % 2 == 0 ? GrisFondo : "#FFFFFF";
                        var gastoProductos = f.Productos?.Sum(p => p.Total) ?? 0;
                        var tipoPedido = f.TipoPedido.ToString();

                        CeldaDato(table.Cell(), i.ToString(), bgColor);
                        CeldaDato(table.Cell(), f.NombreReceptor, bgColor);
                        CeldaDato(table.Cell(), f.NombreAgencia, bgColor);
                        CeldaDato(table.Cell(), tipoPedido, bgColor);
                        CeldaDatoMoneda(table.Cell(), $"${f.PrecioVentaUSD:N2}", bgColor, Verde);
                        CeldaDatoMoneda(table.Cell(), $"${gastoProductos:N2}", bgColor, Rojo);
                        CeldaDatoMoneda(table.Cell(), $"${f.CostoTransportacion:N2}", bgColor, Rojo);
                    }
                });

                // Totales
                col.Item().PaddingTop(12).LineHorizontal(1).LineColor(GrisBorde);
                col.Item().PaddingTop(8).Row(row =>
                {
                    var totalVentas = fichas.Sum(f => f.PrecioVentaUSD);
                    var totalProductos = fichas.Sum(f => f.Productos?.Sum(p => p.Total) ?? 0);
                    var totalTransporte = fichas.Sum(f => f.CostoTransportacion);

                    row.RelativeItem().Text($"Total Fichas: {fichas.Count}")
                        .FontSize(11).Bold().FontColor(AzulOscuro);

                    row.ConstantItem(150).AlignRight().Text($"Ventas: ${totalVentas:N2}")
                        .FontSize(11).Bold().FontColor(Verde);

                    row.ConstantItem(150).AlignRight().Text($"Productos: ${totalProductos:N2}")
                        .FontSize(11).Bold().FontColor(Rojo);

                    row.ConstantItem(150).AlignRight().Text($"Transporte: ${totalTransporte:N2}")
                        .FontSize(11).Bold().FontColor(Rojo);
                });
            });
        }

        // ═══════════════════════════════════════════════════════════
        // FICHA DE COSTO INDIVIDUAL (1 POR PÁGINA)
        // ═══════════════════════════════════════════════════════════

        private void EncabezadoFicha(IContainer container, FichaCosto ficha)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantItem(90).AlignCenter().AlignMiddle().Element(c => InsertarLogo(c, "rios"));

                    row.RelativeItem().PaddingHorizontal(12).AlignMiddle().Column(inner =>
                    {
                        inner.Item().Text($"Ficha de Costo — {ficha.NumeroFicha}")
                            .FontSize(18).Bold().FontColor(AzulOscuro);
                        inner.Item().Text($"{ficha.TipoPedido} • {ficha.FechaEnvio:dd/MM/yyyy}")
                            .FontSize(11).FontColor(GrisTexto);
                    });

                    row.ConstantItem(90).AlignCenter().AlignMiddle().Element(c => InsertarLogo(c, "yumury"));
                });

                col.Item().PaddingTop(6).LineHorizontal(1.5f).LineColor(AzulPrimario);
            });
        }

        private void ContenidoFicha(IContainer container, FichaCosto ficha)
        {
            container.PaddingTop(12).Column(col =>
            {
                // Sección: Datos del envío
                col.Item().Element(c => SeccionFichaDatos(c, ficha));

                col.Item().PaddingTop(12);

                // Sección: Productos
                col.Item().Element(c => SeccionFichaProductos(c, ficha));

                col.Item().PaddingTop(12);

                // Sección: Resumen de costos (sin ganancia)
                col.Item().Element(c => SeccionFichaResumen(c, ficha));
            });
        }

        private void SeccionFichaDatos(IContainer container, FichaCosto ficha)
        {
            container.Border(1).BorderColor(GrisBorde).Background("#FAFAFA")
                .Padding(12).Column(col =>
            {
                col.Item().Text("Datos del Envío").FontSize(13).Bold().FontColor(AzulOscuro);
                col.Item().PaddingTop(8);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        CampoInfo(left, "Distribuidor", ficha.Distribuidor);
                        CampoInfo(left, "Remitente", ficha.Remitente);
                        CampoInfo(left, "Agencia", ficha.NombreAgencia);
                    });
                    row.RelativeItem().Column(right =>
                    {
                        CampoInfo(right, "Receptor", ficha.NombreReceptor);
                        CampoInfo(right, "Dirección", ficha.DireccionReceptor);
                        CampoInfo(right, "Teléfono", ficha.TelefonoReceptor);
                    });
                });

                col.Item().PaddingTop(6);
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        CampoInfo(left, "Fecha de envío", ficha.FechaEnvio.ToString("dd/MM/yyyy"));
                    });
                    row.RelativeItem().Column(right =>
                    {
                        CampoInfo(right, "Fecha de entrega", ficha.FechaEntrega?.ToString("dd/MM/yyyy") ?? "Pendiente");
                    });
                });

                if (!string.IsNullOrWhiteSpace(ficha.Notas))
                {
                    col.Item().PaddingTop(6);
                    CampoInfo(col, "Notas", ficha.Notas);
                }
            });
        }

        private void SeccionFichaProductos(IContainer container, FichaCosto ficha)
        {
            container.Column(col =>
            {
                col.Item().Text("Productos").FontSize(13).Bold().FontColor(AzulOscuro);
                col.Item().PaddingTop(6);

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);   // #
                        cols.RelativeColumn(3);    // Producto
                        cols.ConstantColumn(75);   // Unidad
                        cols.ConstantColumn(65);   // Cantidad
                        cols.ConstantColumn(85);   // Costo Unit.
                        cols.ConstantColumn(85);   // Total
                    });

                    table.Header(header =>
                    {
                        CeldaEncabezado(header.Cell(), "#");
                        CeldaEncabezado(header.Cell(), "Producto");
                        CeldaEncabezado(header.Cell(), "Unidad");
                        CeldaEncabezado(header.Cell(), "Cantidad");
                        CeldaEncabezadoDerecha(header.Cell(), "Costo Unit.");
                        CeldaEncabezadoDerecha(header.Cell(), "Total");
                    });

                    var productos = ficha.Productos?.ToList() ?? new List<FichaCostoProducto>();
                    for (var i = 0; i < productos.Count; i++)
                    {
                        var p = productos[i];
                        var bg = (i + 1) % 2 == 0 ? GrisFondo : "#FFFFFF";

                        CeldaDato(table.Cell(), (i + 1).ToString(), bg);
                        CeldaDato(table.Cell(), p.NombreProducto, bg);
                        CeldaDato(table.Cell(), p.Unidad.ToString(), bg);
                        CeldaDato(table.Cell(), $"{p.Cantidad:G}", bg);
                        CeldaDatoMoneda(table.Cell(), $"${p.CostoUnitario:N2}", bg, null);
                        CeldaDatoMoneda(table.Cell(), $"${p.Total:N2}", bg, AzulPrimario);
                    }
                });
            });
        }

        private void SeccionFichaResumen(IContainer container, FichaCosto ficha)
        {
            var gastoProductos = ficha.Productos?.Sum(p => p.Total) ?? 0;

            container.Border(1).BorderColor(AzulPrimario).Background("#E3F2FD")
                .Padding(14).Column(col =>
            {
                col.Item().Text("Resumen de Costos").FontSize(13).Bold().FontColor(AzulOscuro);
                col.Item().PaddingTop(10);

                // Fila 1: Precio de venta
                col.Item().PaddingBottom(4).Row(r =>
                {
                    r.ConstantItem(180).Text("Precio de Venta (USD):").FontSize(11);
                    r.RelativeItem().Text($"${ficha.PrecioVentaUSD:N2}").FontSize(12).Bold().FontColor(Verde);
                });

                // Fila 2: Gasto en productos
                col.Item().PaddingBottom(4).Row(r =>
                {
                    r.ConstantItem(180).Text("Gasto en Productos:").FontSize(11);
                    r.RelativeItem().Text($"${gastoProductos:N2}").FontSize(12).Bold().FontColor(Rojo);
                });

                // Fila 3: Costo transportación
                col.Item().PaddingBottom(4).Row(r =>
                {
                    r.ConstantItem(180).Text("Costo Transportación:").FontSize(11);
                    r.RelativeItem().Text($"${ficha.CostoTransportacion:N2}").FontSize(12).Bold().FontColor(Rojo);
                });

                // Separador
                col.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(AzulPrimario);

                // Fila 4: Costo total
                col.Item().Row(r =>
                {
                    r.ConstantItem(180).Text("Costo Total:").FontSize(12).Bold();
                    r.RelativeItem().Text($"${ficha.CostoTotal:N2}").FontSize(13).Bold().FontColor(AzulOscuro);
                });
            });
        }

        // ═══════════════════════════════════════════════════════════
        // PIE DE PÁGINA
        // ═══════════════════════════════════════════════════════════

        private void PieReporte(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(0.5f).LineColor(GrisBorde);
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(8).FontColor(GrisTexto);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Pág. ").FontSize(8).FontColor(GrisTexto);
                        text.CurrentPageNumber().FontSize(8).FontColor(GrisTexto);
                        text.Span(" / ").FontSize(8).FontColor(GrisTexto);
                        text.TotalPages().FontSize(8).FontColor(GrisTexto);
                    });
                });
            });
        }

        // ═══════════════════════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════════════════════

        private static void CeldaEncabezado(IContainer cell, string texto)
        {
            cell.Background(AzulOscuro).Padding(6)
                .Text(texto).FontSize(9).Bold().FontColor(Colors.White);
        }

        private static void CeldaEncabezadoDerecha(IContainer cell, string texto)
        {
            cell.Background(AzulOscuro).Padding(6).AlignRight()
                .Text(texto).FontSize(9).Bold().FontColor(Colors.White);
        }

        private static void CeldaDato(IContainer cell, string texto, string bgColor)
        {
            cell.Background(bgColor).BorderBottom(0.5f).BorderColor("#E0E0E0")
                .Padding(6).Text(texto).FontSize(9);
        }

        private static void CeldaDatoMoneda(IContainer cell, string texto, string bgColor, string? color)
        {
            var td = cell.Background(bgColor).BorderBottom(0.5f).BorderColor("#E0E0E0")
                .Padding(6).AlignRight().Text(texto).FontSize(9).Bold();
            if (color != null)
                td.FontColor(color);
        }

        private void CampoInfo(ColumnDescriptor col, string label, string valor)
        {
            col.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(90).Text($"{label}:")
                    .FontSize(9).Bold().FontColor(GrisTexto);
                row.RelativeItem().Text(valor ?? "—")
                    .FontSize(9);
            });
        }

        private void InsertarLogo(IContainer container, string nombre)
        {
            var path = Path.Combine(_logosPath, $"{nombre}.png");
            if (File.Exists(path))
            {
                container.AlignCenter().AlignMiddle()
                    .Width(75).Height(75)
                    .Image(path);
            }
            else
            {
                container.AlignCenter().AlignMiddle()
                    .Width(75).Height(75)
                    .Background("#E3F2FD")
                    .AlignCenter().AlignMiddle()
                    .Text(nombre.ToUpper().Substring(0, 1))
                    .FontSize(22).Bold().FontColor(AzulPrimario);
            }
        }

        private static string ObtenerCarpetaReportes(string? rutaPersonalizada = null)
        {
            var carpeta = !string.IsNullOrWhiteSpace(rutaPersonalizada)
                ? rutaPersonalizada
                : Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GestionApp", "Reportes");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);
            return carpeta;
        }
    }
}
