using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class VendedorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendedorController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsVendedor()
        {
            return HttpContext.Session.GetString("UsuarioRol") == "vendedor";
        }

        private int GetUsuarioId()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            return int.TryParse(usuarioId, out int id) ? id : 0;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsVendedor())
            {
                return RedirectToAction("Login", "Account");
            }

            var usuarioId = GetUsuarioId();

            var estadisticas = new
            {
                VentasHoy = await _context.Ventas
                    .Where(v => v.IdVendedor == usuarioId && v.Fecha.Date == DateTime.Today)
                    .CountAsync(),
                VentasMes = await _context.Ventas
                    .Where(v => v.IdVendedor == usuarioId && 
                                v.Fecha.Month == DateTime.Now.Month && 
                                v.Fecha.Year == DateTime.Now.Year)
                    .CountAsync(),
                TotalVentas = await _context.Ventas
                    .Where(v => v.IdVendedor == usuarioId)
                    .CountAsync(),
                IngresosMes = await _context.Ventas
                    .Where(v => v.IdVendedor == usuarioId && 
                                v.Estado == "confirmada" &&
                                v.Fecha.Month == DateTime.Now.Month && 
                                v.Fecha.Year == DateTime.Now.Year)
                    .SumAsync(v => v.Total)
            };

            ViewBag.Estadisticas = estadisticas;

            var ventasRecientes = await _context.Ventas
                .Include(v => v.Cliente)
                .Where(v => v.IdVendedor == usuarioId)
                .OrderByDescending(v => v.Fecha)
                .Take(5)
                .ToListAsync();

            ViewBag.VentasRecientes = ventasRecientes;

            return View();
        }
    }
}
