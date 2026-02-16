namespace GestionApp.Models
{
    /// <summary>
    /// DTO para mostrar la disponibilidad de stock de un producto del combo
    /// basándose en las vinculaciones con el inventario.
    /// </summary>
    public class ProductoDisponibilidad
    {
        public string NombreProducto { get; set; } = string.Empty;
        public decimal CantidadRequerida { get; set; }
        public string Unidad { get; set; } = string.Empty;
        public decimal StockDisponible { get; set; }
        public bool TieneVinculacion { get; set; }

        /// <summary>
        /// "Disponible", "Parcial", "Agotado", "Sin vincular"
        /// </summary>
        public string Estado { get; set; } = string.Empty;

        /// <summary>
        /// ✅, 🟡, 🔴, ⚪
        /// </summary>
        public string Icono { get; set; } = string.Empty;

        /// <summary>
        /// Color de fondo para el chip de estado.
        /// </summary>
        public string ColorFondo { get; set; } = string.Empty;

        /// <summary>
        /// Color de texto para el chip de estado.
        /// </summary>
        public string ColorTexto { get; set; } = string.Empty;

        /// <summary>
        /// Texto descriptivo: "10 Lb disponibles de 5 Lb requeridas"
        /// </summary>
        public string Detalle { get; set; } = string.Empty;
    }
}
