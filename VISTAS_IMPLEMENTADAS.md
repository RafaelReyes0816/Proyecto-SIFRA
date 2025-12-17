# Vistas Implementadas - Tienda de Repuestos

## ✅ Vistas Completadas

### 1. Autenticación
- **Login** (`Views/Account/Login.cshtml`)
  - Formulario de inicio de sesión
  - Validación de credenciales
  - Redirección según rol

### 2. Dashboards

#### Dashboard Administrador (`Views/Admin/Dashboard.cshtml`)
- Estadísticas generales:
  - Total de usuarios
  - Total de clientes
  - Total de productos
  - Total de ventas
  - Ventas del día
  - Productos con stock bajo
  - Total de ingresos
- Accesos rápidos a todas las secciones

#### Dashboard Vendedor (`Views/Vendedor/Dashboard.cshtml`)
- Estadísticas personales:
  - Ventas del día
  - Ventas del mes
  - Total de ventas
  - Ingresos del mes
- Lista de ventas recientes
- Accesos rápidos

### 3. Gestión de Productos (`Views/Productos/`)

- **Index**: Lista de productos con:
  - Código, nombre, categoría, proveedor
  - Precio de venta
  - Stock (resaltado si está bajo)
  - Acciones según rol

- **Details**: Detalles completos del producto

- **Create**: Formulario para crear nuevo producto (solo admin)

- **Edit**: Formulario para editar producto (solo admin)

- **Delete**: Confirmación de eliminación (solo admin)

### 4. Gestión de Clientes (`Views/Clientes/`)

- **Index**: Lista de clientes con:
  - Información de contacto
  - Estado de verificación
  - Fecha de registro

- **Details**: Detalles del cliente con historial de ventas

- **Create**: Formulario para crear nuevo cliente (solo admin)

- **Edit**: Formulario para editar cliente (solo admin)

- **Delete**: Confirmación de eliminación (solo admin)

### 5. Gestión de Ventas (`Views/Ventas/`)

- **Index**: Lista de ventas con:
  - Información de cliente y vendedor
  - Tipo de venta (presencial/web)
  - Estado (pendiente/confirmada/cancelada)
  - Método de pago
  - Total
  - Los vendedores solo ven sus propias ventas

- **Details**: Detalles completos de la venta con:
  - Información general
  - Lista de productos vendidos
  - Detalles de cada producto

- **Create**: Formulario para crear nueva venta

- **Edit**: Formulario para editar venta (cambiar estado, etc.)

## 🎨 Características de las Vistas

### Diseño
- Bootstrap 5 para estilos responsivos
- Cards para organizar información
- Tablas con hover effects
- Badges para estados
- Colores diferenciados según estado (éxito, advertencia, peligro)

### Funcionalidades
- Navegación dinámica según rol
- Validación de formularios
- Mensajes de error
- Confirmaciones para acciones destructivas
- Indicadores visuales (stock bajo, estados, etc.)

### Seguridad
- Verificación de sesión en cada controlador
- Restricción de acciones según rol
- Vendedores solo ven sus propias ventas
- Solo admin puede crear/editar/eliminar productos y clientes

## 📋 Navegación por Rol

### Administrador
- Dashboard
- Productos (CRUD completo)
- Clientes (CRUD completo)
- Ventas (ver todas)

### Vendedor
- Dashboard
- Ventas (crear y ver propias)
- Productos (solo lectura)
- Clientes (solo lectura)

## 🚀 Cómo Usar

1. **Iniciar Sesión**: 
   - Ir a `/Account/Login`
   - Usar credenciales de prueba:
     - Admin: admin@tienda.com / admin123
     - Vendedor: vendedor@tienda.com / vendedor123

2. **Navegar**: 
   - El menú superior cambia según el rol
   - Usar los dashboards para acceso rápido

3. **Gestionar Datos**:
   - Los botones de acción aparecen según permisos
   - Las validaciones previenen errores

## 📝 Notas

- Las contraseñas NO están encriptadas (solo para pruebas)
- Las sesiones se mantienen por 30 minutos
- Los vendedores solo pueden ver y editar sus propias ventas
- Los productos con stock bajo se resaltan en amarillo
