-- Script de datos de prueba para Tienda de Repuestos
-- Ejecutar después de crear la base de datos con sifra_db.sql

USE sifra_db;

-- Insertar usuarios del sistema
INSERT INTO usuarios (nombre, correo, contraseña, rol, activo) VALUES
('Rafael Reyes', 'rafael.reyes@tienda.com', 'Rafael123!', 'admin', TRUE),
('David Cruz', 'david.cruz@tienda.com', 'David123!', 'vendedor', TRUE);

-- Insertar clientes de prueba (con contraseña)
INSERT INTO clientes (nombre, correo, contraseña, telefono, direccion, verificado) VALUES
('Marco Benitez', 'marco.benitez@tienda.com', 'Marco123!', '555-0101', 'Calle Principal 123, Ciudad', TRUE),
('Carlos Aranibar', 'carlos.aranibar@tienda.com', 'Carlos123!', '555-0102', 'Avenida Central 456, Ciudad', TRUE);

-- Insertar proveedores
INSERT INTO proveedores (nombre, contacto, telefono, correo) VALUES
('Proveedor ABC', 'Juan Pérez', '555-1001', 'contacto@proveedorabc.com'),
('Repuestos XYZ', 'María González', '555-1002', 'ventas@repuestosxyz.com'),
('Distribuidora Central', 'Carlos Rodríguez', '555-1003', 'info@distribuidoracentral.com');

-- Insertar categorías
INSERT INTO categorias (nombre) VALUES
('Frenos'),
('Motor'),
('Transmisión'),
('Suspensión'),
('Eléctrico'),
('Carrocería');

-- Insertar productos
INSERT INTO productos (codigo, nombre, descripcion, id_categoria, id_proveedor, precio_compra, precio_venta, stock, stock_minimo) VALUES
('FR-001', 'Pastillas de Freno Delanteras', 'Pastillas de freno para ruedas delanteras, material cerámico', 1, 1, 25.00, 45.00, 50, 10),
('FR-002', 'Discos de Freno', 'Discos de freno ventilados, diámetro estándar', 1, 1, 35.00, 65.00, 30, 5),
('MO-001', 'Filtro de Aceite', 'Filtro de aceite estándar, compatible con múltiples modelos', 2, 2, 8.00, 15.00, 100, 20),
('MO-002', 'Bujías', 'Bujías de encendido, set de 4 unidades', 2, 2, 12.00, 25.00, 80, 15),
('TR-001', 'Aceite de Transmisión', 'Aceite para transmisión automática, 1 litro', 3, 3, 10.00, 20.00, 60, 10),
('SU-001', 'Amortiguadores Delanteros', 'Par de amortiguadores delanteros', 4, 1, 80.00, 150.00, 20, 5),
('EL-001', 'Batería 12V', 'Batería de 12V, 60Ah', 5, 2, 60.00, 120.00, 25, 5),
('CA-001', 'Faros Delanteros', 'Par de faros delanteros LED', 6, 3, 50.00, 95.00, 15, 5);

-- Insertar ventas de ejemplo
INSERT INTO ventas (id_cliente, id_vendedor, tipo_venta, fecha, estado, metodo_pago, total) VALUES
(1, 2, 'presencial', '2024-01-15 10:30:00', 'confirmada', 'efectivo', 90.00),
(1, 2, 'web', '2024-01-20 14:15:00', 'confirmada', 'transferencia', 150.00),
(2, 2, 'presencial', '2024-01-22 09:00:00', 'pendiente', 'qr', 65.00);

-- Insertar detalles de venta
INSERT INTO detalles_venta (id_venta, id_producto, cantidad, precio_unitario) VALUES
-- Detalles de la primera venta (id_venta = 1)
(1, 1, 2, 45.00),  -- 2 pastillas de freno

-- Detalles de la segunda venta (id_venta = 2)
(2, 2, 1, 65.00),  -- 1 disco de freno
(2, 3, 2, 15.00),  -- 2 filtros de aceite
(2, 4, 1, 25.00),  -- 1 set de bujías

-- Detalles de la tercera venta (id_venta = 3)
(3, 2, 1, 65.00);  -- 1 disco de freno
