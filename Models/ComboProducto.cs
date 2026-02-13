using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Representa un producto dentro de un combo con su cantidad.
    /// </summary>
    public class ComboProducto
    {
        [Key]
        public int Id { get; set; }

        public int ComboId { get; set; }
        public Combo? Combo { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        /// <summary>
        /// Cantidad o peso del producto en el combo.
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Unidad de medida para este producto en el combo.
        /// </summary>
        public UnidadMedida Unidad { get; set; } = UnidadMedida.Unidad;

        /// <summary>
        /// Costo unitario del producto al momento de crear el combo.
        /// </summary>
        public decimal CostoUnitario { get; set; }

        /// <summary>
        /// Total calculado: Cantidad * CostoUnitario
        /// </summary>
        public decimal Total => Cantidad * CostoUnitario;
    }
}
