using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tienda_Repuestos_Demo.Models
{
    public class Cliente
    {
        [Key]
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("contraseña")]
        public string Contraseña { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        public string? Direccion { get; set; }

        [MaxLength(255)]
        [Column("foto_ci")]
        public string? FotoCI { get; set; }

        public bool Verificado { get; set; } = false;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Navegación
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    }
}
