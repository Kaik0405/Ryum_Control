using GestionApp.Data;
using GestionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApp.Services
{
    /// <summary>
    /// Servicio para operaciones con productos.
    /// </summary>
    public class ProductoService : IProductoService
    {
        private readonly AppDbContext _context;

        public ProductoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Producto>> ObtenerTodosAsync()
        {
            return await _context.Productos
                .Include(p => p.Variantes)
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Variantes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Producto>> ObtenerEnStockAsync()
        {
            return await _context.Productos
                .Where(p => p.Activo && p.EnStock)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<List<Producto>> BuscarPorNombreAsync(string nombre)
        {
            return await _context.Productos
                .Where(p => p.Activo && p.Nombre.Contains(nombre))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<Producto> CrearAsync(Producto producto)
        {
            producto.FechaCreacion = DateTime.Now;
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task ActualizarAsync(Producto producto)
        {
            producto.FechaModificacion = DateTime.Now;
            _context.Productos.Update(producto);
            await _context.SaveChangesAsync();
        }

        public async Task ActualizarStockAsync(int productoId, decimal cantidad)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto != null)
            {
                producto.CantidadStock += cantidad;
                producto.EnStock = producto.CantidadStock > 0;
                producto.FechaModificacion = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<string>> ObtenerCombosVinculadosAsync(int productoId)
        {
            return await _context.Set<ComboProductoInventario>()
                .Where(cpi => cpi.ProductoId == productoId)
                .Include(cpi => cpi.ComboProducto)
                    .ThenInclude(cp => cp.Combo)
                .Select(cpi => cpi.ComboProducto!.Combo!.Nombre)
                .Distinct()
                .ToListAsync();
        }

        public async Task EliminarVinculacionesComboAsync(int productoId)
        {
            var vinculaciones = await _context.Set<ComboProductoInventario>()
                .Where(cpi => cpi.ProductoId == productoId)
                .ToListAsync();

            if (vinculaciones.Any())
            {
                _context.Set<ComboProductoInventario>().RemoveRange(vinculaciones);
                await _context.SaveChangesAsync();
            }
        }

        public async Task EliminarAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.Activo = false; // Soft delete
                producto.FechaModificacion = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
