# Instrucciones de Configuración - Tienda de Repuestos

## ✅ Lo que se ha configurado

1. **Base de Datos SQL**: Script completo para MySQL Workbench
2. **Modelos de Datos**: Todos los modelos Entity Framework Core creados
3. **DbContext**: Configuración de Entity Framework Core con MySQL
4. **Datos de Prueba**: Scripts con usuarios y datos de ejemplo
5. **Documentación**: Archivos MD con información de usuarios y roles

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

## 👥 Usuarios de Prueba

Consulta `Database/Usuarios_Prueba.md` para ver los usuarios disponibles:

- **Admin**: admin@tienda.com / admin123
- **Vendedor**: vendedor@tienda.com / vendedor123
- **Cliente**: cliente@tienda.com (ver tabla clientes)

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
│   ├── Cliente.cs
│   ├── Proveedor.cs
│   ├── Categoria.cs
│   ├── Producto.cs
│   ├── Venta.cs
│   ├── DetalleVenta.cs
│   └── Reporte.cs
├── Data/
│   └── ApplicationDbContext.cs
└── Program.cs                 # Configuración de servicios
```

## 🔧 Tecnologías Utilizadas

- **.NET 8.0**: Framework principal
- **Entity Framework Core**: ORM para acceso a datos
- **Pomelo.EntityFrameworkCore.MySql**: Proveedor MySQL para EF Core
- **MySQL**: Base de datos

## ⚠️ Notas Importantes

1. **Contraseñas**: Las contraseñas NO están encriptadas (solo para pruebas)
2. **Base de Datos**: Respeta la estructura tal como está en el script SQL
3. **Roles**: Solo 'admin' y 'vendedor' en la tabla usuarios. Los clientes son entidades separadas.

## 🚀 Próximos Pasos

Ahora puedes:
1. Crear controladores para cada entidad
2. Implementar vistas para cada rol
3. Agregar autenticación y autorización
4. Implementar las funcionalidades según los roles definidos

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

### Cliente (si se implementa portal web)
- Ver catálogo
- Realizar pedidos
- Ver estado de pedidos
- Subir comprobantes
