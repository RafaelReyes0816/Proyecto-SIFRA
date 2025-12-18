using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UsuarioId"));
        }

        // GET: Categorias
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login", "Account");
            }

            return View(await _context.Categorias
                .Include(c => c.Productos)
                .OrderBy(c => c.Nombre)
                .ToListAsync());
        }

        // GET: Categorias/Details/5
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

            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                    .ThenInclude(p => p.Proveedor)
                .FirstOrDefaultAsync(m => m.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create()
        {
            if (!IsAuthenticated() || HttpContext.Session.GetString("UsuarioRol") != "admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        // POST: Categorias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Categoria categoria)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el nombre ya existe (case insensitive)
                var existe = await _context.Categorias
                    .AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower());

                if (existe)
                {
                    ViewBag.Error = "Ya existe una categoría con este nombre";
                    return View(categoria);
                }

                _context.Add(categoria);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Categoría creada correctamente";
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // GET: Categorias/Edit/5
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

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }
            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCategoria,Nombre")] Categoria categoria)
        {
            if (id != categoria.IdCategoria)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el nombre ya existe en otra categoría
                    var existe = await _context.Categorias
                        .AnyAsync(c => c.Nombre.ToLower() == categoria.Nombre.ToLower() && c.IdCategoria != id);

                    if (existe)
                    {
                        ViewBag.Error = "Ya existe otra categoría con este nombre";
                        return View(categoria);
                    }

                    _context.Update(categoria);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Categoría actualizada correctamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriaExists(categoria.IdCategoria))
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
                    return View(categoria);
                }
            }
            return View(categoria);
        }

        // GET: Categorias/Delete/5
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

            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(m => m.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Productos)
                .FirstOrDefaultAsync(c => c.IdCategoria == id);

            if (categoria == null)
            {
                return NotFound();
            }

            // Verificar si tiene productos asociados
            if (categoria.Productos.Any())
            {
                TempData["Error"] = $"No se puede eliminar la categoría porque tiene {categoria.Productos.Count} producto(s) asociado(s). Primero debe eliminar o cambiar la categoría de esos productos.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Categoría eliminada correctamente";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.IdCategoria == id);
        }
    }
}
