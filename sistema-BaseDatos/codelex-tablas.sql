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

IF OBJECT_ID('cargos') IS NULL
CREATE TABLE cargos
(
	cargo_id INT PRIMARY KEY IDENTITY,
	nombre_cargo VARCHAR(50) UNIQUE
)
GO

IF OBJECT_ID('empleados') IS NULL
CREATE TABLE empleados
(
	empleado_id INT PRIMARY KEY IDENTITY,
	cargo_id INT FOREIGN KEY REFERENCES cargos(cargo_id),
	nombre_empleado VARCHAR(30),
	apellido_empleado VARCHAR(30),
	identidad VARCHAR(30) UNIQUE NOT NULL,
	telefono_empleado Varchar(50),
	correo_empleado VARCHAR(50),
	estado_empleado BIT NOT NULL CONSTRAINT DF_EMPLEADOS_estadoEmpleado DEFAULT 1
)
GO

IF OBJECT_ID('roles') IS NULL
CREATE TABLE roles
(
	rol_id INT PRIMARY KEY IDENTITY,
	nombre_rol VARCHAR(50) UNIQUE NOT NULL
)
GO

IF OBJECT_ID('usuarios') IS NULL
CREATE TABLE usuarios
(
	usuario_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES empleados(empleado_id) NOT NULL UNIQUE,
	usuario VARCHAR(50) UNIQUE NOT NULL,
	contrasena_hash VARCHAR(255),
	passkey_id VARBINARY(1023) NULL,
	passkey_clave VARBINARY(1024) NULL,
	foto_perfil VARBINARY(MAX) NULL,
	rol_id INT NULL CONSTRAINT FK_USUARIOS_ROLES REFERENCES roles(rol_id),
	estado_usuario BIT NOT NULL CONSTRAINT DF_USUARIOS_estadoUsuario DEFAULT 1
)
GO

IF OBJECT_ID('bitacoras') IS NULL
CREATE TABLE bitacoras
(
	bitacora_id INT PRIMARY KEY IDENTITY,
	usuario_id INT FOREIGN KEY REFERENCES usuarios(usuario_id),
	accion VARCHAR(50),
	tabla Varchar(50),
	equipo_sistema VARCHAR(50),
	fecha_bitacora DATETIME
)
GO

IF OBJECT_ID('asistencias') IS NULL
CREATE TABLE asistencias
(
	asistencia_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES empleados(empleado_id),
	fecha_asistencia DATE,
	hora_entrada TIME,
	hora_salida TIME
)
GO

IF OBJECT_ID('clientes') IS NULL
CREATE TABLE clientes
(
	cliente_id INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_CLIENTES PRIMARY KEY,
	identificacion_cliente VARCHAR(30) NOT NULL CONSTRAINT UQ_CLIENTES_identificacion UNIQUE,
	tipo_identificacion VARCHAR(10) NOT NULL
		CONSTRAINT DF_CLIENTES_tipoIdentificacion DEFAULT 'RTN'
		CONSTRAINT CK_CLIENTES_tipoIdentificacion CHECK (tipo_identificacion IN ('RTN', 'Identidad')),
	nombre_empresa VARCHAR(50),
	nombre_cliente VARCHAR(50),
	apellido_cliente VARCHAR(50),
	telefono_cliente VARCHAR(50),
	correo_cliente VARCHAR(50),
	estado_cliente BIT NOT NULL CONSTRAINT DF_CLIENTES_estadoCliente DEFAULT 1
)
GO

IF OBJECT_ID('locaciones') IS NULL
CREATE TABLE locaciones
(
	locacion_id INT PRIMARY KEY IDENTITY,
	nombre_locacion VARCHAR(60),
	direccion VARCHAR(80),
	tipo_locacion VARCHAR(25),
	identificacion_cliente VARCHAR(30) CONSTRAINT FK_LOCACIONES_CLIENTES REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE
)
GO

