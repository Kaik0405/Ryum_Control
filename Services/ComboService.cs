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
                    .ThenInclude(cp => cp.Producto)
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Combo?> ObtenerPorIdAsync(int id)
        {
            return await _context.Combos
                .Include(c => c.Productos)
                    .ThenInclude(cp => cp.Producto)
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
                    .ThenInclude(cp => cp.Producto)
                .Where(c => c.Activo && c.Tipo == tipo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<Combo> CrearAsync(Combo combo)
        {
            combo.FechaCreacion = DateTime.Now;
            _context.Combos.Add(combo);
            await _context.SaveChangesAsync();
            return combo;
        }

        public async Task ActualizarAsync(Combo combo)
        {
            // Eliminar productos viejos del combo
            var productosViejos = await _context.Set<ComboProducto>()
                .Where(cp => cp.ComboId == combo.Id)
                .ToListAsync();
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
                    ProductoId = producto.ProductoId,
                    Cantidad = producto.Cantidad,
                    Unidad = producto.Unidad,
                    CostoUnitario = producto.CostoUnitario
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
