IF DB_ID('Codelex') IS NULL
	CREATE DATABASE Codelex;
GO

USE Codelex
GO

IF DB_NAME() <> 'Codelex'
BEGIN
	RAISERROR('No se pudo usar la base Codelex.', 16, 1);
	SET NOEXEC ON;
END
GO

-- Tablas

IF OBJECT_ID('CARGOS') IS NULL
CREATE TABLE CARGOS
(
	cargo_id INT PRIMARY KEY IDENTITY,
	nombreCargo VARCHAR(50) UNIQUE
)
GO

IF OBJECT_ID('EMPLEADOS') IS NULL
CREATE TABLE EMPLEADOS
(
	empleado_id INT PRIMARY KEY IDENTITY,
	cargo_id INT FOREIGN KEY REFERENCES CARGOS(cargo_id),
	nombreEmpleado VARCHAR(30),
	apellidoEmpleado VARCHAR(30),
	identidad VARCHAR(30) UNIQUE NOT NULL,
	telefonoEmpleado Varchar(50),
	correoEmpleado VARCHAR(50),
	huella_id VARCHAR(50) UNIQUE NOT NULL,
	estadoEmpleado BIT NOT NULL CONSTRAINT DF_EMPLEADOS_estadoEmpleado DEFAULT 1
)
GO

IF OBJECT_ID('USUARIOS') IS NULL
CREATE TABLE USUARIOS
(
	usuario_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id) NOT NULL UNIQUE,
	usuario VARCHAR(50) UNIQUE NOT NULL,
	contrasena_hash VARCHAR(255),
	passkey_id VARBINARY(1023) NULL,
	passkey_clave VARBINARY(1024) NULL
)
GO

IF OBJECT_ID('BITACORAS') IS NULL
CREATE TABLE BITACORAS
(
	bitacora_id INT PRIMARY KEY IDENTITY,
	usuario_id INT FOREIGN KEY REFERENCES USUARIOS(usuario_id),
	accion VARCHAR(50),
	tabla Varchar(50),
	equipoSistema VARCHAR(50),
	fechaBitacora DATETIME
)
GO

IF OBJECT_ID('ASISTENCIAS') IS NULL
CREATE TABLE ASISTENCIAS
(
	asistencia_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	fechaAsistencia DATE,
	hora_entrada TIME,
	hora_salida TIME
)
GO

IF OBJECT_ID('CLIENTES') IS NULL
CREATE TABLE CLIENTES
(
	identificacionCliente VARCHAR(30) NOT NULL CONSTRAINT PK_CLIENTES PRIMARY KEY,
	nombreEmpresa VARCHAR(50),
	nombreCliente VARCHAR(50),
	apellidoCliente VARCHAR(50),
	telefonoCliente VARCHAR(50),
	correoCliente VARCHAR(50),
	estadoCliente BIT NOT NULL CONSTRAINT DF_CLIENTES_estadoCliente DEFAULT 1
)
GO

IF OBJECT_ID('LOCACIONES') IS NULL
CREATE TABLE LOCACIONES
(
	locacion_id INT PRIMARY KEY IDENTITY,
	nombreLocacion VARCHAR(60),
	direccion VARCHAR(80),
	tipoLocacion VARCHAR(25),
	identificacionCliente VARCHAR(30) CONSTRAINT FK_LOCACIONES_CLIENTES REFERENCES CLIENTES(identificacionCliente)
)
GO

IF OBJECT_ID('SERVICIOS') IS NULL
CREATE TABLE SERVICIOS
(
	servicio_id INT PRIMARY KEY IDENTITY,
	nombreServicio VARCHAR(30)
)
GO

IF OBJECT_ID('VEHICULOS') IS NULL
CREATE TABLE VEHICULOS
(
	vehiculo_id INT PRIMARY KEY IDENTITY,
	placa VARCHAR(15) UNIQUE,
	marcaVehiculo varchar(20),
	modeloVehiculo Varchar(25),
	estadoVehiculo BIT NOT NULL CONSTRAINT DF_VEHICULOS_estadoVehiculo DEFAULT 1
)
GO

