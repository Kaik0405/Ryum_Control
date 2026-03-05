using GestionApp.Data;
using GestionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionApp.Services
{
    /// <summary>
    /// Servicio para la configuración del distribuidor.
    /// </summary>
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly AppDbContext _context;

        public ConfiguracionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConfiguracionDistribuidor> ObtenerConfiguracionAsync()
        {
            var config = await _context.Configuracion
                .Include(c => c.PeriodoActual)
                .FirstOrDefaultAsync();

            // Crear configuración por defecto si no existe
            if (config == null)
            {
                config = new ConfiguracionDistribuidor
                {
                    Nombre = "Distribuidor",
                    PrefijoFicha = "FC-",
                    PrefijoConformidad = "CONF-",
                    UltimoNumeroFicha = 0,
                    UltimoNumeroConformidad = 0,
                    TasaCambioCUP = 300m,
                    FechaCreacion = DateTime.Now
                };
                _context.Configuracion.Add(config);
                await _context.SaveChangesAsync();
            }

            return config;
        }

        public async Task ActualizarConfiguracionAsync(ConfiguracionDistribuidor config)
        {
            config.FechaModificacion = DateTime.Now;
            _context.Configuracion.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task<int> ObtenerSiguienteNumeroFichaAsync()
        {
            var config = await ObtenerConfiguracionAsync();
            config.UltimoNumeroFicha++;
            await ActualizarConfiguracionAsync(config);
            return config.UltimoNumeroFicha;
        }

        public async Task<int> ObtenerSiguienteNumeroConformidadAsync()
        {
            var config = await ObtenerConfiguracionAsync();
            config.UltimoNumeroConformidad++;
            await ActualizarConfiguracionAsync(config);
            return config.UltimoNumeroConformidad;
        }
    }

    /// <summary>
    /// Servicio para operaciones con clientes.
    /// </summary>
    public class ClienteService : IClienteService
    {
        private readonly AppDbContext _context;

        public ClienteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> ObtenerTodosAsync()
        {
            return await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return await _context.Clientes.FindAsync(id);
        }

        public async Task<List<Cliente>> BuscarPorNombreAsync(string nombre)
        {
            return await _context.Clientes
                .Where(c => c.Activo && c.NombreCompleto.Contains(nombre))
                .OrderBy(c => c.NombreCompleto)
                .ToListAsync();
        }

        public async Task<Cliente> CrearAsync(Cliente cliente)
        {
            cliente.FechaCreacion = DateTime.Now;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            return cliente;
        }

        public async Task ActualizarAsync(Cliente cliente)
        {
            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                cliente.Activo = false;
                await _context.SaveChangesAsync();
            }
        }
    }

    /// <summary>
    /// Servicio para operaciones con agencias.
    /// </summary>
    public class AgenciaService : IAgenciaService
    {
        private readonly AppDbContext _context;

        public AgenciaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Agencia>> ObtenerTodosAsync()
        {
            return await _context.Agencias
                .Where(a => a.Activo)
                .OrderBy(a => a.Nombre)
                .ToListAsync();
        }

        public async Task<Agencia?> ObtenerPorIdAsync(int id)
        {
            return await _context.Agencias.FindAsync(id);
        }

        public async Task<Agencia> CrearAsync(Agencia agencia)
        {
            agencia.FechaCreacion = DateTime.Now;
            _context.Agencias.Add(agencia);
            await _context.SaveChangesAsync();
            return agencia;
        }

        public async Task ActualizarAsync(Agencia agencia)
        {
            _context.Agencias.Update(agencia);
            await _context.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            var agencia = await _context.Agencias.FindAsync(id);
            if (agencia != null)
            {
                agencia.Activo = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
