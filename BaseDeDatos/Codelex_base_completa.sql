CREATE DATABASE Codelex
GO
USE Codelex
Go
--Empleados

CREATE TABLE CARGOS
(
	cargo_id INT PRIMARY KEY IDENTITY,
	nombreCargo VARCHAR(50) UNIQUE
)
GO

CREATE TABLE EMPLEADOS
(
	empleado_id INT PRIMARY KEY IDENTITY,
	cargo_id INT FOREIGN KEY REFERENCES CARGOS(cargo_id),
	nombreEmpleado VARCHAR(30),
	apellidoEmpleado VARCHAR(30),
	identidad VARCHAR(30) UNIQUE NOT NULL,
	telefonoEmpleado Varchar(50),
	correoEmpleado VARCHAR(50),
	huella_id VARCHAR(50) UNIQUE NOT NULL
)
GO

CREATE TABLE USUARIOS
(
	usuario_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id) NOT NULL UNIQUE,
	usuario VARCHAR(50) UNIQUE NOT NULL,
	contrasena_hash VARCHAR(255)
)
GO
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
CREATE TABLE ASISTENCIAS
(
	asistencia_id INT PRIMARY KEY IDENTITY,
	empleado_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	fechaAsistencia DATE,
	hora_entrada TIME,
	hora_salida TIME
)
GO

--clientes


CREATE TABLE LOCACIONES
(
	locacion_id INT PRIMARY KEY IDENTITY,
	nombreLocacion VARCHAR(60),
	direccion VARCHAR(80),
	tipoLocacion VARCHAR(25)
)
GO
CREATE TABLE CLIENTES
(
	cliente_id INT IDENTITY PRIMARY KEY,
	locacion_id INT FOREIGN KEY REFERENCES LOCACIONES(locacion_id),
	nombreEmpresa VARCHAR(50),
	nombreCliente VARCHAR(50),
	apellidoCliente VARCHAR(50),
	telefonoCliente VARCHAR(50),
	identificacionCliente VARCHAR(30) UNIQUE,
	correoCliente VARCHAR(50)
)
GO
CREATE TABLE SERVICIOS
(
	servicio_id INT PRIMARY KEY IDENTITY,
	nombreServicio VARCHAR(30)
)
GO 
CREATE TABLE VEHICULOS
(
	vehiculo_id INT PRIMARY KEY IDENTITY,
	placa VARCHAR(15) UNIQUE,
	marcaVehiculo varchar(20),
	modeloVehiculo Varchar(25)
)
GO
CREATE TABLE ESTADO_ORDENES
(
	estado_orden_id INT PRIMARY KEY IDENTITY,
	nombreEstadoOrden VARCHAR(25) UNIQUE
)
GO
CREATE TABLE ORDENES
(
	orden_id VARCHAR(50) PRIMARY KEY NOT NULL,
	cliente_id INT FOREIGN KEY REFERENCES CLIENTES(cliente_id),
	locacion_id INT FOREIGN KEY REFERENCES LOCACIONES(locacion_id),
	servicio_id INT FOREIGN KEY REFERENCES SERVICIOS(servicio_id),
	supervisor_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	vehiculo_id INT FOREIGN KEY REFERENCES VEHICULOS(vehiculo_id),
	fecha_solicitud DATE,
	fecha_servicio DATE,
	estado_orden_id INT FOREIGN KEY REFERENCES ESTADO_ORDENES(estado_orden_id),
	observaciones VARCHAR(50),
	conformidad_cliente BIT
)
GO
CREATE TABLE ACTIVIDADES	
(
	actividad_id INT PRIMARY KEY IDENTITY,
	servicio_id INT FOREIGN KEY REFERENCES SERVICIOS(servicio_id),
	descripcion VARCHAR(50),
	frecuencia VARCHAR(50),
	precio MONEY
)
GO
CREATE TABLE PROVEEDORES
(
	proveedor_id INT PRIMARY KEY IDENTITY,
	nombreProveedor VARCHAR(50),
	telefonoProveedor VARCHAR(50),
	correoProveedor VARCHAR(50)
)
GO
CREATE TABLE PRODUCTOS
(
	producto_id INT PRIMARY KEY IDENTITY,
	nombreProducto VARCHAR(50),
	tipoProducto VARCHAR(20),
	stock INT
)
GO
CREATE TABLE COMPRAS
(
	compra_id INT PRIMARY KEY IDENTITY, 
	proveedor_id INT FOREIGN KEY REFERENCES PROVEEDORES(proveedor_id),
	firma_empleado_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	numero_factura VARCHAR(50) UNIQUE,
	fechaCompra DATE
)
GO
CREATE TABLE COMPRAS_PRODUCTOS
(
	compra_id INT PRIMARY KEY FOREIGN KEY REFERENCES COMPRAS(compra_id),
	producto_id INT FOREIGN KEY REFERENCES PRODUCTOS(producto_id),
	cantidad INT,
	precio_unitario MONEY
)
GO
CREATE TABLE ESTADO_EQUIPOS
(
	estado_equipo_id INT PRIMARY KEY IDENTITY,
	nombreEstadoEquipo VARCHAR(30) UNIQUE
)
GO
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
CREATE TABLE ORDEN_EMPLEADOS
(
	orden_id VARCHAR(50) PRIMARY KEY FOREIGN KEY REFERENCES ORDENES(orden_id),
	empleado_id INT FOREIGN KEY REFERENCES EMPLEADOS(empleado_id),
	turno VARCHAR(20),
	piso_area VARCHAR(20)
)
GO
CREATE TABLE ORDEN_PRODUCTOS
(
	orden_id VARCHAR(50) PRIMARY KEY FOREIGN KEY REFERENCES ORDENES(orden_id),
	producto_id INT FOREIGN KEY REFERENCES PRODUCTOS(producto_id),
	cantidad INT
)
GO
CREATE TABLE ORDEN_EQUIPOS
(
	orden_id varchar(50) PRIMARY KEY FOREIGN KEY REFERENCES ORDENES(orden_id),
	equipo_id INT FOREIGN KEY REFERENCES EQUIPOS(equipo_id),
	fecha_asignacion DATE,
	devuelto BIT
)
GO
CREATE TABLE ORDEN_ACTIVIDADES
(
	orden_id varchar(50) PRIMARY KEY FOREIGN KEY REFERENCES ORDENES(orden_id),
	actividad_id INT FOREIGN KEY REFERENCES ACTIVIDADES(actividad_id),
	estado BIT,
	duracion_min INT
)
GO
CREATE TABLE FORMAS_PAGOS
(
	forma_pago_id INT PRIMARY KEY IDENTITY,
	nombreFormaPago VARCHAR(30) UNIQUE
)
GO
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
CREATE TABLE COMPRAS_EQUIPOS
(
	compra_id INT PRIMARY KEY FOREIGN KEY REFERENCES COMPRAS(compra_id),
	equipo_id INT FOREIGN KEY REFERENCES EQUIPOS(equipo_id),
	precio_unitario MONEY
)
GO
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