IF OBJECT_ID('ESTADO_ORDENES') IS NULL
CREATE TABLE ESTADO_ORDENES
(
	estado_orden_id INT PRIMARY KEY IDENTITY,
	nombreEstadoOrden VARCHAR(25) UNIQUE
)
GO

IF OBJECT_ID('ORDENES') IS NULL
CREATE TABLE ORDENES
(
	orden_id VARCHAR(50) PRIMARY KEY NOT NULL,
	identificacionCliente VARCHAR(30) CONSTRAINT FK_ORDENES_CLIENTES REFERENCES CLIENTES(identificacionCliente),
	locacion_id INT FOREIGN KEY REFERENCES LOCACIONES(locacion_id),
	servicio_id INT FOREIGN KEY REFERENCES SERVICIOS(servicio_id),
	supervisor_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	vehiculo_id INT FOREIGN KEY REFERENCES VEHICULOS(vehiculo_id),
	fecha_solicitud DATE,
	fecha_servicio DATE,
	estado_orden_id INT FOREIGN KEY REFERENCES ESTADO_ORDENES(estado_orden_id),
	observaciones VARCHAR(200),
	conformidad_cliente BIT
)
GO

IF OBJECT_ID('ACTIVIDADES') IS NULL
CREATE TABLE ACTIVIDADES
(
	actividad_id INT PRIMARY KEY IDENTITY,
	servicio_id INT FOREIGN KEY REFERENCES SERVICIOS(servicio_id),
	descripcion VARCHAR(50),
	frecuencia VARCHAR(50),
	precio MONEY
)
GO

IF OBJECT_ID('PROVEEDORES') IS NULL
CREATE TABLE PROVEEDORES
(
	proveedor_id INT PRIMARY KEY IDENTITY,
	nombreProveedor VARCHAR(50),
	telefonoProveedor VARCHAR(50),
	correoProveedor VARCHAR(50)
)
GO

IF OBJECT_ID('PRODUCTOS') IS NULL
CREATE TABLE PRODUCTOS
(
	producto_id INT PRIMARY KEY IDENTITY,
	nombreProducto VARCHAR(50),
	tipoProducto VARCHAR(20),
	stock INT
)
GO

IF OBJECT_ID('COMPRAS') IS NULL
CREATE TABLE COMPRAS
(
	compra_id INT PRIMARY KEY IDENTITY,
	proveedor_id INT FOREIGN KEY REFERENCES PROVEEDORES(proveedor_id),
	firma_empleado_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	numero_factura VARCHAR(50) UNIQUE,
	fechaCompra DATE
)
GO

IF OBJECT_ID('COMPRAS_PRODUCTOS') IS NULL
CREATE TABLE COMPRAS_PRODUCTOS
(
	compra_producto_id INT IDENTITY CONSTRAINT PK_COMPRAS_PRODUCTOS PRIMARY KEY,
	compra_id INT NOT NULL FOREIGN KEY REFERENCES COMPRAS(compra_id),
	producto_id INT FOREIGN KEY REFERENCES PRODUCTOS(producto_id),
	cantidad INT,
	precio_unitario MONEY
)
GO

IF OBJECT_ID('ESTADO_EQUIPOS') IS NULL
CREATE TABLE ESTADO_EQUIPOS
(
	estado_equipo_id INT PRIMARY KEY IDENTITY,
	nombreEstadoEquipo VARCHAR(30) UNIQUE
)
GO

