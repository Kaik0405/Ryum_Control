using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Tipo de movimiento financiero.
    /// </summary>
    public enum TipoMovimiento
    {
        Ingreso,
        Egreso
    }

    /// <summary>
    /// Categoría del movimiento para mejor clasificación.
    /// </summary>
    public enum CategoriaMovimiento
    {
        Venta,
        CompraProducto,
        Transporte,
        Remesa,
        FondoInicial,
        Otro
    }

    /// <summary>
    /// Representa un movimiento de dinero (ingreso o egreso).
    /// </summary>
    public class Movimiento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Concepto { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Monto del movimiento (siempre positivo).
        /// </summary>
        public decimal Monto { get; set; }

        /// <summary>
        /// Tipo: Ingreso o Egreso.
        /// </summary>
        public TipoMovimiento Tipo { get; set; }

        /// <summary>
        /// Categoría para clasificación y reportes.
        /// </summary>
        public CategoriaMovimiento Categoria { get; set; }

        /// <summary>
        /// Referencia a la ficha de costo si el movimiento es una venta.
        /// </summary>
        public int? FichaCostoId { get; set; }
        public FichaCosto? FichaCosto { get; set; }

        /// <summary>
        /// Referencia al producto si el movimiento es una compra de producto.
        /// </summary>
        public int? ProductoId { get; set; }
        public Producto? Producto { get; set; }

        /// <summary>
        /// Período de inventario al que pertenece este movimiento.
        /// </summary>
        public int? PeriodoInventarioId { get; set; }
        public PeriodoInventario? PeriodoInventario { get; set; }

        /// <summary>
        /// Fecha del movimiento.
        /// </summary>
        public DateTime Fecha { get; set; } = DateTime.Now;

        /// <summary>
        /// Mes/Año para agrupación en reportes.
        /// </summary>
        public int Mes => Fecha.Month;
        public int Año => Fecha.Year;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
