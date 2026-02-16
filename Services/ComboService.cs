using GestionApp.Data;
using GestionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApp.Services
{
    /// <summary>
    /// Servicio para operaciones con combos.
    /// </summary>
    public class ComboService : IComboService
    {
        private readonly AppDbContext _context;

        public ComboService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Combo>> ObtenerTodosAsync()
        {
            return await _context.Combos
                .Include(c => c.Productos)
                    .ThenInclude(p => p.ProductosInventario)
                        .ThenInclude(pi => pi.Producto)
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Combo?> ObtenerPorIdAsync(int id)
        {
            return await _context.Combos
                .Include(c => c.Productos)
                    .ThenInclude(p => p.ProductosInventario)
                        .ThenInclude(pi => pi.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Combo?> ObtenerConProductosAsync(int id)
        {
            return await ObtenerPorIdAsync(id);
        }

        public async Task<List<Combo>> ObtenerPorTipoAsync(TipoCombo tipo)
        {
            return await _context.Combos
                .Include(c => c.Productos)
                    .ThenInclude(p => p.ProductosInventario)
                        .ThenInclude(pi => pi.Producto)
                .Where(c => c.Activo && c.Tipo == tipo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Combo> CrearAsync(Combo combo)
        {
            combo.FechaCreacion = DateTime.Now;

            // Auto-generar número único (máximo de TODOS los combos + 1)
            var maxNumero = await _context.Combos.MaxAsync(c => (int?)c.Numero) ?? 0;
            combo.Numero = maxNumero + 1;

            // Extraer vinculaciones de inventario antes de guardar
            var vinculacionesPendientes = new List<(int comboProductoIndex, int productoId)>();
            for (int i = 0; i < combo.Productos.Count; i++)
            {
                var cp = combo.Productos.ElementAt(i);
                foreach (var cpi in cp.ProductosInventario)
                {
                    vinculacionesPendientes.Add((i, cpi.ProductoId));
                }
                cp.ProductosInventario.Clear();
            }

            // Guardar combo sin vinculaciones
            _context.Combos.Add(combo);
            await _context.SaveChangesAsync();

            // Ahora agregar vinculaciones con los IDs ya generados
            if (vinculacionesPendientes.Any())
            {
                var productosGuardados = combo.Productos.ToList();
                foreach (var (idx, productoId) in vinculacionesPendientes)
                {
                    _context.Set<ComboProductoInventario>().Add(new ComboProductoInventario
                    {
                        ComboProductoId = productosGuardados[idx].Id,
                        ProductoId = productoId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return combo;
        }

        public async Task ActualizarAsync(Combo combo)
        {
            // Eliminar vinculaciones de inventario viejas
            var productosViejos = await _context.Set<ComboProducto>()
                .Where(cp => cp.ComboId == combo.Id)
                .ToListAsync();
            
            foreach (var pv in productosViejos)
            {
                var vinculaciones = await _context.Set<ComboProductoInventario>()
                    .Where(cpi => cpi.ComboProductoId == pv.Id)
                    .ToListAsync();
                _context.Set<ComboProductoInventario>().RemoveRange(vinculaciones);
            }
            
            // Eliminar productos viejos del combo
            _context.Set<ComboProducto>().RemoveRange(productosViejos);

            // EF detecta los nuevos productos en la colección
            _context.Combos.Update(combo);
            await _context.SaveChangesAsync();
        }

        public async Task<Combo> DuplicarComboAsync(int comboId, string nuevoNombre)
        {
            var original = await ObtenerConProductosAsync(comboId);
            if (original == null)
                throw new InvalidOperationException("Combo no encontrado");

            var nuevoCombo = new Combo
            {
                Nombre = nuevoNombre,
                Descripcion = original.Descripcion,
                Tipo = original.Tipo,
                PrecioVenta = original.PrecioVenta,
                FechaCreacion = DateTime.Now,
                Activo = true
            };

            // Duplicar productos
            foreach (var producto in original.Productos)
            {
                nuevoCombo.Productos.Add(new ComboProducto
                {
                    NombreProducto = producto.NombreProducto,
                    Cantidad = producto.Cantidad,
                    Unidad = producto.Unidad
                });
            }

            _context.Combos.Add(nuevoCombo);
            await _context.SaveChangesAsync();
            return nuevoCombo;
        }

        public async Task EliminarAsync(int id)
        {
            var combo = await _context.Combos.FindAsync(id);
            if (combo != null)
            {
                combo.Activo = false; // Soft delete
                await _context.SaveChangesAsync();
            }
        }
    }
}