IF OBJECT_ID('servicios') IS NULL
CREATE TABLE servicios
(
	servicio_id INT PRIMARY KEY IDENTITY,
	nombre_servicio VARCHAR(30)
)
GO

IF OBJECT_ID('vehiculos') IS NULL
CREATE TABLE vehiculos
(
	vehiculo_id INT PRIMARY KEY IDENTITY,
	placa VARCHAR(15) UNIQUE,
	marca_vehiculo varchar(20),
	modelo_vehiculo Varchar(25),
	estado_vehiculo BIT NOT NULL CONSTRAINT DF_VEHICULOS_estadoVehiculo DEFAULT 1
)
GO

IF OBJECT_ID('estados_orden') IS NULL
CREATE TABLE estados_orden
(
	estado_orden_id INT PRIMARY KEY IDENTITY,
	nombre_estado_orden VARCHAR(25) UNIQUE
)
GO

IF OBJECT_ID('ordenes') IS NULL
CREATE TABLE ordenes
(
	orden_id VARCHAR(50) PRIMARY KEY NOT NULL,
	identificacion_cliente VARCHAR(30) CONSTRAINT FK_ORDENES_CLIENTES REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE,
	locacion_id INT FOREIGN KEY REFERENCES locaciones(locacion_id),
	servicio_id INT FOREIGN KEY REFERENCES servicios(servicio_id),
	supervisor_id INT FOREIGN KEY REFERENCES empleados(empleado_id),
	vehiculo_id INT FOREIGN KEY REFERENCES vehiculos(vehiculo_id),
	fecha_solicitud DATE,
	fecha_servicio DATE,
	estado_orden_id INT FOREIGN KEY REFERENCES estados_orden(estado_orden_id),
	observacion VARCHAR(200),
	conformidad_cliente BIT
)
GO

IF OBJECT_ID('actividades') IS NULL
CREATE TABLE actividades
(
	actividad_id INT PRIMARY KEY IDENTITY,
	servicio_id INT FOREIGN KEY REFERENCES servicios(servicio_id),
	descripcion VARCHAR(50),
	frecuencia VARCHAR(50),
	precio MONEY
)
GO

IF OBJECT_ID('proveedores') IS NULL
CREATE TABLE proveedores
(
	proveedor_id INT PRIMARY KEY IDENTITY,
	nombre_proveedor VARCHAR(50),
	telefono_proveedor VARCHAR(50),
	correo_proveedor VARCHAR(50)
)
GO

IF OBJECT_ID('productos') IS NULL
CREATE TABLE productos
(
	producto_id INT PRIMARY KEY IDENTITY,
	nombre_producto VARCHAR(50),
	tipo_producto VARCHAR(20),
	stock INT
)
GO

IF OBJECT_ID('compras') IS NULL
CREATE TABLE compras
(
	compra_id INT PRIMARY KEY IDENTITY,
	proveedor_id INT FOREIGN KEY REFERENCES proveedores(proveedor_id),
	firma_empleado_id INT FOREIGN KEY REFERENCES empleados(empleado_id),
	numero_factura VARCHAR(50) UNIQUE,
	fecha_compra DATE
)
GO

IF OBJECT_ID('compras_productos') IS NULL
CREATE TABLE compras_productos
(
	compra_producto_id INT IDENTITY CONSTRAINT PK_COMPRAS_PRODUCTOS PRIMARY KEY,
	compra_id INT NOT NULL FOREIGN KEY REFERENCES compras(compra_id),
	producto_id INT FOREIGN KEY REFERENCES productos(producto_id),
	cantidad INT,
	precio_unitario MONEY
)
GO

IF OBJECT_ID('estados_equipo') IS NULL
CREATE TABLE estados_equipo
(
	estado_equipo_id INT PRIMARY KEY IDENTITY,
	nombre_estado_equipo VARCHAR(30) UNIQUE
)
GO

