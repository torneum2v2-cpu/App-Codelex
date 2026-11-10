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
	estadoEmpleado BIT NOT NULL CONSTRAINT DF_EMPLEADOS_estadoEmpleado DEFAULT 1
)
GO

IF OBJECT_ID('ROLES') IS NULL
CREATE TABLE ROLES
(
	rol_id INT PRIMARY KEY IDENTITY,
	nombreRol VARCHAR(50) UNIQUE NOT NULL
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
	passkey_clave VARBINARY(1024) NULL,
	foto_perfil VARBINARY(MAX) NULL,
	rol_id INT NULL CONSTRAINT FK_USUARIOS_ROLES REFERENCES ROLES(rol_id),
	estadoUsuario BIT NOT NULL CONSTRAINT DF_USUARIOS_estadoUsuario DEFAULT 1
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
	cliente_id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_CLIENTES PRIMARY KEY,
	identificacionCliente VARCHAR(30) NOT NULL CONSTRAINT UQ_CLIENTES_identificacion UNIQUE,
	tipoIdentificacion VARCHAR(10) NOT NULL
		CONSTRAINT DF_CLIENTES_tipoIdentificacion DEFAULT 'RTN'
		CONSTRAINT CK_CLIENTES_tipoIdentificacion CHECK (tipoIdentificacion IN ('RTN', 'Identidad')),
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
	identificacionCliente VARCHAR(30) CONSTRAINT FK_LOCACIONES_CLIENTES REFERENCES CLIENTES(identificacionCliente) ON UPDATE CASCADE
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
	identificacionCliente VARCHAR(30) CONSTRAINT FK_ORDENES_CLIENTES REFERENCES CLIENTES(identificacionCliente) ON UPDATE CASCADE,
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

IF OBJECT_ID('COTIZACIONES') IS NULL
CREATE TABLE COTIZACIONES
(
	cotizacion_id VARCHAR(50) PRIMARY KEY NOT NULL,
	identificacionCliente VARCHAR(30),
	nombreEmpresa VARCHAR(50),
	nombreCliente VARCHAR(50),
	apellidoCliente VARCHAR(50),
	telefonoCliente VARCHAR(50),
	correoCliente VARCHAR(50),
	locacion_id INT NULL FOREIGN KEY REFERENCES LOCACIONES(locacion_id),
	direccion VARCHAR(80),
	fechaCotizacion DATE,
	valida_hasta DATE,
	observaciones VARCHAR(200),
	precio_cotizado MONEY
)
GO

IF OBJECT_ID('COTIZACION_DETALLE') IS NULL
CREATE TABLE COTIZACION_DETALLE
(
	cotizacion_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES COTIZACIONES(cotizacion_id),
	linea INT NOT NULL,
	concepto VARCHAR(150) NOT NULL,
	cantidad INT NOT NULL,
	precio_unitario MONEY NOT NULL,
	CONSTRAINT PK_COTIZACION_DETALLE PRIMARY KEY (cotizacion_id, linea)
)
GO

IF COL_LENGTH('ORDENES', 'cotizacion_id') IS NULL
	ALTER TABLE ORDENES ADD cotizacion_id VARCHAR(50) NULL;
GO

IF OBJECT_ID('FK_ORDENES_COTIZACIONES') IS NULL
	ALTER TABLE ORDENES
	ADD CONSTRAINT FK_ORDENES_COTIZACIONES FOREIGN KEY (cotizacion_id) REFERENCES COTIZACIONES(cotizacion_id);
GO

IF OBJECT_ID('FACTURA_DETALLE') IS NULL
CREATE TABLE FACTURA_DETALLE
(
	factura_id INT NOT NULL FOREIGN KEY REFERENCES FACTURAS(factura_id),
	linea INT NOT NULL,
	concepto VARCHAR(150) NOT NULL,
	cantidad INT NOT NULL,
	precio_unitario MONEY NOT NULL,
	adicional BIT NOT NULL DEFAULT 0,
	CONSTRAINT PK_FACTURA_DETALLE PRIMARY KEY (factura_id, linea)
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

IF COL_LENGTH('ACTIVIDADES', 'unidad') IS NULL
	ALTER TABLE ACTIVIDADES ADD unidad VARCHAR(20) NULL;
GO

IF COL_LENGTH('ACTIVIDADES', 'rendimiento') IS NULL
	ALTER TABLE ACTIVIDADES ADD rendimiento DECIMAL(10, 2) NULL;
GO

IF OBJECT_ID('PARAMETROS_COSTEO') IS NULL
CREATE TABLE PARAMETROS_COSTEO
(
	clave VARCHAR(40) PRIMARY KEY,
	valor DECIMAL(14, 4) NOT NULL,
	descripcion VARCHAR(150) NOT NULL
)
GO

IF OBJECT_ID('ACTIVIDAD_RANGOS') IS NULL
CREATE TABLE ACTIVIDAD_RANGOS
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES ACTIVIDADES(actividad_id),
	desde DECIMAL(12, 2) NOT NULL,
	hasta DECIMAL(12, 2) NULL,
	personas INT NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_RANGOS PRIMARY KEY (actividad_id, desde)
)
GO

IF OBJECT_ID('ACTIVIDAD_PRODUCTOS') IS NULL
CREATE TABLE ACTIVIDAD_PRODUCTOS
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES ACTIVIDADES(actividad_id),
	producto_id INT NOT NULL FOREIGN KEY REFERENCES PRODUCTOS(producto_id),
	cantidad_por_unidad DECIMAL(10, 4) NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_PRODUCTOS PRIMARY KEY (actividad_id, producto_id)
)
GO

