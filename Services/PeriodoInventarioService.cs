using GestionApp.Data;
using GestionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApp.Services
{
    /// <summary>
    /// Servicio para gestión de períodos de inventario.
    /// </summary>
    public class PeriodoInventarioService : IPeriodoInventarioService
    {
        private readonly AppDbContext _context;

        public PeriodoInventarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PeriodoInventario>> ObtenerTodosAsync()
        {
            return await _context.PeriodosInventario
                .OrderByDescending(p => p.Año)
                .ThenByDescending(p => p.Mes)
                .ToListAsync();
        }

        public async Task<PeriodoInventario?> ObtenerPorIdAsync(int id)
        {
            return await _context.PeriodosInventario
                .Include(p => p.Movimientos)
                .Include(p => p.FichasCosto)
                .Include(p => p.Compras)
                .Include(p => p.InventarioInicial)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<PeriodoInventario?> ObtenerPorMesAsync(int año, int mes)
        {
            return await _context.PeriodosInventario
                .Include(p => p.Movimientos)
                .Include(p => p.FichasCosto)
                .FirstOrDefaultAsync(p => p.Año == año && p.Mes == mes);
        }

        public async Task<PeriodoInventario?> ObtenerPeriodoActualAsync()
        {
            var config = await _context.Configuracion.FirstOrDefaultAsync();
            if (config?.PeriodoActualId != null)
            {
                return await ObtenerPorIdAsync(config.PeriodoActualId.Value);
            }

            // Si no hay período actual, obtener el más reciente no cerrado
            return await _context.PeriodosInventario
                .Where(p => !p.Cerrado)
                .OrderByDescending(p => p.Año)
                .ThenByDescending(p => p.Mes)
                .FirstOrDefaultAsync();
        }

        public async Task<PeriodoInventario> IniciarNuevoPeriodoAsync(int año, int mes, decimal fondosIniciales)
        {
            // Verificar que no exista
            var existente = await ObtenerPorMesAsync(año, mes);
            if (existente != null)
                throw new InvalidOperationException($"Ya existe un período para {mes}/{año}");

            var nombreMes = new DateTime(año, mes, 1).ToString("MMMM yyyy");
            
            var periodo = new PeriodoInventario
            {
                Año = año,
                Mes = mes,
                Nombre = nombreMes,
                FondosIniciales = fondosIniciales,
                FondosFinales = 0,
                TotalIngresos = 0,
                TotalEgresos = 0,
                TotalFichas = 0,
                TotalVentasUSD = 0,
                Cerrado = false,
                FechaCreacion = DateTime.Now
            };

            _context.PeriodosInventario.Add(periodo);
            await _context.SaveChangesAsync();

            // Crear snapshot del inventario inicial
            await GenerarSnapshotInventarioAsync(periodo.Id, TipoSnapshot.Inicial);

            // Actualizar configuración con el período actual
            var config = await _context.Configuracion.FirstOrDefaultAsync();
            if (config != null)
            {
                config.PeriodoActualId = periodo.Id;
                config.FechaModificacion = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            // Registrar fondo inicial como movimiento
            if (fondosIniciales > 0)
            {
                var movimiento = new Movimiento
                {
                    Concepto = "Fondo inicial del período",
                    Descripcion = $"Fondos iniciales para {nombreMes}",
                    Monto = fondosIniciales,
                    Tipo = TipoMovimiento.Ingreso,
                    Categoria = CategoriaMovimiento.FondoInicial,
                    PeriodoInventarioId = periodo.Id,
                    Fecha = new DateTime(año, mes, 1),
                    FechaCreacion = DateTime.Now
                };
                _context.Movimientos.Add(movimiento);
                await _context.SaveChangesAsync();
            }

            return periodo;
        }

        public async Task CerrarPeriodoAsync(int periodoId, decimal fondosFinales)
        {
            var periodo = await _context.PeriodosInventario
                .Include(p => p.Movimientos)
                .Include(p => p.FichasCosto)
                .FirstOrDefaultAsync(p => p.Id == periodoId);

            if (periodo == null)
                throw new InvalidOperationException("Período no encontrado");

            if (periodo.Cerrado)
                throw new InvalidOperationException("El período ya está cerrado");

            // Calcular totales
            periodo.TotalIngresos = periodo.Movimientos
                .Where(m => m.Tipo == TipoMovimiento.Ingreso)
                .Sum(m => m.Monto);
            
            periodo.TotalEgresos = periodo.Movimientos
                .Where(m => m.Tipo == TipoMovimiento.Egreso)
                .Sum(m => m.Monto);

            periodo.TotalFichas = periodo.FichasCosto.Count;
            periodo.TotalVentasUSD = periodo.FichasCosto.Sum(f => f.PrecioVentaUSD);
            periodo.FondosFinales = fondosFinales;
            periodo.Cerrado = true;
            periodo.FechaCierre = DateTime.Now;
            periodo.FechaModificacion = DateTime.Now;

            // Crear snapshot del inventario final
            await GenerarSnapshotInventarioAsync(periodoId, TipoSnapshot.Final);

            await _context.SaveChangesAsync();
        }

        public async Task GenerarSnapshotInventarioAsync(int periodoId, TipoSnapshot tipo)
        {
            var productos = await _context.Productos
                .Where(p => p.Activo)
                .ToListAsync();

            foreach (var producto in productos)
            {
                var snapshot = new InventarioSnapshot
                {
                    PeriodoInventarioId = periodoId,
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,
                    Cantidad = producto.CantidadStock,
                    CostoUnitario = producto.CostoCompra,
                    Tipo = tipo,
                    Fecha = DateTime.Now
                };
                _context.InventarioSnapshots.Add(snapshot);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PeriodoInventario> CrearAsync(PeriodoInventario periodo)
        {
            _context.PeriodosInventario.Add(periodo);
            await _context.SaveChangesAsync();
            return periodo;
        }

        public async Task ActualizarAsync(PeriodoInventario periodo)
        {
            periodo.FechaModificacion = DateTime.Now;
            _context.PeriodosInventario.Update(periodo);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var periodo = await _context.PeriodosInventario.FindAsync(id);
            if (periodo != null && !periodo.Cerrado)
            {
                _context.PeriodosInventario.Remove(periodo);
                await _context.SaveChangesAsync();
            }
        }
    }
}