IF OBJECT_ID('equipos') IS NULL
CREATE TABLE equipos
(
	equipo_id INT PRIMARY KEY IDENTITY,
	nombre_equipo VARCHAR(50),
	tipo_equipo VARCHAR(50),
	marca_equipo VARCHAR(50),
	modelo_equipo VARCHAR(50),
	estado_equipo_id INT FOREIGN KEY REFERENCES estados_equipo(estado_equipo_id)
)
GO

IF OBJECT_ID('orden_empleados') IS NULL
CREATE TABLE orden_empleados
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	empleado_id INT NOT NULL FOREIGN KEY REFERENCES empleados(empleado_id),
	turno VARCHAR(20),
	piso_area VARCHAR(20),
	CONSTRAINT PK_ORDEN_EMPLEADOS PRIMARY KEY (orden_id, empleado_id)
)
GO

IF OBJECT_ID('orden_productos') IS NULL
CREATE TABLE orden_productos
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	producto_id INT NOT NULL FOREIGN KEY REFERENCES productos(producto_id),
	cantidad INT,
	CONSTRAINT PK_ORDEN_PRODUCTOS PRIMARY KEY (orden_id, producto_id)
)
GO

IF OBJECT_ID('orden_equipos') IS NULL
CREATE TABLE orden_equipos
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	equipo_id INT NOT NULL FOREIGN KEY REFERENCES equipos(equipo_id),
	fecha_asignacion DATE,
	devuelto BIT,
	CONSTRAINT PK_ORDEN_EQUIPOS PRIMARY KEY (orden_id, equipo_id)
)
GO

IF OBJECT_ID('orden_actividades') IS NULL
CREATE TABLE orden_actividades
(
	orden_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES ordenes(orden_id),
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	estado BIT,
	duracion_min INT,
	CONSTRAINT PK_ORDEN_ACTIVIDADES PRIMARY KEY (orden_id, actividad_id)
)
GO

IF OBJECT_ID('formas_pago') IS NULL
CREATE TABLE formas_pago
(
	forma_pago_id INT PRIMARY KEY IDENTITY,
	nombre_forma_pago VARCHAR(30) UNIQUE
)
GO

IF OBJECT_ID('facturas') IS NULL
CREATE TABLE facturas
(
	factura_id INT PRIMARY KEY IDENTITY,
	orden_id VARCHAR(50) FOREIGN KEY REFERENCES ordenes(orden_id) UNIQUE,
	fecha_factura DATE,
	tipo_pago VARCHAR(10) NOT NULL DEFAULT 'Contado'
		CHECK (tipo_pago IN ('Contado', 'Credito')),
	estado_pago VARCHAR(50) DEFAULT 'Pendiente'
		CHECK (estado_pago IN ('Pendiente', 'Pagada')),
	fecha_pago DATE,
	forma_pago_id INT FOREIGN KEY REFERENCES formas_pago(forma_pago_id),
	precio_facturado MONEY
)
GO

IF OBJECT_ID('cotizaciones') IS NULL
CREATE TABLE cotizaciones
(
	cotizacion_id VARCHAR(50) PRIMARY KEY NOT NULL,
	identificacion_cliente VARCHAR(30),
	nombre_empresa VARCHAR(50),
	nombre_cliente VARCHAR(50),
	apellido_cliente VARCHAR(50),
	telefono_cliente VARCHAR(50),
	correo_cliente VARCHAR(50),
	locacion_id INT NULL FOREIGN KEY REFERENCES locaciones(locacion_id),
	direccion VARCHAR(80),
	fecha_cotizacion DATE,
	valida_hasta DATE,
	observacion VARCHAR(200),
	precio_cotizado MONEY
)
GO

IF OBJECT_ID('detalles_cotizacion') IS NULL
CREATE TABLE detalles_cotizacion
(
	cotizacion_id VARCHAR(50) NOT NULL FOREIGN KEY REFERENCES cotizaciones(cotizacion_id),
	linea INT NOT NULL,
	concepto VARCHAR(150) NOT NULL,
	cantidad INT NOT NULL,
	precio_unitario MONEY NOT NULL,
	CONSTRAINT PK_COTIZACION_DETALLE PRIMARY KEY (cotizacion_id, linea)
)
GO