IF OBJECT_ID('ACTIVIDAD_EQUIPOS') IS NULL
CREATE TABLE ACTIVIDAD_EQUIPOS
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES ACTIVIDADES(actividad_id),
	nombreEquipo VARCHAR(50) NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_EQUIPOS PRIMARY KEY (actividad_id, nombreEquipo)
)
GO

IF COL_LENGTH('COTIZACIONES', 'servicio_id') IS NULL
	ALTER TABLE COTIZACIONES ADD servicio_id INT NULL;
GO

IF OBJECT_ID('FK_COTIZACIONES_SERVICIOS') IS NULL
	ALTER TABLE COTIZACIONES
	ADD CONSTRAINT FK_COTIZACIONES_SERVICIOS FOREIGN KEY (servicio_id) REFERENCES SERVICIOS(servicio_id);
GO

IF COL_LENGTH('COTIZACION_DETALLE', 'actividad_id') IS NULL
	ALTER TABLE COTIZACION_DETALLE ADD actividad_id INT NULL;
GO

IF COL_LENGTH('COTIZACION_DETALLE', 'medida') IS NULL
	ALTER TABLE COTIZACION_DETALLE ADD medida DECIMAL(12, 2) NULL;
GO

IF COL_LENGTH('COTIZACION_DETALLE', 'personas') IS NULL
	ALTER TABLE COTIZACION_DETALLE ADD personas INT NULL;
GO

IF OBJECT_ID('FK_COTIZACION_DETALLE_ACTIVIDADES') IS NULL
	ALTER TABLE COTIZACION_DETALLE
	ADD CONSTRAINT FK_COTIZACION_DETALLE_ACTIVIDADES FOREIGN KEY (actividad_id) REFERENCES ACTIVIDADES(actividad_id);
GO

IF OBJECT_ID('ABONOS') IS NULL
CREATE TABLE ABONOS
(
	abono_id INT PRIMARY KEY IDENTITY,
	factura_id INT NOT NULL FOREIGN KEY REFERENCES FACTURAS(factura_id),
	fecha_abono DATE NOT NULL,
	monto MONEY NOT NULL,
	forma_pago_id INT FOREIGN KEY REFERENCES FORMAS_PAGOS(forma_pago_id),
	saldo_despues MONEY NOT NULL
)
GO

IF COL_LENGTH('ABONOS', 'concepto') IS NULL
	ALTER TABLE ABONOS
	ADD concepto VARCHAR(20) NOT NULL
		CONSTRAINT DF_ABONOS_concepto DEFAULT 'Abono';
GO

