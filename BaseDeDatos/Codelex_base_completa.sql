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
	contrasena_hash VARCHAR(255)
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

-- Actualización de bases creadas con versiones anteriores (si la base ya está al día, no cambia nada)

IF COL_LENGTH('CLIENTES', 'estadoCliente') IS NULL
	ALTER TABLE CLIENTES
	ADD estadoCliente BIT NOT NULL
		CONSTRAINT DF_CLIENTES_estadoCliente DEFAULT 1;
GO

IF COL_LENGTH('CLIENTES', 'cliente_id') IS NOT NULL
BEGIN
	IF EXISTS (SELECT 1 FROM CLIENTES
			   WHERE identificacionCliente IS NULL OR LTRIM(RTRIM(identificacionCliente)) = '')
	BEGIN
		RAISERROR('Hay clientes sin RTN / identidad. Póngales uno antes de ejecutar este script.', 16, 1);
		SET NOEXEC ON;
	END
END
GO

IF COL_LENGTH('CLIENTES', 'cliente_id') IS NOT NULL
BEGIN
	SET XACT_ABORT ON;
	BEGIN TRY
		BEGIN TRANSACTION;

		-- Nuevas columnas
		EXEC('ALTER TABLE LOCACIONES ADD identificacionCliente VARCHAR(30) NULL');
		EXEC('ALTER TABLE ORDENES ADD identificacionCliente VARCHAR(30) NULL');

		-- Pasar los datos que ya existen
		EXEC('UPDATE l SET l.identificacionCliente = c.identificacionCliente
			  FROM LOCACIONES l
			  INNER JOIN CLIENTES c ON c.locacion_id = l.locacion_id');

		EXEC('UPDATE o SET o.identificacionCliente = c.identificacionCliente
			  FROM ORDENES o
			  INNER JOIN CLIENTES c ON c.cliente_id = o.cliente_id');

		-- Quitar las llaves viejas de CLIENTES y las que apuntan a CLIENTES
		DECLARE @sql NVARCHAR(MAX) = N'';

		SELECT @sql = @sql + N'ALTER TABLE ' + QUOTENAME(OBJECT_NAME(parent_object_id))
					+ N' DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
		FROM sys.foreign_keys
		WHERE referenced_object_id = OBJECT_ID('CLIENTES')
		   OR parent_object_id = OBJECT_ID('CLIENTES');

		SELECT @sql = @sql + N'ALTER TABLE CLIENTES DROP CONSTRAINT ' + QUOTENAME(name) + N'; '
		FROM sys.key_constraints
		WHERE parent_object_id = OBJECT_ID('CLIENTES');

		EXEC sp_executesql @sql;

		-- Borrar las columnas que ya no se usan
		EXEC('ALTER TABLE ORDENES DROP COLUMN cliente_id');
		EXEC('ALTER TABLE CLIENTES DROP COLUMN locacion_id');
		EXEC('ALTER TABLE CLIENTES DROP COLUMN cliente_id');

		-- Nueva llave primaria y llaves foráneas
		EXEC('ALTER TABLE CLIENTES ALTER COLUMN identificacionCliente VARCHAR(30) NOT NULL');
		EXEC('ALTER TABLE CLIENTES ADD CONSTRAINT PK_CLIENTES PRIMARY KEY (identificacionCliente)');
		EXEC('ALTER TABLE LOCACIONES ADD CONSTRAINT FK_LOCACIONES_CLIENTES
			  FOREIGN KEY (identificacionCliente) REFERENCES CLIENTES(identificacionCliente)');
		EXEC('ALTER TABLE ORDENES ADD CONSTRAINT FK_ORDENES_CLIENTES
			  FOREIGN KEY (identificacionCliente) REFERENCES CLIENTES(identificacionCliente)');

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH
END
GO

IF COL_LENGTH('EMPLEADOS', 'estadoEmpleado') IS NULL
	ALTER TABLE EMPLEADOS
	ADD estadoEmpleado BIT NOT NULL
		CONSTRAINT DF_EMPLEADOS_estadoEmpleado DEFAULT 1;
GO

IF COL_LENGTH('VEHICULOS', 'estadoVehiculo') IS NULL
	ALTER TABLE VEHICULOS
	ADD estadoVehiculo BIT NOT NULL
		CONSTRAINT DF_VEHICULOS_estadoVehiculo DEFAULT 1;
GO

IF COL_LENGTH('ORDENES', 'observaciones') < 200
	ALTER TABLE ORDENES ALTER COLUMN observaciones VARCHAR(200) NULL;
GO

DECLARE @detalle TABLE (tabla SYSNAME, columna SYSNAME);
INSERT INTO @detalle VALUES ('ORDEN_ACTIVIDADES', 'actividad_id'),
							('ORDEN_EMPLEADOS', 'empleado_id'),
							('ORDEN_PRODUCTOS', 'producto_id'),
							('ORDEN_EQUIPOS', 'equipo_id');

DECLARE @tabla SYSNAME, @columna SYSNAME, @pk SYSNAME, @sql NVARCHAR(MAX);

DECLARE cursorDetalle CURSOR LOCAL FAST_FORWARD FOR
	SELECT tabla, columna FROM @detalle;

OPEN cursorDetalle;
FETCH NEXT FROM cursorDetalle INTO @tabla, @columna;

WHILE @@FETCH_STATUS = 0
BEGIN
	SET @pk = NULL;

	SELECT @pk = kc.name
	FROM sys.key_constraints kc
	WHERE kc.parent_object_id = OBJECT_ID(@tabla)
	  AND kc.type = 'PK'
	  AND (SELECT COUNT(1) FROM sys.index_columns ic
		   WHERE ic.object_id = kc.parent_object_id
			 AND ic.index_id = kc.unique_index_id) = 1;

	IF @pk IS NOT NULL
	BEGIN
		SET @sql = 'ALTER TABLE ' + @tabla + ' DROP CONSTRAINT ' + QUOTENAME(@pk);
		EXEC (@sql);

		SET @sql = 'ALTER TABLE ' + @tabla + ' ALTER COLUMN ' + @columna + ' INT NOT NULL';
		EXEC (@sql);

		SET @sql = 'ALTER TABLE ' + @tabla + ' ADD CONSTRAINT ' + QUOTENAME('PK_' + @tabla)
				 + ' PRIMARY KEY (orden_id, ' + @columna + ')';
		EXEC (@sql);
	END

	FETCH NEXT FROM cursorDetalle INTO @tabla, @columna;
END

CLOSE cursorDetalle;
DEALLOCATE cursorDetalle;
GO

IF COL_LENGTH('ORDEN_ACTIVIDADES', 'orden_actividad_id') IS NOT NULL
	ALTER TABLE ORDEN_ACTIVIDADES DROP COLUMN orden_actividad_id;
IF COL_LENGTH('ORDEN_EMPLEADOS', 'orden_empleado_id') IS NOT NULL
	ALTER TABLE ORDEN_EMPLEADOS DROP COLUMN orden_empleado_id;
IF COL_LENGTH('ORDEN_PRODUCTOS', 'orden_producto_id') IS NOT NULL
	ALTER TABLE ORDEN_PRODUCTOS DROP COLUMN orden_producto_id;
IF COL_LENGTH('ORDEN_EQUIPOS', 'orden_equipo_id') IS NOT NULL
	ALTER TABLE ORDEN_EQUIPOS DROP COLUMN orden_equipo_id;
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints k
		   INNER JOIN sys.index_columns ic ON ic.object_id = k.parent_object_id AND ic.index_id = k.unique_index_id
		   INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
		   WHERE k.parent_object_id = OBJECT_ID('COMPRAS_PRODUCTOS') AND k.type = 'PK' AND c.name = 'compra_id')
   AND COL_LENGTH('COMPRAS_PRODUCTOS', 'compra_producto_id') IS NULL
BEGIN
	DECLARE @pkProductos SYSNAME = (SELECT name FROM sys.key_constraints
									WHERE parent_object_id = OBJECT_ID('COMPRAS_PRODUCTOS') AND type = 'PK');
	EXEC('ALTER TABLE COMPRAS_PRODUCTOS DROP CONSTRAINT ' + @pkProductos);
	EXEC('ALTER TABLE COMPRAS_PRODUCTOS ADD compra_producto_id INT IDENTITY(1,1)
		  CONSTRAINT PK_COMPRAS_PRODUCTOS PRIMARY KEY');
END
GO

IF EXISTS (SELECT 1 FROM sys.key_constraints k
		   INNER JOIN sys.index_columns ic ON ic.object_id = k.parent_object_id AND ic.index_id = k.unique_index_id
		   INNER JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
		   WHERE k.parent_object_id = OBJECT_ID('COMPRAS_EQUIPOS') AND k.type = 'PK' AND c.name = 'compra_id')
   AND COL_LENGTH('COMPRAS_EQUIPOS', 'compra_equipo_id') IS NULL
BEGIN
	DECLARE @pkEquipos SYSNAME = (SELECT name FROM sys.key_constraints
								  WHERE parent_object_id = OBJECT_ID('COMPRAS_EQUIPOS') AND type = 'PK');
	EXEC('ALTER TABLE COMPRAS_EQUIPOS DROP CONSTRAINT ' + @pkEquipos);
	EXEC('ALTER TABLE COMPRAS_EQUIPOS ADD compra_equipo_id INT IDENTITY(1,1)
		  CONSTRAINT PK_COMPRAS_EQUIPOS PRIMARY KEY');
END
GO

-- Login y recuperación de contraseña

CREATE OR ALTER PROCEDURE sp_ObtenerUsuarioLogin
	@usuario VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT u.usuario_id,
		   u.empleado_id,
		   u.usuario,
		   u.contrasena_hash,
		   e.nombreEmpleado,
		   e.apellidoEmpleado,
		   c.nombreCargo
	FROM USUARIOS u
	INNER JOIN EMPLEADOS e ON e.empleado_id = u.empleado_id
	LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
	WHERE u.usuario = @usuario;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarUsuarioRecuperacion
	@usuario VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT u.usuario_id,
		   e.nombreEmpleado,
		   e.correoEmpleado
	FROM USUARIOS u
	INNER JOIN EMPLEADOS e ON e.empleado_id = u.empleado_id
	WHERE u.usuario = @usuario;
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarContrasena
	@usuario_id INT,
	@nuevo_hash VARCHAR(255),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM USUARIOS WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	IF @nuevo_hash IS NULL OR LTRIM(RTRIM(@nuevo_hash)) = ''
	BEGIN
		SET @mensaje = 'La contrasena no puede estar vacia.';
		RETURN;
	END

	UPDATE USUARIOS
	SET contrasena_hash = @nuevo_hash
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = 'La contrasena se cambio correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarContrasenaPorCorreo
	@correo VARCHAR(50),
	@contrasena_hash VARCHAR(255),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @correo = LTRIM(RTRIM(ISNULL(@correo, '')));

	DECLARE @cantidad INT;

	SELECT @cantidad = COUNT(1)
	FROM USUARIOS u
	INNER JOIN EMPLEADOS e ON e.empleado_id = u.empleado_id
	WHERE e.correoEmpleado = @correo;

	IF @cantidad = 0
	BEGIN
		SET @mensaje = 'No existe un usuario asociado a ese correo.';
		RETURN;
	END

	IF @cantidad > 1
	BEGIN
		SET @mensaje = 'Hay mas de un usuario con ese correo. Contacta a soporte.';
		RETURN;
	END

	UPDATE u
	SET u.contrasena_hash = @contrasena_hash
	FROM USUARIOS u
	INNER JOIN EMPLEADOS e ON e.empleado_id = u.empleado_id
	WHERE e.correoEmpleado = @correo;

	SET @resultado = 1;
	SET @mensaje = 'Contrasena cambiada correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ExisteCorreoEmpleado
	@correo VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT(1) AS cantidad
	FROM EMPLEADOS
	WHERE correoEmpleado = LTRIM(RTRIM(@correo));
END
GO

-- Clientes

CREATE OR ALTER PROCEDURE sp_InsertarCliente
	@identificacionCliente VARCHAR(30),
	@nombreEmpresa VARCHAR(50),
	@nombreCliente VARCHAR(50),
	@apellidoCliente VARCHAR(50),
	@telefonoCliente VARCHAR(50),
	@correoCliente VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @identificacionCliente = LTRIM(RTRIM(ISNULL(@identificacionCliente, '')));

	IF @identificacionCliente = ''
	BEGIN
		SET @mensaje = 'La identificacion (RTN / identidad) es obligatoria.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM CLIENTES WHERE identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'Ya existe un cliente con esa identificacion.';
		RETURN;
	END

	INSERT INTO CLIENTES (identificacionCliente, nombreEmpresa, nombreCliente, apellidoCliente,
						  telefonoCliente, correoCliente)
	VALUES (@identificacionCliente, @nombreEmpresa, @nombreCliente, @apellidoCliente,
			@telefonoCliente, @correoCliente);

	SET @resultado = 1;
	SET @mensaje = 'Cliente registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarCliente
	@identificacionCliente VARCHAR(30),
	@nombreEmpresa VARCHAR(50),
	@nombreCliente VARCHAR(50),
	@apellidoCliente VARCHAR(50),
	@telefonoCliente VARCHAR(50),
	@correoCliente VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM CLIENTES WHERE identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'El cliente no existe.';
		RETURN;
	END

	UPDATE CLIENTES
	SET nombreEmpresa = @nombreEmpresa,
		nombreCliente = @nombreCliente,
		apellidoCliente = @apellidoCliente,
		telefonoCliente = @telefonoCliente,
		correoCliente = @correoCliente
	WHERE identificacionCliente = @identificacionCliente;

	SET @resultado = 1;
	SET @mensaje = 'Cliente actualizado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoCliente
	@identificacionCliente VARCHAR(30),
	@estadoCliente BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM CLIENTES WHERE identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'El cliente no existe.';
		RETURN;
	END

	UPDATE CLIENTES
	SET estadoCliente = @estadoCliente
	WHERE identificacionCliente = @identificacionCliente;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estadoCliente = 1
						THEN 'Cliente activado correctamente.'
						ELSE 'Cliente desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarClientes
	@texto VARCHAR(50) = NULL,
	@buscarPor VARCHAR(10) = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @buscarPor = UPPER(LTRIM(RTRIM(ISNULL(@buscarPor, ''))));

	SELECT c.identificacionCliente, c.nombreEmpresa, c.nombreCliente, c.apellidoCliente,
		   c.telefonoCliente, c.correoCliente, c.estadoCliente,
		   CASE WHEN c.estadoCliente = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM CLIENTES c
	WHERE (@estado IS NULL OR c.estadoCliente = @estado)
	  AND (@texto = ''
		   OR (@buscarPor = 'RTN'
			   AND c.identificacionCliente LIKE '%' + @texto + '%')
		   OR (@buscarPor = 'NOMBRE'
			   AND (c.nombreEmpresa LIKE '%' + @texto + '%'
					OR c.nombreCliente LIKE '%' + @texto + '%'
					OR c.apellidoCliente LIKE '%' + @texto + '%'
					OR ISNULL(c.nombreCliente, '') + ' ' + ISNULL(c.apellidoCliente, '') LIKE '%' + @texto + '%'))
		   OR (@buscarPor NOT IN ('RTN', 'NOMBRE')
			   AND (c.identificacionCliente LIKE '%' + @texto + '%'
					OR c.nombreEmpresa LIKE '%' + @texto + '%'
					OR c.nombreCliente LIKE '%' + @texto + '%'
					OR c.apellidoCliente LIKE '%' + @texto + '%')))
	ORDER BY c.nombreEmpresa, c.nombreCliente, c.apellidoCliente;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerCliente
	@identificacionCliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.identificacionCliente, c.nombreEmpresa, c.nombreCliente, c.apellidoCliente,
		   c.telefonoCliente, c.correoCliente, c.estadoCliente,
		   CASE WHEN c.estadoCliente = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM CLIENTES c
	WHERE c.identificacionCliente = @identificacionCliente;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarOrdenesPorCliente
	@identificacionCliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   s.nombreServicio AS servicio,
		   ISNULL(o.fecha_servicio, o.fecha_solicitud) AS fecha,
		   e.nombreEstadoOrden AS estado
	FROM ORDENES o
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	LEFT JOIN ESTADO_ORDENES e ON e.estado_orden_id = o.estado_orden_id
	WHERE o.identificacionCliente = @identificacionCliente
	ORDER BY fecha DESC, o.orden_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarFacturasPorCliente
	@identificacionCliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT f.factura_id,
		   f.fechaFactura AS fecha,
		   f.estado_pago,
		   f.precio_facturado AS monto
	FROM FACTURAS f
	INNER JOIN ORDENES o ON o.orden_id = f.orden_id
	WHERE o.identificacionCliente = @identificacionCliente
	ORDER BY f.fechaFactura DESC, f.factura_id DESC;
END
GO

-- Locaciones

CREATE OR ALTER PROCEDURE sp_ListarLocacionesPorCliente
	@identificacionCliente VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT locacion_id, nombreLocacion, direccion, tipoLocacion, identificacionCliente
	FROM LOCACIONES
	WHERE identificacionCliente = @identificacionCliente
	ORDER BY nombreLocacion;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarLocaciones
AS
BEGIN
	SET NOCOUNT ON;

	SELECT locacion_id, nombreLocacion, direccion, tipoLocacion, identificacionCliente
	FROM LOCACIONES
	ORDER BY nombreLocacion;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarLocacion
	@identificacionCliente VARCHAR(30),
	@nombreLocacion VARCHAR(60),
	@direccion VARCHAR(80),
	@tipoLocacion VARCHAR(25) = NULL,
	@locacion_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @locacion_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM CLIENTES WHERE identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'El cliente de la locacion no existe.';
		RETURN;
	END

	IF @nombreLocacion IS NULL OR LTRIM(RTRIM(@nombreLocacion)) = ''
	BEGIN
		SET @mensaje = 'El nombre de la locacion es obligatorio.';
		RETURN;
	END

	INSERT INTO LOCACIONES (identificacionCliente, nombreLocacion, direccion, tipoLocacion)
	VALUES (@identificacionCliente, @nombreLocacion, @direccion, @tipoLocacion);

	SET @locacion_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Locacion registrada correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarLocacion
	@locacion_id INT,
	@identificacionCliente VARCHAR(30),
	@nombreLocacion VARCHAR(60),
	@direccion VARCHAR(80),
	@tipoLocacion VARCHAR(25) = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM LOCACIONES WHERE locacion_id = @locacion_id)
	BEGIN
		SET @mensaje = 'La locacion no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM CLIENTES WHERE identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'El cliente de la locacion no existe.';
		RETURN;
	END

	UPDATE LOCACIONES
	SET identificacionCliente = @identificacionCliente,
		nombreLocacion = @nombreLocacion,
		direccion = @direccion,
		tipoLocacion = @tipoLocacion
	WHERE locacion_id = @locacion_id;

	SET @resultado = 1;
	SET @mensaje = 'Locacion actualizada correctamente.';
END
GO

-- Empleados

CREATE OR ALTER PROCEDURE sp_ListarCargos
AS
BEGIN
	SET NOCOUNT ON;
	SELECT cargo_id, nombreCargo FROM CARGOS ORDER BY nombreCargo;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarEmpleados
	@texto VARCHAR(50) = '',
	@buscarPor VARCHAR(20) = '',
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @buscarPor = UPPER(LTRIM(RTRIM(ISNULL(@buscarPor, ''))));

	SELECT e.empleado_id, e.cargo_id, c.nombreCargo,
		   e.nombreEmpleado, e.apellidoEmpleado,
		   e.nombreEmpleado + ' ' + e.apellidoEmpleado AS nombreCompleto,
		   e.identidad, e.telefonoEmpleado, e.correoEmpleado, e.huella_id,
		   u.usuario, e.estadoEmpleado,
		   CASE WHEN e.estadoEmpleado = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM EMPLEADOS e
	LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
	LEFT JOIN USUARIOS u ON u.empleado_id = e.empleado_id
	WHERE (@estado IS NULL OR e.estadoEmpleado = @estado)
	  AND (@texto = ''
		   OR (@buscarPor = 'NOMBRE' AND (e.nombreEmpleado + ' ' + e.apellidoEmpleado) LIKE '%' + @texto + '%')
		   OR (@buscarPor = 'IDENTIDAD' AND e.identidad LIKE '%' + @texto + '%')
		   OR (@buscarPor = 'CARGO' AND c.nombreCargo LIKE '%' + @texto + '%')
		   OR (@buscarPor = ''
			   AND ((e.nombreEmpleado + ' ' + e.apellidoEmpleado) LIKE '%' + @texto + '%'
				 OR e.identidad LIKE '%' + @texto + '%'
				 OR c.nombreCargo LIKE '%' + @texto + '%'
				 OR e.correoEmpleado LIKE '%' + @texto + '%'
				 OR u.usuario LIKE '%' + @texto + '%')))
	ORDER BY e.nombreEmpleado, e.apellidoEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerEmpleado
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id, e.cargo_id, c.nombreCargo,
		   e.nombreEmpleado, e.apellidoEmpleado, e.identidad,
		   e.telefonoEmpleado, e.correoEmpleado, e.huella_id,
		   u.usuario, e.estadoEmpleado,
		   CASE WHEN e.estadoEmpleado = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM EMPLEADOS e
	LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
	LEFT JOIN USUARIOS u ON u.empleado_id = e.empleado_id
	WHERE e.empleado_id = @empleado_id;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarEmpleado
	@cargo_id INT,
	@nombreEmpleado VARCHAR(30),
	@apellidoEmpleado VARCHAR(30),
	@identidad VARCHAR(30),
	@telefonoEmpleado VARCHAR(50) = NULL,
	@correoEmpleado VARCHAR(50) = NULL,
	@huella_id VARCHAR(50),
	@empleado_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @empleado_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM CARGOS WHERE cargo_id = @cargo_id)
	BEGIN
		SET @mensaje = 'El cargo seleccionado no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM EMPLEADOS WHERE identidad = @identidad)
	BEGIN
		SET @mensaje = 'Ya existe un empleado con esa identidad.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM EMPLEADOS WHERE huella_id = @huella_id)
	BEGIN
		SET @mensaje = 'Ya existe un empleado con ese ID de huella.';
		RETURN;
	END

	INSERT INTO EMPLEADOS (cargo_id, nombreEmpleado, apellidoEmpleado, identidad,
						   telefonoEmpleado, correoEmpleado, huella_id)
	VALUES (@cargo_id, @nombreEmpleado, @apellidoEmpleado, @identidad,
			NULLIF(@telefonoEmpleado, ''), NULLIF(@correoEmpleado, ''), @huella_id);

	SET @empleado_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Empleado registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarEmpleado
	@empleado_id INT,
	@cargo_id INT,
	@nombreEmpleado VARCHAR(30),
	@apellidoEmpleado VARCHAR(30),
	@identidad VARCHAR(30),
	@telefonoEmpleado VARCHAR(50) = NULL,
	@correoEmpleado VARCHAR(50) = NULL,
	@huella_id VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM EMPLEADOS WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'El empleado no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM CARGOS WHERE cargo_id = @cargo_id)
	BEGIN
		SET @mensaje = 'El cargo seleccionado no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM EMPLEADOS WHERE identidad = @identidad AND empleado_id <> @empleado_id)
	BEGIN
		SET @mensaje = 'Ya existe otro empleado con esa identidad.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM EMPLEADOS WHERE huella_id = @huella_id AND empleado_id <> @empleado_id)
	BEGIN
		SET @mensaje = 'Ya existe otro empleado con ese ID de huella.';
		RETURN;
	END

	UPDATE EMPLEADOS
	SET cargo_id = @cargo_id,
		nombreEmpleado = @nombreEmpleado,
		apellidoEmpleado = @apellidoEmpleado,
		identidad = @identidad,
		telefonoEmpleado = NULLIF(@telefonoEmpleado, ''),
		correoEmpleado = NULLIF(@correoEmpleado, ''),
		huella_id = @huella_id
	WHERE empleado_id = @empleado_id;

	SET @resultado = 1;
	SET @mensaje = 'Empleado actualizado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoEmpleado
	@empleado_id INT,
	@estadoEmpleado BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM EMPLEADOS WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'El empleado no existe.';
		RETURN;
	END

	UPDATE EMPLEADOS SET estadoEmpleado = @estadoEmpleado WHERE empleado_id = @empleado_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estadoEmpleado = 1 THEN 'Empleado activado correctamente.'
						ELSE 'Empleado desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_AsistenciasPorEmpleado
	@empleado_id INT,
	@desde DATE,
	@hasta DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT CONVERT(VARCHAR(10), a.fechaAsistencia, 103) AS fechaAsistencia,
		   CONVERT(VARCHAR(5), a.hora_entrada, 108) AS hora_entrada,
		   CONVERT(VARCHAR(5), a.hora_salida, 108) AS hora_salida,
		   CASE WHEN a.hora_entrada IS NOT NULL AND a.hora_salida IS NOT NULL
				THEN CAST(DATEDIFF(MINUTE, a.hora_entrada, a.hora_salida) / 60.0 AS DECIMAL(6, 2))
				ELSE 0 END AS horas
	FROM ASISTENCIAS a
	WHERE a.empleado_id = @empleado_id
	  AND a.fechaAsistencia BETWEEN @desde AND @hasta
	ORDER BY a.fechaAsistencia;
END
GO

CREATE OR ALTER PROCEDURE sp_OrdenesPorEmpleado
	@empleado_id INT,
	@desde DATE,
	@hasta DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   ISNULL(c.nombreEmpresa, o.identificacionCliente) AS cliente,
		   s.nombreServicio,
		   CONVERT(VARCHAR(10), o.fecha_servicio, 103) AS fecha_servicio,
		   oe.turno, oe.piso_area
	FROM ORDEN_EMPLEADOS oe
	INNER JOIN ORDENES o ON o.orden_id = oe.orden_id
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	WHERE oe.empleado_id = @empleado_id
	  AND o.fecha_servicio BETWEEN @desde AND @hasta
	ORDER BY o.fecha_servicio, o.orden_id;
END
GO

-- Órdenes

CREATE OR ALTER PROCEDURE sp_ListarEstadosOrden
AS
BEGIN
	SET NOCOUNT ON;

	SELECT estado_orden_id, nombreEstadoOrden
	FROM ESTADO_ORDENES
	ORDER BY estado_orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarServicios
AS
BEGIN
	SET NOCOUNT ON;

	SELECT servicio_id, nombreServicio
	FROM SERVICIOS
	ORDER BY nombreServicio;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarActividades
AS
BEGIN
	SET NOCOUNT ON;

	SELECT actividad_id,
		   ISNULL(servicio_id, 0) AS servicio_id,
		   ISNULL(descripcion, '') +
		   CASE WHEN ISNULL(frecuencia, '') = '' THEN '' ELSE ' (' + frecuencia + ')' END AS texto
	FROM ACTIVIDADES
	ORDER BY descripcion;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEmpleadosOrden
AS
BEGIN
	SET NOCOUNT ON;

	SELECT empleado_id,
		   LTRIM(ISNULL(nombreEmpleado, '') + ' ' + ISNULL(apellidoEmpleado, '')) AS nombreCompleto
	FROM EMPLEADOS
	WHERE estadoEmpleado = 1
	ORDER BY nombreEmpleado, apellidoEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProductosEquipos
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 'P' AS tipo, p.producto_id AS id, ISNULL(p.nombreProducto, '') AS nombre,
		   ISNULL(p.stock, 0) AS stock, '' AS detalle
	FROM PRODUCTOS p

	UNION ALL

	SELECT 'E', e.equipo_id, ISNULL(e.nombreEquipo, ''), 0,
		   LTRIM(RTRIM(ISNULL(e.marcaEquipo, '') + ' ' + ISNULL(e.modeloEquipo, '')))
	FROM EQUIPOS e
	WHERE NOT EXISTS (SELECT 1 FROM ORDEN_EQUIPOS oe
					  WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0)

	ORDER BY tipo DESC, nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarOrden
	@identificacionCliente VARCHAR(30),
	@locacion_id INT,
	@servicio_id INT,
	@supervisor_id INT,
	@vehiculo_id INT,
	@fecha_solicitud DATE,
	@fecha_servicio DATE,
	@estado_orden_id INT,
	@observaciones VARCHAR(200),
	@turno VARCHAR(20),
	@piso_area VARCHAR(20),
	@actividades dbo.tipo_lista_actividades READONLY,
	@empleados dbo.tipo_lista_empleados READONLY,
	@productos dbo.tipo_lista_productos READONLY,
	@equipos dbo.tipo_lista_equipos READONLY,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT,
	@orden_id VARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @orden_id = '';

	SET @identificacionCliente = LTRIM(RTRIM(ISNULL(@identificacionCliente, '')));

	IF NOT EXISTS (SELECT 1 FROM CLIENTES
				   WHERE identificacionCliente = @identificacionCliente AND estadoCliente = 1)
	BEGIN
		SET @mensaje = 'El cliente no existe o esta inactivo.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM LOCACIONES
				   WHERE locacion_id = @locacion_id AND identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'La locacion no pertenece al cliente.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM VEHICULOS WHERE vehiculo_id = @vehiculo_id AND estadoVehiculo = 1)
	BEGIN
		SET @mensaje = 'El vehiculo no existe o esta inactivo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos x
			   INNER JOIN PRODUCTOS p ON p.producto_id = x.producto_id
			   WHERE x.cantidad > ISNULL(p.stock, 0))
	BEGIN
		SET @mensaje = 'No hay stock suficiente de uno de los productos.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @equipos x
			   INNER JOIN ORDEN_EQUIPOS oe ON oe.equipo_id = x.equipo_id
			   WHERE ISNULL(oe.devuelto, 0) = 0)
	BEGIN
		SET @mensaje = 'Uno de los equipos ya esta asignado a otra orden.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @siguiente INT;

		SELECT @siguiente = ISNULL(MAX(TRY_CAST(SUBSTRING(orden_id, 5, 20) AS INT)), 0) + 1
		FROM ORDENES WITH (UPDLOCK, HOLDLOCK)
		WHERE orden_id LIKE 'ORD-%';

		SET @orden_id = 'ORD-' + RIGHT('000000' + CAST(@siguiente AS VARCHAR(10)), 6);

		INSERT INTO ORDENES (orden_id, locacion_id, servicio_id, supervisor_id, vehiculo_id,
							 fecha_solicitud, fecha_servicio, estado_orden_id, observaciones,
							 conformidad_cliente, identificacionCliente)
		VALUES (@orden_id, @locacion_id, @servicio_id, @supervisor_id, @vehiculo_id,
				@fecha_solicitud, @fecha_servicio, @estado_orden_id, @observaciones,
				0, @identificacionCliente);

		INSERT INTO ORDEN_ACTIVIDADES (orden_id, actividad_id, estado, duracion_min)
		SELECT @orden_id, actividad_id, 0, NULL FROM @actividades;

		INSERT INTO ORDEN_EMPLEADOS (orden_id, empleado_id, turno, piso_area)
		SELECT @orden_id, empleado_id, @turno, @piso_area FROM @empleados;

		INSERT INTO ORDEN_PRODUCTOS (orden_id, producto_id, cantidad)
		SELECT @orden_id, producto_id, cantidad FROM @productos;

		UPDATE p
		SET p.stock = ISNULL(p.stock, 0) - x.cantidad
		FROM PRODUCTOS p
		INNER JOIN @productos x ON x.producto_id = p.producto_id;

		INSERT INTO ORDEN_EQUIPOS (orden_id, equipo_id, fecha_asignacion, devuelto)
		SELECT @orden_id, equipo_id, @fecha_servicio, 0 FROM @equipos;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Orden registrada correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @orden_id = '';
		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

-- Inventario

CREATE OR ALTER PROCEDURE sp_BuscarInventario
	@texto VARCHAR(50) = NULL,
	@tipo VARCHAR(10) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @tipo = UPPER(LTRIM(RTRIM(ISNULL(@tipo, ''))));

	WITH unidades AS
	(
		SELECT ISNULL(e.nombreEquipo, '') AS nombreEquipo,
			   ISNULL(e.tipoEquipo, '') AS tipoEquipo,
			   ISNULL(e.marcaEquipo, '') AS marcaEquipo,
			   ISNULL(e.modeloEquipo, '') AS modeloEquipo,
			   CASE WHEN EXISTS (SELECT 1 FROM ORDEN_EQUIPOS oe
								 WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0)
					THEN 0 ELSE 1 END AS libre,
			   (SELECT TOP 1 ce.precio_unitario FROM COMPRAS_EQUIPOS ce
				WHERE ce.equipo_id = e.equipo_id) AS costo
		FROM EQUIPOS e
	),
	grupos AS
	(
		SELECT nombreEquipo, tipoEquipo, marcaEquipo, modeloEquipo,
			   COUNT(1) AS total,
			   SUM(libre) AS disponible,
			   SUM(costo) AS valor
		FROM unidades
		GROUP BY nombreEquipo, tipoEquipo, marcaEquipo, modeloEquipo
	)
	SELECT tipo, nombre, categoria, marcaModelo, disponible, total, proveedor, valor
	FROM
	(
		SELECT 'Producto' AS tipo,
			   p.nombreProducto AS nombre,
			   p.tipoProducto AS categoria,
			   N'—' AS marcaModelo,
			   N'—' AS disponible,
			   ISNULL(p.stock, 0) AS total,
			   (SELECT TOP 1 pr.nombreProveedor
				FROM COMPRAS_PRODUCTOS cp
				INNER JOIN COMPRAS c ON c.compra_id = cp.compra_id
				INNER JOIN PROVEEDORES pr ON pr.proveedor_id = c.proveedor_id
				WHERE cp.producto_id = p.producto_id
				ORDER BY c.fechaCompra DESC, c.compra_id DESC) AS proveedor,
			   CAST(ISNULL(p.stock, 0) *
					(SELECT SUM(cp.cantidad * cp.precio_unitario) / NULLIF(SUM(cp.cantidad), 0)
					 FROM COMPRAS_PRODUCTOS cp
					 WHERE cp.producto_id = p.producto_id) AS MONEY) AS valor
		FROM PRODUCTOS p
		WHERE @tipo IN ('', 'PRODUCTO')
		  AND (@texto = '' OR p.nombreProducto LIKE '%' + @texto + '%' OR p.tipoProducto LIKE '%' + @texto + '%')

		UNION ALL

		SELECT 'Equipo' AS tipo,
			   g.nombreEquipo AS nombre,
			   g.tipoEquipo AS categoria,
			   LTRIM(RTRIM(g.marcaEquipo + ' ' + g.modeloEquipo)) AS marcaModelo,
			   CAST(g.disponible AS NVARCHAR(10)) AS disponible,
			   g.total,
			   (SELECT TOP 1 pr.nombreProveedor
				FROM EQUIPOS e
				INNER JOIN COMPRAS_EQUIPOS ce ON ce.equipo_id = e.equipo_id
				INNER JOIN COMPRAS c ON c.compra_id = ce.compra_id
				INNER JOIN PROVEEDORES pr ON pr.proveedor_id = c.proveedor_id
				WHERE ISNULL(e.nombreEquipo, '') = g.nombreEquipo
				  AND ISNULL(e.tipoEquipo, '') = g.tipoEquipo
				  AND ISNULL(e.marcaEquipo, '') = g.marcaEquipo
				  AND ISNULL(e.modeloEquipo, '') = g.modeloEquipo
				ORDER BY c.fechaCompra DESC, c.compra_id DESC) AS proveedor,
			   CAST(g.valor AS MONEY) AS valor
		FROM grupos g
		WHERE @tipo IN ('', 'EQUIPO')
		  AND (@texto = '' OR g.nombreEquipo LIKE '%' + @texto + '%' OR g.tipoEquipo LIKE '%' + @texto + '%'
			   OR g.marcaEquipo LIKE '%' + @texto + '%' OR g.modeloEquipo LIKE '%' + @texto + '%')
	) inventario
	ORDER BY tipo DESC, nombre;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProductosInventario
	@texto VARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT producto_id, nombreProducto, tipoProducto, ISNULL(stock, 0) AS stock
	FROM PRODUCTOS
	WHERE @texto = '' OR nombreProducto LIKE '%' + @texto + '%' OR tipoProducto LIKE '%' + @texto + '%'
	ORDER BY nombreProducto;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEquiposInventario
	@texto VARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT ISNULL(nombreEquipo, '') AS nombreEquipo,
		   ISNULL(tipoEquipo, '') AS tipoEquipo,
		   ISNULL(marcaEquipo, '') AS marcaEquipo,
		   ISNULL(modeloEquipo, '') AS modeloEquipo,
		   COUNT(1) AS total
	FROM EQUIPOS
	WHERE @texto = '' OR nombreEquipo LIKE '%' + @texto + '%' OR tipoEquipo LIKE '%' + @texto + '%'
	   OR marcaEquipo LIKE '%' + @texto + '%' OR modeloEquipo LIKE '%' + @texto + '%'
	GROUP BY ISNULL(nombreEquipo, ''), ISNULL(tipoEquipo, ''), ISNULL(marcaEquipo, ''), ISNULL(modeloEquipo, '')
	ORDER BY nombreEquipo, marcaEquipo, modeloEquipo;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProveedores
AS
BEGIN
	SET NOCOUNT ON;

	SELECT proveedor_id, nombreProveedor, telefonoProveedor, correoProveedor
	FROM PROVEEDORES
	ORDER BY nombreProveedor;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarProveedor
	@nombreProveedor VARCHAR(50),
	@telefonoProveedor VARCHAR(50),
	@correoProveedor VARCHAR(50),
	@proveedor_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @proveedor_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	SET @nombreProveedor = LTRIM(RTRIM(ISNULL(@nombreProveedor, '')));

	IF @nombreProveedor = ''
	BEGIN
		SET @mensaje = 'El nombre del proveedor es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM PROVEEDORES WHERE nombreProveedor = @nombreProveedor)
	BEGIN
		SET @mensaje = 'Ya existe un proveedor con ese nombre.';
		RETURN;
	END

	INSERT INTO PROVEEDORES (nombreProveedor, telefonoProveedor, correoProveedor)
	VALUES (@nombreProveedor, @telefonoProveedor, @correoProveedor);

	SET @proveedor_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Proveedor registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarProducto
	@nombreProducto VARCHAR(50),
	@tipoProducto VARCHAR(20),
	@producto_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @producto_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	SET @nombreProducto = LTRIM(RTRIM(ISNULL(@nombreProducto, '')));
	SET @tipoProducto = LTRIM(RTRIM(ISNULL(@tipoProducto, '')));

	IF @nombreProducto = ''
	BEGIN
		SET @mensaje = 'El nombre del producto es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM PRODUCTOS WHERE nombreProducto = @nombreProducto AND ISNULL(tipoProducto, '') = @tipoProducto)
	BEGIN
		SET @mensaje = 'Ese producto ya está registrado.';
		RETURN;
	END

	INSERT INTO PRODUCTOS (nombreProducto, tipoProducto, stock)
	VALUES (@nombreProducto, @tipoProducto, 0);

	SET @producto_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Producto registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_RegistrarIngresoInventario
	@proveedor_id INT,
	@firma_empleado_id INT,
	@numero_factura VARCHAR(50),
	@fechaCompra DATE,
	@productos dbo.tipo_ingreso_productos READONLY,
	@equipos dbo.tipo_ingreso_equipos READONLY,
	@compra_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @compra_id = NULL;
	SET @resultado = 0;
	SET @mensaje = '';

	SET @numero_factura = LTRIM(RTRIM(ISNULL(@numero_factura, '')));

	IF NOT EXISTS (SELECT 1 FROM PROVEEDORES WHERE proveedor_id = @proveedor_id)
	BEGIN
		SET @mensaje = 'El proveedor no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM EMPLEADOS WHERE empleado_id = @firma_empleado_id)
	BEGIN
		SET @mensaje = 'El empleado que recibe no existe.';
		RETURN;
	END

	IF @numero_factura = ''
	BEGIN
		SET @mensaje = 'El número de factura es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM COMPRAS WHERE numero_factura = @numero_factura)
	BEGIN
		SET @mensaje = 'Ya existe una compra con ese número de factura.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @productos) AND NOT EXISTS (SELECT 1 FROM @equipos)
	BEGIN
		SET @mensaje = 'La lista a ingresar está vacía.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos WHERE ISNULL(cantidad, 0) <= 0 OR ISNULL(precio_unitario, -1) < 0)
	   OR EXISTS (SELECT 1 FROM @equipos WHERE ISNULL(cantidad, 0) <= 0 OR ISNULL(precio_unitario, -1) < 0)
	BEGIN
		SET @mensaje = 'Hay cantidades o precios no válidos en la lista.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @productos x WHERE NOT EXISTS (SELECT 1 FROM PRODUCTOS p WHERE p.producto_id = x.producto_id))
	BEGIN
		SET @mensaje = 'Uno de los productos de la lista no existe.';
		RETURN;
	END

	DECLARE @estadoDisponible INT = (SELECT TOP 1 estado_equipo_id FROM ESTADO_EQUIPOS
									 WHERE nombreEstadoEquipo = 'Disponible');

	DECLARE @lista TABLE
	(
		fila INT IDENTITY(1,1),
		nombreEquipo VARCHAR(50),
		tipoEquipo VARCHAR(50),
		marcaEquipo VARCHAR(50),
		modeloEquipo VARCHAR(50),
		cantidad INT,
		precio_unitario MONEY
	);

	INSERT INTO @lista (nombreEquipo, tipoEquipo, marcaEquipo, modeloEquipo, cantidad, precio_unitario)
	SELECT nombreEquipo, tipoEquipo, marcaEquipo, modeloEquipo, cantidad, precio_unitario
	FROM @equipos;

	BEGIN TRY
		BEGIN TRANSACTION;

		INSERT INTO COMPRAS (proveedor_id, firma_empleado_id, numero_factura, fechaCompra)
		VALUES (@proveedor_id, @firma_empleado_id, @numero_factura, @fechaCompra);

		SET @compra_id = CAST(SCOPE_IDENTITY() AS INT);

		INSERT INTO COMPRAS_PRODUCTOS (compra_id, producto_id, cantidad, precio_unitario)
		SELECT @compra_id, producto_id, cantidad, precio_unitario
		FROM @productos;

		UPDATE p
		SET p.stock = ISNULL(p.stock, 0) + x.cantidad
		FROM PRODUCTOS p
		INNER JOIN (SELECT producto_id, SUM(cantidad) AS cantidad
					FROM @productos
					GROUP BY producto_id) x ON x.producto_id = p.producto_id;

		DECLARE @fila INT = 1;
		DECLARE @totalFilas INT = (SELECT COUNT(1) FROM @lista);
		DECLARE @unidad INT;
		DECLARE @cantidad INT;
		DECLARE @precio MONEY;
		DECLARE @equipo_id INT;

		WHILE @fila <= @totalFilas
		BEGIN
			SELECT @cantidad = cantidad, @precio = precio_unitario FROM @lista WHERE fila = @fila;
			SET @unidad = 1;

			WHILE @unidad <= @cantidad
			BEGIN
				INSERT INTO EQUIPOS (nombreEquipo, tipoEquipo, marcaEquipo, modeloEquipo, estado_equipo_id)
				SELECT nombreEquipo, tipoEquipo, marcaEquipo, modeloEquipo, @estadoDisponible
				FROM @lista WHERE fila = @fila;

				SET @equipo_id = CAST(SCOPE_IDENTITY() AS INT);

				INSERT INTO COMPRAS_EQUIPOS (compra_id, equipo_id, precio_unitario)
				VALUES (@compra_id, @equipo_id, @precio);

				SET @unidad = @unidad + 1;
			END

			SET @fila = @fila + 1;
		END

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Ingreso registrado correctamente (factura ' + @numero_factura + ').';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @compra_id = NULL;
		SET @resultado = 0;
		SET @mensaje = LEFT('No se pudo registrar el ingreso: ' + ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEmpleadosInventario
AS
BEGIN
	SET NOCOUNT ON;

	SELECT empleado_id,
		   LTRIM(RTRIM(ISNULL(nombreEmpleado, '') + ' ' + ISNULL(apellidoEmpleado, ''))) AS nombreCompleto
	FROM EMPLEADOS
	WHERE estadoEmpleado = 1
	ORDER BY nombreEmpleado, apellidoEmpleado;
END
GO

SET NOEXEC OFF;
GO
