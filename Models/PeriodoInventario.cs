using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Representa un período de inventario mensual.
    /// Controla el dinero inicial, final, productos al inicio y fin del mes.
    /// </summary>
    public class PeriodoInventario
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Mes del período (1-12).
        /// </summary>
        public int Mes { get; set; }

        /// <summary>
        /// Año del período.
        /// </summary>
        public int Año { get; set; }

        /// <summary>
        /// Nombre descriptivo del período (ej: "Febrero 2026").
        /// </summary>
        [MaxLength(50)]
        public string Nombre { get; set; } = string.Empty;

        #region Fondos

        /// <summary>
        /// Dinero disponible al inicio del mes.
        /// </summary>
        public decimal FondosIniciales { get; set; }

        /// <summary>
        /// Dinero disponible al cierre del mes (calculado o ingresado manualmente).
        /// </summary>
        public decimal FondosFinales { get; set; }

        /// <summary>
        /// Total de ingresos del período.
        /// </summary>
        public decimal TotalIngresos { get; set; }

        /// <summary>
        /// Total de egresos del período.
        /// </summary>
        public decimal TotalEgresos { get; set; }

        /// <summary>
        /// Balance calculado: FondosIniciales + TotalIngresos - TotalEgresos
        /// </summary>
        public decimal BalanceCalculado => FondosIniciales + TotalIngresos - TotalEgresos;

        /// <summary>
        /// Diferencia entre el balance calculado y los fondos finales reales.
        /// </summary>
        public decimal Diferencia => FondosFinales - BalanceCalculado;

        #endregion

        #region Contadores

        /// <summary>
        /// Cantidad de fichas de costo generadas en el período.
        /// </summary>
        public int TotalFichas { get; set; }

        /// <summary>
        /// Total de ventas en USD del período.
        /// </summary>
        public decimal TotalVentasUSD { get; set; }

        #endregion

        #region Estado

        /// <summary>
        /// Indica si el período ha sido cerrado.
        /// </summary>
        public bool Cerrado { get; set; }

        /// <summary>
        /// Fecha de cierre del período.
        /// </summary>
        public DateTime? FechaCierre { get; set; }

        /// <summary>
        /// Notas del cierre de período.
        /// </summary>
        [MaxLength(1000)]
        public string? NotasCierre { get; set; }

        #endregion

        #region Relaciones

        /// <summary>
        /// Movimientos financieros del período.
        /// </summary>
        public ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

        /// <summary>
        /// Fichas de costo del período.
        /// </summary>
        public ICollection<FichaCosto> FichasCosto { get; set; } = new List<FichaCosto>();

        /// <summary>
        /// Compras de productos del período.
        /// </summary>
        public ICollection<CompraProducto> Compras { get; set; } = new List<CompraProducto>();

        /// <summary>
        /// Snapshot del inventario al inicio del período.
        /// </summary>
        public ICollection<InventarioSnapshot> InventarioInicial { get; set; } = new List<InventarioSnapshot>();

        #endregion

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
    }

    /// <summary>
    /// Snapshot del inventario en un momento dado.
    /// Usado para guardar el estado del inventario al inicio/fin de período.
    /// </summary>
    public class InventarioSnapshot
    {
        [Key]
        public int Id { get; set; }

        public int PeriodoInventarioId { get; set; }
        public PeriodoInventario? PeriodoInventario { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [MaxLength(200)]
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad en inventario al momento del snapshot.
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Costo unitario al momento del snapshot.
        /// </summary>
        public decimal CostoUnitario { get; set; }

        /// <summary>
        /// Valor total: Cantidad * CostoUnitario
        /// </summary>
        public decimal ValorTotal => Cantidad * CostoUnitario;

        /// <summary>
        /// Tipo de snapshot: Inicial o Final.
        /// </summary>
        public TipoSnapshot Tipo { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }

    public enum TipoSnapshot
    {
        Inicial,
        Final
    }
}