IF COL_LENGTH('CLIENTES', 'cliente_id') IS NULL AND COL_LENGTH('CLIENTES', 'identificacionCliente') IS NOT NULL
BEGIN
	DECLARE @sql NVARCHAR(MAX) = N'';

	SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(parent_object_id))
				+ N' DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.foreign_keys
	WHERE referenced_object_id = OBJECT_ID('CLIENTES');

	SELECT @sql = @sql + N'ALTER TABLE CLIENTES DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.key_constraints
	WHERE parent_object_id = OBJECT_ID('CLIENTES') AND type = 'PK';

	SET @sql = @sql + N'ALTER TABLE CLIENTES ADD cliente_id INT IDENTITY(1, 1) NOT NULL; '
			 + N'ALTER TABLE CLIENTES ADD CONSTRAINT PK_CLIENTES PRIMARY KEY (cliente_id); '
			 + N'ALTER TABLE CLIENTES ADD CONSTRAINT UQ_CLIENTES_identificacion UNIQUE (identificacionCliente); '
			 + N'ALTER TABLE LOCACIONES ADD CONSTRAINT FK_LOCACIONES_CLIENTES '
			 + N'FOREIGN KEY (identificacionCliente) REFERENCES CLIENTES(identificacionCliente) ON UPDATE CASCADE; '
			 + N'ALTER TABLE ORDENES ADD CONSTRAINT FK_ORDENES_CLIENTES '
			 + N'FOREIGN KEY (identificacionCliente) REFERENCES CLIENTES(identificacionCliente) ON UPDATE CASCADE; ';

	EXEC sp_executesql @sql;
END
GO

IF COL_LENGTH('CLIENTES', 'tipoIdentificacion') IS NULL
	ALTER TABLE CLIENTES
	ADD tipoIdentificacion VARCHAR(10) NOT NULL
		CONSTRAINT DF_CLIENTES_tipoIdentificacion DEFAULT 'RTN';
GO

IF OBJECT_ID('CK_CLIENTES_tipoIdentificacion') IS NULL
	ALTER TABLE CLIENTES
	ADD CONSTRAINT CK_CLIENTES_tipoIdentificacion CHECK (tipoIdentificacion IN ('RTN', 'Identidad'));
GO

UPDATE CLIENTES
SET tipoIdentificacion = 'Identidad'
WHERE tipoIdentificacion = 'RTN'
  AND LEN(identificacionCliente) = 13
  AND identificacionCliente NOT LIKE '%[^0-9]%';
GO

IF COL_LENGTH('EMPLEADOS', 'huella_id') IS NOT NULL
BEGIN
	DECLARE @quitar NVARCHAR(MAX) = N'';

	SELECT @quitar = @quitar + N'ALTER TABLE EMPLEADOS DROP CONSTRAINT ' + QUOTENAME(kc.name) + N'; '
	FROM sys.key_constraints kc
	INNER JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
	INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
	WHERE kc.parent_object_id = OBJECT_ID('EMPLEADOS') AND c.name = 'huella_id';

	SET @quitar = @quitar + N'ALTER TABLE EMPLEADOS DROP COLUMN huella_id; ';

	EXEC sp_executesql @quitar;
END
GO

-- Tipos de tabla que usan los procedimientos

IF TYPE_ID('dbo.tipo_lista_empleados') IS NULL
	CREATE TYPE dbo.tipo_lista_empleados AS TABLE (empleado_id INT NOT NULL PRIMARY KEY);
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

IF TYPE_ID('dbo.tipo_lista_medidas') IS NULL
	CREATE TYPE dbo.tipo_lista_medidas AS TABLE
	(
		actividad_id INT NOT NULL,
		medida DECIMAL(12, 2) NOT NULL
	);
GO

-- Datos iniciales

INSERT INTO FORMAS_PAGOS (nombreFormaPago)
SELECT v.nombre
FROM (VALUES ('Efectivo'), ('Tarjeta'), ('Transferencia'), ('Cheque')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM FORMAS_PAGOS f WHERE f.nombreFormaPago = v.nombre);
GO

INSERT INTO ROLES (nombreRol)
SELECT v.nombre
FROM (VALUES ('Usuario Básico'), ('Usuario Medio'), ('Usuario Avanzado')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM ROLES r WHERE r.nombreRol = v.nombre);
GO

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

