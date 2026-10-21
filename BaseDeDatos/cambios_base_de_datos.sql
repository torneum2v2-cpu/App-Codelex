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
