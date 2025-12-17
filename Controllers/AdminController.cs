using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UsuarioRol") == "admin";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var productosBajoStock = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Stock <= p.StockMinimo)
                .OrderBy(p => p.Stock)
                .ToListAsync();

            var estadisticas = new
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                TotalClientes = await _context.Clientes.CountAsync(),
                TotalProductos = await _context.Productos.CountAsync(),
                TotalVentas = await _context.Ventas.CountAsync(),
                VentasHoy = await _context.Ventas
                    .Where(v => v.Fecha.Date == DateTime.Today)
                    .CountAsync(),
                ProductosBajoStock = productosBajoStock.Count,
                TotalIngresos = await _context.Ventas
                    .Where(v => v.Estado == "confirmada")
                    .SumAsync(v => v.Total)
            };

            ViewBag.Estadisticas = estadisticas;
            ViewBag.ProductosBajoStock = productosBajoStock;
            return View();
        }
    }
}
