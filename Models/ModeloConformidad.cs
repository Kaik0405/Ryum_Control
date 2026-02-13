using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Modelo de conformidad del cliente.
    /// Documento que el cliente firma al recibir el pedido.
    /// Similar a la ficha de costo pero sin información de precios.
    /// </summary>
    public class ModeloConformidad
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Número de conformidad para referencia.
        /// </summary>
        [MaxLength(50)]
        public string NumeroConformidad { get; set; } = string.Empty;

        /// <summary>
        /// Ficha de costo relacionada (opcional).
        /// </summary>
        public int? FichaCostoId { get; set; }
        public FichaCosto? FichaCosto { get; set; }

        #region Datos del receptor

        [Required]
        [MaxLength(200)]
        public string NombreReceptor { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DireccionReceptor { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TelefonoReceptor { get; set; } = string.Empty;

        #endregion

        #region Datos del envío

        [MaxLength(200)]
        public string NombreRemitente { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Agencia { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; }

        public DateTime? FechaRecepcion { get; set; }

        #endregion

        /// <summary>
        /// Productos incluidos en el documento de conformidad.
        /// Solo muestra producto y cantidad, sin precios.
        /// </summary>
        public ICollection<ConformidadProducto> Productos { get; set; } = new List<ConformidadProducto>();

        #region Conformidad

        /// <summary>
        /// Indica si el cliente confirmó la recepción.
        /// </summary>
        public bool Confirmado { get; set; }

        /// <summary>
        /// Fecha de confirmación.
        /// </summary>
        public DateTime? FechaConfirmacion { get; set; }

        /// <summary>
        /// Firma o nombre de quien recibe (texto).
        /// </summary>
        [MaxLength(200)]
        public string? FirmaReceptor { get; set; }

        /// <summary>
        /// Observaciones del receptor.
        /// </summary>
        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        #endregion

        [MaxLength(500)]
        public string? Notas { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Producto en el modelo de conformidad.
    /// Solo contiene producto y cantidad, sin precios.
    /// </summary>
    public class ConformidadProducto
    {
        [Key]
        public int Id { get; set; }

        public int ModeloConformidadId { get; set; }
        public ModeloConformidad? ModeloConformidad { get; set; }

        [Required]
        [MaxLength(200)]
        public string NombreProducto { get; set; } = string.Empty;

        /// <summary>
        /// Unidad de medida.
        /// </summary>
        public UnidadMedida Unidad { get; set; } = UnidadMedida.Unidad;

        /// <summary>
        /// Cantidad o peso del producto.
        /// </summary>
        public decimal Cantidad { get; set; }

        /// <summary>
        /// Estado del producto al recibir (opcional).
        /// </summary>
        [MaxLength(200)]
        public string? Estado { get; set; }
    }
}
