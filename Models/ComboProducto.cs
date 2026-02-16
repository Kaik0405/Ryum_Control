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
        /// </summary>
        public decimal Cantidad { get; set; }

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
        /// Descripción formateada: "2 Libra" o "1 Paquete"
        /// </summary>
        public string Descripcion => $"{Cantidad:G} {Unidad}";
    }
}
