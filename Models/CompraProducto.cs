using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Registro de compra de producto para el inventario.
    /// Cada compra descuenta del dinero disponible y aumenta el stock.
    /// </summary>
    public class CompraProducto
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Producto comprado.
        /// </summary>
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        /// <summary>
        /// Período de inventario al que pertenece esta compra.
        /// </summary>
        public int? PeriodoInventarioId { get; set; }
        public PeriodoInventario? PeriodoInventario { get; set; }

        /// <summary>
        /// Cantidad comprada (en la unidad del producto).
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Costo unitario de compra.
        /// </summary>
        public decimal CostoUnitario { get; set; }

        /// <summary>
        /// Total de la compra: Cantidad * CostoUnitario.
        /// </summary>
        public decimal Total => Cantidad * CostoUnitario;

        /// <summary>
        /// Proveedor o lugar de compra (opcional).
        /// </summary>
        [MaxLength(200)]
        public string? Proveedor { get; set; }

        [MaxLength(500)]
        public string? Notas { get; set; }

        /// <summary>
        /// Fecha de la compra.
        /// </summary>
        public DateTime Fecha { get; set; } = DateTime.Now;

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
