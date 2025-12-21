using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
            // Validaciones manuales adicionales
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido");
            }
            else if (cliente.Nombre.Length > 100)
            {
                ModelState.AddModelError("Nombre", "El nombre no puede exceder 100 caracteres");
            }

            if (string.IsNullOrWhiteSpace(cliente.Correo))
            {
                ModelState.AddModelError("Correo", "El correo electrónico es requerido");
            }
            else if (!cliente.Correo.Contains("@") || !cliente.Correo.Contains("."))
            {
                ModelState.AddModelError("Correo", "El correo electrónico no es válido");
            }
            else if (cliente.Correo.Length > 100)
            {
                ModelState.AddModelError("Correo", "El correo electrónico no puede exceder 100 caracteres");
            }

            if (string.IsNullOrWhiteSpace(cliente.Contraseña))
            {
                ModelState.AddModelError("Contraseña", "La contraseña es requerida");
            }
            else if (cliente.Contraseña.Length < 6)
            {
                ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 6 caracteres");
            }
            else if (cliente.Contraseña.Length > 255)
            {
                ModelState.AddModelError("Contraseña", "La contraseña no puede exceder 255 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Telefono) && cliente.Telefono.Length > 20)
            {
                ModelState.AddModelError("Telefono", "El teléfono no puede exceder 20 caracteres");
            }

            // FotoCI es opcional, no se valida

            if (ModelState.IsValid)
            {
                // Verificar si el correo ya existe
                var existe = await _context.Clientes
                    .AnyAsync(c => c.Correo == cliente.Correo);

                if (existe)
                {
                    ModelState.AddModelError("Correo", "Este correo electrónico ya está registrado");
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
                    cliente.FotoCI = null;
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
        public async Task<IActionResult> Perfil([Bind("IdCliente,Nombre,Correo,Telefono,Direccion,FechaRegistro")] Cliente cliente, IFormFile? fotoCI)
        {
            // Verificar autenticación
            if (!IsCliente())
            {
                return RedirectToAction("Login", "Account");
            }

            var clienteId = GetClienteId();
            
            // Si el IdCliente es 0 o no coincide, obtenerlo de la sesión
            if (cliente.IdCliente == 0 || cliente.IdCliente != clienteId)
            {
                cliente.IdCliente = clienteId;
            }
            
            // Verificar que el cliente esté editando su propio perfil
            if (cliente.IdCliente == 0 || cliente.IdCliente != clienteId)
            {
                TempData["Error"] = "Error de autenticación. Por favor, inicia sesión nuevamente.";
                return RedirectToAction("Login", "Account");
            }

            // Excluir Contraseña de la validación del ModelState ya que lo asignamos manualmente
            ModelState.Remove("Contraseña");
            ModelState.Remove("Verificado");
            ModelState.Remove("FotoCI");

            // Validaciones manuales adicionales
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido");
            }
            else if (cliente.Nombre.Length > 100)
            {
                ModelState.AddModelError("Nombre", "El nombre no puede exceder 100 caracteres");
            }

            if (string.IsNullOrWhiteSpace(cliente.Correo))
            {
                ModelState.AddModelError("Correo", "El correo electrónico es requerido");
            }
            else if (!cliente.Correo.Contains("@") || !cliente.Correo.Contains("."))
            {
                ModelState.AddModelError("Correo", "El correo electrónico no es válido");
            }
            else if (cliente.Correo.Length > 100)
            {
                ModelState.AddModelError("Correo", "El correo electrónico no puede exceder 100 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Telefono) && cliente.Telefono.Length > 20)
            {
                ModelState.AddModelError("Telefono", "El teléfono no puede exceder 20 caracteres");
            }

            // FotoCI es opcional, no se valida

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el cliente actual de la base de datos
                    var clienteActual = await _context.Clientes.FindAsync(cliente.IdCliente);
                    if (clienteActual == null)
                    {
                        return NotFound();
                    }

                    // Actualizar las propiedades directamente en la entidad rastreada
                    clienteActual.Nombre = cliente.Nombre;
                    clienteActual.Correo = cliente.Correo;
                    clienteActual.Telefono = cliente.Telefono;
                    clienteActual.Direccion = cliente.Direccion;
                    
                    // Procesar nueva foto del CI si se subió
                    if (fotoCI != null && fotoCI.Length > 0)
                    {
                        // Validar que realmente es un archivo de imagen válido
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(fotoCI.FileName).ToLowerInvariant();
                        
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("fotoCI", "Solo se permiten archivos de imagen (JPG, JPEG, PNG, GIF)");
                        }
                        else if (fotoCI.Length > 5 * 1024 * 1024) // 5MB
                        {
                            ModelState.AddModelError("fotoCI", "El archivo no puede exceder 5MB");
                        }
                        else
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
                            var fileName = $"cliente_{cliente.IdCliente}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            // Guardar archivo
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await fotoCI.CopyToAsync(stream);
                            }

                            // Guardar ruta relativa en la base de datos
                            clienteActual.FotoCI = $"/uploads/ci/{fileName}";
                            
                            // Solo si se subió un CI nuevo, se marca como verificado automáticamente
                            clienteActual.Verificado = true;
                        }
                    }
                    // Si no se subió nueva foto, NO cambiar el estado de verificación
                    // Mantener el estado actual (ya sea verificado o no)

                    // No necesitamos Update() porque estamos modificando la entidad rastreada directamente
                    await _context.SaveChangesAsync();

                    // Actualizar sesión
                    HttpContext.Session.SetString("ClienteNombre", clienteActual.Nombre);
                    HttpContext.Session.SetString("ClienteCorreo", clienteActual.Correo);

                    // Mensaje de éxito según si se subió imagen o no
                    if (fotoCI != null && fotoCI.Length > 0 && ModelState.IsValid)
                    {
                        TempData["Success"] = "Perfil actualizado correctamente. Tu cuenta ha sido verificada automáticamente.";
                    }
                    else
                    {
                        TempData["Success"] = "Perfil actualizado correctamente.";
                    }
                    
                    return RedirectToAction("Perfil");
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

            // Si hay errores de validación, recargar el cliente desde la BD y mostrar la vista con los errores
            var clienteParaVista = await _context.Clientes.FindAsync(cliente.IdCliente);
            if (clienteParaVista == null)
            {
                TempData["Error"] = "No se pudo encontrar el cliente. Por favor, inicia sesión nuevamente.";
                return RedirectToAction("Login", "Account");
            }
            
            // Mantener los valores del formulario pero con los datos completos de la BD
            clienteParaVista.Nombre = cliente.Nombre;
            clienteParaVista.Correo = cliente.Correo;
            clienteParaVista.Telefono = cliente.Telefono;
            clienteParaVista.Direccion = cliente.Direccion;
            
            // Mostrar errores de validación
            var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (errores.Any())
            {
                TempData["Error"] = string.Join(" ", errores);
            }
            else
            {
                TempData["Error"] = "Por favor, corrige los errores en el formulario.";
            }
            
            return View(clienteParaVista);
        }

        // ============================================
        // NUEVOS MÉTODOS PARA ClienteController.cs
        // 2 métodos  añadidos marco 18/12/2025
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
            // Solo mostrar productos con stock disponible (Stock > 0)
            var productosFavoritos = await _context.DetallesVenta
                .Include(d => d.Venta)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Categoria)
                .Where(d => d.Venta.IdCliente == clienteId && 
                           d.Venta.Estado == "confirmada" &&
                           d.Producto.Stock > 0) // Solo productos con stock disponible
                .GroupBy(d => d.Producto.IdProducto)
                .Select(g => new {
                    ProductoId = g.Key,
                    ProductoNombre = g.First().Producto.Nombre,
                    ProductoDescripcion = g.First().Producto.Descripcion,
                    Precio = g.First().Producto.PrecioVenta,
                    Stock = g.First().Producto.Stock,
                    CategoriaNombre = g.First().Producto.Categoria != null ? g.First().Producto.Categoria.Nombre : "Sin categoría",
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