INSERT INTO PARAMETROS_COSTEO (clave, valor, descripcion)
SELECT v.clave, v.valor, v.descripcion
FROM (VALUES
	('SALARIO_MINIMO', 12500, 'Salario minimo mensual en Honduras (L). Ajustar al vigente'),
	('CARGAS_SOCIALES', 0.3033, 'IHSS patronal 7%, RAP 1.5%, INFOP 1%, decimo tercer y cuarto mes, vacaciones'),
	('HORAS_MES', 190, 'Horas de trabajo al mes (jornada de 44 horas semanales)'),
	('FACTOR_SUPERVISOR', 1.5, 'El supervisor gana este multiplo del salario minimo'),
	('CUADRILLAS_POR_SUPERVISOR', 5, 'Cuadrillas que atiende el supervisor al mismo tiempo'),
	('LITROS_GASOLINA_VIAJE', 6, 'Litros de gasolina estandar para ir y volver del servicio'),
	('PRECIO_LITRO_GASOLINA', 100, 'Precio del litro de gasolina (L)'),
	('VIDA_UTIL_EQUIPO_MESES', 36, 'Meses de vida util del equipo'),
	('MANTENIMIENTO_EQUIPO_MENSUAL', 0.01, 'Mantenimiento mensual del equipo (fraccion de su precio)'),
	('GASTOS_ADMINISTRATIVOS', 0.10, 'Gastos administrativos sobre el costo directo'),
	('MARGEN_UTILIDAD', 0.50, 'Margen de utilidad sobre el precio de venta'),
	('ISV', 0.15, 'Impuesto sobre ventas de Honduras')
) v(clave, valor, descripcion)
WHERE NOT EXISTS (SELECT 1 FROM PARAMETROS_COSTEO p WHERE p.clave = v.clave);
GO

DECLARE @servicios TABLE (nombre VARCHAR(30));
DECLARE @act TABLE (servicio VARCHAR(30), descripcion VARCHAR(50), unidad VARCHAR(20), rendimiento DECIMAL(10, 2), perfil VARCHAR(5));
DECLARE @rango TABLE (perfil VARCHAR(5), desde DECIMAL(12, 2), hasta DECIMAL(12, 2), personas INT);
DECLARE @prod TABLE (nombre VARCHAR(50), tipo VARCHAR(20), stock INT, costo MONEY);
DECLARE @eq TABLE (nombre VARCHAR(50), tipo VARCHAR(50), marca VARCHAR(50), modelo VARCHAR(50), unidades INT, costo MONEY);
DECLARE @ap TABLE (actividad VARCHAR(50), producto VARCHAR(50), cantidad DECIMAL(10, 4));
DECLARE @ae TABLE (actividad VARCHAR(50), equipo VARCHAR(50));

