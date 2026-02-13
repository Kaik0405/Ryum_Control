using GestionApp.Data;
using GestionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApp.Services
{
    /// <summary>
    /// Servicio para operaciones con movimientos financieros.
    /// </summary>
    public class MovimientoService : IMovimientoService
    {
        private readonly AppDbContext _context;

        public MovimientoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Movimiento>> ObtenerTodosAsync()
        {
            return await _context.Movimientos
                .Include(m => m.FichaCosto)
                .Include(m => m.Producto)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<Movimiento?> ObtenerPorIdAsync(int id)
        {
            return await _context.Movimientos
                .Include(m => m.FichaCosto)
                .Include(m => m.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<Movimiento>> ObtenerPorPeriodoAsync(int año, int mes)
        {
            return await _context.Movimientos
                .Include(m => m.FichaCosto)
                .Include(m => m.Producto)
                .Where(m => m.Fecha.Year == año && m.Fecha.Month == mes)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<List<Movimiento>> ObtenerPorTipoAsync(TipoMovimiento tipo)
        {
            return await _context.Movimientos
                .Include(m => m.FichaCosto)
                .Include(m => m.Producto)
                .Where(m => m.Tipo == tipo)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();
        }

        public async Task<(decimal Ingresos, decimal Egresos)> ObtenerResumenAsync(int año, int mes)
        {
            var movimientos = await ObtenerPorPeriodoAsync(año, mes);
            
            var ingresos = movimientos
                .Where(m => m.Tipo == TipoMovimiento.Ingreso)
                .Sum(m => m.Monto);
            
            var egresos = movimientos
                .Where(m => m.Tipo == TipoMovimiento.Egreso)
                .Sum(m => m.Monto);

            return (ingresos, egresos);
        }

        public async Task<decimal> ObtenerBalanceActualAsync()
        {
            var ingresos = await _context.Movimientos
                .Where(m => m.Tipo == TipoMovimiento.Ingreso)
                .SumAsync(m => m.Monto);
            
            var egresos = await _context.Movimientos
                .Where(m => m.Tipo == TipoMovimiento.Egreso)
                .SumAsync(m => m.Monto);

            return ingresos - egresos;
        }

        public async Task<Movimiento> CrearAsync(Movimiento movimiento)
        {
            movimiento.FechaCreacion = DateTime.Now;
            _context.Movimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            return movimiento;
        }

        public async Task ActualizarAsync(Movimiento movimiento)
        {
            _context.Movimientos.Update(movimiento);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var movimiento = await _context.Movimientos.FindAsync(id);
            if (movimiento != null)
            {
                _context.Movimientos.Remove(movimiento);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Registra automáticamente una venta como movimiento de ingreso.
        /// </summary>
        public async Task RegistrarVentaAsync(FichaCosto ficha)
        {
            var movimiento = new Movimiento
            {
                Concepto = $"Venta - Ficha {ficha.NumeroFicha}",
                Descripcion = $"Venta a {ficha.NombreReceptor}",
                Monto = ficha.PrecioVentaUSD,
                Tipo = TipoMovimiento.Ingreso,
                Categoria = CategoriaMovimiento.Venta,
                FichaCostoId = ficha.Id,
                PeriodoInventarioId = ficha.PeriodoInventarioId,
                Fecha = ficha.FechaEnvio,
                FechaCreacion = DateTime.Now
            };

            await CrearAsync(movimiento);
        }

        /// <summary>
        /// Registra automáticamente una compra de producto como egreso.
        /// </summary>
        public async Task RegistrarCompraProductoAsync(CompraProducto compra)
        {
            var producto = await _context.Productos.FindAsync(compra.ProductoId);
            
            var movimiento = new Movimiento
            {
                Concepto = $"Compra - {producto?.Nombre ?? "Producto"}",
                Descripcion = $"Compra de {compra.Cantidad} unidades",
                Monto = compra.Total,
                Tipo = TipoMovimiento.Egreso,
                Categoria = CategoriaMovimiento.CompraProducto,
                ProductoId = compra.ProductoId,
                PeriodoInventarioId = compra.PeriodoInventarioId,
                Fecha = compra.Fecha,
                FechaCreacion = DateTime.Now
            };

            await CrearAsync(movimiento);
        }
    }
}