IF OBJECT_ID('EQUIPOS') IS NULL
CREATE TABLE EQUIPOS
(
	equipo_id INT PRIMARY KEY IDENTITY,
	nombreEquipo VARCHAR(50),
	tipoEquipo VARCHAR(50),
	marcaEquipo VARCHAR(50),
	modeloEquipo VARCHAR(50),
	estado_equipo_id INT FOREIGN KEY REFERENCES ESTADO_EQUIPOS(estado_equipo_id)
)
GO

IF OBJECT_ID('ORDEN_EMPLEADOS') IS NULL
CREATE TABLE ORDEN_EMPLEADOS
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ORDENES(orden_id),
	empleado_id INT NOT NULL FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	turno VARCHAR(20),
	piso_area VARCHAR(20),
	CONSTRAINT PK_ORDEN_EMPLEADOS PRIMARY KEY (orden_id, empleado_id)
)
GO

IF OBJECT_ID('ORDEN_PRODUCTOS') IS NULL
CREATE TABLE ORDEN_PRODUCTOS
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ORDENES(orden_id),
	producto_id INT NOT NULL FOREIGN KEY REFERENCES PRODUCTOS(producto_id),
	cantidad INT,
	CONSTRAINT PK_ORDEN_PRODUCTOS PRIMARY KEY (orden_id, producto_id)
)
GO

IF OBJECT_ID('ORDEN_EQUIPOS') IS NULL
CREATE TABLE ORDEN_EQUIPOS
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ORDENES(orden_id),
	equipo_id INT NOT NULL FOREIGN KEY REFERENCES EQUIPOS(equipo_id),
	fecha_asignacion DATE,
	devuelto BIT,
	CONSTRAINT PK_ORDEN_EQUIPOS PRIMARY KEY (orden_id, equipo_id)
)
GO

IF OBJECT_ID('ORDEN_ACTIVIDADES') IS NULL
CREATE TABLE ORDEN_ACTIVIDADES
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ORDENES(orden_id),
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES ACTIVIDADES(actividad_id),
	estado BIT,
	duracion_min INT,
	CONSTRAINT PK_ORDEN_ACTIVIDADES PRIMARY KEY (orden_id, actividad_id)
)
GO

IF OBJECT_ID('FORMAS_PAGOS') IS NULL
CREATE TABLE FORMAS_PAGOS
(
	forma_pago_id INT PRIMARY KEY IDENTITY,
	nombreFormaPago VARCHAR(30) UNIQUE
)
GO

IF OBJECT_ID('FACTURAS') IS NULL
CREATE TABLE FACTURAS
(
	factura_id INT PRIMARY KEY IDENTITY,
	orden_id VARCHAR(50) FOREIGN KEY REFERENCES ORDENES(orden_id) UNIQUE,
	fechaFactura DATE,
	tipo_pago VARCHAR(10) NOT NULL DEFAULT 'Contado'
		CHECK (tipo_pago IN ('Contado', 'Credito')),
	estado_pago VARCHAR(50) DEFAULT 'Pendiente'
		CHECK (estado_pago IN ('Pendiente', 'Pagada')),
	fecha_pago DATE,
	forma_pago_id INT FOREIGN KEY REFERENCES FORMAS_PAGOS(forma_pago_id),
	precio_facturado MONEY
)
GO

IF OBJECT_ID('COMPRAS_EQUIPOS') IS NULL
CREATE TABLE COMPRAS_EQUIPOS
(
	compra_equipo_id INT IDENTITY CONSTRAINT PK_COMPRAS_EQUIPOS PRIMARY KEY,
	compra_id INT NOT NULL FOREIGN KEY REFERENCES COMPRAS(compra_id),
	equipo_id INT FOREIGN KEY REFERENCES EQUIPOS(equipo_id),
	precio_unitario MONEY
)
GO

