using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Unidad de medida para productos.
    /// </summary>
    public enum UnidadMedida
    {
        Unidad,     // Cantidad entera
        Libra,      // Peso en libras
        Kilogramo,  // Peso en kg
        Paquete,    // Paquete/Bulto
        Pomo        // Botella/Pomo
    }

    /// <summary>
    /// Representa un producto del inventario.
    /// Un mismo producto puede tener múltiples variantes de precio.
    /// </summary>
    public class Producto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Unidad de medida del producto (libra, litro, unidad, etc.)
        /// </summary>
        public UnidadMedida Unidad { get; set; } = UnidadMedida.Unidad;

        /// <summary>
        /// Costo de compra/adquisición.
        /// </summary>
        public decimal CostoCompra { get; set; }

        /// <summary>
        /// Cantidad disponible en inventario.
        /// </summary>
        public decimal CantidadStock { get; set; }

        /// <summary>
        /// Indica si el producto está disponible para usar en combos.
        /// </summary>
        public bool EnStock { get; set; } = true;

        /// <summary>
        /// Costo total = CostoCompra * CantidadStock
        /// Propiedad calculada, no se guarda en BD.
        /// </summary>
        public decimal CostoTotal => CostoCompra * CantidadStock;

        /// <summary>
        /// Muestra el stock con su unidad: "5 Unidad", "2.5 Libra", etc.
        /// </summary>
        public string StockConUnidad => $"{CantidadStock:G} {Unidad}";

        /// <summary>
        /// Fecha en que el producto fue ingresado al inventario.
        /// </summary>
        public DateTime FechaIngreso { get; set; } = DateTime.Now;

        /// <summary>
        /// Variantes de precio para el mismo producto.
        /// </summary>
        public ICollection<ProductoVariante> Variantes { get; set; } = new List<ProductoVariante>();

        /// <summary>
        /// Historial de compras de este producto.
        /// </summary>
        public ICollection<CompraProducto> Compras { get; set; } = new List<CompraProducto>();

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }

        public bool Activo { get; set; } = true;
    }

    /// <summary>
    /// Variante de precio de un producto.
    /// Permite tener el mismo producto con diferentes precios.
    /// </summary>
    public class ProductoVariante
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [MaxLength(100)]
        public string Descripcion { get; set; } = string.Empty;

        public decimal CostoCompra { get; set; }

        public bool Activo { get; set; } = true;
    }
}
