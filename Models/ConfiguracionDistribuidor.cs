using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Configuración del distribuidor/usuario de la aplicación.
    /// </summary>
    public class ConfiguracionDistribuidor
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del distribuidor.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Nombre del negocio.
        /// </summary>
        [MaxLength(200)]
        public string? NombreNegocio { get; set; }

        /// <summary>
        /// Teléfono de contacto.
        /// </summary>
        [MaxLength(50)]
        public string? Telefono { get; set; }

        /// <summary>
        /// Dirección del negocio.
        /// </summary>
        [MaxLength(500)]
        public string? Direccion { get; set; }

        /// <summary>
        /// Período de inventario activo actualmente.
        /// </summary>
        public int? PeriodoActualId { get; set; }
        public PeriodoInventario? PeriodoActual { get; set; }

        /// <summary>
        /// Prefijo para números de ficha (ej: "FC-").
        /// </summary>
        [MaxLength(20)]
        public string PrefijoFicha { get; set; } = "FC-";

        /// <summary>
        /// Último número de ficha usado.
        /// </summary>
        public int UltimoNumeroFicha { get; set; }

        /// <summary>
        /// Prefijo para números de conformidad.
        /// </summary>
        [MaxLength(20)]
        public string PrefijoConformidad { get; set; } = "CONF-";

        /// <summary>
        /// Último número de conformidad usado.
        /// </summary>
        public int UltimoNumeroConformidad { get; set; }

        /// <summary>
        /// Tasa de cambio: cuántos CUP equivale 1 USD.
        /// Ejemplo: 300 significa que 1 USD = 300 CUP.
        /// </summary>
        public decimal TasaCambioCUP { get; set; } = 300m;

        /// <summary>
        /// Ruta donde se guardan los reportes PDF generados.
        /// Si es null/vacío se usa %AppData%/GestionApp/Reportes/.
        /// </summary>
        [MaxLength(500)]
        public string? RutaReportes { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaModificacion { get; set; }
    }
}