IF COL_LENGTH('ordenes', 'cotizacion_id') IS NULL
	ALTER TABLE ordenes ADD cotizacion_id VARCHAR(50) NULL;
GO

IF OBJECT_ID('FK_ORDENES_COTIZACIONES') IS NULL
	ALTER TABLE ordenes
	ADD CONSTRAINT FK_ORDENES_COTIZACIONES FOREIGN KEY (cotizacion_id) REFERENCES cotizaciones(cotizacion_id);
GO

IF OBJECT_ID('detalles_factura') IS NULL
CREATE TABLE detalles_factura
(
	factura_id INT NOT NULL FOREIGN KEY REFERENCES facturas(factura_id),
	linea INT NOT NULL,
	concepto VARCHAR(150) NOT NULL,
	cantidad INT NOT NULL,
	precio_unitario MONEY NOT NULL,
	adicional BIT NOT NULL DEFAULT 0,
	CONSTRAINT PK_FACTURA_DETALLE PRIMARY KEY (factura_id, linea)
)
GO

IF OBJECT_ID('compras_equipos') IS NULL
CREATE TABLE compras_equipos
(
	compra_equipo_id INT IDENTITY CONSTRAINT PK_COMPRAS_EQUIPOS PRIMARY KEY,
	compra_id INT NOT NULL FOREIGN KEY REFERENCES compras(compra_id),
	equipo_id INT FOREIGN KEY REFERENCES equipos(equipo_id),
	precio_unitario MONEY
)
GO

IF OBJECT_ID('cuentas_por_cobrar') IS NULL
CREATE TABLE cuentas_por_cobrar
(
	cuenta_id INT PRIMARY KEY IDENTITY,
	factura_id INT FOREIGN KEY REFERENCES facturas(factura_id) NOT NULL UNIQUE,
	monto_total MONEY NOT NULL,
	prima MONEY,
	numero_cuotas INT NOT NULL,
	frecuencia VARCHAR(20),
	fecha_inicio DATE,
	estado_cuenta VARCHAR(20) DEFAULT 'Vigente'
		CHECK (estado_cuenta IN ('Vigente', 'Pagada', 'Vencida'))
)
GO

IF OBJECT_ID('cuotas') IS NULL
CREATE TABLE cuotas
(
	cuota_id INT PRIMARY KEY IDENTITY,
	cuenta_id INT FOREIGN KEY REFERENCES cuentas_por_cobrar(cuenta_id) NOT NULL,
	numero_cuota INT NOT NULL,
	fecha_vencimiento DATE NOT NULL,
	monto_cuota MONEY NOT NULL,
	monto_pagado MONEY DEFAULT 0,
	fecha_pago DATE,
	forma_pago_id INT FOREIGN KEY REFERENCES formas_pago(forma_pago_id),
	estado_cuota VARCHAR(20) DEFAULT 'Pendiente'
		CHECK (estado_cuota IN ('Pendiente', 'Parcial', 'Pagada', 'Vencida')),
	CONSTRAINT UQ_CUOTAS_cuenta_numero UNIQUE (cuenta_id, numero_cuota)
)
GO

IF COL_LENGTH('actividades', 'unidad') IS NULL
	ALTER TABLE actividades ADD unidad VARCHAR(20) NULL;
GO

IF COL_LENGTH('actividades', 'rendimiento') IS NULL
	ALTER TABLE actividades ADD rendimiento DECIMAL(10, 2) NULL;
GO

IF OBJECT_ID('parametros_costeo') IS NULL
CREATE TABLE parametros_costeo
(
	clave VARCHAR(40) PRIMARY KEY,
	valor DECIMAL(14, 4) NOT NULL,
	descripcion VARCHAR(150) NOT NULL
)
GO

