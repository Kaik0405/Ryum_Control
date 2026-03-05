using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Representa un producto dentro de un combo.
    /// Los productos del combo son texto libre (no del inventario).
    /// El costo real se calcula después en la Ficha de Costo,
    /// cuando se asignan productos del inventario.
    /// </summary>
    public class ComboProducto
    {
        [Key]
        public int Id { get; set; }

        public int ComboId { get; set; }
        public Combo? Combo { get; set; }

        /// <summary>
        /// Nombre del producto (texto libre, ej: "Pollo", "Aceite", "Jabón").
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad o peso del producto en el combo.
        /// Si hay rango, esta es la cantidad mínima.
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Cantidad máxima del rango (nullable).
        /// Si tiene valor y es mayor que Cantidad, el producto se entrega en rango.
        /// Ej: Carne 12-15 Libra → Cantidad=12, CantidadMaxima=15
        /// </summary>
        public decimal? CantidadMaxima { get; set; }

        /// <summary>
        /// Indica si el producto tiene rango de cantidad.
        /// </summary>
        public bool EsRango => CantidadMaxima.HasValue && CantidadMaxima.Value > Cantidad;

        /// <summary>
        /// Unidad de medida para este producto en el combo.
        /// </summary>
        public UnidadMedida Unidad { get; set; } = UnidadMedida.Unidad;

        /// <summary>
        /// Productos del inventario vinculados a este producto del combo.
        /// Permite vincular varios (ej: cerdo a $40 + cerdo a $50).
        /// </summary>
        public ICollection<ComboProductoInventario> ProductosInventario { get; set; } = new List<ComboProductoInventario>();

        /// <summary>
        /// Descripción formateada: "12-15 Libra" o "2 Libra"
        /// </summary>
        public string Descripcion => EsRango ? $"{Cantidad:G}-{CantidadMaxima:G} {Unidad}" : $"{Cantidad:G} {Unidad}";
    }
}
