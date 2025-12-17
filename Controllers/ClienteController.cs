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
        public async Task<IActionResult> Registro([Bind("Nombre,Correo,Contraseña,Telefono,Direccion")] Cliente cliente)
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

                cliente.Verificado = false;
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
        public async Task<IActionResult> Perfil(int id, [Bind("IdCliente,Nombre,Correo,Telefono,Direccion,Verificado,FechaRegistro")] Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mantener la contraseña actual si no se cambia
                    var clienteActual = await _context.Clientes.FindAsync(id);
                    if (clienteActual != null)
                    {
                        cliente.Contraseña = clienteActual.Contraseña;
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

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}
