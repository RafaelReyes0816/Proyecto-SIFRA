using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tienda_Repuestos_Demo.Models
{
    public class Venta
    {
        [Key]
        [Column("id_venta")]
        public int IdVenta { get; set; }

        [Required]
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Required]
        [Column("id_vendedor")]
        public int IdVendedor { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("tipo_venta")]
        public string TipoVenta { get; set; } = string.Empty; // 'presencial' o 'web'

        public DateTime Fecha { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Estado { get; set; } = "pendiente"; // 'pendiente', 'confirmada', 'cancelada'

        [Required]
        [MaxLength(20)]
        [Column("metodo_pago")]
        public string MetodoPago { get; set; } = string.Empty; // 'efectivo', 'qr', 'transferencia'

        [MaxLength(255)]
        [Column("comprobante_pago")]
        public string? ComprobantePago { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        // Navegación
        [ForeignKey("IdCliente")]
        public Cliente Cliente { get; set; } = null!;

        [ForeignKey("IdVendedor")]
        public Usuario Vendedor { get; set; } = null!;

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
