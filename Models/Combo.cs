using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Tipo de combo/pedido.
    /// </summary>
    public enum TipoCombo
    {
        Combo,
        Agrego,
        Festejo
    }

    /// <summary>
    /// Representa un combo de productos que puede ser enviado.
    /// </summary>
    public class Combo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Descripcion { get; set; }

        /// <summary>
        /// Tipo de pedido: Combo, Agrego o Festejo.
        /// </summary>
        public TipoCombo Tipo { get; set; } = TipoCombo.Combo;

        /// <summary>
        /// Precio de venta del combo en USD.
        /// </summary>
        public decimal PrecioVenta { get; set; }

        /// <summary>
        /// Lista de productos que componen el combo (texto libre, no del inventario).
        /// </summary>
        public ICollection<ComboProducto> Productos { get; set; } = new List<ComboProducto>();

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;
    }
}
