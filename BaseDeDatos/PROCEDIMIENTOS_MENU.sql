USE Codelex
GO

-- Ahora también devuelve el rol del usuario
CREATE OR ALTER PROCEDURE sp_ObtenerUsuarioLogin
	@usuario VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT u.usuario_id,
		   u.empleado_id,
		   u.usuario,
		   u.contrasena_hash,
		   u.passkey_id,
		   u.passkey_clave,
		   e.nombreEmpleado,
		   e.apellidoEmpleado,
		   c.nombreCargo,
		   ISNULL(r.nombreRol, '') AS rol
	FROM USUARIOS u
	INNER JOIN EMPLEADOS e ON e.empleado_id = u.empleado_id
	LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
	LEFT JOIN ROLES r ON r.rol_id = u.rol_id
	WHERE u.usuario = @usuario;
END
GO

-- Órdenes en las que el empleado está asignado
CREATE OR ALTER PROCEDURE sp_MisOrdenes
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   COALESCE(NULLIF(c.nombreEmpresa, ''),
					NULLIF(LTRIM(ISNULL(c.nombreCliente, '') + ' ' + ISNULL(c.apellidoCliente, '')), ''),
					o.identificacionCliente) AS cliente,
		   ISNULL(s.nombreServicio, 'Sin servicio') AS nombreServicio,
		   COALESCE(o.fecha_servicio, o.fecha_solicitud) AS fecha,
		   ISNULL(eo.nombreEstadoOrden, 'Sin estado') AS estado
	FROM ORDEN_EMPLEADOS oe
	INNER JOIN ORDENES o ON o.orden_id = oe.orden_id
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE oe.empleado_id = @empleado_id
	ORDER BY COALESCE(o.fecha_servicio, o.fecha_solicitud) DESC, o.orden_id;
END
GO

-- Cabecera de una orden (solo si el empleado está asignado)
CREATE OR ALTER PROCEDURE sp_DetalleMiOrden
	@orden_id VARCHAR(50),
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   COALESCE(NULLIF(c.nombreEmpresa, ''),
					NULLIF(LTRIM(ISNULL(c.nombreCliente, '') + ' ' + ISNULL(c.apellidoCliente, '')), ''),
					o.identificacionCliente) AS cliente,
		   ISNULL(s.nombreServicio, '') AS servicio,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   ISNULL(eo.nombreEstadoOrden, 'Sin estado') AS estado,
		   ISNULL(l.nombreLocacion, '') AS locacion,
		   ISNULL(l.direccion, '') AS direccion,
		   CASE WHEN v.vehiculo_id IS NULL THEN ''
				ELSE ISNULL(v.placa, '') + ' - ' + ISNULL(v.marcaVehiculo, '') + ' ' + ISNULL(v.modeloVehiculo, '')
		   END AS vehiculo,
		   LTRIM(ISNULL(sup.nombreEmpleado, '') + ' ' + ISNULL(sup.apellidoEmpleado, '')) AS supervisor,
		   ISNULL(oe.turno, '') AS turno,
		   ISNULL(oe.piso_area, '') AS piso_area,
		   ISNULL(o.observaciones, '') AS observaciones
	FROM ORDEN_EMPLEADOS oe
	INNER JOIN ORDENES o ON o.orden_id = oe.orden_id
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	LEFT JOIN LOCACIONES l ON l.locacion_id = o.locacion_id
	LEFT JOIN VEHICULOS v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN EMPLEADOS sup ON sup.empleado_id = o.supervisor_id
	WHERE oe.orden_id = @orden_id
	  AND oe.empleado_id = @empleado_id;
END
GO

-- Equipo de trabajo, actividades, equipos y productos de la orden
CREATE OR ALTER PROCEDURE sp_ListasMiOrden
	@orden_id VARCHAR(50),
	@empleado_id INT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM ORDEN_EMPLEADOS WHERE orden_id = @orden_id AND empleado_id = @empleado_id)
		RETURN;

	SELECT 1 AS orden,
		   'Equipo de trabajo' AS seccion,
		   LTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '')) AS titulo,
		   ISNULL(cg.nombreCargo, '')
			 + CASE WHEN ISNULL(oe.turno, '') = '' THEN '' ELSE ' - Turno: ' + oe.turno END
			 + CASE WHEN ISNULL(oe.piso_area, '') = '' THEN '' ELSE ' - ' + oe.piso_area END AS detalle
	FROM ORDEN_EMPLEADOS oe
	INNER JOIN EMPLEADOS e ON e.empleado_id = oe.empleado_id
	LEFT JOIN CARGOS cg ON cg.cargo_id = e.cargo_id
	WHERE oe.orden_id = @orden_id
	  AND oe.empleado_id <> @empleado_id

	UNION ALL

	SELECT 2,
		   'Actividades',
		   ISNULL(a.descripcion, ''),
		   CASE WHEN ISNULL(a.frecuencia, '') = '' THEN '' ELSE a.frecuencia + ' - ' END
			 + CASE WHEN oa.estado = 1 THEN 'Realizada' ELSE 'Pendiente' END
	FROM ORDEN_ACTIVIDADES oa
	INNER JOIN ACTIVIDADES a ON a.actividad_id = oa.actividad_id
	WHERE oa.orden_id = @orden_id

	UNION ALL

	SELECT 3,
		   'Equipos asignados',
		   ISNULL(q.nombreEquipo, ''),
		   LTRIM(ISNULL(q.marcaEquipo, '') + ' ' + ISNULL(q.modeloEquipo, ''))
			 + CASE WHEN oq.devuelto = 1 THEN ' - Devuelto' ELSE ' - En uso' END
	FROM ORDEN_EQUIPOS oq
	INNER JOIN EQUIPOS q ON q.equipo_id = oq.equipo_id
	WHERE oq.orden_id = @orden_id

	UNION ALL

	SELECT 4,
		   'Productos',
		   ISNULL(p.nombreProducto, ''),
		   'Cantidad: ' + CAST(ISNULL(op.cantidad, 0) AS VARCHAR(10))
	FROM ORDEN_PRODUCTOS op
	INNER JOIN PRODUCTOS p ON p.producto_id = op.producto_id
	WHERE op.orden_id = @orden_id

	ORDER BY orden, titulo;
END
GO