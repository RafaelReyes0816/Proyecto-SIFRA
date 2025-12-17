using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tienda_Repuestos_Demo.Models
{
    public class Producto
    {
        [Key]
        [Column("id_producto")]
        public int IdProducto { get; set; }

        [MaxLength(50)]
        public string? Codigo { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("id_proveedor")]
        public int IdProveedor { get; set; }

        [Required]
        [Column("precio_compra", TypeName = "decimal(10,2)")]
        public decimal PrecioCompra { get; set; }

        [Required]
        [Column("precio_venta", TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        public int Stock { get; set; } = 0;

        [Column("stock_minimo")]
        public int StockMinimo { get; set; } = 5;

        // Navegación
        [ForeignKey("IdCategoria")]
        public Categoria Categoria { get; set; } = null!;

        [ForeignKey("IdProveedor")]
        public Proveedor Proveedor { get; set; } = null!;

        public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    }
}