INSERT INTO @act VALUES
	('Limpieza general', 'Barrido y trapeado de pisos', 'm²', 25, 'M2'),
	('Limpieza general', 'Limpieza de ventanas y vidrios', 'm²', 15, 'VID'),
	('Limpieza general', 'Limpieza de paredes y zócalos', 'm²', 20, 'M2'),
	('Limpieza general', 'Desempolvado de techos y esquinas', 'm²', 30, 'M2'),
	('Limpieza general', 'Recolección y retiro de basura', 'm²', 80, 'M2'),
	('Limpieza general', 'Limpieza y desinfección de baños', 'baños', 1.5, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de escritorios', 'escritorios', 8, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de equipo de cómputo', 'equipos', 6, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de sillas y mobiliario', 'muebles', 10, 'UNI'),
	('Limpieza de oficinas', 'Limpieza de salas de reuniones', 'salas', 1, 'UNI'),
	('Limpieza de oficinas', 'Vaciado de papeleras', 'papeleras', 40, 'UNI'),
	('Limpieza de tanques', 'Limpieza de tanque de 1,000 litros', 'tanques', 0.5, 'TNQ'),
	('Limpieza de tanques', 'Limpieza de tanque de 5,000 litros', 'tanques', 0.2, 'TNQ'),
	('Limpieza de tanques', 'Limpieza de tanque de 10,000 litros', 'tanques', 0.1, 'TNQ'),
	('Pisos y alfombras', 'Pulido y encerado de pisos', 'm²', 12, 'M2'),
	('Pisos y alfombras', 'Lavado de alfombras', 'm²', 10, 'M2'),
	('Pisos y alfombras', 'Lavado a presión de áreas exteriores', 'm²', 20, 'M2');

INSERT INTO @rango VALUES
	('M2', 0.01, 50, 1), ('M2', 51, 150, 2), ('M2', 151, 400, 3), ('M2', 401, 1000, 4), ('M2', 1001, NULL, 6),
	('VID', 0.01, 30, 1), ('VID', 31, 100, 2), ('VID', 101, 300, 3), ('VID', 301, 800, 4), ('VID', 801, NULL, 6),
	('UNI', 0.01, 10, 1), ('UNI', 11, 30, 2), ('UNI', 31, 100, 3), ('UNI', 101, NULL, 4),
	('TNQ', 0.01, 2, 2), ('TNQ', 3, 6, 3), ('TNQ', 7, NULL, 4);

INSERT INTO @prod VALUES
	('Desinfectante multiusos (litro)', 'Químico', 200, 45),
	('Detergente líquido (litro)', 'Químico', 200, 38),
	('Limpiavidrios (litro)', 'Químico', 150, 55),
	('Desengrasante (litro)', 'Químico', 100, 60),
	('Cloro (litro)', 'Químico', 300, 20),
	('Cera para piso (litro)', 'Químico', 100, 120),
	('Shampoo para alfombras (litro)', 'Químico', 100, 90),
	('Alcohol isopropílico (litro)', 'Químico', 100, 70),
	('Bolsas para basura (unidad)', 'Consumible', 1000, 3),
	('Paños de microfibra (unidad)', 'Consumible', 500, 25),
	('Esponja de limpieza (unidad)', 'Consumible', 500, 12);

INSERT INTO @eq VALUES
	('Aspiradora industrial', 'Eléctrico', 'Karcher', 'NT 70', 3, 9500),
	('Trapeador industrial con balde', 'Manual', 'Rubbermaid', 'Pro', 3, 1800),
	('Carro de limpieza', 'Manual', 'Rubbermaid', 'Pro', 3, 4500),
	('Escalera telescópica', 'Manual', 'Werner', 'Telescópica', 2, 3200),
	('Kit rasqueta para vidrios', 'Manual', 'Unger', 'Profesional', 3, 900),
	('Hidrolavadora', 'Eléctrico', 'Karcher', 'K5', 2, 7500),
	('Pulidora de pisos', 'Eléctrico', 'Hako', 'Industrial', 2, 18000),
	('Extractor de alfombras', 'Eléctrico', 'Karcher', 'Puzzi', 2, 14000),
	('Bomba de agua sumergible', 'Eléctrico', 'Truper', '1 HP', 2, 3500);

INSERT INTO @ap VALUES
	('Barrido y trapeado de pisos', 'Desinfectante multiusos (litro)', 0.01),
	('Barrido y trapeado de pisos', 'Detergente líquido (litro)', 0.005),
	('Barrido y trapeado de pisos', 'Paños de microfibra (unidad)', 0.002),
	('Limpieza de ventanas y vidrios', 'Limpiavidrios (litro)', 0.008),
	('Limpieza de ventanas y vidrios', 'Paños de microfibra (unidad)', 0.01),
	('Limpieza de paredes y zócalos', 'Desinfectante multiusos (litro)', 0.006),
	('Limpieza de paredes y zócalos', 'Esponja de limpieza (unidad)', 0.004),
	('Desempolvado de techos y esquinas', 'Paños de microfibra (unidad)', 0.005),
	('Recolección y retiro de basura', 'Bolsas para basura (unidad)', 0.02),
	('Limpieza y desinfección de baños', 'Cloro (litro)', 0.15),
	('Limpieza y desinfección de baños', 'Desinfectante multiusos (litro)', 0.2),
	('Limpieza y desinfección de baños', 'Esponja de limpieza (unidad)', 0.5),
	('Limpieza y desinfección de baños', 'Bolsas para basura (unidad)', 1),
	('Limpieza de escritorios', 'Desinfectante multiusos (litro)', 0.02),
	('Limpieza de escritorios', 'Paños de microfibra (unidad)', 0.1),
	('Limpieza de equipo de cómputo', 'Alcohol isopropílico (litro)', 0.03),
	('Limpieza de equipo de cómputo', 'Paños de microfibra (unidad)', 0.1),
	('Limpieza de sillas y mobiliario', 'Desinfectante multiusos (litro)', 0.03),
	('Limpieza de sillas y mobiliario', 'Paños de microfibra (unidad)', 0.1),
	('Limpieza de salas de reuniones', 'Desinfectante multiusos (litro)', 0.15),
	('Limpieza de salas de reuniones', 'Paños de microfibra (unidad)', 1),
	('Limpieza de salas de reuniones', 'Bolsas para basura (unidad)', 1),
	('Vaciado de papeleras', 'Bolsas para basura (unidad)', 1),
	('Limpieza de tanque de 1,000 litros', 'Cloro (litro)', 1.5),
	('Limpieza de tanque de 1,000 litros', 'Detergente líquido (litro)', 2),
	('Limpieza de tanque de 1,000 litros', 'Esponja de limpieza (unidad)', 3),
	('Limpieza de tanque de 5,000 litros', 'Cloro (litro)', 6),
	('Limpieza de tanque de 5,000 litros', 'Detergente líquido (litro)', 5),
	('Limpieza de tanque de 5,000 litros', 'Esponja de limpieza (unidad)', 5),
	('Limpieza de tanque de 10,000 litros', 'Cloro (litro)', 12),
	('Limpieza de tanque de 10,000 litros', 'Detergente líquido (litro)', 10),
	('Limpieza de tanque de 10,000 litros', 'Esponja de limpieza (unidad)', 8),
	('Pulido y encerado de pisos', 'Cera para piso (litro)', 0.02),
	('Pulido y encerado de pisos', 'Detergente líquido (litro)', 0.005),
	('Pulido y encerado de pisos', 'Paños de microfibra (unidad)', 0.005),
	('Lavado de alfombras', 'Shampoo para alfombras (litro)', 0.02),
	('Lavado de alfombras', 'Paños de microfibra (unidad)', 0.004),
	('Lavado a presión de áreas exteriores', 'Detergente líquido (litro)', 0.01),
	('Lavado a presión de áreas exteriores', 'Desengrasante (litro)', 0.005);

INSERT INTO @ae VALUES
	('Barrido y trapeado de pisos', 'Trapeador industrial con balde'),
	('Barrido y trapeado de pisos', 'Carro de limpieza'),
	('Barrido y trapeado de pisos', 'Aspiradora industrial'),
	('Limpieza de ventanas y vidrios', 'Escalera telescópica'),
	('Limpieza de ventanas y vidrios', 'Kit rasqueta para vidrios'),
	('Limpieza de paredes y zócalos', 'Escalera telescópica'),
	('Limpieza de paredes y zócalos', 'Carro de limpieza'),
	('Desempolvado de techos y esquinas', 'Escalera telescópica'),
	('Desempolvado de techos y esquinas', 'Aspiradora industrial'),
	('Recolección y retiro de basura', 'Carro de limpieza'),
	('Limpieza y desinfección de baños', 'Carro de limpieza'),
	('Limpieza de escritorios', 'Carro de limpieza'),
	('Limpieza de sillas y mobiliario', 'Carro de limpieza'),
	('Limpieza de salas de reuniones', 'Carro de limpieza'),
	('Limpieza de salas de reuniones', 'Aspiradora industrial'),
	('Vaciado de papeleras', 'Carro de limpieza'),
	('Limpieza de tanque de 1,000 litros', 'Bomba de agua sumergible'),
	('Limpieza de tanque de 1,000 litros', 'Hidrolavadora'),
	('Limpieza de tanque de 5,000 litros', 'Bomba de agua sumergible'),
	('Limpieza de tanque de 5,000 litros', 'Hidrolavadora'),
	('Limpieza de tanque de 10,000 litros', 'Bomba de agua sumergible'),
	('Limpieza de tanque de 10,000 litros', 'Hidrolavadora'),
	('Pulido y encerado de pisos', 'Pulidora de pisos'),
	('Pulido y encerado de pisos', 'Aspiradora industrial'),
	('Lavado de alfombras', 'Extractor de alfombras'),
	('Lavado a presión de áreas exteriores', 'Hidrolavadora');

INSERT INTO @servicios SELECT DISTINCT servicio FROM @act;

INSERT INTO SERVICIOS (nombreServicio)
SELECT s.nombre FROM @servicios s
WHERE NOT EXISTS (SELECT 1 FROM SERVICIOS x WHERE x.nombreServicio = s.nombre);

INSERT INTO ACTIVIDADES (servicio_id, descripcion, frecuencia, precio, unidad, rendimiento)
SELECT sv.servicio_id, a.descripcion, NULL, NULL, a.unidad, a.rendimiento
FROM @act a
INNER JOIN SERVICIOS sv ON sv.nombreServicio = a.servicio
WHERE NOT EXISTS (SELECT 1 FROM ACTIVIDADES x WHERE x.descripcion = a.descripcion);

UPDATE x
SET x.unidad = a.unidad, x.rendimiento = a.rendimiento
FROM ACTIVIDADES x
INNER JOIN @act a ON a.descripcion = x.descripcion
WHERE x.rendimiento IS NULL;

INSERT INTO ACTIVIDAD_RANGOS (actividad_id, desde, hasta, personas)
SELECT x.actividad_id, r.desde, r.hasta, r.personas
FROM @act a
INNER JOIN ACTIVIDADES x ON x.descripcion = a.descripcion
INNER JOIN @rango r ON r.perfil = a.perfil
WHERE NOT EXISTS (SELECT 1 FROM ACTIVIDAD_RANGOS y WHERE y.actividad_id = x.actividad_id);

IF NOT EXISTS (SELECT 1 FROM COMPRAS WHERE numero_factura = 'INVENTARIO-INICIAL')
BEGIN
	DECLARE @nuevosP TABLE (producto_id INT, nombre VARCHAR(50));
	DECLARE @nuevosE TABLE (equipo_id INT, nombre VARCHAR(50));
	DECLARE @proveedor INT;
	DECLARE @compra INT;
	DECLARE @disponible INT = (SELECT TOP 1 estado_equipo_id FROM ESTADO_EQUIPOS WHERE nombreEstadoEquipo = 'Disponible');

	IF NOT EXISTS (SELECT 1 FROM PROVEEDORES WHERE nombreProveedor = 'Proveedor inicial')
		INSERT INTO PROVEEDORES (nombreProveedor, telefonoProveedor, correoProveedor) VALUES ('Proveedor inicial', '', '');

	SELECT TOP 1 @proveedor = proveedor_id FROM PROVEEDORES WHERE nombreProveedor = 'Proveedor inicial';

	INSERT INTO COMPRAS (proveedor_id, firma_empleado_id, numero_factura, fechaCompra)
	VALUES (@proveedor, NULL, 'INVENTARIO-INICIAL', CAST(GETDATE() AS DATE));

	SET @compra = CAST(SCOPE_IDENTITY() AS INT);

	INSERT INTO PRODUCTOS (nombreProducto, tipoProducto, stock)
	OUTPUT inserted.producto_id, inserted.nombreProducto INTO @nuevosP
	SELECT p.nombre, p.tipo, p.stock FROM @prod p
	WHERE NOT EXISTS (SELECT 1 FROM PRODUCTOS x WHERE x.nombreProducto = p.nombre);

	INSERT INTO COMPRAS_PRODUCTOS (compra_id, producto_id, cantidad, precio_unitario)
	SELECT @compra, n.producto_id, p.stock, p.costo
	FROM @nuevosP n INNER JOIN @prod p ON p.nombre = n.nombre;

	INSERT INTO EQUIPOS (nombreEquipo, tipoEquipo, marcaEquipo, modeloEquipo, estado_equipo_id)
	OUTPUT inserted.equipo_id, inserted.nombreEquipo INTO @nuevosE
	SELECT e.nombre, e.tipo, e.marca, e.modelo, @disponible
	FROM @eq e
	INNER JOIN (VALUES (1), (2), (3), (4)) n(numero) ON n.numero <= e.unidades
	WHERE NOT EXISTS (SELECT 1 FROM EQUIPOS x WHERE x.nombreEquipo = e.nombre);

	INSERT INTO COMPRAS_EQUIPOS (compra_id, equipo_id, precio_unitario)
	SELECT @compra, n.equipo_id, e.costo
	FROM @nuevosE n INNER JOIN @eq e ON e.nombre = n.nombre;
END

INSERT INTO ACTIVIDAD_PRODUCTOS (actividad_id, producto_id, cantidad_por_unidad)
SELECT x.actividad_id, p.producto_id, a.cantidad
FROM @ap a
INNER JOIN ACTIVIDADES x ON x.descripcion = a.actividad
INNER JOIN PRODUCTOS p ON p.nombreProducto = a.producto
WHERE NOT EXISTS (SELECT 1 FROM ACTIVIDAD_PRODUCTOS y WHERE y.actividad_id = x.actividad_id);

INSERT INTO ACTIVIDAD_EQUIPOS (actividad_id, nombreEquipo)
SELECT x.actividad_id, a.equipo
FROM @ae a
INNER JOIN ACTIVIDADES x ON x.descripcion = a.actividad
WHERE NOT EXISTS (SELECT 1 FROM ACTIVIDAD_EQUIPOS y WHERE y.actividad_id = x.actividad_id);
GO

SET NOEXEC OFF;
GO
