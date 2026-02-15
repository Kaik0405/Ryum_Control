using GestionApp.Data;
using GestionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApp.Services
{
    /// <summary>
    /// Servicio para operaciones con entregas (órdenes de envío).
    /// Flujo: Combo → Entrega (orden) → FichaCosto (costos).
    /// Las entregas tienen un plazo de 5 días y se destacan las urgentes.
    /// </summary>
    public class EntregaService : IEntregaService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguracionService _configuracionService;

        public EntregaService(AppDbContext context, IConfiguracionService configuracionService)
        {
            _context = context;
            _configuracionService = configuracionService;
        }

        public async Task<List<Entrega>> ObtenerTodosAsync()
        {
            return await _context.Entregas
                .Include(e => e.Combo)
                .Include(e => e.Productos)
                .OrderByDescending(e => e.FechaCreacion)
                .ToListAsync();
        }

        public async Task<Entrega?> ObtenerPorIdAsync(int id)
        {
            return await _context.Entregas
                .Include(e => e.Combo)
                .Include(e => e.Productos)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Entrega> CrearAsync(Entrega entidad)
        {
            if (string.IsNullOrEmpty(entidad.NumeroOrden))
            {
                entidad.NumeroOrden = await GenerarNumeroOrdenAsync();
            }

            entidad.FechaCreacion = DateTime.Now;
            _context.Entregas.Add(entidad);
            await _context.SaveChangesAsync();
            return entidad;
        }

        public async Task ActualizarAsync(Entrega entidad)
        {
            _context.Entregas.Update(entidad);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var entrega = await _context.Entregas.FindAsync(id);
            if (entrega != null)
            {
                _context.Entregas.Remove(entrega);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Crea una entrega a partir de un combo.
        /// Copia los productos del combo a la entrega.
        /// La fecha se asigna automáticamente.
        /// </summary>
        public async Task<Entrega> CrearDesdeComboAsync(
            int comboId, string receptor, string direccion,
            string telMovil, string telFijo, string remitente, string agencia)
        {
            var combo = await _context.Combos
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.Id == comboId);

            if (combo == null)
                throw new InvalidOperationException("El combo especificado no existe.");

            var entrega = new Entrega
            {
                NumeroOrden = await GenerarNumeroOrdenAsync(),
                ComboId = comboId,
                NombreReceptor = receptor,
                DireccionReceptor = direccion,
                TelefonoMovil = telMovil,
                TelefonoFijo = telFijo,
                NombreRemitente = remitente,
                Agencia = agencia,
                FechaOrden = DateTime.Now,
                FechaCreacion = DateTime.Now,
                Productos = combo.Productos.Select(p => new EntregaProducto
                {
                    NombreProducto = p.NombreProducto,
                    Unidad = p.Unidad,
                    Cantidad = p.Cantidad
                }).ToList()
            };

            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();
            return entrega;
        }

        /// <summary>
        /// Obtiene las entregas no entregadas (pendientes).
        /// </summary>
        public async Task<List<Entrega>> ObtenerPendientesAsync()
        {
            return await _context.Entregas
                .Include(e => e.Combo)
                .Include(e => e.Productos)
                .Where(e => e.FechaEntregada == null)
                .OrderBy(e => e.FechaOrden)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene las entregas urgentes (2 días o menos) o vencidas.
        /// </summary>
        public async Task<List<Entrega>> ObtenerUrgentesAsync()
        {
            var todas = await ObtenerPendientesAsync();
            return todas.Where(e => e.Urgente || e.Vencida).ToList();
        }

        /// <summary>
        /// Genera el siguiente número de orden (ej: ENT-00001).
        /// </summary>
        public async Task<string> GenerarNumeroOrdenAsync()
        {
            var config = await _configuracionService.ObtenerConfiguracionAsync();
            var numero = await _configuracionService.ObtenerSiguienteNumeroConformidadAsync();
            return $"ENT-{numero:D5}";
        }

        /// <summary>
        /// Marca una entrega como entregada con la fecha actual.
        /// </summary>
        public async Task MarcarEntregadaAsync(int entregaId)
        {
            var entrega = await _context.Entregas.FindAsync(entregaId);
            if (entrega != null)
            {
                entrega.FechaEntregada = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }
}
