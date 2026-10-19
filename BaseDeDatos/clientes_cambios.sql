USE Codelex
GO

/* =====================================================================
   Cambios para las pantallas Clientes y Detalle del cliente.
   Ejecutar UNA vez, después de procedimientos_avance_semana_1.sql.
   ===================================================================== */

-- 1. Estado del cliente (1 = Activo, 0 = Inactivo).
--    Los clientes que ya existen quedan como Activos.
IF COL_LENGTH('CLIENTES', 'estadoCliente') IS NULL
	ALTER TABLE CLIENTES
	ADD estadoCliente BIT NOT NULL
		CONSTRAINT DF_CLIENTES_estadoCliente DEFAULT 1;
GO

-- 2. Buscar clientes (se modifica para buscar por RTN o por nombre y devolver el estado).
--    @buscarPor: 'RTN', 'NOMBRE' o NULL (busca en todos los campos).
CREATE OR ALTER PROCEDURE sp_BuscarClientes
	@texto VARCHAR(50) = NULL,
	@buscarPor VARCHAR(10) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @buscarPor = UPPER(LTRIM(RTRIM(ISNULL(@buscarPor, ''))));

	SELECT c.cliente_id, c.nombreEmpresa, c.nombreCliente, c.apellidoCliente,
		   c.telefonoCliente, c.identificacionCliente, c.correoCliente,
		   c.locacion_id, l.nombreLocacion, l.direccion, l.tipoLocacion,
		   c.estadoCliente,
		   CASE WHEN c.estadoCliente = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM CLIENTES c
	LEFT JOIN LOCACIONES l ON l.locacion_id = c.locacion_id
	WHERE @texto = ''
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
				OR c.apellidoCliente LIKE '%' + @texto + '%'))
	ORDER BY c.nombreEmpresa, c.nombreCliente, c.apellidoCliente;
END
GO

-- 3. Obtener un cliente (se modifica para devolver el estado).
CREATE OR ALTER PROCEDURE sp_ObtenerCliente
	@cliente_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT c.cliente_id, c.nombreEmpresa, c.nombreCliente, c.apellidoCliente,
		   c.telefonoCliente, c.identificacionCliente, c.correoCliente,
		   c.locacion_id, l.nombreLocacion, l.direccion, l.tipoLocacion,
		   c.estadoCliente,
		   CASE WHEN c.estadoCliente = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM CLIENTES c
	LEFT JOIN LOCACIONES l ON l.locacion_id = c.locacion_id
	WHERE c.cliente_id = @cliente_id;
END
GO

-- 4. Activar / desactivar un cliente (reemplaza a eliminar: no se borra nada).
CREATE OR ALTER PROCEDURE sp_CambiarEstadoCliente
	@cliente_id INT,
	@estadoCliente BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM CLIENTES WHERE cliente_id = @cliente_id)
	BEGIN
		SET @mensaje = 'El cliente no existe.';
		RETURN;
	END

	UPDATE CLIENTES
	SET estadoCliente = @estadoCliente
	WHERE cliente_id = @cliente_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estadoCliente = 1
						THEN 'Cliente activado correctamente.'
						ELSE 'Cliente desactivado correctamente.' END;
END
GO

-- 5. Órdenes de un cliente (para el detalle).
CREATE OR ALTER PROCEDURE sp_ListarOrdenesPorCliente
	@cliente_id INT
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
	WHERE o.cliente_id = @cliente_id
	ORDER BY fecha DESC, o.orden_id DESC;
END
GO

-- 6. Facturas de un cliente (para el detalle).
CREATE OR ALTER PROCEDURE sp_ListarFacturasPorCliente
	@cliente_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT f.factura_id,
		   f.fechaFactura AS fecha,
		   f.estado_pago,
		   f.precio_facturado AS monto
	FROM FACTURAS f
	INNER JOIN ORDENES o ON o.orden_id = f.orden_id
	WHERE o.cliente_id = @cliente_id
	ORDER BY f.fechaFactura DESC, f.factura_id DESC;
END
GO
