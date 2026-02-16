using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Tabla puente que vincula un producto de un combo con uno o más productos del inventario.
    /// Permite que "Carne de cerdo 30lb" del combo se cubra con varios productos del inventario
    /// (ej: 20lb a $40 + 10lb a $50), sumando el stock total para verificar disponibilidad.
    /// </summary>
    public class ComboProductoInventario
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// El producto del combo que se está vinculando.
        /// </summary>
        public int ComboProductoId { get; set; }
        public ComboProducto? ComboProducto { get; set; }

        /// <summary>
        /// El producto del inventario vinculado.
        /// </summary>
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
    }
}
