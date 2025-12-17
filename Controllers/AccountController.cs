using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tienda_Repuestos_Demo.Data;
using Tienda_Repuestos_Demo.Models;

namespace Tienda_Repuestos_Demo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string contraseña, string tipoUsuario = "usuario")
        {
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contraseña))
            {
                ViewBag.Error = "Por favor ingrese correo y contraseña";
                return View();
            }

            // Si es cliente, buscar en tabla clientes
            if (tipoUsuario == "cliente")
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Correo == correo && c.Contraseña == contraseña);

                if (cliente == null)
                {
                    ViewBag.Error = "Credenciales incorrectas";
                    return View();
                }

                // Guardar sesión de cliente
                HttpContext.Session.SetString("ClienteId", cliente.IdCliente.ToString());
                HttpContext.Session.SetString("ClienteNombre", cliente.Nombre);
                HttpContext.Session.SetString("UsuarioRol", "cliente");
                HttpContext.Session.SetString("ClienteCorreo", cliente.Correo);

                return RedirectToAction("Dashboard", "Cliente");
            }
            else
            {
                // Buscar en tabla usuarios (admin/vendedor)
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Correo == correo && u.Contraseña == contraseña && u.Activo);

                if (usuario == null)
                {
                    ViewBag.Error = "Credenciales incorrectas o usuario inactivo";
                    return View();
                }

                // Guardar sesión simple (sin encriptación como solicitó el usuario)
                HttpContext.Session.SetString("UsuarioId", usuario.IdUsuario.ToString());
                HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
                HttpContext.Session.SetString("UsuarioRol", usuario.Rol);
                HttpContext.Session.SetString("UsuarioCorreo", usuario.Correo);

                // Redirigir según el rol
                if (usuario.Rol == "admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (usuario.Rol == "vendedor")
                {
                    return RedirectToAction("Dashboard", "Vendedor");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
