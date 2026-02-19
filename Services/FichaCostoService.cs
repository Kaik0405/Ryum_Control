using GestionApp.Data;
using GestionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApp.Services
{
    /// <summary>
    /// Servicio para operaciones con fichas de costo.
    /// </summary>
    public class FichaCostoService : IFichaCostoService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguracionService _configuracionService;
        private readonly IMovimientoService _movimientoService;

        public FichaCostoService(
            AppDbContext context, 
            IConfiguracionService configuracionService,
            IMovimientoService movimientoService)
        {
            _context = context;
            _configuracionService = configuracionService;
            _movimientoService = movimientoService;
        }

        public async Task<List<FichaCosto>> ObtenerTodosAsync()
        {
            return await _context.FichasCosto
                .Include(f => f.Combo)
                .Include(f => f.Cliente)
                .Include(f => f.Agencia)
                .Include(f => f.Productos)
                .OrderByDescending(f => f.FechaEnvio)
                .ToListAsync();
        }

        public async Task<FichaCosto?> ObtenerPorIdAsync(int id)
        {
            return await _context.FichasCosto
                .Include(f => f.Combo)
                .Include(f => f.Cliente)
                .Include(f => f.Agencia)
                .Include(f => f.Productos)
                    .ThenInclude(p => p.Producto)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<FichaCosto?> ObtenerConDetallesAsync(int id)
        {
            return await ObtenerPorIdAsync(id);
        }

        public async Task<List<FichaCosto>> ObtenerPorPeriodoAsync(int año, int mes)
        {
            var inicio = new DateTime(año, mes, 1);
            var fin = inicio.AddMonths(1).AddDays(-1);
            return await ObtenerPorRangoFechaAsync(inicio, fin);
        }

        public async Task<List<FichaCosto>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta)
        {
            return await _context.FichasCosto
                .Include(f => f.Combo)
                .Include(f => f.Cliente)
                .Include(f => f.Agencia)
                .Include(f => f.Productos)
                .Where(f => f.FechaEnvio >= desde && f.FechaEnvio <= hasta)
                .OrderByDescending(f => f.FechaEnvio)
                .ToListAsync();
        }

        public async Task<string> GenerarNumeroFichaAsync()
        {
            var numero = await _configuracionService.ObtenerSiguienteNumeroFichaAsync();
            var config = await _configuracionService.ObtenerConfiguracionAsync();
            return $"{config.PrefijoFicha}{numero:D5}";
        }

        public async Task<FichaCosto> CrearAsync(FichaCosto ficha)
        {
            // Generar número de ficha si no tiene
            if (string.IsNullOrEmpty(ficha.NumeroFicha))
            {
                ficha.NumeroFicha = await GenerarNumeroFichaAsync();
            }

            ficha.FechaCreacion = DateTime.Now;
            _context.FichasCosto.Add(ficha);
            await _context.SaveChangesAsync();

            // Registrar como venta (ingreso)
            await _movimientoService.RegistrarVentaAsync(ficha);

            return ficha;
        }

        public async Task ActualizarAsync(FichaCosto ficha)
        {
            _context.FichasCosto.Update(ficha);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var ficha = await _context.FichasCosto.FindAsync(id);
            if (ficha != null)
            {
                _context.FichasCosto.Remove(ficha);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<FichaCosto?> ObtenerPorEntregaIdAsync(int entregaId)
        {
            return await _context.FichasCosto
                .Include(f => f.Productos)
                    .ThenInclude(p => p.Producto)
                .FirstOrDefaultAsync(f => f.EntregaId == entregaId);
        }

        /// <summary>
        /// Descuenta del inventario las cantidades de cada producto de la ficha.
        /// Solo descuenta productos que tengan ProductoId asignado.
        /// </summary>
        public async Task DescontarInventarioAsync(FichaCosto ficha)
        {
            if (ficha.InventarioDescontado) return;

            // Recargar con productos si es necesario
            var fichaCompleta = await ObtenerPorIdAsync(ficha.Id) ?? ficha;

            foreach (var prod in fichaCompleta.Productos)
            {
                if (prod.ProductoId.HasValue && prod.ProductoId.Value > 0)
                {
                    var producto = await _context.Productos.FindAsync(prod.ProductoId.Value);
                    if (producto != null)
                    {
                        producto.CantidadStock -= prod.Cantidad;
                        if (producto.CantidadStock < 0) producto.CantidadStock = 0;
                        producto.EnStock = producto.CantidadStock > 0;
                        producto.FechaModificacion = DateTime.Now;
                    }
                }
            }

            fichaCompleta.InventarioDescontado = true;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Restaura al inventario las cantidades de cada producto de la ficha.
        /// Solo restaura si el inventario fue previamente descontado.
        /// </summary>
        public async Task RestaurarInventarioAsync(FichaCosto ficha)
        {
            if (!ficha.InventarioDescontado) return;

            var fichaCompleta = await ObtenerPorIdAsync(ficha.Id) ?? ficha;

            foreach (var prod in fichaCompleta.Productos)
            {
                if (prod.ProductoId.HasValue && prod.ProductoId.Value > 0)
                {
                    var producto = await _context.Productos.FindAsync(prod.ProductoId.Value);
                    if (producto != null)
                    {
                        producto.CantidadStock += prod.Cantidad;
                        producto.EnStock = true;
                        producto.FechaModificacion = DateTime.Now;
                    }
                }
            }

            fichaCompleta.InventarioDescontado = false;
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> ObtenerTotalVentasAsync(int año, int mes)
        {
            var fichas = await ObtenerPorPeriodoAsync(año, mes);
            return fichas.Sum(f => f.PrecioVentaUSD);
        }
    }
}