IF OBJECT_ID('actividad_rangos') IS NULL
CREATE TABLE actividad_rangos
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	desde DECIMAL(12, 2) NOT NULL,
	hasta DECIMAL(12, 2) NULL,
	cantidad_personas INT NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_RANGOS PRIMARY KEY (actividad_id, desde)
)
GO

IF OBJECT_ID('actividad_productos') IS NULL
CREATE TABLE actividad_productos
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	producto_id INT NOT NULL FOREIGN KEY REFERENCES productos(producto_id),
	cantidad_por_unidad DECIMAL(10, 4) NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_PRODUCTOS PRIMARY KEY (actividad_id, producto_id)
)
GO

IF OBJECT_ID('actividad_equipos') IS NULL
CREATE TABLE actividad_equipos
(
	actividad_id INT NOT NULL FOREIGN KEY REFERENCES actividades(actividad_id),
	nombre_equipo VARCHAR(50) NOT NULL,
	CONSTRAINT PK_ACTIVIDAD_EQUIPOS PRIMARY KEY (actividad_id, nombre_equipo)
)
GO

IF COL_LENGTH('cotizaciones', 'servicio_id') IS NULL
	ALTER TABLE cotizaciones ADD servicio_id INT NULL;
GO

IF OBJECT_ID('FK_COTIZACIONES_SERVICIOS') IS NULL
	ALTER TABLE cotizaciones
	ADD CONSTRAINT FK_COTIZACIONES_SERVICIOS FOREIGN KEY (servicio_id) REFERENCES servicios(servicio_id);
GO

IF COL_LENGTH('detalles_cotizacion', 'actividad_id') IS NULL
	ALTER TABLE detalles_cotizacion ADD actividad_id INT NULL;
GO

IF COL_LENGTH('detalles_cotizacion', 'medida') IS NULL
	ALTER TABLE detalles_cotizacion ADD medida DECIMAL(12, 2) NULL;
GO

IF COL_LENGTH('detalles_cotizacion', 'cantidad_personas') IS NULL
	ALTER TABLE detalles_cotizacion ADD cantidad_personas INT NULL;
GO

IF OBJECT_ID('FK_COTIZACION_DETALLE_ACTIVIDADES') IS NULL
	ALTER TABLE detalles_cotizacion
	ADD CONSTRAINT FK_COTIZACION_DETALLE_ACTIVIDADES FOREIGN KEY (actividad_id) REFERENCES actividades(actividad_id);
GO

IF OBJECT_ID('abonos') IS NULL
CREATE TABLE abonos
(
	abono_id INT PRIMARY KEY IDENTITY,
	factura_id INT NOT NULL FOREIGN KEY REFERENCES facturas(factura_id),
	fecha_abono DATE NOT NULL,
	monto MONEY NOT NULL,
	forma_pago_id INT FOREIGN KEY REFERENCES formas_pago(forma_pago_id),
	saldo_despues MONEY NOT NULL
)
GO

IF COL_LENGTH('abonos', 'concepto') IS NULL
	ALTER TABLE abonos
	ADD concepto VARCHAR(20) NOT NULL
		CONSTRAINT DF_ABONOS_concepto DEFAULT 'Abono';
GO

IF COL_LENGTH('clientes', 'cliente_id') IS NULL AND COL_LENGTH('clientes', 'identificacion_cliente') IS NOT NULL
BEGIN
	DECLARE @sql NVARCHAR(MAX) = N'';

	SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(parent_object_id))
				+ N' DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.foreign_keys
	WHERE referenced_object_id = OBJECT_ID('clientes');

	SELECT @sql = @sql + N'ALTER TABLE clientes DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
	FROM sys.key_constraints
	WHERE parent_object_id = OBJECT_ID('clientes') AND type = 'PK';

	SET @sql = @sql + N'ALTER TABLE clientes ADD cliente_id INT IDENTITY(1, 1) NOT NULL; '
			 + N'ALTER TABLE clientes ADD CONSTRAINT PK_CLIENTES PRIMARY KEY (cliente_id); '
			 + N'ALTER TABLE clientes ADD CONSTRAINT UQ_CLIENTES_identificacion UNIQUE (identificacion_cliente); '
			 + N'ALTER TABLE locaciones ADD CONSTRAINT FK_LOCACIONES_CLIENTES '
			 + N'FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE; '
			 + N'ALTER TABLE ordenes ADD CONSTRAINT FK_ORDENES_CLIENTES '
			 + N'FOREIGN KEY (identificacion_cliente) REFERENCES clientes(identificacion_cliente) ON UPDATE CASCADE; ';

	EXEC sp_executesql @sql;