IF OBJECT_ID('CUENTAS_POR_COBRAR') IS NULL
CREATE TABLE CUENTAS_POR_COBRAR
(
	cuenta_id INT PRIMARY KEY IDENTITY,
	factura_id INT FOREIGN KEY REFERENCES FACTURAS(factura_id) NOT NULL UNIQUE,
	monto_total MONEY NOT NULL,
	prima MONEY,
	numero_cuotas INT NOT NULL,
	frecuencia VARCHAR(20),
	fecha_inicio DATE,
	estado_cuenta VARCHAR(20) DEFAULT 'Vigente'
		CHECK (estado_cuenta IN ('Vigente', 'Pagada', 'Vencida'))
)
GO

IF OBJECT_ID('CUOTAS') IS NULL
CREATE TABLE CUOTAS
(
	cuota_id INT PRIMARY KEY IDENTITY,
	cuenta_id INT FOREIGN KEY REFERENCES CUENTAS_POR_COBRAR(cuenta_id) NOT NULL,
	numero_cuota INT NOT NULL,
	fecha_vencimiento DATE NOT NULL,
	monto_cuota MONEY NOT NULL,
	monto_pagado MONEY DEFAULT 0,
	fecha_pago DATE,
	forma_pago_id INT FOREIGN KEY REFERENCES FORMAS_PAGOS(forma_pago_id),
	estado_cuota VARCHAR(20) DEFAULT 'Pendiente'
		CHECK (estado_cuota IN ('Pendiente', 'Parcial', 'Pagada', 'Vencida')),
	CONSTRAINT UQ_CUOTAS_cuenta_numero UNIQUE (cuenta_id, numero_cuota)
)
GO

-- Tipos de tabla que usan los procedimientos

IF TYPE_ID('dbo.tipo_lista_actividades') IS NULL
	CREATE TYPE dbo.tipo_lista_actividades AS TABLE (actividad_id INT NOT NULL PRIMARY KEY);
GO

IF TYPE_ID('dbo.tipo_lista_empleados') IS NULL
	CREATE TYPE dbo.tipo_lista_empleados AS TABLE (empleado_id INT NOT NULL PRIMARY KEY);
GO

IF TYPE_ID('dbo.tipo_lista_productos') IS NULL
	CREATE TYPE dbo.tipo_lista_productos AS TABLE (producto_id INT NOT NULL PRIMARY KEY, cantidad INT NOT NULL);
GO

IF TYPE_ID('dbo.tipo_lista_equipos') IS NULL
	CREATE TYPE dbo.tipo_lista_equipos AS TABLE (equipo_id INT NOT NULL PRIMARY KEY);
GO

IF TYPE_ID('dbo.tipo_ingreso_productos') IS NULL
	CREATE TYPE dbo.tipo_ingreso_productos AS TABLE
	(
		producto_id INT,
		cantidad INT,
		precio_unitario MONEY
	);
GO

IF TYPE_ID('dbo.tipo_ingreso_equipos') IS NULL
	CREATE TYPE dbo.tipo_ingreso_equipos AS TABLE
	(
		nombreEquipo VARCHAR(50),
		tipoEquipo VARCHAR(50),
		marcaEquipo VARCHAR(50),
		modeloEquipo VARCHAR(50),
		cantidad INT,
		precio_unitario MONEY
	);
GO

-- Datos iniciales

IF NOT EXISTS (SELECT 1 FROM CARGOS)
	INSERT INTO CARGOS (nombreCargo) VALUES ('Administrador'), ('Operario'), ('Supervisor');
GO

INSERT INTO ESTADO_ORDENES (nombreEstadoOrden)
SELECT v.nombre
FROM (VALUES ('Pendiente'), ('En proceso'), ('Completada'), ('Cancelada')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM ESTADO_ORDENES e WHERE e.nombreEstadoOrden = v.nombre);
GO

IF NOT EXISTS (SELECT 1 FROM ESTADO_EQUIPOS WHERE nombreEstadoEquipo = 'Disponible')
	INSERT INTO ESTADO_EQUIPOS (nombreEstadoEquipo) VALUES ('Disponible');
GO

SET NOEXEC OFF;
GO
