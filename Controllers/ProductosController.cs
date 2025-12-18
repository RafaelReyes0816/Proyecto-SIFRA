using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            // Verificar si hay sesión de usuario o cliente
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId")) ||
                   !string.IsNullOrEmpty(HttpContext.Session.GetString("ClienteId"));
        }

        // GET: Productos
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Where(p => p.Stock > 0) // Solo productos con stock disponible
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var rol = HttpContext.Session.GetString("UsuarioRol");
            
            // Si es cliente, usar vista de catálogo
            if (rol == "cliente")
            {
                ViewBag.Categorias = await _context.Categorias.ToListAsync();
                return View("Catalogo", productos);
            }

            // Para admin y vendedor, vista administrativa
            return View(productos);
        }

        // GET: Productos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(m => m.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productos/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAuthenticated() || HttpContext.Session.GetString("UsuarioRol") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre");
            ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre");
            return View();
        }

        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,Nombre,Descripcion,IdCategoria,IdProveedor,PrecioCompra,PrecioVenta,Stock,StockMinimo")] Producto producto)
        {
            // Cargar ViewData antes de validar (necesario si hay errores)
            ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
            ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);

            // Remover errores de validación de propiedades de navegación (solo validamos los IDs)
            ModelState.Remove("Categoria");
            ModelState.Remove("Proveedor");

            // Validación manual adicional
            if (producto.IdCategoria <= 0)
            {
                ModelState.AddModelError("IdCategoria", "Debe seleccionar una categoría");
            }

            if (producto.IdProveedor <= 0)
            {
                ModelState.AddModelError("IdProveedor", "Debe seleccionar un proveedor");
            }

            if (producto.PrecioCompra < 0)
            {
                ModelState.AddModelError("PrecioCompra", "El precio de compra no puede ser negativo");
            }

            if (producto.PrecioVenta < 0)
            {
                ModelState.AddModelError("PrecioVenta", "El precio de venta no puede ser negativo");
            }

            if (producto.Stock < 0)
            {
                ModelState.AddModelError("Stock", "El stock no puede ser negativo");
            }

            if (producto.StockMinimo < 0)
            {
                ModelState.AddModelError("StockMinimo", "El stock mínimo no puede ser negativo");
            }

            if (ModelState.IsValid)
            {
                // Verificar si el código ya existe (si se proporcionó)
                if (!string.IsNullOrEmpty(producto.Codigo))
                {
                    var codigoExiste = await _context.Productos
                        .AnyAsync(p => p.Codigo == producto.Codigo);
                    
                    if (codigoExiste)
                    {
                        ModelState.AddModelError("Codigo", "Ya existe un producto con este código");
                        ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
                        ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
                        return View(producto);
                    }
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto creado correctamente";
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
            ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
            return View(producto);
        }

        // GET: Productos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated() || HttpContext.Session.GetString("UsuarioRol") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
            ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
            return View(producto);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProducto,Codigo,Nombre,Descripcion,IdCategoria,IdProveedor,PrecioCompra,PrecioVenta,Stock,StockMinimo")] Producto producto)
        {
            if (id != producto.IdProducto)
            {
                return NotFound();
            }

            // Cargar ViewData antes de validar (necesario si hay errores)
            ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
            ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);

            // Remover errores de validación de propiedades de navegación (solo validamos los IDs)
            ModelState.Remove("Categoria");
            ModelState.Remove("Proveedor");

            // Validación manual adicional
            if (producto.IdCategoria <= 0)
            {
                ModelState.AddModelError("IdCategoria", "Debe seleccionar una categoría");
            }

            if (producto.IdProveedor <= 0)
            {
                ModelState.AddModelError("IdProveedor", "Debe seleccionar un proveedor");
            }

            if (producto.PrecioCompra < 0)
            {
                ModelState.AddModelError("PrecioCompra", "El precio de compra no puede ser negativo");
            }

            if (producto.PrecioVenta < 0)
            {
                ModelState.AddModelError("PrecioVenta", "El precio de venta no puede ser negativo");
            }

            if (producto.Stock < 0)
            {
                ModelState.AddModelError("Stock", "El stock no puede ser negativo");
            }

            if (producto.StockMinimo < 0)
            {
                ModelState.AddModelError("StockMinimo", "El stock mínimo no puede ser negativo");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el código ya existe en otro producto
                    if (!string.IsNullOrEmpty(producto.Codigo))
                    {
                        var codigoExiste = await _context.Productos
                            .AnyAsync(p => p.Codigo == producto.Codigo && p.IdProducto != id);
                        
                        if (codigoExiste)
                        {
                            ModelState.AddModelError("Codigo", "Ya existe otro producto con este código");
                            ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
                            ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
                            return View(producto);
                        }
                    }

                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto actualizado correctamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.IdProducto))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdCategoria"] = new SelectList(await _context.Categorias.ToListAsync(), "IdCategoria", "Nombre", producto.IdCategoria);
            ViewData["IdProveedor"] = new SelectList(await _context.Proveedores.ToListAsync(), "IdProveedor", "Nombre", producto.IdProveedor);
            return View(producto);
        }

        // GET: Productos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAuthenticated() || HttpContext.Session.GetString("UsuarioRol") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .FirstOrDefaultAsync(m => m.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Productos/Comprar/5 - Para clientes
        public async Task<IActionResult> Comprar(int? id)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "cliente")
            {
                return RedirectToAction("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.IdProducto == id);

            if (producto == null)
            {
                return NotFound();
            }

            if (producto.Stock <= 0)
            {
                TempData["Error"] = "Este producto no tiene stock disponible";
                return RedirectToAction("Index");
            }

            ViewBag.Producto = producto;
            return View();
        }

        // POST: Productos/Comprar/5 - Procesar compra del cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comprar(int id, int cantidad, string metodoPago)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "cliente")
            {
                return RedirectToAction("Index");
            }

            var clienteIdStr = HttpContext.Session.GetString("ClienteId");
            if (string.IsNullOrEmpty(clienteIdStr) || !int.TryParse(clienteIdStr, out int clienteId))
            {
                TempData["Error"] = "Debes iniciar sesión como cliente";
                return RedirectToAction("Login", "Account");
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            if (producto.Stock < cantidad)
            {
                TempData["Error"] = $"Solo hay {producto.Stock} unidades disponibles";
                return RedirectToAction("Comprar", new { id });
            }

            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a 0";
                return RedirectToAction("Comprar", new { id });
            }

            // Obtener un vendedor por defecto (el primero activo)
            var vendedor = await _context.Usuarios
                .Where(u => u.Rol == "vendedor" && u.Activo)
                .FirstOrDefaultAsync();

            if (vendedor == null)
            {
                TempData["Error"] = "No hay vendedores disponibles en el sistema";
                return RedirectToAction("Comprar", new { id });
            }

            // Calcular total
            var total = producto.PrecioVenta * cantidad;

            // Crear la venta
            var venta = new Venta
            {
                IdCliente = clienteId,
                IdVendedor = vendedor.IdUsuario,
                TipoVenta = "web",
                Estado = "pendiente",
                MetodoPago = metodoPago ?? "transferencia",
                Total = total,
                Fecha = DateTime.Now
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            // Crear el detalle de venta
            var detalleVenta = new DetalleVenta
            {
                IdVenta = venta.IdVenta,
                IdProducto = producto.IdProducto,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta
            };

            _context.DetallesVenta.Add(detalleVenta);

            // Reducir stock
            producto.Stock -= cantidad;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Compra realizada exitosamente. Total: ${total:N2}";
            return RedirectToAction("MisCompras", "Cliente");
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.IdProducto == id);
        }
    }
}
