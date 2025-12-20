using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        private int GetUsuarioId()
        {
            var usuarioId = HttpContext.Session.GetString("UsuarioId");
            return int.TryParse(usuarioId, out int id) ? id : 0;
        }

        /// <summary>
        /// Genera un número de comprobante automático con formato COMP-YYYY-NNNN
        /// donde YYYY es el año y NNNN es un número secuencial de 4 dígitos
        /// </summary>
        private async Task<string> GenerarComprobanteAutomatico()
        {
            var añoActual = DateTime.Now.Year;
            var prefijo = $"COMP-{añoActual}-";

            // Buscar el último comprobante del año actual que siga el formato COMP-YYYY-NNNN
            var ultimoComprobante = await _context.Ventas
                .Where(v => v.ComprobantePago != null && 
                           v.ComprobantePago.StartsWith(prefijo))
                .OrderByDescending(v => v.ComprobantePago)
                .Select(v => v.ComprobantePago)
                .FirstOrDefaultAsync();

            int siguienteNumero = 1;

            if (ultimoComprobante != null)
            {
                // Extraer el número del último comprobante
                // Formato esperado: COMP-YYYY-NNNN
                var partes = ultimoComprobante.Split('-');
                if (partes.Length >= 3 && int.TryParse(partes[2], out int ultimoNumero))
                {
                    siguienteNumero = ultimoNumero + 1;
                }
            }

            // Generar el nuevo comprobante con formato COMP-YYYY-NNNN (NNNN con 4 dígitos)
            return $"{prefijo}{siguienteNumero:D4}";
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            var rol = HttpContext.Session.GetString("UsuarioRol");
            var usuarioId = GetUsuarioId();

            IQueryable<Venta> ventas = _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Vendedor);

            // Si es vendedor, solo ver sus ventas
            if (rol == "vendedor")
            {
                ventas = ventas.Where(v => v.IdVendedor == usuarioId);
            }

            return View(await ventas.OrderByDescending(v => v.Fecha).ToListAsync());
        }

        // GET: Ventas/Details/5
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

            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Vendedor)
                .Include(v => v.DetallesVenta)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            // Verificar que el vendedor solo vea sus propias ventas
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "vendedor" && venta.IdVendedor != GetUsuarioId())
            {
                return RedirectToAction("Index");
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public async Task<IActionResult> Create()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["IdCliente"] = new SelectList(await _context.Clientes.ToListAsync(), "IdCliente", "Nombre");
            
            var productos = await _context.Productos
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
            
            ViewData["Productos"] = new SelectList(productos, "IdProducto", "Nombre");
            
            // Pasar datos de productos para JavaScript
            var productosJson = productos.Select(p => new {
                id = p.IdProducto,
                nombre = p.Nombre,
                precio = p.PrecioVenta,
                stock = p.Stock,
                codigo = p.Codigo ?? ""
            }).ToList();
            
            ViewBag.ProductosJson = System.Text.Json.JsonSerializer.Serialize(productosJson);
            
            return View();
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,TipoVenta,MetodoPago,Total")] Venta venta, int[]? productos, int[]? cantidades, string? comprobantePago)
        {
            // Cargar ViewData antes de validar (necesario si hay errores)
            ViewData["IdCliente"] = new SelectList(await _context.Clientes.ToListAsync(), "IdCliente", "Nombre", venta.IdCliente);
            ViewData["Productos"] = new SelectList(await _context.Productos
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToListAsync(), "IdProducto", "Nombre");

            // Remover errores de validación de propiedades de navegación (solo validamos los IDs)
            ModelState.Remove("Cliente");
            ModelState.Remove("Vendedor");
            ModelState.Remove("DetallesVenta");
            ModelState.Remove("IdVendedor");
            ModelState.Remove("Estado");
            ModelState.Remove("Fecha");

            // Validaciones manuales adicionales
            if (venta.IdCliente <= 0)
            {
                ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente");
            }
            else
            {
                // Verificar que el cliente existe
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == venta.IdCliente);
                if (!clienteExiste)
                {
                    ModelState.AddModelError("IdCliente", "El cliente seleccionado no existe");
                }
            }

            if (string.IsNullOrWhiteSpace(venta.TipoVenta))
            {
                ModelState.AddModelError("TipoVenta", "El tipo de venta es requerido");
            }
            else if (venta.TipoVenta != "presencial" && venta.TipoVenta != "web")
            {
                ModelState.AddModelError("TipoVenta", "El tipo de venta debe ser 'presencial' o 'web'");
            }
            else if (venta.TipoVenta.Length > 20)
            {
                ModelState.AddModelError("TipoVenta", "El tipo de venta no puede exceder 20 caracteres");
            }

            if (string.IsNullOrWhiteSpace(venta.MetodoPago))
            {
                ModelState.AddModelError("MetodoPago", "El método de pago es requerido");
            }
            else if (venta.MetodoPago != "efectivo" && venta.MetodoPago != "qr" && venta.MetodoPago != "transferencia")
            {
                ModelState.AddModelError("MetodoPago", "El método de pago debe ser 'efectivo', 'qr' o 'transferencia'");
            }
            else if (venta.MetodoPago.Length > 20)
            {
                ModelState.AddModelError("MetodoPago", "El método de pago no puede exceder 20 caracteres");
            }

            // Validar productos y cantidades
            if (productos == null || productos.Length == 0)
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto a la venta");
            }
            else if (cantidades == null || cantidades.Length != productos.Length)
            {
                ModelState.AddModelError("", "Debe especificar la cantidad para cada producto");
            }
            else
            {
                // Validar cada producto y cantidad
                for (int i = 0; i < productos.Length; i++)
                {
                    var productoId = productos[i];
                    var cantidad = cantidades[i];

                    if (productoId <= 0)
                    {
                        ModelState.AddModelError("", $"El producto en la posición {i + 1} no es válido");
                        continue;
                    }

                    var producto = await _context.Productos.FindAsync(productoId);
                    if (producto == null)
                    {
                        ModelState.AddModelError("", $"El producto en la posición {i + 1} no existe");
                        continue;
                    }

                    if (cantidad <= 0)
                    {
                        ModelState.AddModelError("", $"La cantidad del producto '{producto.Nombre}' debe ser mayor a 0");
                        continue;
                    }

                    if (cantidad > producto.Stock)
                    {
                        ModelState.AddModelError("", $"No hay suficiente stock para '{producto.Nombre}'. Stock disponible: {producto.Stock}, solicitado: {cantidad}");
                    }
                }

                // Calcular total automáticamente si no se proporcionó o es 0
                if (venta.Total <= 0)
                {
                    decimal totalCalculado = 0;
                    for (int i = 0; i < productos.Length; i++)
                    {
                        var producto = await _context.Productos.FindAsync(productos[i]);
                        if (producto != null)
                        {
                            totalCalculado += producto.PrecioVenta * cantidades[i];
                        }
                    }
                    venta.Total = totalCalculado;
                }
            }

            if (venta.Total <= 0)
            {
                ModelState.AddModelError("Total", "El total debe ser mayor a 0");
            }
            else if (venta.Total > 999999.99m)
            {
                ModelState.AddModelError("Total", "El total no puede exceder 999,999.99");
            }

            // Manejar comprobante de pago (puede venir del formulario o generarse automáticamente)
            if (!string.IsNullOrWhiteSpace(comprobantePago))
            {
                if (comprobantePago.Length > 255)
                {
                    ModelState.AddModelError("ComprobantePago", "El comprobante de pago no puede exceder 255 caracteres");
                }
                else
                {
                    venta.ComprobantePago = comprobantePago;
                }
            }

            if (ModelState.IsValid)
            {
                venta.IdVendedor = GetUsuarioId();
                venta.Fecha = DateTime.Now;
                
                // Establecer estado según el método de pago
                // QR y Transferencia se confirman automáticamente (pago digital verificado)
                // Efectivo queda pendiente para verificación manual del admin
                if (venta.MetodoPago == "qr" || venta.MetodoPago == "transferencia")
                {
                    venta.Estado = "confirmada";
                }
                else if (venta.MetodoPago == "efectivo")
                {
                    venta.Estado = "pendiente";
                }
                else
                {
                    // Por defecto, pendiente si el método de pago no es reconocido
                    venta.Estado = "pendiente";
                }
                
                // Generar comprobante automático si no se proporcionó uno
                if (string.IsNullOrWhiteSpace(venta.ComprobantePago))
                {
                    venta.ComprobantePago = await GenerarComprobanteAutomatico();
                }

                _context.Add(venta);
                await _context.SaveChangesAsync();

                // Crear detalles de venta y actualizar stock
                if (productos != null && cantidades != null)
                {
                    for (int i = 0; i < productos.Length; i++)
                    {
                        var producto = await _context.Productos.FindAsync(productos[i]);
                        if (producto != null && i < cantidades.Length)
                    {
                        // Verificar stock nuevamente antes de crear el detalle
                        if (cantidades[i] > producto.Stock)
                        {
                            ModelState.AddModelError("", $"No hay suficiente stock para '{producto.Nombre}'. Stock disponible: {producto.Stock}, solicitado: {cantidades[i]}");
                            ViewData["IdCliente"] = new SelectList(await _context.Clientes.ToListAsync(), "IdCliente", "Nombre", venta.IdCliente);
                            ViewData["Productos"] = new SelectList(await _context.Productos
                                .Where(p => p.Stock > 0)
                                .OrderBy(p => p.Nombre)
                                .ToListAsync(), "IdProducto", "Nombre");
                            return View(venta);
                        }

                        // Crear detalle de venta
                        var detalleVenta = new DetalleVenta
                        {
                            IdVenta = venta.IdVenta,
                            IdProducto = producto.IdProducto,
                            Cantidad = cantidades[i],
                            PrecioUnitario = producto.PrecioVenta
                        };
                        _context.DetallesVenta.Add(detalleVenta);

                        // Reducir stock
                        producto.Stock -= cantidades[i];
                        _context.Productos.Update(producto);
                    }
                }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Venta creada correctamente";
                return RedirectToAction(nameof(Index));
            }

            ViewData["IdCliente"] = new SelectList(await _context.Clientes.ToListAsync(), "IdCliente", "Nombre", venta.IdCliente);
            ViewData["Productos"] = new SelectList(await _context.Productos
                .Where(p => p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToListAsync(), "IdProducto", "Nombre");
            return View(venta);
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }

            // Verificar permisos
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "vendedor" && venta.IdVendedor != GetUsuarioId())
            {
                return RedirectToAction("Index");
            }

            ViewData["IdCliente"] = new SelectList(await _context.Clientes.ToListAsync(), "IdCliente", "Nombre", venta.IdCliente);
            return View(venta);
        }

        // POST: Ventas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdVenta,IdCliente,IdVendedor,TipoVenta,Estado,MetodoPago,ComprobantePago,Total")] Venta venta)
        {
            if (id != venta.IdVenta)
            {
                return NotFound();
            }

            // Cargar ViewData antes de validar (necesario si hay errores)
            ViewData["IdCliente"] = new SelectList(await _context.Clientes.ToListAsync(), "IdCliente", "Nombre", venta.IdCliente);

            // Remover errores de validación de propiedades de navegación (solo validamos los IDs)
            ModelState.Remove("Cliente");
            ModelState.Remove("Vendedor");
            ModelState.Remove("DetallesVenta");
            ModelState.Remove("Fecha");

            // Verificar permisos
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol == "vendedor" && venta.IdVendedor != GetUsuarioId())
            {
                TempData["Error"] = "No tienes permisos para editar esta venta";
                return RedirectToAction(nameof(Index));
            }

            // Validaciones manuales adicionales
            if (venta.IdCliente <= 0)
            {
                ModelState.AddModelError("IdCliente", "Debe seleccionar un cliente");
            }
            else
            {
                // Verificar que el cliente existe
                var clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == venta.IdCliente);
                if (!clienteExiste)
                {
                    ModelState.AddModelError("IdCliente", "El cliente seleccionado no existe");
                }
            }

            if (venta.IdVendedor <= 0)
            {
                ModelState.AddModelError("IdVendedor", "Debe seleccionar un vendedor");
            }
            else
            {
                // Verificar que el vendedor existe
                var vendedorExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == venta.IdVendedor);
                if (!vendedorExiste)
                {
                    ModelState.AddModelError("IdVendedor", "El vendedor seleccionado no existe");
                }
            }

            if (string.IsNullOrWhiteSpace(venta.TipoVenta))
            {
                ModelState.AddModelError("TipoVenta", "El tipo de venta es requerido");
            }
            else if (venta.TipoVenta != "presencial" && venta.TipoVenta != "web")
            {
                ModelState.AddModelError("TipoVenta", "El tipo de venta debe ser 'presencial' o 'web'");
            }
            else if (venta.TipoVenta.Length > 20)
            {
                ModelState.AddModelError("TipoVenta", "El tipo de venta no puede exceder 20 caracteres");
            }

            if (string.IsNullOrWhiteSpace(venta.Estado))
            {
                ModelState.AddModelError("Estado", "El estado es requerido");
            }
            else if (venta.Estado != "pendiente" && venta.Estado != "confirmada" && venta.Estado != "cancelada")
            {
                ModelState.AddModelError("Estado", "El estado debe ser 'pendiente', 'confirmada' o 'cancelada'");
            }
            else if (venta.Estado.Length > 20)
            {
                ModelState.AddModelError("Estado", "El estado no puede exceder 20 caracteres");
            }

            if (string.IsNullOrWhiteSpace(venta.MetodoPago))
            {
                ModelState.AddModelError("MetodoPago", "El método de pago es requerido");
            }
            else if (venta.MetodoPago != "efectivo" && venta.MetodoPago != "qr" && venta.MetodoPago != "transferencia")
            {
                ModelState.AddModelError("MetodoPago", "El método de pago debe ser 'efectivo', 'qr' o 'transferencia'");
            }
            else if (venta.MetodoPago.Length > 20)
            {
                ModelState.AddModelError("MetodoPago", "El método de pago no puede exceder 20 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(venta.ComprobantePago) && venta.ComprobantePago.Length > 255)
            {
                ModelState.AddModelError("ComprobantePago", "El comprobante de pago no puede exceder 255 caracteres");
            }
            
            // Obtener la venta actual para comparar método de pago
            var ventaActual = await _context.Ventas.AsNoTracking().FirstOrDefaultAsync(v => v.IdVenta == id);
            if (ventaActual != null)
            {
                // Si se cambió el método de pago a QR o Transferencia, confirmar automáticamente
                // (solo si el método de pago cambió y la venta no estaba cancelada)
                if (venta.Estado != "cancelada" && 
                    ventaActual.MetodoPago != venta.MetodoPago &&
                    (venta.MetodoPago == "qr" || venta.MetodoPago == "transferencia"))
                {
                    venta.Estado = "confirmada";
                }
                // Si se cambió a efectivo y estaba confirmada, permitir que el admin decida
                // (mantener el estado que el admin seleccionó)
            }
            
            // Si no hay comprobante, generar uno automáticamente
            if (string.IsNullOrWhiteSpace(venta.ComprobantePago))
            {
                venta.ComprobantePago = await GenerarComprobanteAutomatico();
            }

            if (venta.Total <= 0)
            {
                ModelState.AddModelError("Total", "El total debe ser mayor a 0");
            }
            else if (venta.Total > 999999.99m)
            {
                ModelState.AddModelError("Total", "El total no puede exceder 999,999.99");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(venta);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Venta actualizada correctamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VentaExists(venta.IdVenta))
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

            ViewData["IdCliente"] = new SelectList(await _context.Clientes.ToListAsync(), "IdCliente", "Nombre", venta.IdCliente);
            return View(venta);
        }

        private bool VentaExists(int id)
        {
            return _context.Ventas.Any(e => e.IdVenta == id);
        }
    }
}
