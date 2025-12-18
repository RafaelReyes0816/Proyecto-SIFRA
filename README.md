# 🔧 Tienda de Repuestos - Sistema de Gestión

Sistema web completo para la gestión de inventario, ventas y clientes de una tienda de repuestos automotrices. Desarrollado con ASP.NET Core 8.0 MVC y MySQL.

## 📋 Tabla de Contenidos

- [Características Principales](#-características-principales)
- [Tecnologías Utilizadas](#-tecnologías-utilizadas)
- [Requisitos Previos](#-requisitos-previos)
- [Instalación](#-instalación)
- [Configuración de Base de Datos](#-configuración-de-base-de-datos)
- [Estructura del Proyecto](#-estructura-del-proyecto)
- [Funcionalidades por Rol](#-funcionalidades-por-rol)
- [Usuarios de Prueba](#-usuarios-de-prueba)
- [Sistema de Verificación de Clientes](#-sistema-de-verificación-de-clientes)
- [Sistema de Alertas de Stock](#-sistema-de-alertas-de-stock)
- [Características de la Interfaz](#-características-de-la-interfaz)
- [Capturas de Pantalla](#-capturas-de-pantalla)
- [Configuración Adicional](#-configuración-adicional)
- [Solución de Problemas](#-solución-de-problemas)
- [Notas Importantes](#-notas-importantes)

---

## ✨ Características Principales

### 🔐 Autenticación y Roles
- **Sistema de autenticación** basado en sesiones
- **Tres tipos de usuarios**: Administrador, Vendedor y Cliente
- **Login unificado** con selección de tipo de usuario
- **Control de acceso** basado en roles (RBAC)

### 👨‍💼 Administrador
- ✅ Gestión completa de usuarios del sistema (admin/vendedor)
- ✅ Gestión completa de clientes
- ✅ Gestión de productos (CRUD completo)
- ✅ Gestión de ventas
- ✅ Dashboard con estadísticas generales
- ✅ **Sistema de alertas de stock bajo** con niveles de urgencia
- ✅ Visualización de productos críticos y agotados

### 💼 Vendedor
- ✅ Dashboard personalizado con estadísticas de ventas
- ✅ Visualización de productos disponibles
- ✅ Creación y gestión de ventas
- ✅ Visualización de clientes
- ✅ Historial de ventas realizadas

### 🛒 Cliente (E-commerce)
- ✅ **Registro de nuevos clientes** desde el portal web
- ✅ **Verificación automática** al subir foto de CI durante el registro
- ✅ **Dashboard personalizado** con estadísticas de compras
- ✅ **Catálogo de productos** disponible
- ✅ **Historial de compras** (Mis Compras)
- ✅ **Gestión de perfil** personal
- ✅ **Subida de foto de CI** (Cédula de Identidad)
- ✅ **Verificación automática** al actualizar foto de CI en el perfil
- ✅ **Visualización del estado de verificación** en tiempo real
- ✅ Visualización de estado de pedidos

### 📦 Gestión de Inventario
- ✅ Control de stock de productos
- ✅ **Alertas automáticas** de stock bajo
- ✅ Niveles de alerta: Agotado, Crítico, Bajo
- ✅ Gestión de categorías y proveedores
- ✅ Precios de compra y venta

### 💰 Gestión de Ventas
- ✅ Ventas presenciales y web
- ✅ Múltiples métodos de pago (efectivo, QR, transferencia)
- ✅ Estados de venta (pendiente, confirmada, cancelada)
- ✅ Detalles de venta con productos y cantidades
- ✅ Comprobantes de pago

---

## 🛠️ Tecnologías Utilizadas

### Backend
- **ASP.NET Core 8.0** - Framework web
- **Entity Framework Core 8.0** - ORM para acceso a datos
- **Pomelo.EntityFrameworkCore.MySql** - Proveedor MySQL para EF Core
- **C#** - Lenguaje de programación

### Frontend
- **Bootstrap 5** - Framework CSS
- **Razor Views** - Motor de vistas
- **JavaScript/jQuery** - Interactividad
- **CSS Personalizado** - Diseño moderno y responsive

### Base de Datos
- **MySQL 8.0+** - Sistema de gestión de bases de datos
- **MySQL Workbench** - Herramienta de administración

### Herramientas de Desarrollo
- **Visual Studio / VS Code** - IDE
- **.NET SDK 8.0** - Kit de desarrollo

---

## 📦 Requisitos Previos

Antes de comenzar, asegúrate de tener instalado:

1. **.NET SDK 8.0** o superior
   - Descarga: https://dotnet.microsoft.com/download/dotnet/8.0

2. **MySQL Server 8.0+**
   - Descarga: https://dev.mysql.com/downloads/mysql/
   - O instala **MySQL Workbench** que incluye el servidor

3. **Editor de código** (opcional pero recomendado)
   - Visual Studio 2022
   - Visual Studio Code
   - Rider

4. **Git** (para clonar el repositorio)

---

## 🚀 Instalación

### 1. Clonar el Repositorio

```bash
git clone https://github.com/RafaelReyes0816/Proyecto-SIFRA.git
cd Tienda-Repuestos-Demo
```

> **📌 Nota para equipos**: Si eres parte de un equipo, consulta el archivo `CONFIGURACION_GIT.md` para configurar tus credenciales de Git correctamente.

### 2. Restaurar Dependencias

```bash
dotnet restore
```

### 3. Configurar la Cadena de Conexión

Edita los archivos `appsettings.json` y `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=sifra_db;User=root;Password=TU_CONTRASEÑA;"
  }
}
```

**Nota**: Reemplaza `TU_CONTRASEÑA` con tu contraseña de MySQL.

### 4. Crear la Base de Datos

Ejecuta el script SQL en MySQL Workbench o desde la línea de comandos:

```bash
mysql -u root -p < Database/sifra_db.sql
```

O abre `Database/sifra_db.sql` en MySQL Workbench y ejecútalo.

### 5. Insertar Datos de Prueba

```bash
mysql -u root -p sifra_db < Database/Datos_Prueba.sql
```

O ejecuta `Database/Datos_Prueba.sql` en MySQL Workbench.

### 6. Ejecutar la Aplicación

```bash
dotnet run
```

O desde Visual Studio, presiona `F5`.

La aplicación estará disponible en: `https://localhost:5001` o `http://localhost:5000`

---

## 🗄️ Configuración de Base de Datos

### Estructura de la Base de Datos

La base de datos `sifra_db` contiene las siguientes tablas:

- **usuarios** - Usuarios del sistema (admin/vendedor)
- **clientes** - Clientes del e-commerce
- **productos** - Catálogo de productos
- **categorias** - Categorías de productos
- **proveedores** - Proveedores de productos
- **ventas** - Registro de ventas
- **detalles_venta** - Detalles de cada venta

### Scripts SQL Incluidos

- `Database/sifra_db.sql` - Script de creación de la base de datos
- `Database/Datos_Prueba.sql` - Datos de prueba (usuarios, productos, ventas)

### Credenciales por Defecto

- **Usuario MySQL**: `root`
- **Contraseña**: Configurar según tu instalación
- **Base de datos**: `sifra_db`

---

## 📁 Estructura del Proyecto

```
Tienda-Repuestos-Demo/
│
├── Controllers/              # Controladores MVC
│   ├── AccountController.cs      # Autenticación (login/logout)
│   ├── AdminController.cs        # Dashboard y funciones de admin
│   ├── ClienteController.cs      # Portal del cliente (e-commerce)
│   ├── ClientesController.cs     # CRUD de clientes (admin)
│   ├── ProductosController.cs    # CRUD de productos
│   ├── UsuariosController.cs     # CRUD de usuarios (admin)
│   ├── VendedorController.cs     # Dashboard del vendedor
│   └── VentasController.cs       # Gestión de ventas
│
├── Models/                  # Modelos de datos
│   ├── Usuario.cs
│   ├── Cliente.cs
│   ├── Producto.cs
│   ├── Venta.cs
│   └── ...
│
├── Views/                   # Vistas Razor
│   ├── Account/             # Login
│   ├── Admin/               # Dashboard admin
│   ├── Cliente/             # Portal cliente
│   ├── Clientes/            # CRUD clientes
│   ├── Productos/           # CRUD productos
│   ├── Usuarios/            # CRUD usuarios
│   ├── Vendedor/            # Dashboard vendedor
│   └── Ventas/              # Gestión ventas
│
├── Data/                    # Contexto de base de datos
│   └── ApplicationDbContext.cs
│
├── Database/                # Scripts SQL
│   ├── sifra_db.sql         # Creación de BD
│   ├── Datos_Prueba.sql     # Datos de prueba
│   └── Usuarios_Prueba.md   # Documentación usuarios
│
├── wwwroot/                 # Archivos estáticos
│   ├── css/                 # Estilos personalizados
│   ├── js/                  # JavaScript
│   └── uploads/ci/          # Fotos de CI subidas
│
├── Program.cs               # Configuración de la aplicación
├── appsettings.json         # Configuración
└── README.md                # Este archivo
```

---

## 👥 Funcionalidades por Rol

### 🔴 Administrador

#### Gestión de Usuarios
- Ver lista de usuarios del sistema
- Crear nuevos usuarios (admin/vendedor)
- Editar información de usuarios
- Eliminar usuarios
- Activar/desactivar usuarios

#### Gestión de Clientes
- Ver lista de todos los clientes
- Crear clientes manualmente (con contraseña opcional)
- Editar información de clientes
- **Verificar/desverificar clientes manualmente** mediante checkbox
- Eliminar clientes
- Ver estado de verificación de cada cliente
- Visualizar foto de CI de los clientes

#### Gestión de Productos
- Ver catálogo completo
- Crear nuevos productos
- Editar productos (precio, stock, descripción)
- Eliminar productos
- **Ver alertas de stock bajo**

#### Dashboard
- Estadísticas generales del sistema
- Total de usuarios, clientes, productos
- Ventas del día
- **Lista de productos con stock bajo**
- Ingresos totales

### 🟡 Vendedor

#### Ventas
- Crear nuevas ventas (presencial/web)
- Ver historial de ventas realizadas
- Actualizar estado de ventas
- Ver detalles de ventas

#### Productos
- Ver catálogo de productos
- Consultar disponibilidad y precios
- Ver información detallada

#### Clientes
- Ver información de clientes
- Consultar historial de compras

#### Dashboard
- Estadísticas personales de ventas
- Ventas realizadas hoy
- Total de ventas del mes

### 🟢 Cliente

#### Portal E-commerce
- **Registro de cuenta** desde el sitio web
- **Verificación automática** al subir foto de CI durante el registro
- **Login** con correo y contraseña
- **Ver catálogo** de productos disponibles
- **Realizar pedidos** (ventas tipo web)

#### Mi Cuenta
- **Dashboard personal** con estadísticas
- **Ver mis compras** (historial completo)
- **Editar perfil** personal
- **Subir foto de CI** (Cédula de Identidad)
- **Verificación automática** al subir foto de CI durante el registro
- **Verificación automática** al actualizar foto de CI en el perfil
- **Visualización del estado de verificación** (verificado/pendiente)
- Ver estado de pedidos

---

## 🔑 Usuarios de Prueba

### Usuarios del Sistema

#### Administrador
- **Correo**: `rafael.reyes@tienda.com`
- **Contraseña**: `Rafael123!`
- **Rol**: admin

#### Vendedor
- **Correo**: `david.cruz@tienda.com`
- **Contraseña**: `David123!`
- **Rol**: vendedor

### Clientes

#### Cliente 1
- **Correo**: `marco.benitez@tienda.com`
- **Contraseña**: `Marco123!`
- **Verificado**: Sí

#### Cliente 2
- **Correo**: `carlos.aranibar@tienda.com`
- **Contraseña**: `Carlos123!`
- **Verificado**: Sí

> **⚠️ Nota de Seguridad**: Las contraseñas NO están encriptadas. Esto es solo para propósitos de demostración. En producción, todas las contraseñas deben estar encriptadas usando técnicas como bcrypt o ASP.NET Identity.

---

## 🔐 Sistema de Verificación de Clientes

El sistema implementa un **sistema inteligente de verificación** para clientes:

### Verificación Automática
- **Al registrarse**: Si el cliente sube su foto de CI durante el registro, su cuenta se verifica automáticamente
- **Al actualizar perfil**: Si un cliente no verificado sube una foto de CI en su perfil, se verifica automáticamente
- **Preservación de estado**: Si un cliente ya está verificado y edita su perfil sin cambiar el CI, mantiene su estado verificado

### Verificación Manual (Admin)
- El administrador puede verificar o desverificar clientes manualmente desde la edición
- Checkbox de verificación en el formulario de edición de clientes
- Estado visible en la lista de clientes con badges (Verificado/No verificado)

### Beneficios
- **Proceso simplificado**: Los clientes se verifican automáticamente al proporcionar su CI
- **Control administrativo**: El admin puede gestionar verificaciones manualmente cuando sea necesario
- **Transparencia**: Los clientes pueden ver su estado de verificación en su perfil

---

## ⚠️ Sistema de Alertas de Stock

El sistema incluye un **sistema completo de alertas** para productos con stock bajo:

### Niveles de Alerta

1. **🔴 Agotado** - Stock = 0
   - Fondo rojo en tablas
   - Badge de alerta rojo
   - Prioridad máxima

2. **🔴 Crítico** - Stock ≤ 50% del mínimo
   - Fondo rojo en tablas
   - Badge de alerta rojo
   - Requiere atención inmediata

3. **🟡 Bajo** - Stock ≤ stock mínimo
   - Fondo amarillo en tablas
   - Badge de alerta amarillo
   - Requiere reabastecimiento

### Características

- **Alerta en Dashboard**: Banner destacado cuando hay productos con stock bajo
- **Lista Detallada**: Tabla completa con todos los productos afectados
- **Indicadores Visuales**: Colores y badges para identificación rápida
- **Acciones Rápidas**: Botones directos para actualizar stock
- **Alertas en Vista de Productos**: Resaltado automático en la lista de productos
- **Contador de Alertas**: Estadística visible en el dashboard

### Ubicación de Alertas

1. **Dashboard del Admin**: Alerta principal + lista completa
2. **Vista de Productos**: Alerta superior + resaltado en tabla
3. **Card de Estadísticas**: Indicador visual con contador

---

## 🎨 Características de la Interfaz

### Diseño Moderno
- **Paleta de colores** agradable y profesional
- **Gradientes** en elementos clave
- **Sombras suaves** para profundidad
- **Bordes redondeados** para un look moderno
- **Transiciones suaves** en interacciones

### Responsive Design
- **Adaptable** a dispositivos móviles, tablets y desktop
- **Navegación optimizada** para diferentes tamaños de pantalla
- **Tablas responsive** con scroll horizontal cuando es necesario

### Experiencia de Usuario
- **Navegación intuitiva** con menús contextuales por rol
- **Feedback visual** en todas las acciones
- **Mensajes de confirmación** claros
- **Formularios validados** con mensajes de error descriptivos
- **Iconos** para mejor comprensión visual

### Componentes Personalizados
- **Cards de estadísticas** con gradientes
- **Tablas estilizadas** con hover effects
- **Botones** con efectos de hover y sombras
- **Alertas** con diseño moderno y dismissible
- **Formularios** con inputs estilizados

---

## 📸 Capturas de Pantalla

### Login
- Interfaz moderna con selección de tipo de usuario (Empleado/Cliente)
- Enlace para registro de nuevos clientes

### Dashboard Administrador
- Estadísticas generales en cards
- Alerta de stock bajo destacada
- Lista de productos críticos
- Acciones rápidas

### Gestión de Productos
- Tabla con productos resaltados según stock
- Badges de alerta por nivel
- Filtros y búsqueda

### Portal del Cliente
- Dashboard personalizado
- Catálogo de productos
- Historial de compras
- Perfil con subida de foto de CI
- Indicador de estado de verificación
- Verificación automática al subir CI

### Gestión de Clientes (Admin)
- Lista de clientes con estado de verificación
- Formulario de edición con checkbox de verificación
- Preservación automática de foto de CI al editar
- Gestión de contraseñas (opcional al crear/editar)

---

## 🔧 Configuración Adicional

### Variables de Entorno

Puedes configurar variables de entorno para la cadena de conexión:

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Database=sifra_db;User=root;Password=tu_contraseña;"
```

### Sesiones

El sistema utiliza sesiones en memoria para la autenticación. Para producción, considera usar:
- Redis para sesiones distribuidas
- SQL Server para sesiones persistentes

### Archivos Subidos

Las fotos de CI se guardan en: `wwwroot/uploads/ci/`

Asegúrate de que este directorio tenga permisos de escritura.

---

## 🐛 Solución de Problemas

### Error de Conexión a MySQL

1. Verifica que MySQL esté corriendo:
   ```bash
   sudo systemctl status mysql
   ```

2. Verifica la cadena de conexión en `appsettings.json`

3. Asegúrate de que el usuario `root` tenga los permisos necesarios

### Error al Ejecutar Migraciones

Si usas migraciones de EF Core:
```bash
dotnet ef database update
```

### Error de Permisos en Uploads

```bash
chmod -R 755 wwwroot/uploads/
```

### Error: "Entity cannot be tracked because another instance is already being tracked"

Este error puede ocurrir al editar entidades. El sistema usa `AsNoTracking()` para evitar conflictos de rastreo en Entity Framework. Si persiste, verifica que no estés cargando la misma entidad múltiples veces en el mismo contexto.

### Problemas con Checkboxes en Formularios

Los checkboxes de verificación usan un patrón especial con input hidden para asegurar que siempre se envíe un valor (true/false) al servidor, incluso cuando el checkbox no está marcado.

---

## 📝 Notas Importantes

1. **Contraseñas**: Las contraseñas NO están encriptadas. Solo para demostración.

2. **Sesiones**: El sistema usa sesiones en memoria. En producción, usa almacenamiento persistente.

3. **Base de Datos**: Asegúrate de tener MySQL 8.0+ instalado y corriendo.

4. **Archivos**: Las fotos de CI se guardan en `wwwroot/uploads/ci/`. Este directorio debe existir y tener permisos de escritura.

5. **Verificación de Clientes**: 
   - Los clientes se verifican automáticamente al subir su CI
   - El admin puede verificar/desverificar manualmente desde la edición
   - El estado de verificación se preserva correctamente al editar

6. **Gestión de Clientes por Admin**:
   - Al crear un cliente, se puede asignar una contraseña o dejar vacío (se asigna "Cliente123!" por defecto)
   - Al editar, la contraseña solo se cambia si se proporciona una nueva
   - La foto de CI se preserva automáticamente al editar

7. **Seguridad**: Este es un sistema de demostración. Para producción, implementa:
   - Encriptación de contraseñas
   - HTTPS obligatorio
   - Validación de entrada más estricta
   - Protección CSRF (ya implementada)
   - Logs de seguridad

---

## 🚧 Próximas Mejoras

- [ ] Sistema de notificaciones en tiempo real
- [ ] Reportes avanzados con gráficos
- [ ] Exportación de datos a Excel/PDF
- [ ] Búsqueda avanzada de productos
- [ ] Carrito de compras para clientes
- [ ] Sistema de reseñas y calificaciones
- [ ] Integración con pasarelas de pago
- [ ] API REST para integraciones externas
- [ ] Validación de imágenes de CI (tamaño, formato, calidad)
- [ ] Historial de cambios en verificaciones de clientes
- [ ] Notificaciones por email al verificar clientes

---

## 📄 Licencia

Este proyecto es de código abierto y está disponible bajo la licencia MIT.

---

## 👨‍💻 Autor

**Rafael Reyes**

---

## 🙏 Agradecimientos

- Bootstrap por el framework CSS
- MySQL por el sistema de base de datos
- Microsoft por ASP.NET Core

---

## 📞 Soporte

Para preguntas o problemas, abre un issue en el repositorio.

---

## 👥 Trabajo en Equipo

Este es un proyecto grupal. Para configurar Git correctamente:

1. **Lee el archivo `CONFIGURACION_GIT.md`** - Contiene instrucciones detalladas
2. **Cada miembro debe configurar sus propias credenciales** (token o SSH)
3. **NUNCA compartas tu token de acceso personal**
4. El remote está configurado sin credenciales para seguridad del equipo

### Configuración Rápida

```bash
# Configurar tu usuario
git config user.name "Tu Nombre"
git config user.email "tu.email@ejemplo.com"

# Configurar para guardar credenciales
git config --global credential.helper store

# Hacer push (te pedirá usuario y token la primera vez)
git push
```

Para más detalles, consulta `CONFIGURACION_GIT.md`.

---

**¡Gracias por usar Tienda de Repuestos!** 🔧
