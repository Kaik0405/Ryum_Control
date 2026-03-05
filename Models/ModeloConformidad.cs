using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Representa una orden de entrega de un combo.
    /// Se crea al registrar un envío. Tiene plazo de 5 días para entregarse.
    /// FLUJO: Combo → Entrega (orden) → Ficha de Costo (después de entregado).
    /// </summary>
    public class Entrega
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Número de orden auto-generado (ej: ENT-00001).
        /// </summary>
        [MaxLength(50)]
        public string NumeroOrden { get; set; } = string.Empty;

        #region Combo

        /// <summary>
        /// Combo que se envía/entrega. Null si es remesa.
        /// </summary>
        public int? ComboId { get; set; }
        public Combo? Combo { get; set; }

        #endregion

        #region Remesa

        /// <summary>
        /// Indica si esta entrega es una remesa (envío de dinero) en vez de un combo.
        /// </summary>
        public bool EsRemesa { get; set; }

        /// <summary>
        /// Monto en CUP de la remesa. Solo aplica cuando EsRemesa = true.
        /// </summary>
        public decimal MontoRemesa { get; set; }

        #endregion

        #region Datos del receptor

        /// <summary>
        /// Nombre completo de quien recibe el paquete.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string NombreReceptor { get; set; } = string.Empty;

        /// <summary>
        /// Dirección de quien recibe (campo legacy, se compone de los campos estructurados).
        /// </summary>
        [MaxLength(500)]
        public string DireccionReceptor { get; set; } = string.Empty;

        /// <summary>
        /// Provincia de Cuba donde se entrega.
        /// </summary>
        [MaxLength(100)]
        public string Provincia { get; set; } = string.Empty;

        /// <summary>
        /// Municipio dentro de la provincia.
        /// </summary>
        [MaxLength(100)]
        public string Municipio { get; set; } = string.Empty;

        /// <summary>
        /// Localidad / reparto / zona dentro del municipio.
        /// </summary>
        [MaxLength(200)]
        public string Localidad { get; set; } = string.Empty;

        /// <summary>
        /// Dirección particular (calle, número, entre calles, etc.).
        /// </summary>
        [MaxLength(300)]
        public string DireccionParticular { get; set; } = string.Empty;

        /// <summary>
        /// Dirección completa formateada para mostrar.
        /// Formato: "DireccionParticular, Localidad, Municipio, Provincia"
        /// </summary>
        public string DireccionCompleta
        {
            get
            {
                var partes = new List<string>();
                if (!string.IsNullOrWhiteSpace(DireccionParticular)) partes.Add(DireccionParticular);
                if (!string.IsNullOrWhiteSpace(Localidad)) partes.Add(Localidad);
                if (!string.IsNullOrWhiteSpace(Municipio)) partes.Add(Municipio);
                if (!string.IsNullOrWhiteSpace(Provincia)) partes.Add(Provincia);
                return partes.Count > 0 ? string.Join(", ", partes) : DireccionReceptor;
            }
        }

        /// <summary>
        /// Provincia y municipio resumidos para la tabla.
        /// </summary>
        public string DireccionResumida
        {
            get
            {
                var partes = new List<string>();
                if (!string.IsNullOrWhiteSpace(Municipio)) partes.Add(Municipio);
                if (!string.IsNullOrWhiteSpace(Provincia)) partes.Add(Provincia);
                return partes.Count > 0 ? string.Join(", ", partes) : DireccionReceptor;
            }
        }

        /// <summary>
        /// Teléfono móvil del receptor.
        /// </summary>
        [MaxLength(50)]
        public string TelefonoMovil { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono fijo del receptor.
        /// </summary>
        [MaxLength(50)]
        public string TelefonoFijo { get; set; } = string.Empty;

        #endregion

        #region Datos del envío

        /// <summary>
        /// Persona que envía (remitente en el exterior).
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string NombreRemitente { get; set; } = string.Empty;

        /// <summary>
        /// Agencia usada para el envío.
        /// </summary>
        [MaxLength(200)]
        public string Agencia { get; set; } = string.Empty;

        #endregion

        #region Fechas y plazo

        /// <summary>
        /// Fecha en que se creó/registró la orden.
        /// Se asigna automáticamente al crear, pero se puede editar.
        /// </summary>
        public DateTime FechaOrden { get; set; } = DateTime.Now;

        /// <summary>
        /// Fecha en que se entregó efectivamente (null = aún no entregada).
        /// </summary>
        public DateTime? FechaEntregada { get; set; }

        /// <summary>
        /// Plazo máximo en días para la entrega (por defecto 5).
        /// </summary>
        public int PlazoDias { get; set; } = 5;

        /// <summary>
        /// Fecha límite calculada: FechaOrden + PlazoDias.
        /// </summary>
        public DateTime FechaLimite => FechaOrden.AddDays(PlazoDias);

        /// <summary>
        /// Días restantes para entregar. Negativo = vencido.
        /// </summary>
        public int DiasRestantes => (FechaLimite.Date - DateTime.Now.Date).Days;

        /// <summary>
        /// true si ya se venció el plazo y no se ha entregado.
        /// </summary>
        public bool Vencida => !Entregada && DiasRestantes < 0;

        /// <summary>
        /// true si le quedan 2 días o menos y no se ha entregado.
        /// </summary>
        public bool Urgente => !Entregada && DiasRestantes >= 0 && DiasRestantes <= 2;

        /// <summary>
        /// true si ya fue entregada.
        /// </summary>
        public bool Entregada => FechaEntregada.HasValue;

        #endregion

        /// <summary>
        /// Productos incluidos en la entrega.
        /// Copiados del combo al momento de crear la orden.
        /// </summary>
        public ICollection<EntregaProducto> Productos { get; set; } = new List<EntregaProducto>();

        /// <summary>
        /// Observaciones del envío.
        /// </summary>
        [MaxLength(1000)]
        public string? Observaciones { get; set; }

        /// <summary>
        /// Indica si ya se creó la Ficha de Costo para esta entrega.
        /// </summary>
        public bool TieneFichaCosto { get; set; }

        /// <summary>
        /// Resumen descriptivo.
        /// </summary>
        public string Resumen => EsRemesa 
            ? $"💵 Remesa {MontoRemesa:N0} CUP — {NombreReceptor}" 
            : $"{NombreReceptor} — Combo #{Combo?.Numero}";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Producto en una orden de entrega.
    /// Copia del ComboProducto al momento de crear la orden.
    /// </summary>
    public class EntregaProducto
    {
        [Key]
        public int Id { get; set; }

        public int EntregaId { get; set; }
        public Entrega? Entrega { get; set; }

        /// <summary>
        /// Nombre del producto (copiado del combo).
        /// </summary>
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
        /// Descripción formateada: "2 Libra" o "1 Paquete"
        /// </summary>
        public string Descripcion => $"{Cantidad:G} {Unidad}";
    }
}
