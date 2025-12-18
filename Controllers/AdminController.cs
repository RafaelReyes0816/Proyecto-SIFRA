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

        // ============================================
        // NUEVOS MÉTODOS PARA AdminController.cs 18/12/2025 marco
        // 3 metodos añadidos
        // ============================================

        public async Task<IActionResult> Reportes()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Reportes de ventas por mes (últimos 6 meses)
            var ventasPorMes = _context.Ventas
                .Where(v => v.Fecha >= DateTime.Now.AddMonths(-6) && v.Estado == "confirmada")
                .GroupBy(v => new { v.Fecha.Year, v.Fecha.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(v => v.Total),
                    Cantidad = g.Count()
                })
                .AsEnumerable() // 👈 CAMBIO: evita error SQL en MySQL
                .Select(x => new
                {
                    Mes = x.Month + "/" + x.Year,
                    x.Total,
                    x.Cantidad
                })
                .OrderBy(x => x.Mes)
                .ToList();

            // Top 10 productos más vendidos
            var topProductos = await _context.DetallesVenta
                .Include(d => d.Producto)
                .GroupBy(d => new { d.Producto.IdProducto, d.Producto.Nombre })
                .Select(g => new {
                    ProductoId = g.Key.IdProducto,
                    ProductoNombre = g.Key.Nombre,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVentas = g.Sum(d => d.PrecioUnitario * d.Cantidad)
                })
                .OrderByDescending(x => x.CantidadVendida)
                .Take(10)
                .ToListAsync();

            // Clientes con más compras
            var topClientes = await _context.Ventas
                .Include(v => v.Cliente)
                .Where(v => v.Estado == "confirmada")
                .GroupBy(v => new { v.Cliente.IdCliente, v.Cliente.Nombre })
                .Select(g => new {
                    ClienteId = g.Key.IdCliente,
                    ClienteNombre = g.Key.Nombre,
                    TotalCompras = g.Count(),
                    TotalGastado = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.TotalCompras)
                .Take(10)
                .ToListAsync();

            // Ventas por método de pago
            var ventasPorMetodo = await _context.Ventas
                .Where(v => v.Estado == "confirmada")
                .GroupBy(v => v.MetodoPago)
                .Select(g => new {
                    Metodo = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(v => v.Total)
                })
                .ToListAsync();

            var estadisticas = new
            {
                VentasTotales = await _context.Ventas.Where(v => v.Estado == "confirmada").CountAsync(),
                IngresosTotales = await _context.Ventas.Where(v => v.Estado == "confirmada").SumAsync(v => v.Total),
                PromedioVenta = await _context.Ventas.Where(v => v.Estado == "confirmada").AnyAsync()
                    ? await _context.Ventas.Where(v => v.Estado == "confirmada").AverageAsync(v => (double)v.Total)
                    : 0,
                VentasEsteMes = await _context.Ventas
                    .Where(v => v.Fecha.Month == DateTime.Now.Month &&
                                v.Fecha.Year == DateTime.Now.Year &&
                                v.Estado == "confirmada")
                    .CountAsync()
            };

            ViewBag.VentasPorMes = ventasPorMes;
            ViewBag.TopProductos = topProductos;
            ViewBag.TopClientes = topClientes;
            ViewBag.VentasPorMetodo = ventasPorMetodo;
            ViewBag.Estadisticas = estadisticas;

            return View();
        }

        public async Task<IActionResult> Actividad()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            // Últimas ventas
            var ultimasVentas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Vendedor)
                .OrderByDescending(v => v.Fecha)
                .Take(10)
                .ToListAsync();

            // Últimos usuarios registrados
            var ultimosUsuarios = await _context.Usuarios
                .OrderByDescending(u => u.FechaRegistro)
                .Take(10)
                .ToListAsync();

            // Últimos clientes registrados
            var ultimosClientes = await _context.Clientes
                .OrderByDescending(c => c.FechaRegistro)
                .Take(10)
                .ToListAsync();

            // Productos con cambios recientes (simulado con stock bajo)
            var productosRecientes = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.Stock)
                .Take(10)
                .ToListAsync();

            ViewBag.UltimasVentas = ultimasVentas;
            ViewBag.UltimosUsuarios = ultimosUsuarios;
            ViewBag.UltimosClientes = ultimosClientes;
            ViewBag.ProductosRecientes = productosRecientes;

            var estadisticas = new
            {
                VentasHoy = await _context.Ventas.Where(v => v.Fecha.Date == DateTime.Today).CountAsync(),
                UsuariosActivos = await _context.Usuarios.Where(u => u.Activo).CountAsync(),
                ClientesVerificados = await _context.Clientes.Where(c => c.Verificado).CountAsync(),
                ProductosBajoStock = await _context.Productos.Where(p => p.Stock <= p.StockMinimo).CountAsync()
            };

            ViewBag.Estadisticas = estadisticas;

            return View();
        }

        public async Task<IActionResult> Configuracion()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            var configuracion = new
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                UsuariosActivos = await _context.Usuarios.Where(u => u.Activo).CountAsync(),
                TotalClientes = await _context.Clientes.CountAsync(),
                ClientesVerificados = await _context.Clientes.Where(c => c.Verificado).CountAsync(),
                TotalProductos = await _context.Productos.CountAsync(),
                TotalCategorias = await _context.Categorias.CountAsync(),
                TotalProveedores = await _context.Proveedores.CountAsync(),
                VentasPendientes = await _context.Ventas.Where(v => v.Estado == "pendiente").CountAsync()
            };

            // Distribución de roles
            var distribucionRoles = await _context.Usuarios
                .GroupBy(u => u.Rol)
                .Select(g => new {
                    Rol = g.Key,
                    Cantidad = g.Count()
                })
                .ToListAsync();

            ViewBag.Configuracion = configuracion;
            ViewBag.DistribucionRoles = distribucionRoles;

            return View();
        }
    }
}
