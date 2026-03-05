using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Representa una agencia de envío.
    /// </summary>
    public class Agencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Direccion { get; set; }

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(500)]
        public string? Notas { get; set; }

        /// <summary>
        /// Ruta al logo de la agencia.
        /// Para agencias embebidas: "pack://application:,,,/Assets/Logos/rios.png"
        /// Para agencias añadidas: ruta en %AppData%/GestionApp/Logos/
        /// </summary>
        [MaxLength(500)]
        public string? LogoPath { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public bool Activo { get; set; } = true;
    }
}
