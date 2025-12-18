# Usuarios de Prueba - Tienda de Repuestos

Este documento contiene los usuarios de prueba para el sistema de tienda de repuestos. **Nota: Las contraseñas NO están encriptadas** para facilitar las pruebas.

## Usuarios del Sistema

### 1. Administrador (Admin)
- **ID**: 1
- **Nombre**: Rafael Reyes
- **Correo**: rafael.reyes@tienda.com
- **Contraseña**: Rafael123!
- **Rol**: admin
- **Activo**: Sí

**Funcionalidades y Acciones**:
- Gestión completa de usuarios (crear, editar, eliminar, activar/desactivar)
- Gestión de clientes (ver, crear, editar, eliminar)
- Gestión de proveedores (ver, crear, editar, eliminar)
- Gestión de categorías (ver, crear, editar, eliminar)
- Gestión de productos (ver, crear, editar, eliminar, control de stock)
- Ver todas las ventas del sistema
- Generar y ver reportes
- Configuración del sistema
- Dashboard con estadísticas generales

---

### 2. Vendedor
- **ID**: 2
- **Nombre**: David Cruz
- **Correo**: david.cruz@tienda.com
- **Contraseña**: David123!
- **Rol**: vendedor
- **Activo**: Sí

**Funcionalidades y Acciones**:
- Ver lista de productos disponibles
- Ver información de clientes
- Crear nuevas ventas (presencial y web)
- Ver sus propias ventas realizadas
- Actualizar estado de ventas (pendiente, confirmada, cancelada)
- Ver reportes de sus ventas
- Dashboard con estadísticas de ventas personales

---

### 3. Cliente - Marco Benitez
- **ID Cliente**: 1
- **Nombre**: Marco Benitez
- **Correo**: marco.benitez@tienda.com
- **Contraseña**: Marco123!
- **Teléfono**: 555-0101
- **Dirección**: Calle Principal 123, Ciudad
- **Verificado**: Sí

**Funcionalidades y Acciones**:
- Ver catálogo de productos disponibles
- Ver sus propias compras/ventas
- Realizar pedidos (ventas tipo 'web')
- Ver estado de sus pedidos
- Subir comprobantes de pago
- Actualizar información personal
- Dashboard con estadísticas personales

---

### 4. Cliente - Carlos Aranibar
- **ID Cliente**: 2
- **Nombre**: Carlos Aranibar
- **Correo**: carlos.aranibar@tienda.com
- **Contraseña**: Carlos123!
- **Teléfono**: 555-0102
- **Dirección**: Avenida Central 456, Ciudad
- **Verificado**: Sí

**Funcionalidades y Acciones**:
- Ver catálogo de productos disponibles
- Ver sus propias compras/ventas
- Realizar pedidos (ventas tipo 'web')
- Ver estado de sus pedidos
- Subir comprobantes de pago
- Actualizar información personal
- Dashboard con estadísticas personales

---

## Script SQL para Insertar Usuarios de Prueba

```sql
-- Insertar usuarios del sistema
INSERT INTO usuarios (nombre, correo, contraseña, rol, activo) VALUES
('Rafael Reyes', 'rafael.reyes@tienda.com', 'Rafael123!', 'admin', TRUE),
('David Cruz', 'david.cruz@tienda.com', 'David123!', 'vendedor', TRUE);

-- Insertar clientes de prueba
INSERT INTO clientes (nombre, correo, contraseña, telefono, direccion, verificado) VALUES
('Marco Benitez', 'marco.benitez@tienda.com', 'Marco123!', '555-0101', 'Calle Principal 123, Ciudad', TRUE),
('Carlos Aranibar', 'carlos.aranibar@tienda.com', 'Carlos123!', '555-0102', 'Avenida Central 456, Ciudad', TRUE);
```

---

## Notas Importantes

1. **Seguridad**: Estas contraseñas NO están encriptadas y son SOLO para pruebas. En producción, todas las contraseñas deben estar encriptadas.

2. **Roles**: 
   - Los usuarios con rol 'admin' tienen acceso completo al sistema
   - Los usuarios con rol 'vendedor' tienen acceso limitado a ventas y productos
   - Los clientes tienen acceso al portal ecommerce para realizar compras y gestionar su cuenta

3. **Pruebas**: Puedes usar estos usuarios para probar todas las funcionalidades del sistema sin necesidad de crear nuevos usuarios cada vez.
