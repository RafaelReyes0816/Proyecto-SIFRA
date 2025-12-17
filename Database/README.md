# Base de Datos - Tienda de Repuestos

Este directorio contiene los scripts SQL necesarios para configurar la base de datos del proyecto.

## Archivos

1. **sifra_db.sql** - Script principal para crear la estructura de la base de datos
2. **Datos_Prueba.sql** - Script con datos de prueba (usuarios, productos, ventas, etc.)
3. **Usuarios_Prueba.md** - Documentación de los usuarios de prueba y sus roles

## Instrucciones de Uso

### Paso 1: Crear la Base de Datos

1. Abre MySQL Workbench
2. Conéctate a tu servidor MySQL
3. Abre el archivo `sifra_db.sql`
4. Ejecuta el script completo (Ctrl+Shift+Enter o botón Execute)
5. Verifica que la base de datos `sifra_db` se haya creado correctamente

### Paso 2: Insertar Datos de Prueba

1. Abre el archivo `Datos_Prueba.sql`
2. Ejecuta el script completo
3. Verifica que los datos se hayan insertado correctamente

### Paso 3: Configurar la Conexión en el Proyecto

1. Abre `appsettings.json` o `appsettings.Development.json`
2. Actualiza la cadena de conexión con tus credenciales de MySQL:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=sifra_db;User=TU_USUARIO;Password=TU_CONTRASEÑA;Port=3306;CharSet=utf8mb4;"
   }
   ```

### Paso 4: Verificar la Conexión

1. Restaura los paquetes NuGet del proyecto:
   ```bash
   dotnet restore
   ```
2. Compila el proyecto:
   ```bash
   dotnet build
   ```
3. Ejecuta el proyecto:
   ```bash
   dotnet run
   ```

## Usuarios de Prueba

Consulta el archivo `Usuarios_Prueba.md` para ver los usuarios disponibles y sus credenciales.

**Importante**: Las contraseñas NO están encriptadas y son SOLO para pruebas.

## Estructura de la Base de Datos

- **usuarios**: Usuarios del sistema (admin, vendedor)
- **clientes**: Clientes de la tienda
- **proveedores**: Proveedores de productos
- **categorias**: Categorías de productos
- **productos**: Catálogo de productos
- **ventas**: Registro de ventas
- **detalles_venta**: Detalles de cada venta
- **reportes**: Reportes generados del sistema

## Notas

- Asegúrate de que MySQL esté corriendo antes de ejecutar los scripts
- La base de datos usa el charset `utf8mb4` para soportar caracteres especiales
- Todos los nombres de tablas y columnas están en minúsculas según la convención de MySQL
