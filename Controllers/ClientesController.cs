using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
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

            var cliente = await _context.Clientes
                .Include(c => c.Ventas)
                .FirstOrDefaultAsync(m => m.IdCliente == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            if (!IsAuthenticated() || HttpContext.Session.GetString("UsuarioRol") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Correo,Telefono,Direccion,Verificado")] Cliente cliente, string? contraseña)
        {
            // Asignar contraseña
            if (string.IsNullOrEmpty(contraseña))
            {
                cliente.Contraseña = "Cliente123!"; // Contraseña temporal por defecto
            }
            else
            {
                cliente.Contraseña = contraseña;
            }

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

                cliente.FechaRegistro = DateTime.Now;
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Edit/5
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

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,Nombre,Correo,Telefono,Direccion,FechaRegistro")] Cliente cliente, string? contraseña, string? Verificado)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            // Obtener el cliente actual de la base de datos SIN rastreo
            var clienteActual = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
            if (clienteActual == null)
            {
                return NotFound();
            }

            // Preservar la foto de CI actual
            cliente.FotoCI = clienteActual.FotoCI;

            // Manejar el estado de verificación desde el checkbox
            // Si Verificado viene como "true" (string), significa que el checkbox estaba marcado
            cliente.Verificado = Verificado == "true";

            // Manejar contraseña
            if (!string.IsNullOrEmpty(contraseña))
            {
                cliente.Contraseña = contraseña;
            }
            else
            {
                cliente.Contraseña = clienteActual.Contraseña;
            }

            // Limpiar errores de validación para campos que manejamos manualmente
            ModelState.Remove("Contraseña");
            ModelState.Remove("FotoCI");
            ModelState.Remove("Verificado");

            // Validar manualmente los campos requeridos
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido");
            }
            if (string.IsNullOrWhiteSpace(cliente.Correo))
            {
                ModelState.AddModelError("Correo", "El correo es requerido");
            }
            else if (!cliente.Correo.Contains("@"))
            {
                ModelState.AddModelError("Correo", "El correo no es válido");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cliente actualizado correctamente";
                    return RedirectToAction(nameof(Index));
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
                catch (Exception ex)
                {
                    ViewBag.Error = $"Error al guardar: {ex.Message}";
                    // Recargar el cliente para la vista
                    var clienteParaVista = await _context.Clientes.FindAsync(id);
                    return View(clienteParaVista ?? clienteActual);
                }
            }
            
            // Si hay errores de validación, mostrarlos
            var errorMessages = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            
            if (errorMessages.Any())
            {
                ViewBag.Error = "Por favor, corrige los siguientes errores: " + string.Join(", ", errorMessages);
            }
            
            // Recargar el cliente para la vista
            var clienteVista = await _context.Clientes.FindAsync(id);
            return View(clienteVista ?? clienteActual);
        }

        // GET: Clientes/Delete/5
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

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}
