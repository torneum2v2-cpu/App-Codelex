USE Codelex
GO

-- Estados de orden (solo agrega los que falten)
IF NOT EXISTS (SELECT 1 FROM ESTADO_ORDENES WHERE nombreEstadoOrden = 'Pendiente')
	INSERT INTO ESTADO_ORDENES (nombreEstadoOrden) VALUES ('Pendiente');
IF NOT EXISTS (SELECT 1 FROM ESTADO_ORDENES WHERE nombreEstadoOrden = 'En proceso')
	INSERT INTO ESTADO_ORDENES (nombreEstadoOrden) VALUES ('En proceso');
IF NOT EXISTS (SELECT 1 FROM ESTADO_ORDENES WHERE nombreEstadoOrden = 'Completada')
	INSERT INTO ESTADO_ORDENES (nombreEstadoOrden) VALUES ('Completada');
GO

-- Observaciones mas largas (la descripcion de la limpieza)
IF EXISTS (SELECT 1 FROM sys.columns
		   WHERE object_id = OBJECT_ID('ORDENES') AND name = 'observaciones' AND max_length < 200)
	ALTER TABLE ORDENES ALTER COLUMN observaciones VARCHAR(200) NULL;
GO

-- Llaves compuestas en las tablas de detalle de la orden
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

-- Tipos de lista para guardar varias filas en un solo procedimiento
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

	SELECT 'E', e.equipo_id, ISNULL(e.nombreEquipo, ''), 0, ISNULL(ee.nombreEstadoEquipo, '')
	FROM EQUIPOS e
	LEFT JOIN ESTADO_EQUIPOS ee ON ee.estado_equipo_id = e.estado_equipo_id

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