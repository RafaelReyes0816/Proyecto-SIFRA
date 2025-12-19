using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UsuarioRol") == "admin";
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Ventas)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewData["Roles"] = new SelectList(new[] { "admin", "vendedor" });
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Correo,Contraseña,Rol,Activo")] Usuario usuario)
        {
            // Validaciones manuales adicionales
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido");
            }
            else if (usuario.Nombre.Length > 100)
            {
                ModelState.AddModelError("Nombre", "El nombre no puede exceder 100 caracteres");
            }

            if (string.IsNullOrWhiteSpace(usuario.Correo))
            {
                ModelState.AddModelError("Correo", "El correo electrónico es requerido");
            }
            else if (!usuario.Correo.Contains("@") || !usuario.Correo.Contains("."))
            {
                ModelState.AddModelError("Correo", "El correo electrónico no es válido");
            }
            else if (usuario.Correo.Length > 100)
            {
                ModelState.AddModelError("Correo", "El correo electrónico no puede exceder 100 caracteres");
            }

            if (string.IsNullOrWhiteSpace(usuario.Contraseña))
            {
                ModelState.AddModelError("Contraseña", "La contraseña es requerida");
            }
            else if (usuario.Contraseña.Length < 6)
            {
                ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 6 caracteres");
            }
            else if (usuario.Contraseña.Length > 255)
            {
                ModelState.AddModelError("Contraseña", "La contraseña no puede exceder 255 caracteres");
            }

            if (string.IsNullOrWhiteSpace(usuario.Rol))
            {
                ModelState.AddModelError("Rol", "El rol es requerido");
            }
            else if (usuario.Rol != "admin" && usuario.Rol != "vendedor")
            {
                ModelState.AddModelError("Rol", "El rol debe ser 'admin' o 'vendedor'");
            }
            else if (usuario.Rol.Length > 20)
            {
                ModelState.AddModelError("Rol", "El rol no puede exceder 20 caracteres");
            }

            if (ModelState.IsValid)
            {
                // Verificar si el correo ya existe
                var existe = await _context.Usuarios
                    .AnyAsync(u => u.Correo == usuario.Correo);

                if (existe)
                {
                    ModelState.AddModelError("Correo", "Este correo electrónico ya está registrado");
                    ViewData["Roles"] = new SelectList(new[] { "admin", "vendedor" }, usuario.Rol);
                    return View(usuario);
                }

                usuario.FechaRegistro = DateTime.Now;
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Roles"] = new SelectList(new[] { "admin", "vendedor" }, usuario.Rol);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            ViewData["Roles"] = new SelectList(new[] { "admin", "vendedor" }, usuario.Rol);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,Nombre,Correo,Contraseña,Rol,Activo,FechaRegistro")] Usuario usuario)
        {
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            // Validaciones manuales adicionales
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
            {
                ModelState.AddModelError("Nombre", "El nombre es requerido");
            }
            else if (usuario.Nombre.Length > 100)
            {
                ModelState.AddModelError("Nombre", "El nombre no puede exceder 100 caracteres");
            }

            if (string.IsNullOrWhiteSpace(usuario.Correo))
            {
                ModelState.AddModelError("Correo", "El correo electrónico es requerido");
            }
            else if (!usuario.Correo.Contains("@") || !usuario.Correo.Contains("."))
            {
                ModelState.AddModelError("Correo", "El correo electrónico no es válido");
            }
            else if (usuario.Correo.Length > 100)
            {
                ModelState.AddModelError("Correo", "El correo electrónico no puede exceder 100 caracteres");
            }

            if (!string.IsNullOrWhiteSpace(usuario.Contraseña))
            {
                if (usuario.Contraseña.Length < 6)
                {
                    ModelState.AddModelError("Contraseña", "La contraseña debe tener al menos 6 caracteres");
                }
                else if (usuario.Contraseña.Length > 255)
                {
                    ModelState.AddModelError("Contraseña", "La contraseña no puede exceder 255 caracteres");
                }
            }

            if (string.IsNullOrWhiteSpace(usuario.Rol))
            {
                ModelState.AddModelError("Rol", "El rol es requerido");
            }
            else if (usuario.Rol != "admin" && usuario.Rol != "vendedor")
            {
                ModelState.AddModelError("Rol", "El rol debe ser 'admin' o 'vendedor'");
            }
            else if (usuario.Rol.Length > 20)
            {
                ModelState.AddModelError("Rol", "El rol no puede exceder 20 caracteres");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el correo ya existe en otro usuario
                    var existe = await _context.Usuarios
                        .AnyAsync(u => u.Correo == usuario.Correo && u.IdUsuario != id);

                    if (existe)
                    {
                        ModelState.AddModelError("Correo", "Este correo electrónico ya está registrado en otro usuario");
                        ViewData["Roles"] = new SelectList(new[] { "admin", "vendedor" }, usuario.Rol);
                        return View(usuario);
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Usuario actualizado correctamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
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

            ViewData["Roles"] = new SelectList(new[] { "admin", "vendedor" }, usuario.Rol);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(m => m.IdUsuario == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
