# Instrucciones de Configuración - Tienda de Repuestos

## ✅ Lo que se ha configurado

1. **Base de Datos SQL**: Script completo para MySQL Workbench
2. **Modelos de Datos**: Todos los modelos Entity Framework Core creados
3. **DbContext**: Configuración de Entity Framework Core con MySQL
4. **Datos de Prueba**: Scripts con usuarios y datos de ejemplo
5. **Autenticación Completa**: Sistema de login para admin, vendedor y clientes
6. **Portal Ecommerce**: Área completa para clientes con registro y gestión
7. **Vistas y Controladores**: CRUD completo para todas las entidades
8. **Documentación**: Archivos MD con información de usuarios y roles

## 📋 Pasos para comenzar

### 1. Instalar paquetes NuGet

Ejecuta en la terminal desde la raíz del proyecto:

```bash
dotnet restore
```

### 2. Configurar MySQL Workbench

1. Abre MySQL Workbench
2. Conéctate a tu servidor MySQL
3. Abre y ejecuta: `Database/sifra_db.sql`
4. Luego ejecuta: `Database/Datos_Prueba.sql`

### 3. Configurar la cadena de conexión

Edita `appsettings.json` o `appsettings.Development.json` y actualiza:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=sifra_db;User=root;Password=TU_CONTRASEÑA;Port=3306;CharSet=utf8mb4;"
}
```

**Nota**: Cambia `TU_CONTRASEÑA` por tu contraseña de MySQL.

### 4. Compilar y ejecutar

```bash
dotnet build
dotnet run
```

### 5. Acceder al Sistema

1. Abre tu navegador en la URL que muestra la consola (generalmente `https://localhost:5001` o `http://localhost:5000`)
2. Serás redirigido al login
3. Selecciona el tipo de usuario (Empleado o Cliente)
4. Ingresa las credenciales de prueba

**Opciones de acceso:**
- **Como Admin**: Selecciona "Empleado" → rafael.reyes@tienda.com / Rafael123!
- **Como Vendedor**: Selecciona "Empleado" → david.cruz@tienda.com / David123!
- **Como Cliente**: Selecciona "Cliente" → marco.benitez@tienda.com / Marco123!
- **Registro Nuevo**: Click en "Regístrate aquí" para crear cuenta de cliente nueva

## 👥 Usuarios de Prueba

Consulta `Database/Usuarios_Prueba.md` para ver los usuarios disponibles:

### Usuarios del Sistema (Empleados)
- **👨‍💼 Admin**: rafael.reyes@tienda.com / `Rafael123!`
- **👤 Vendedor**: david.cruz@tienda.com / `David123!`

### Clientes (Ecommerce)
- **🛒 Cliente 1**: marco.benitez@tienda.com / `Marco123!`
- **🛒 Cliente 2**: carlos.aranibar@tienda.com / `Carlos123!`

**Nota**: En el login, selecciona "Empleado" para admin/vendedor o "Cliente" para acceder como cliente.

## 📁 Estructura del Proyecto

```
Tienda-Repuestos-Demo/
├── Database/
│   ├── sifra_db.sql          # Script de creación de BD
│   ├── Datos_Prueba.sql      # Datos de prueba
│   ├── Usuarios_Prueba.md    # Documentación de usuarios
│   └── README.md             # Guía de la base de datos
├── Models/                    # Modelos Entity Framework
│   ├── Usuario.cs
│   ├── Cliente.cs            # Con campo contraseña para autenticación
│   ├── Proveedor.cs
│   ├── Categoria.cs
│   ├── Producto.cs
│   ├── Venta.cs
│   ├── DetalleVenta.cs
│   └── Reporte.cs
├── Controllers/               # Controladores MVC
│   ├── AccountController.cs  # Login unificado (empleados y clientes)
│   ├── AdminController.cs    # Dashboard administrador
│   ├── VendedorController.cs # Dashboard vendedor
│   ├── ClienteController.cs  # Área del cliente (registro, perfil, compras)
│   ├── ProductosController.cs
│   ├── ClientesController.cs # Gestión de clientes (admin)
│   └── VentasController.cs
├── Views/                     # Vistas Razor
│   ├── Account/              # Login
│   ├── Admin/                # Dashboard admin
│   ├── Vendedor/             # Dashboard vendedor
│   ├── Cliente/              # Área cliente (dashboard, registro, perfil, compras)
│   ├── Productos/            # CRUD productos
│   ├── Clientes/             # CRUD clientes
│   └── Ventas/               # CRUD ventas
├── Data/
│   └── ApplicationDbContext.cs
└── Program.cs                 # Configuración de servicios y sesiones
```

## 🔧 Tecnologías Utilizadas

- **.NET 8.0**: Framework principal
- **Entity Framework Core**: ORM para acceso a datos
- **Pomelo.EntityFrameworkCore.MySql**: Proveedor MySQL para EF Core
- **MySQL**: Base de datos

## ⚠️ Notas Importantes

1. **Contraseñas**: Las contraseñas NO están encriptadas (solo para pruebas). En producción, implementar hash de contraseñas.
2. **Base de Datos**: Respeta la estructura tal como está en el script SQL. La tabla `clientes` incluye el campo `contraseña` para autenticación.
3. **Roles**: 
   - **admin** y **vendedor**: En la tabla `usuarios`
   - **cliente**: En la tabla `clientes` (entidad separada con autenticación propia)
4. **Sesiones**: El sistema usa sesiones para mantener el estado de autenticación (30 minutos de timeout).

## 🚀 Sistema Implementado

El sistema está completamente funcional con:

✅ **Autenticación y Autorización**
- Login unificado para empleados y clientes
- Registro de nuevos clientes
- Sesiones diferenciadas por rol
- Protección de rutas según permisos

✅ **Controladores y Vistas Completas**
- CRUD completo para productos, clientes y ventas
- Dashboards personalizados por rol
- Gestión de perfiles y compras

✅ **Portal Ecommerce para Clientes**
- Registro de nuevos clientes
- Dashboard personal con estadísticas
- Historial de compras
- Edición de perfil
- Catálogo de productos

## 📝 Funcionalidades por Rol

### Admin
- Gestión completa de usuarios, clientes, proveedores, categorías y productos
- Ver todas las ventas
- Generar reportes
- Dashboard con estadísticas

### Vendedor
- Ver productos y clientes
- Crear ventas
- Ver sus propias ventas
- Reportes personales

### Cliente (Portal Ecommerce) ✅ IMPLEMENTADO
- **Registro**: Crear cuenta nueva en `/Cliente/Registro`
- **Login**: Iniciar sesión seleccionando "Cliente" en el login
- **Dashboard**: Panel personal con estadísticas de compras
- **Mis Compras**: Ver historial completo de compras realizadas
- **Mi Perfil**: Editar información personal (nombre, correo, teléfono, dirección)
- **Catálogo**: Ver todos los productos disponibles
- **Detalles de Compra**: Ver información detallada de cada venta realizada
