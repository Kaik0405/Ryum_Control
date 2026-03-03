using GestionApp.Models;

namespace GestionApp.Services
{
    /// <summary>
    /// Interfaz genérica para operaciones CRUD básicas.
    /// </summary>
    public interface IBaseService<T> where T : class
    {
        Task<List<T>> ObtenerTodosAsync();
        Task<T?> ObtenerPorIdAsync(int id);
        Task<T> CrearAsync(T entidad);
        Task ActualizarAsync(T entidad);
        Task EliminarAsync(int id);
    }

    /// <summary>
    /// Interfaz para operaciones con productos.
    /// </summary>
    public interface IProductoService : IBaseService<Producto>
    {
        Task<List<Producto>> ObtenerEnStockAsync();
        Task<List<Producto>> BuscarPorNombreAsync(string nombre);
        Task ActualizarStockAsync(int productoId, decimal cantidad);
        /// <summary>
        /// Obtiene los nombres de los combos que tienen vinculación con este producto de inventario.
        /// </summary>
        Task<List<string>> ObtenerCombosVinculadosAsync(int productoId);
        /// <summary>
        /// Elimina todas las vinculaciones de ComboProductoInventario para este producto.
        /// </summary>
        Task EliminarVinculacionesComboAsync(int productoId);
    }

    /// <summary>
    /// Interfaz para operaciones con combos.
    /// </summary>
    public interface IComboService : IBaseService<Combo>
    {
        Task<List<Combo>> ObtenerPorTipoAsync(TipoCombo tipo);
        Task<Combo?> ObtenerConProductosAsync(int id);
        Task<Combo> DuplicarComboAsync(int comboId, string nuevoNombre);
    }

    /// <summary>
    /// Interfaz para operaciones con fichas de costo.
    /// </summary>
    public interface IFichaCostoService : IBaseService<FichaCosto>
    {
        Task<List<FichaCosto>> ObtenerPorPeriodoAsync(int año, int mes);
        Task<List<FichaCosto>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta);
        Task<FichaCosto?> ObtenerConDetallesAsync(int id);
        Task<string> GenerarNumeroFichaAsync();
        Task<decimal> ObtenerTotalVentasAsync(int año, int mes);
        Task<FichaCosto?> ObtenerPorEntregaIdAsync(int entregaId);
        Task DescontarInventarioAsync(FichaCosto ficha);
        Task RevertirYRedescontarAsync(FichaCosto ficha);
        Task RestaurarInventarioAsync(FichaCosto ficha);
    }

    /// <summary>
    /// Interfaz para operaciones con movimientos financieros.
    /// </summary>
    public interface IMovimientoService : IBaseService<Movimiento>
    {
        Task<List<Movimiento>> ObtenerPorPeriodoAsync(int año, int mes);
        Task<List<Movimiento>> ObtenerPorTipoAsync(TipoMovimiento tipo);
        Task<List<Movimiento>> ObtenerPorProductoAsync(int productoId);
        Task<(decimal Ingresos, decimal Egresos)> ObtenerResumenAsync(int año, int mes);
        Task<decimal> ObtenerBalanceActualAsync();
        Task RegistrarVentaAsync(FichaCosto ficha);
        Task RegistrarCompraProductoAsync(CompraProducto compra);
    }

    /// <summary>
    /// Interfaz para operaciones con períodos de inventario.
    /// </summary>
    public interface IPeriodoInventarioService : IBaseService<PeriodoInventario>
    {
        Task<PeriodoInventario?> ObtenerPeriodoActualAsync();
        Task<PeriodoInventario> IniciarNuevoPeriodoAsync(int año, int mes, decimal fondosIniciales);
        Task CerrarPeriodoAsync(int periodoId, decimal fondosFinales);
        Task<PeriodoInventario?> ObtenerPorMesAsync(int año, int mes);
        Task GenerarSnapshotInventarioAsync(int periodoId, TipoSnapshot tipo);
    }

    /// <summary>
    /// Interfaz para operaciones con clientes.
    /// </summary>
    public interface IClienteService : IBaseService<Cliente>
    {
        Task<List<Cliente>> BuscarPorNombreAsync(string nombre);
    }

    /// <summary>
    /// Interfaz para operaciones con agencias.
    /// </summary>
    public interface IAgenciaService : IBaseService<Agencia>
    {
    }

    /// <summary>
    /// Interfaz para el servicio de entregas.
    /// </summary>
    public interface IEntregaService : IBaseService<Entrega>
    {
        Task<Entrega> CrearDesdeComboAsync(int comboId, string receptor, string direccion, string telMovil, string telFijo, string remitente, string agencia);
        Task<List<Entrega>> ObtenerPendientesAsync();
        Task<List<Entrega>> ObtenerUrgentesAsync();
        Task<string> GenerarNumeroOrdenAsync();
        Task MarcarEntregadaAsync(int entregaId);
    }

    /// <summary>
    /// Interfaz para la configuración del distribuidor.
    /// </summary>
    public interface IConfiguracionService
    {
        Task<ConfiguracionDistribuidor> ObtenerConfiguracionAsync();
        Task ActualizarConfiguracionAsync(ConfiguracionDistribuidor config);
        Task<int> ObtenerSiguienteNumeroFichaAsync();
        Task<int> ObtenerSiguienteNumeroConformidadAsync();
    }
}
