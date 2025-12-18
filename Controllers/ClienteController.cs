using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClienteController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsCliente()
        {
            return HttpContext.Session.GetString("UsuarioRol") == "cliente";
        }

        private int GetClienteId()
        {
            var clienteId = HttpContext.Session.GetString("ClienteId");
            return int.TryParse(clienteId, out int id) ? id : 0;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsCliente())
            {
                return RedirectToAction("Login", "Account");
            }

            var clienteId = GetClienteId();

            var cliente = await _context.Clientes
                .Include(c => c.Ventas)
                    .ThenInclude(v => v.DetallesVenta)
                        .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(c => c.IdCliente == clienteId);

            if (cliente == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var estadisticas = new
            {
                TotalCompras = cliente.Ventas.Count,
                ComprasPendientes = cliente.Ventas.Count(v => v.Estado == "pendiente"),
                ComprasConfirmadas = cliente.Ventas.Count(v => v.Estado == "confirmada"),
                TotalGastado = cliente.Ventas
                    .Where(v => v.Estado == "confirmada")
                    .Sum(v => v.Total)
            };

            ViewBag.Estadisticas = estadisticas;
            ViewBag.Cliente = cliente;
            ViewBag.VentasRecientes = cliente.Ventas
                .OrderByDescending(v => v.Fecha)
                .Take(5)
                .ToList();

            return View();
        }

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro([Bind("Nombre,Correo,Contraseña,Telefono,Direccion")] Cliente cliente, IFormFile? fotoCI)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el correo ya existe
                var existe = await _context.Clientes
                    .AnyAsync(c => c.Correo == cliente.Correo);

                if (existe)
                {
                    ViewBag.Error = "Este correo electrónico ya está registrado";
                    return View(cliente);
                }

                // Procesar foto del CI si se subió
                if (fotoCI != null && fotoCI.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ci");
                    
                    // Crear directorio si no existe
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generar nombre único para el archivo
                    var fileName = $"cliente_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(fotoCI.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Guardar archivo
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fotoCI.CopyToAsync(stream);
                    }

                    // Guardar ruta relativa en la base de datos
                    cliente.FotoCI = $"/uploads/ci/{fileName}";
                    
                    // Si subió el CI, se marca como verificado automáticamente
                    cliente.Verificado = true;
                }
                else
                {
                    // Si no subió CI, queda sin verificar
                    cliente.Verificado = false;
                }

                cliente.FechaRegistro = DateTime.Now;

                _context.Add(cliente);
                await _context.SaveChangesAsync();

                // Auto-login después del registro
                HttpContext.Session.SetString("ClienteId", cliente.IdCliente.ToString());
                HttpContext.Session.SetString("ClienteNombre", cliente.Nombre);
                HttpContext.Session.SetString("UsuarioRol", "cliente");
                HttpContext.Session.SetString("ClienteCorreo", cliente.Correo);

                return RedirectToAction("Dashboard");
            }

            return View(cliente);
        }

        public async Task<IActionResult> MisCompras()
        {
            if (!IsCliente())
            {
                return RedirectToAction("Login", "Account");
            }

            var clienteId = GetClienteId();

            var ventas = await _context.Ventas
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.IdCliente == clienteId)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            return View(ventas);
        }

        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            if (!IsCliente())
            {
                return RedirectToAction("Login", "Account");
            }

            var clienteId = GetClienteId();
            var cliente = await _context.Clientes.FindAsync(clienteId);

            if (cliente == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(int id, [Bind("IdCliente,Nombre,Correo,Telefono,Direccion,FechaRegistro")] Cliente cliente, IFormFile? fotoCI)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el cliente actual de la base de datos
                    var clienteActual = await _context.Clientes.FindAsync(id);
                    if (clienteActual == null)
                    {
                        return NotFound();
                    }

                    // Mantener la contraseña actual
                    cliente.Contraseña = clienteActual.Contraseña;
                    
                    // Preservar el estado de verificación actual por defecto
                    cliente.Verificado = clienteActual.Verificado;
                    cliente.FotoCI = clienteActual.FotoCI; // Mantener foto actual por defecto

                    // Procesar nueva foto del CI si se subió
                    if (fotoCI != null && fotoCI.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "ci");
                        
                        // Crear directorio si no existe
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Eliminar foto anterior si existe
                        if (!string.IsNullOrEmpty(clienteActual.FotoCI))
                        {
                            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", clienteActual.FotoCI.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Generar nombre único para el archivo
                        var fileName = $"cliente_{id}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(fotoCI.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Guardar archivo
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fotoCI.CopyToAsync(stream);
                        }

                        // Guardar ruta relativa en la base de datos
                        cliente.FotoCI = $"/uploads/ci/{fileName}";
                        
                        // Si subió un CI nuevo, se marca como verificado automáticamente
                        cliente.Verificado = true;
                    }

                    _context.Update(cliente);
                    await _context.SaveChangesAsync();

                    // Actualizar sesión
                    HttpContext.Session.SetString("ClienteNombre", cliente.Nombre);
                    HttpContext.Session.SetString("ClienteCorreo", cliente.Correo);

                    ViewBag.Success = "Perfil actualizado correctamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.IdCliente))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(cliente);
        }

        // ============================================
        // NUEVOS MÉTODOS PARA ClienteController.cs
        // Copia estos 2 métodos dentro de tu clase ClienteController
        // ============================================

        public async Task<IActionResult> Notificaciones()
        {
            if (!IsCliente())
            {
                return RedirectToAction("Login", "Account");
            }

            var clienteId = GetClienteId();

            // Obtener todas las ventas del cliente con sus detalles
            var ventas = await _context.Ventas
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.IdCliente == clienteId)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();

            // Crear notificaciones basadas en el estado de las ventas
            var notificaciones = ventas.Select(v => new
            {
                Tipo = v.Estado == "confirmada" ? "success" : 
                       v.Estado == "pendiente" ? "warning" : "danger",
                Mensaje = v.Estado == "confirmada" ? $"Tu compra #{v.IdVenta} ha sido confirmada" :
                         v.Estado == "pendiente" ? $"Tu compra #{v.IdVenta} está pendiente de confirmación" :
                         $"Tu compra #{v.IdVenta} ha sido cancelada",
                Fecha = v.Fecha,
                VentaId = v.IdVenta,
                Estado = v.Estado,
                Total = v.Total
            }).ToList();

            ViewBag.Notificaciones = notificaciones;
            ViewBag.TotalNotificaciones = notificaciones.Count;
            ViewBag.NotificacionesPendientes = notificaciones.Count(n => n.Estado == "pendiente");

            return View();
        }

        public async Task<IActionResult> Favoritos()
        {
            if (!IsCliente())
            {
                return RedirectToAction("Login", "Account");
            }

            var clienteId = GetClienteId();

            // Obtener los productos que el cliente ha comprado más frecuentemente
            var productosFavoritos = await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Categoria)
                .Where(d => d.Venta.IdCliente == clienteId && d.Venta.Estado == "confirmada")
                .GroupBy(d => new { 
                    d.Producto.IdProducto, 
                    ProductoNombre = d.Producto.Nombre,  // CORREGIDO: Nombre explícito
                    d.Producto.Descripcion,
                    d.Producto.PrecioVenta,
                    d.Producto.Stock,
                    CategoriaNombre = d.Producto.Categoria.Nombre // CORREGIDO: Nombre explícito
                })
                .Select(g => new {
                    ProductoId = g.Key.IdProducto,
                    ProductoNombre = g.Key.ProductoNombre, // Usamos el nombre corregido
                    ProductoDescripcion = g.Key.Descripcion,
                    Precio = g.Key.PrecioVenta,
                    Stock = g.Key.Stock,
                    CategoriaNombre = g.Key.CategoriaNombre, // Usamos el nombre corregido
                    CantidadComprada = g.Sum(d => d.Cantidad),
                    UltimaCompra = g.Max(d => d.Venta.Fecha),
                    TotalGastado = g.Sum(d => d.PrecioUnitario * d.Cantidad)
                })
                .OrderByDescending(x => x.CantidadComprada)
                .Take(20)
                .ToListAsync();

            ViewBag.ProductosFavoritos = productosFavoritos;
            ViewBag.TotalFavoritos = productosFavoritos.Count;

            return View();
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}