END
GO

IF COL_LENGTH('clientes', 'tipo_identificacion') IS NULL
	ALTER TABLE clientes
	ADD tipo_identificacion VARCHAR(10) NOT NULL
		CONSTRAINT DF_CLIENTES_tipoIdentificacion DEFAULT 'RTN';
GO

IF OBJECT_ID('CK_CLIENTES_tipoIdentificacion') IS NULL
	ALTER TABLE clientes
	ADD CONSTRAINT CK_CLIENTES_tipoIdentificacion CHECK (tipo_identificacion IN ('RTN', 'Identidad'));
GO

-- Los numeros de identidad y RTN se guardan sin guiones ni espacios (corrige datos viejos)
UPDATE cotizaciones
SET identificacion_cliente = REPLACE(REPLACE(identificacion_cliente, '-', ''), ' ', '')
WHERE identificacion_cliente LIKE '%[- ]%';
GO

UPDATE clientes
SET identificacion_cliente = REPLACE(REPLACE(identificacion_cliente, '-', ''), ' ', '')
WHERE identificacion_cliente LIKE '%[- ]%'
  AND NOT EXISTS (SELECT 1 FROM clientes x
				  WHERE x.identificacion_cliente = REPLACE(REPLACE(clientes.identificacion_cliente, '-', ''), ' ', '')
					AND x.cliente_id <> clientes.cliente_id);
GO

UPDATE empleados
SET identidad = REPLACE(REPLACE(identidad, '-', ''), ' ', '')
WHERE identidad LIKE '%[- ]%'
  AND NOT EXISTS (SELECT 1 FROM empleados x
				  WHERE x.identidad = REPLACE(REPLACE(empleados.identidad, '-', ''), ' ', '')
					AND x.empleado_id <> empleados.empleado_id);
GO

UPDATE clientes
SET tipo_identificacion = 'Identidad'
WHERE tipo_identificacion = 'RTN'
  AND LEN(identificacion_cliente) = 13
  AND identificacion_cliente NOT LIKE '%[^0-9]%';
GO

IF COL_LENGTH('empleados', 'huella_id') IS NOT NULL
BEGIN
	DECLARE @quitar NVARCHAR(MAX) = N'';

	SELECT @quitar = @quitar + N'ALTER TABLE empleados DROP CONSTRAINT ' + QUOTENAME(kc.name) + N'; '
	FROM sys.key_constraints kc
	INNER JOIN sys.index_columns ic ON ic.object_id = kc.parent_object_id AND ic.index_id = kc.unique_index_id
	INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
	WHERE kc.parent_object_id = OBJECT_ID('empleados') AND c.name = 'huella_id';

	SET @quitar = @quitar + N'ALTER TABLE empleados DROP COLUMN huella_id; ';

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
		nombre_equipo VARCHAR(50),
		tipo_equipo VARCHAR(50),
		marca_equipo VARCHAR(50),
		modelo_equipo VARCHAR(50),
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

