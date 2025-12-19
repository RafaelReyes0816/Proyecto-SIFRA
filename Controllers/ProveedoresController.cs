using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        // GET: Proveedores
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await _context.Proveedores
                .Include(p => p.Productos)
                .OrderBy(p => p.Nombre)
                .ToListAsync());
        }

        // GET: Proveedores/Details/5
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

            var proveedor = await _context.Proveedores
                .Include(p => p.Productos)
                    .ThenInclude(pr => pr.Categoria)
                .FirstOrDefaultAsync(m => m.IdProveedor == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // GET: Proveedores/Create
        public IActionResult Create()
        {
            if (!IsAuthenticated() || HttpContext.Session.GetString("UsuarioRol") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Proveedores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Contacto,Telefono,Correo")] Proveedor proveedor)
        {
            // Validaciones manuales adicionales
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre del proveedor es requerido");
            }
            else if (proveedor.Nombre.Length > 100)
            {
                ModelState.AddModelError("Nombre", "El nombre del proveedor no puede exceder 100 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(proveedor.Contacto) && proveedor.Contacto.Length > 100)
            {
                ModelState.AddModelError("Contacto", "El contacto no puede exceder 100 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(proveedor.Telefono) && proveedor.Telefono.Length > 20)
            {
                ModelState.AddModelError("Telefono", "El teléfono no puede exceder 20 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(proveedor.Correo))
            {
                if (!proveedor.Correo.Contains("@") || !proveedor.Correo.Contains("."))
                {
                    ModelState.AddModelError("Correo", "El correo electrónico no es válido");
                }
                else if (proveedor.Correo.Length > 100)
                {
                    ModelState.AddModelError("Correo", "El correo electrónico no puede exceder 100 caracteres");
                }
            }

            if (ModelState.IsValid)
            {
                // Verificar si el nombre ya existe
                var existe = await _context.Proveedores
                    .AnyAsync(p => p.Nombre.ToLower() == proveedor.Nombre.ToLower());

                if (existe)
                {
                    ModelState.AddModelError("Nombre", "Ya existe un proveedor con este nombre");
                    return View(proveedor);
                }

                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Proveedor creado correctamente";
                return RedirectToAction(nameof(Index));
            }
            return View(proveedor);
        }

        // GET: Proveedores/Edit/5
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

            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }
            return View(proveedor);
        }

        // POST: Proveedores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProveedor,Nombre,Contacto,Telefono,Correo")] Proveedor proveedor)
        {
            if (id != proveedor.IdProveedor)
            {
                return NotFound();
            }

            // Validaciones manuales adicionales
            if (string.IsNullOrWhiteSpace(proveedor.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre del proveedor es requerido");
            }
            else if (proveedor.Nombre.Length > 100)
            {
                ModelState.AddModelError("Nombre", "El nombre del proveedor no puede exceder 100 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(proveedor.Contacto) && proveedor.Contacto.Length > 100)
            {
                ModelState.AddModelError("Contacto", "El contacto no puede exceder 100 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(proveedor.Telefono) && proveedor.Telefono.Length > 20)
            {
                ModelState.AddModelError("Telefono", "El teléfono no puede exceder 20 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(proveedor.Correo))
            {
                if (!proveedor.Correo.Contains("@") || !proveedor.Correo.Contains("."))
                {
                    ModelState.AddModelError("Correo", "El correo electrónico no es válido");
                }
                else if (proveedor.Correo.Length > 100)
                {
                    ModelState.AddModelError("Correo", "El correo electrónico no puede exceder 100 caracteres");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el nombre ya existe en otro proveedor
                    var existeNombre = await _context.Proveedores
                        .AnyAsync(p => p.Nombre.ToLower() == proveedor.Nombre.ToLower() && p.IdProveedor != id);

                    if (existeNombre)
                    {
                        ModelState.AddModelError("Nombre", "Ya existe otro proveedor con este nombre");
                        return View(proveedor);
                    }

                    _context.Update(proveedor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Proveedor actualizado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProveedorExists(proveedor.IdProveedor))
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
                    ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                    return View(proveedor);
                }
            }
            return View(proveedor);
        }

        // GET: Proveedores/Delete/5
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

            var proveedor = await _context.Proveedores
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(m => m.IdProveedor == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }

        // POST: Proveedores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proveedor = await _context.Proveedores
                .Include(p => p.Productos)
                .FirstOrDefaultAsync(p => p.IdProveedor == id);

            if (proveedor == null)
            {
                return NotFound();
            }

            // Verificar si tiene productos asociados
            if (proveedor.Productos.Any())
            {
                TempData["Error"] = $"No se puede eliminar el proveedor porque tiene {proveedor.Productos.Count} producto(s) asociado(s). Primero debe eliminar o cambiar el proveedor de esos productos.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Proveedor eliminado correctamente";
            return RedirectToAction(nameof(Index));
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.IdProveedor == id);
        }
    }
}