GO

USE Codelex
GO

CREATE PROCEDURE sp_ObtenerUsuarioLogin
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

CREATE PROCEDURE sp_BuscarUsuarioRecuperacion
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

CREATE PROCEDURE sp_CambiarContrasena
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

CREATE PROCEDURE sp_InsertarLocacion
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

	IF @nombreLocacion IS NULL OR LTRIM(RTRIM(@nombreLocacion)) = ''
	BEGIN
		SET @mensaje = 'El nombre de la locacion es obligatorio.';
		RETURN;
	END

	INSERT INTO LOCACIONES (nombreLocacion, direccion, tipoLocacion)
	VALUES (@nombreLocacion, @direccion, @tipoLocacion);

	SET @locacion_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Locacion registrada correctamente.';
END
GO

CREATE PROCEDURE sp_ActualizarLocacion
	@locacion_id INT,
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

	UPDATE LOCACIONES
	SET nombreLocacion = @nombreLocacion,
		direccion = @direccion,
		tipoLocacion = @tipoLocacion
	WHERE locacion_id = @locacion_id;

	SET @resultado = 1;
	SET @mensaje = 'Locacion actualizada correctamente.';
END
GO

CREATE PROCEDURE sp_ListarLocaciones
AS
BEGIN
	SET NOCOUNT ON;

	SELECT locacion_id, nombreLocacion, direccion, tipoLocacion
	FROM LOCACIONES
	ORDER BY nombreLocacion;
END
GO

CREATE PROCEDURE sp_ListarLocacionesPorCliente
	@cliente_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT l.locacion_id, l.nombreLocacion, l.direccion, l.tipoLocacion
	FROM CLIENTES c
	INNER JOIN LOCACIONES l ON l.locacion_id = c.locacion_id
	WHERE c.cliente_id = @cliente_id;
END
GO

CREATE PROCEDURE sp_InsertarCliente
	@nombreEmpresa VARCHAR(50) = NULL,
	@nombreCliente VARCHAR(50),
	@apellidoCliente VARCHAR(50),
	@telefonoCliente VARCHAR(50) = NULL,
	@identificacionCliente VARCHAR(30),
	@correoCliente VARCHAR(50) = NULL,
	@locacion_id INT = NULL,
	@cliente_id INT OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @cliente_id = NULL;
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

	IF @locacion_id IS NOT NULL
	   AND NOT EXISTS (SELECT 1 FROM LOCACIONES WHERE locacion_id = @locacion_id)
	BEGIN
		SET @mensaje = 'La locacion seleccionada no existe.';
		RETURN;
	END

	INSERT INTO CLIENTES (locacion_id, nombreEmpresa, nombreCliente, apellidoCliente,
						  telefonoCliente, identificacionCliente, correoCliente)
	VALUES (@locacion_id, @nombreEmpresa, @nombreCliente, @apellidoCliente,
			@telefonoCliente, @identificacionCliente, @correoCliente);

	SET @cliente_id = CAST(SCOPE_IDENTITY() AS INT);
	SET @resultado = 1;
	SET @mensaje = 'Cliente registrado correctamente.';