IF TYPE_ID('dbo.tipo_actividad_rangos') IS NULL
	CREATE TYPE dbo.tipo_actividad_rangos AS TABLE
	(
		desde DECIMAL(12, 2) NOT NULL,
		hasta DECIMAL(12, 2) NULL,
		cantidad_personas INT NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_actividad_productos') IS NULL
	CREATE TYPE dbo.tipo_actividad_productos AS TABLE
	(
		producto_id INT NOT NULL PRIMARY KEY,
		cantidad_por_unidad DECIMAL(10, 4) NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_actividad_productos_nuevos') IS NULL
	CREATE TYPE dbo.tipo_actividad_productos_nuevos AS TABLE
	(
		nombre_producto VARCHAR(50) NOT NULL,
		tipo_producto VARCHAR(20) NOT NULL,
		cantidad_por_unidad DECIMAL(10, 4) NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_actividad_equipos') IS NULL
	CREATE TYPE dbo.tipo_actividad_equipos AS TABLE
	(
		nombre_equipo VARCHAR(50) NOT NULL PRIMARY KEY
	);
GO

IF TYPE_ID('dbo.tipo_producto_actividades') IS NULL
	CREATE TYPE dbo.tipo_producto_actividades AS TABLE
	(
		actividad_id INT NOT NULL PRIMARY KEY,
		cantidad_por_unidad DECIMAL(10, 4) NOT NULL
	);
GO

IF TYPE_ID('dbo.tipo_equipo_actividades') IS NULL
	CREATE TYPE dbo.tipo_equipo_actividades AS TABLE
	(
		actividad_id INT NOT NULL PRIMARY KEY
	);
GO

-- Datos iniciales

INSERT INTO formas_pago (nombre_forma_pago)
SELECT v.nombre
FROM (VALUES ('Efectivo'), ('Tarjeta'), ('Transferencia'), ('Cheque')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM formas_pago f WHERE f.nombre_forma_pago = v.nombre);
GO

INSERT INTO roles (nombre_rol)
SELECT v.nombre
FROM (VALUES ('Usuario Básico'), ('Usuario Medio'), ('Usuario Avanzado')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM roles r WHERE r.nombre_rol = v.nombre);
GO

IF NOT EXISTS (SELECT 1 FROM cargos)
	INSERT INTO cargos (nombre_cargo) VALUES ('Administrador'), ('Operario'), ('Supervisor');
GO

INSERT INTO estados_orden (nombre_estado_orden)
SELECT v.nombre
FROM (VALUES ('Pendiente'), ('En proceso'), ('Completada'), ('Cancelada')) v(nombre)
WHERE NOT EXISTS (SELECT 1 FROM estados_orden e WHERE e.nombre_estado_orden = v.nombre);
GO

IF NOT EXISTS (SELECT 1 FROM estados_equipo WHERE nombre_estado_equipo = 'Disponible')
	INSERT INTO estados_equipo (nombre_estado_equipo) VALUES ('Disponible');
GO

INSERT INTO parametros_costeo (clave, valor, descripcion)
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
WHERE NOT EXISTS (SELECT 1 FROM parametros_costeo p WHERE p.clave = v.clave);
GO

DECLARE @servicios TABLE (nombre VARCHAR(30));
DECLARE @act TABLE (servicio VARCHAR(30), descripcion VARCHAR(50), unidad VARCHAR(20), rendimiento DECIMAL(10, 2), perfil VARCHAR(5));
DECLARE @rango TABLE (perfil VARCHAR(5), desde DECIMAL(12, 2), hasta DECIMAL(12, 2), cantidad_personas INT);
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

INSERT INTO servicios (nombre_servicio)
SELECT s.nombre FROM @servicios s
WHERE NOT EXISTS (SELECT 1 FROM servicios x WHERE x.nombre_servicio = s.nombre);

INSERT INTO actividades (servicio_id, descripcion, frecuencia, precio, unidad, rendimiento)
SELECT sv.servicio_id, a.descripcion, NULL, NULL, a.unidad, a.rendimiento
FROM @act a
INNER JOIN servicios sv ON sv.nombre_servicio = a.servicio
WHERE NOT EXISTS (SELECT 1 FROM actividades x WHERE x.descripcion = a.descripcion);

UPDATE x
SET x.unidad = a.unidad, x.rendimiento = a.rendimiento
FROM actividades x
INNER JOIN @act a ON a.descripcion = x.descripcion
WHERE x.rendimiento IS NULL;

INSERT INTO actividad_rangos (actividad_id, desde, hasta, cantidad_personas)
SELECT x.actividad_id, r.desde, r.hasta, r.cantidad_personas
FROM @act a
INNER JOIN actividades x ON x.descripcion = a.descripcion
INNER JOIN @rango r ON r.perfil = a.perfil
WHERE NOT EXISTS (SELECT 1 FROM actividad_rangos y WHERE y.actividad_id = x.actividad_id);

IF NOT EXISTS (SELECT 1 FROM compras WHERE numero_factura = 'INVENTARIO-INICIAL')
BEGIN
	DECLARE @nuevos_p TABLE (producto_id INT, nombre VARCHAR(50));
	DECLARE @nuevos_e TABLE (equipo_id INT, nombre VARCHAR(50));
	DECLARE @proveedor INT;
	DECLARE @compra INT;
	DECLARE @disponible INT = (SELECT TOP 1 estado_equipo_id FROM estados_equipo WHERE nombre_estado_equipo = 'Disponible');

	IF NOT EXISTS (SELECT 1 FROM proveedores WHERE nombre_proveedor = 'Proveedor inicial')
		INSERT INTO proveedores (nombre_proveedor, telefono_proveedor, correo_proveedor) VALUES ('Proveedor inicial', '', '');

	SELECT TOP 1 @proveedor = proveedor_id FROM proveedores WHERE nombre_proveedor = 'Proveedor inicial';

	INSERT INTO compras (proveedor_id, firma_empleado_id, numero_factura, fecha_compra)
	VALUES (@proveedor, NULL, 'INVENTARIO-INICIAL', CAST(GETDATE() AS DATE));

	SET @compra = CAST(SCOPE_IDENTITY() AS INT);

	INSERT INTO productos (nombre_producto, tipo_producto, stock)
	OUTPUT inserted.producto_id, inserted.nombre_producto INTO @nuevos_p
	SELECT p.nombre, p.tipo, p.stock FROM @prod p
	WHERE NOT EXISTS (SELECT 1 FROM productos x WHERE x.nombre_producto = p.nombre);

	INSERT INTO compras_productos (compra_id, producto_id, cantidad, precio_unitario)
	SELECT @compra, n.producto_id, p.stock, p.costo
	FROM @nuevos_p n INNER JOIN @prod p ON p.nombre = n.nombre;

	INSERT INTO equipos (nombre_equipo, tipo_equipo, marca_equipo, modelo_equipo, estado_equipo_id)
	OUTPUT inserted.equipo_id, inserted.nombre_equipo INTO @nuevos_e
	SELECT e.nombre, e.tipo, e.marca, e.modelo, @disponible
	FROM @eq e
	INNER JOIN (VALUES (1), (2), (3), (4)) n(numero) ON n.numero <= e.unidades
	WHERE NOT EXISTS (SELECT 1 FROM equipos x WHERE x.nombre_equipo = e.nombre);

	INSERT INTO compras_equipos (compra_id, equipo_id, precio_unitario)
	SELECT @compra, n.equipo_id, e.costo
	FROM @nuevos_e n INNER JOIN @eq e ON e.nombre = n.nombre;
END

INSERT INTO actividad_productos (actividad_id, producto_id, cantidad_por_unidad)
SELECT x.actividad_id, p.producto_id, a.cantidad
FROM @ap a
INNER JOIN actividades x ON x.descripcion = a.actividad
INNER JOIN productos p ON p.nombre_producto = a.producto
WHERE NOT EXISTS (SELECT 1 FROM actividad_productos y WHERE y.actividad_id = x.actividad_id);

INSERT INTO actividad_equipos (actividad_id, nombre_equipo)
SELECT x.actividad_id, a.equipo
FROM @ae a
INNER JOIN actividades x ON x.descripcion = a.actividad
WHERE NOT EXISTS (SELECT 1 FROM actividad_equipos y WHERE y.actividad_id = x.actividad_id);
GO

SET NOEXEC OFF;
GO
