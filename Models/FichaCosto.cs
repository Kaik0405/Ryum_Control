using System.ComponentModel.DataAnnotations;

namespace GestionApp.Models
{
    /// <summary>
    /// Producto incluido en una ficha de costo específica.
    /// Permite editar cantidades sin afectar el combo original.
    /// </summary>
    public class FichaCostoProducto
    {
        [Key]
        public int Id { get; set; }

        public int FichaCostoId { get; set; }
        public FichaCosto? FichaCosto { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [MaxLength(200)]
        public string NombreProducto { get; set; } = string.Empty;

        public UnidadMedida Unidad { get; set; } = UnidadMedida.Unidad;

        public decimal Cantidad { get; set; }

        public decimal CostoUnitario { get; set; }

        /// <summary>
        /// Total: Cantidad * CostoUnitario
        /// </summary>
        public decimal Total => Cantidad * CostoUnitario;
    }

    /// <summary>
    /// Ficha de costo para un envío/venta.
    /// Contiene toda la información del pedido, destinatario y productos.
    /// </summary>
    public class FichaCosto
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Número de ficha para referencia.
        /// </summary>
        [MaxLength(50)]
        public string NumeroFicha { get; set; } = string.Empty;

        #region Combo

        /// <summary>
        /// Combo base seleccionado (puede ser null si es personalizado).
        /// </summary>
        public int? ComboId { get; set; }
        public Combo? Combo { get; set; }

        /// <summary>
        /// Tipo de pedido.
        /// </summary>
        public TipoCombo TipoPedido { get; set; } = TipoCombo.Combo;

        #endregion

        #region Personas involucradas

        /// <summary>
        /// Distribuidor (usuario que crea la ficha).
        /// </summary>
        [MaxLength(200)]
        public string Distribuidor { get; set; } = string.Empty;

        /// <summary>
        /// Persona que envía el paquete.
        /// </summary>
        [MaxLength(200)]
        public string Remitente { get; set; } = string.Empty;

        /// <summary>
        /// Cliente que recibe el envío.
        /// </summary>
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        [MaxLength(200)]
        public string NombreReceptor { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DireccionReceptor { get; set; } = string.Empty;

        [MaxLength(50)]
        public string TelefonoReceptor { get; set; } = string.Empty;

        #endregion

        #region Agencia y fechas

        public int? AgenciaId { get; set; }
        public Agencia? Agencia { get; set; }

        [MaxLength(200)]
        public string NombreAgencia { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; }

        public DateTime? FechaEntrega { get; set; }

        #endregion

        #region Productos y costos

        /// <summary>
        /// Productos incluidos en esta ficha.
        /// </summary>
        public ICollection<FichaCostoProducto> Productos { get; set; } = new List<FichaCostoProducto>();

        /// <summary>
        /// Precio de venta en USD.
        /// </summary>
        public decimal PrecioVentaUSD { get; set; }

        /// <summary>
        /// Costo total de los productos.
        /// </summary>
        public decimal CostoTotal => Productos?.Sum(p => p.Total) ?? 0;

        /// <summary>
        /// Ganancia: PrecioVenta - CostoTotal
        /// </summary>
        public decimal Ganancia => PrecioVentaUSD - CostoTotal;

        #endregion

        [MaxLength(1000)]
        public string? Notas { get; set; }

        /// <summary>
        /// Período de inventario al que pertenece esta ficha.
        /// </summary>
        public int? PeriodoInventarioId { get; set; }
        public PeriodoInventario? PeriodoInventario { get; set; }

        /// <summary>
        /// Modelo de conformidad asociado (si existe).
        /// </summary>
        public ModeloConformidad? ModeloConformidad { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