END
GO

CREATE PROCEDURE sp_ActualizarCliente
	@cliente_id INT,
	@nombreEmpresa VARCHAR(50) = NULL,
	@nombreCliente VARCHAR(50),
	@apellidoCliente VARCHAR(50),
	@telefonoCliente VARCHAR(50) = NULL,
	@identificacionCliente VARCHAR(30),
	@correoCliente VARCHAR(50) = NULL,
	@locacion_id INT = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @identificacionCliente = LTRIM(RTRIM(ISNULL(@identificacionCliente, '')));

	IF NOT EXISTS (SELECT 1 FROM CLIENTES WHERE cliente_id = @cliente_id)
	BEGIN
		SET @mensaje = 'El cliente no existe.';
		RETURN;
	END

	IF @identificacionCliente = ''
	BEGIN
		SET @mensaje = 'La identificacion (RTN / identidad) es obligatoria.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM CLIENTES
			   WHERE identificacionCliente = @identificacionCliente
				 AND cliente_id <> @cliente_id)
	BEGIN
		SET @mensaje = 'Ya existe otro cliente con esa identificacion.';
		RETURN;
	END

	IF @locacion_id IS NOT NULL
	   AND NOT EXISTS (SELECT 1 FROM LOCACIONES WHERE locacion_id = @locacion_id)
	BEGIN
		SET @mensaje = 'La locacion seleccionada no existe.';
		RETURN;
	END

	UPDATE CLIENTES
	SET locacion_id = @locacion_id,
		nombreEmpresa = @nombreEmpresa,
		nombreCliente = @nombreCliente,
		apellidoCliente = @apellidoCliente,
		telefonoCliente = @telefonoCliente,
		identificacionCliente = @identificacionCliente,
		correoCliente = @correoCliente
	WHERE cliente_id = @cliente_id;

	SET @resultado = 1;
	SET @mensaje = 'Cliente actualizado correctamente.';
END
GO

CREATE PROCEDURE sp_BuscarClientes
	@texto VARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT c.cliente_id, c.nombreEmpresa, c.nombreCliente, c.apellidoCliente,
		   c.telefonoCliente, c.identificacionCliente, c.correoCliente,
		   c.locacion_id, l.nombreLocacion, l.direccion, l.tipoLocacion
	FROM CLIENTES c
	LEFT JOIN LOCACIONES l ON l.locacion_id = c.locacion_id
	WHERE @texto = ''
	   OR c.identificacionCliente LIKE '%' + @texto + '%'
	   OR c.nombreEmpresa LIKE '%' + @texto + '%'
	   OR c.nombreCliente LIKE '%' + @texto + '%'
	   OR c.apellidoCliente LIKE '%' + @texto + '%'
	ORDER BY c.nombreEmpresa, c.nombreCliente, c.apellidoCliente;
END
GO

CREATE PROCEDURE sp_ObtenerCliente
	@cliente_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.cliente_id, c.nombreEmpresa, c.nombreCliente, c.apellidoCliente,
		   c.telefonoCliente, c.identificacionCliente, c.correoCliente,
		   c.locacion_id, l.nombreLocacion, l.direccion, l.tipoLocacion
	FROM CLIENTES c
	LEFT JOIN LOCACIONES l ON l.locacion_id = c.locacion_id
	WHERE c.cliente_id = @cliente_id;
END
GO

GO

USE Codelex
GO

IF COL_LENGTH('CLIENTES', 'estadoCliente') IS NULL
	ALTER TABLE CLIENTES
	ADD estadoCliente BIT NOT NULL
		CONSTRAINT DF_CLIENTES_estadoCliente DEFAULT 1;
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

		EXEC('ALTER TABLE LOCACIONES ADD identificacionCliente VARCHAR(30) NULL');
		EXEC('ALTER TABLE ORDENES ADD identificacionCliente VARCHAR(30) NULL');

		EXEC('UPDATE l SET l.identificacionCliente = c.identificacionCliente
			  FROM LOCACIONES l
			  INNER JOIN CLIENTES c ON c.locacion_id = l.locacion_id');

		EXEC('UPDATE o SET o.identificacionCliente = c.identificacionCliente
			  FROM ORDENES o
			  INNER JOIN CLIENTES c ON c.cliente_id = o.cliente_id');

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

		EXEC('ALTER TABLE ORDENES DROP COLUMN cliente_id');
		EXEC('ALTER TABLE CLIENTES DROP COLUMN locacion_id');
		EXEC('ALTER TABLE CLIENTES DROP COLUMN cliente_id');

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

SET NOEXEC OFF;
GO
