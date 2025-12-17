using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Tienda_Repuestos_Demo.Models
{
    public class Reporte
    {
        [Key]
        [Column("id_reporte")]
        public int IdReporte { get; set; }

        [Required]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Column(TypeName = "json")]
        public string? Contenido { get; set; }

        [Column("fecha_generacion")]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        // Navegación
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;
    }
}
