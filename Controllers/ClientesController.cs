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

            // Remover validación de campos que manejamos manualmente
            ModelState.Remove("Contraseña");
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
                // Verificar si el correo ya existe
                var existe = await _context.Clientes
                    .AnyAsync(c => c.Correo == cliente.Correo);

                if (existe)
                {
                    ModelState.AddModelError("Correo", "Este correo electrónico ya está registrado");
                    return View(cliente);
                }

                cliente.FechaRegistro = DateTime.Now;
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cliente creado correctamente";
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

            // Obtener el cliente actual de la base de datos sin tracking
            var clienteActual = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(c => c.IdCliente == id);
            if (clienteActual == null)
            {
                return NotFound();
            }

            // Preservar la contraseña actual si no se proporciona una nueva
            if (string.IsNullOrEmpty(contraseña))
            {
                cliente.Contraseña = clienteActual.Contraseña;
            }
            else
            {
                cliente.Contraseña = contraseña;
            }

            // Preservar la foto CI actual
            cliente.FotoCI = clienteActual.FotoCI;

            // Manejar el estado de verificación desde el checkbox
            cliente.Verificado = Verificado == "true";

            // Remover validación de campos que manejamos manualmente
            ModelState.Remove("Contraseña");
            ModelState.Remove("FotoCI");
            ModelState.Remove("Verificado");

            // Validación manual básica
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido");
            }

            if (string.IsNullOrWhiteSpace(cliente.Correo) || !cliente.Correo.Contains("@"))
            {
                ModelState.AddModelError("Correo", "El correo electrónico es inválido");
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
                    return View(clienteActual);
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
            
            return View(clienteActual);
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
                // Eliminar foto CI si existe
                if (!string.IsNullOrEmpty(cliente.FotoCI))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", cliente.FotoCI.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cliente eliminado correctamente";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}
