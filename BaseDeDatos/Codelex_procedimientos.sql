USE Codelex
GO

IF DB_NAME() <> 'Codelex'
BEGIN
	RAISERROR('No se pudo usar la base Codelex.', 16, 1);
	SET NOEXEC ON;
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

CREATE OR ALTER PROCEDURE sp_GuardarPasskey
	@usuario_id INT,
	@passkey_id VARBINARY(1023),
	@passkey_clave VARBINARY(1024),
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

	IF EXISTS (SELECT 1 FROM USUARIOS WHERE usuario_id = @usuario_id AND passkey_id IS NOT NULL)
	BEGIN
		SET @mensaje = 'Este usuario ya tiene un teléfono registrado.';
		RETURN;
	END

	UPDATE USUARIOS
	SET passkey_id = @passkey_id,
		passkey_clave = @passkey_clave
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = 'Teléfono registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarPasskey
	@usuario VARCHAR(50),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM USUARIOS WHERE usuario = @usuario)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	UPDATE USUARIOS
	SET passkey_id = NULL,
		passkey_clave = NULL
	WHERE usuario = @usuario;

	SET @resultado = 1;
	SET @mensaje = 'Teléfono eliminado. Al entrar deberá registrar uno nuevo.';
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

-- Usuarios

CREATE OR ALTER PROCEDURE sp_ListarRoles
AS
BEGIN
	SET NOCOUNT ON;

	SELECT rol_id, nombreRol
	FROM ROLES
	ORDER BY rol_id;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarUsuarios
	@texto VARCHAR(50) = NULL,
	@buscarPor VARCHAR(10) = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @buscarPor = UPPER(LTRIM(RTRIM(ISNULL(@buscarPor, ''))));

	SELECT u.usuario_id, u.empleado_id, u.usuario, u.rol_id, u.estadoUsuario,
		   e.nombreEmpleado, e.apellidoEmpleado, e.correoEmpleado, e.telefonoEmpleado,
		   r.nombreRol AS rol,
		   CASE WHEN u.estadoUsuario = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM USUARIOS u
	INNER JOIN EMPLEADOS e ON e.empleado_id = u.empleado_id
	LEFT JOIN ROLES r ON r.rol_id = u.rol_id
	WHERE (@estado IS NULL OR u.estadoUsuario = @estado)
	  AND (@texto = ''
		   OR (@buscarPor = 'NOMBRE'
			   AND (e.nombreEmpleado LIKE '%' + @texto + '%'
					OR e.apellidoEmpleado LIKE '%' + @texto + '%'
					OR ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '') LIKE '%' + @texto + '%'))
		   OR (@buscarPor = 'USUARIO'
			   AND u.usuario LIKE '%' + @texto + '%')
		   OR (@buscarPor = 'CORREO'
			   AND e.correoEmpleado LIKE '%' + @texto + '%')
		   OR (@buscarPor = 'ROL'
			   AND r.nombreRol LIKE '%' + @texto + '%')
		   OR (@buscarPor NOT IN ('NOMBRE', 'USUARIO', 'CORREO', 'ROL')
			   AND (u.usuario LIKE '%' + @texto + '%'
					OR e.nombreEmpleado LIKE '%' + @texto + '%'
					OR e.apellidoEmpleado LIKE '%' + @texto + '%'
					OR e.correoEmpleado LIKE '%' + @texto + '%'
					OR r.nombreRol LIKE '%' + @texto + '%')))
	ORDER BY e.nombreEmpleado, e.apellidoEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarEmpleadosSinUsuario
	@texto VARCHAR(50) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT e.empleado_id, e.nombreEmpleado, e.apellidoEmpleado, e.identidad,
		   e.telefonoEmpleado, e.correoEmpleado, c.nombreCargo
	FROM EMPLEADOS e
	LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
	WHERE NOT EXISTS (SELECT 1 FROM USUARIOS u WHERE u.empleado_id = e.empleado_id)
	  AND (@texto = ''
		   OR e.identidad LIKE '%' + @texto + '%'
		   OR e.nombreEmpleado LIKE '%' + @texto + '%'
		   OR e.apellidoEmpleado LIKE '%' + @texto + '%'
		   OR ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '') LIKE '%' + @texto + '%')
	ORDER BY e.nombreEmpleado, e.apellidoEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ExisteUsuario
	@usuario VARCHAR(50),
	@excluirUsuarioId INT = 0
AS
BEGIN
	SET NOCOUNT ON;

	SELECT COUNT(1) AS cantidad
	FROM USUARIOS
	WHERE usuario = LTRIM(RTRIM(@usuario))
	  AND usuario_id <> @excluirUsuarioId;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarUsuario
	@empleado_id INT,
	@usuario VARCHAR(50),
	@contrasena_hash VARCHAR(255),
	@rol_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @usuario = LTRIM(RTRIM(ISNULL(@usuario, '')));

	IF @usuario = ''
	BEGIN
		SET @mensaje = 'El usuario es obligatorio.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM EMPLEADOS WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'El empleado no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM USUARIOS WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'Ese empleado ya tiene un usuario.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM USUARIOS WHERE usuario = @usuario)
	BEGIN
		SET @mensaje = 'Ya existe un usuario con ese nombre.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM ROLES WHERE rol_id = @rol_id)
	BEGIN
		SET @mensaje = 'El rol seleccionado no existe.';
		RETURN;
	END

	INSERT INTO USUARIOS (empleado_id, usuario, contrasena_hash, rol_id)
	VALUES (@empleado_id, @usuario, @contrasena_hash, @rol_id);

	SET @resultado = 1;
	SET @mensaje = 'Usuario creado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarUsuario
	@usuario_id INT,
	@usuario VARCHAR(50),
	@contrasena_hash VARCHAR(255) = NULL,
	@rol_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @usuario = LTRIM(RTRIM(ISNULL(@usuario, '')));

	IF NOT EXISTS (SELECT 1 FROM USUARIOS WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'El usuario no existe.';
		RETURN;
	END

	IF @usuario = ''
	BEGIN
		SET @mensaje = 'El usuario es obligatorio.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM USUARIOS
			   WHERE usuario = @usuario
				 AND usuario_id <> @usuario_id)
	BEGIN
		SET @mensaje = 'Ya existe otro usuario con ese nombre.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM ROLES WHERE rol_id = @rol_id)
	BEGIN
		SET @mensaje = 'El rol seleccionado no existe.';
		RETURN;
	END

	-- Si la contraseña viene vacia, se deja la que ya tenia
	UPDATE USUARIOS
	SET usuario = @usuario,
		rol_id = @rol_id,
		contrasena_hash = CASE WHEN ISNULL(@contrasena_hash, '') = ''
							   THEN contrasena_hash
							   ELSE @contrasena_hash END
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN ISNULL(@contrasena_hash, '') = ''
						THEN 'Usuario actualizado correctamente.'
						ELSE 'Usuario y contraseña actualizados correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoUsuario
	@usuario_id INT,
	@estadoUsuario BIT,
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

	UPDATE USUARIOS
	SET estadoUsuario = @estadoUsuario
	WHERE usuario_id = @usuario_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estadoUsuario = 1
						THEN 'Usuario activado correctamente.'
						ELSE 'Usuario desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_EliminarUsuario
	@usuario_id INT,
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

	IF EXISTS (SELECT 1 FROM BITACORAS WHERE usuario_id = @usuario_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: tiene registros en la bitácora. Desactívelo en su lugar.';
		RETURN;
	END

	BEGIN TRY
		DELETE FROM USUARIOS
		WHERE usuario_id = @usuario_id;

		SET @resultado = 1;
		SET @mensaje = 'Usuario eliminado correctamente.';
	END TRY
	BEGIN CATCH
		SET @resultado = 0;
		SET @mensaje = 'No se puede eliminar: tiene registros asociados. Desactívelo en su lugar.';
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerFotoUsuario
	@usuario_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT foto_perfil
	FROM USUARIOS
	WHERE usuario_id = @usuario_id;
END
GO

CREATE OR ALTER PROCEDURE sp_GuardarFotoUsuario
	@usuario_id INT,
	@foto VARBINARY(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE USUARIOS
	SET foto_perfil = @foto
	WHERE usuario_id = @usuario_id;
END
GO

-- Clientes

CREATE OR ALTER FUNCTION dbo.fn_ValidarIdentificacion (@tipo VARCHAR(10), @numero VARCHAR(30))
RETURNS VARCHAR(100)
AS
BEGIN
	DECLARE @n VARCHAR(30) = LTRIM(RTRIM(ISNULL(@numero, '')));

	IF @n = ''
		RETURN 'El RTN / identidad es obligatorio.';
	IF ISNULL(@tipo, '') NOT IN ('RTN', 'Identidad')
		RETURN 'Seleccione si es RTN o identidad.';
	IF @n LIKE '%[^0-9]%'
		RETURN 'Solo puede tener numeros, sin guiones ni espacios.';
	IF @n NOT LIKE '%[^0]%'
		RETURN 'El numero no puede ser todo ceros.';
	IF @tipo = 'Identidad' AND LEN(@n) <> 13
		RETURN 'La identidad debe tener 13 digitos.';
	IF @tipo = 'RTN' AND LEN(@n) <> 14
		RETURN 'El RTN debe tener 14 digitos.';

	RETURN '';
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarCliente
	@identificacionCliente VARCHAR(30),
	@tipoIdentificacion VARCHAR(10),
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
	SET @mensaje = dbo.fn_ValidarIdentificacion(@tipoIdentificacion, @identificacionCliente);

	IF @mensaje <> ''
		RETURN;

	IF EXISTS (SELECT 1 FROM CLIENTES WHERE identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'Ya existe un cliente con esa identificacion.';
		RETURN;
	END

	INSERT INTO CLIENTES (identificacionCliente, tipoIdentificacion, nombreEmpresa, nombreCliente,
						  apellidoCliente, telefonoCliente, correoCliente)
	VALUES (@identificacionCliente, @tipoIdentificacion, @nombreEmpresa, @nombreCliente,
			@apellidoCliente, @telefonoCliente, @correoCliente);

	SET @resultado = 1;
	SET @mensaje = 'Cliente registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarCliente
	@cliente_id INT,
	@identificacionCliente VARCHAR(30),
	@tipoIdentificacion VARCHAR(10),
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
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @anterior VARCHAR(30);
	SELECT @anterior = identificacionCliente FROM CLIENTES WHERE cliente_id = @cliente_id;

	IF @anterior IS NULL
	BEGIN
		SET @mensaje = 'El cliente no existe.';
		RETURN;
	END

	SET @identificacionCliente = LTRIM(RTRIM(ISNULL(@identificacionCliente, '')));
	SET @mensaje = dbo.fn_ValidarIdentificacion(@tipoIdentificacion, @identificacionCliente);

	IF @mensaje <> ''
		RETURN;

	IF EXISTS (SELECT 1 FROM CLIENTES
			   WHERE identificacionCliente = @identificacionCliente AND cliente_id <> @cliente_id)
	BEGIN
		SET @mensaje = 'Ya existe otro cliente con esa identificacion.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		-- Las locaciones y las ordenes se actualizan solas (ON UPDATE CASCADE)
		UPDATE CLIENTES
		SET identificacionCliente = @identificacionCliente,
			tipoIdentificacion = @tipoIdentificacion,
			nombreEmpresa = @nombreEmpresa,
			nombreCliente = @nombreCliente,
			apellidoCliente = @apellidoCliente,
			telefonoCliente = @telefonoCliente,
			correoCliente = @correoCliente
		WHERE cliente_id = @cliente_id;

		-- Las cotizaciones guardan la identificacion como texto
		IF @anterior <> @identificacionCliente
			UPDATE COTIZACIONES
			SET identificacionCliente = @identificacionCliente
			WHERE identificacionCliente = @anterior;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Cliente actualizado correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
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

	SELECT c.cliente_id, c.identificacionCliente, c.tipoIdentificacion, c.nombreEmpresa, c.nombreCliente,
		   c.apellidoCliente, c.telefonoCliente, c.correoCliente, c.estadoCliente,
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

	SELECT c.cliente_id, c.identificacionCliente, c.tipoIdentificacion, c.nombreEmpresa, c.nombreCliente,
		   c.apellidoCliente, c.telefonoCliente, c.correoCliente, c.estadoCliente,
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
		   e.identidad, e.telefonoEmpleado, e.correoEmpleado,
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
		   e.telefonoEmpleado, e.correoEmpleado,
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

	INSERT INTO EMPLEADOS (cargo_id, nombreEmpleado, apellidoEmpleado, identidad,
						   telefonoEmpleado, correoEmpleado)
	VALUES (@cargo_id, @nombreEmpleado, @apellidoEmpleado, @identidad,
			NULLIF(@telefonoEmpleado, ''), NULLIF(@correoEmpleado, ''));

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

	UPDATE EMPLEADOS
	SET cargo_id = @cargo_id,
		nombreEmpleado = @nombreEmpleado,
		apellidoEmpleado = @apellidoEmpleado,
		identidad = @identidad,
		telefonoEmpleado = NULLIF(@telefonoEmpleado, ''),
		correoEmpleado = NULLIF(@correoEmpleado, '')
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

CREATE OR ALTER PROCEDURE sp_EliminarEmpleado
	@empleado_id INT,
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

	IF EXISTS (SELECT 1 FROM ORDEN_EMPLEADOS WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: esta asignado a ordenes. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM ORDENES WHERE supervisor_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: es supervisor de ordenes. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM ASISTENCIAS WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: tiene asistencias registradas. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM COMPRAS WHERE firma_empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: firmo ingresos de inventario. Desactivelo.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM USUARIOS WHERE empleado_id = @empleado_id)
	BEGIN
		SET @mensaje = 'No se puede eliminar: tiene un usuario del sistema. Desactivelo.';
		RETURN;
	END

	BEGIN TRY
		DELETE FROM EMPLEADOS WHERE empleado_id = @empleado_id;

		SET @resultado = 1;
		SET @mensaje = 'Empleado eliminado correctamente.';
	END TRY
	BEGIN CATCH
		-- Cualquier otra tabla ligada al empleado: la llave foranea lo impide
		SET @resultado = 0;
		SET @mensaje = 'No se puede eliminar: tiene registros asociados. Desactivelo.';
	END CATCH
END
GO

-- Asistencia

CREATE OR ALTER PROCEDURE sp_RegistrarAsistencia
	@identidad VARCHAR(30),
	@tipo VARCHAR(10) OUTPUT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @tipo = '';
	SET @resultado = 0;
	SET @mensaje = '';

	SET @identidad = LTRIM(RTRIM(ISNULL(@identidad, '')));

	DECLARE @empleado_id INT, @activo BIT;

	SELECT @empleado_id = empleado_id, @activo = estadoEmpleado
	FROM EMPLEADOS
	WHERE identidad = @identidad;

	IF @empleado_id IS NULL
	BEGIN
		SET @mensaje = 'La identidad no esta registrada.';
		RETURN;
	END

	IF @activo = 0
	BEGIN
		SET @mensaje = 'El empleado esta inactivo.';
		RETURN;
	END

	DECLARE @hoy DATE = CAST(GETDATE() AS DATE),
			@ahora TIME(0) = CAST(GETDATE() AS TIME(0)),
			@asistencia_id INT,
			@salida TIME;

	SELECT TOP 1 @asistencia_id = asistencia_id, @salida = hora_salida
	FROM ASISTENCIAS
	WHERE empleado_id = @empleado_id AND fechaAsistencia = @hoy
	ORDER BY asistencia_id DESC;

	IF @asistencia_id IS NULL
	BEGIN
		INSERT INTO ASISTENCIAS (empleado_id, fechaAsistencia, hora_entrada, hora_salida)
		VALUES (@empleado_id, @hoy, @ahora, NULL);

		SET @tipo = 'ENTRADA';
		SET @resultado = 1;
		SET @mensaje = 'Entrada registrada.';
	END
	ELSE IF @salida IS NULL
	BEGIN
		UPDATE ASISTENCIAS SET hora_salida = @ahora WHERE asistencia_id = @asistencia_id;

		SET @tipo = 'SALIDA';
		SET @resultado = 1;
		SET @mensaje = 'Salida registrada.';
	END
	ELSE
	BEGIN
		SET @mensaje = 'Ya registro entrada y salida hoy.';
	END
END
GO

-- Mi trabajo (órdenes del empleado)

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

-- Vehículos

CREATE OR ALTER PROCEDURE sp_InsertarVehiculo
	@placa VARCHAR(15),
	@marcaVehiculo VARCHAR(20),
	@modeloVehiculo VARCHAR(25),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @placa = UPPER(LTRIM(RTRIM(ISNULL(@placa, ''))));

	IF @placa = ''
	BEGIN
		SET @mensaje = 'La placa es obligatoria.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM VEHICULOS WHERE placa = @placa)
	BEGIN
		SET @mensaje = 'Ya existe un vehiculo con esa placa.';
		RETURN;
	END

	INSERT INTO VEHICULOS (placa, marcaVehiculo, modeloVehiculo)
	VALUES (@placa, @marcaVehiculo, @modeloVehiculo);

	SET @resultado = 1;
	SET @mensaje = 'Vehiculo registrado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarVehiculo
	@vehiculo_id INT,
	@placa VARCHAR(15),
	@marcaVehiculo VARCHAR(20),
	@modeloVehiculo VARCHAR(25),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	SET @placa = UPPER(LTRIM(RTRIM(ISNULL(@placa, ''))));

	IF NOT EXISTS (SELECT 1 FROM VEHICULOS WHERE vehiculo_id = @vehiculo_id)
	BEGIN
		SET @mensaje = 'El vehiculo no existe.';
		RETURN;
	END

	IF @placa = ''
	BEGIN
		SET @mensaje = 'La placa es obligatoria.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM VEHICULOS WHERE placa = @placa AND vehiculo_id <> @vehiculo_id)
	BEGIN
		SET @mensaje = 'Ya existe otro vehiculo con esa placa.';
		RETURN;
	END

	UPDATE VEHICULOS
	SET placa = @placa,
		marcaVehiculo = @marcaVehiculo,
		modeloVehiculo = @modeloVehiculo
	WHERE vehiculo_id = @vehiculo_id;

	SET @resultado = 1;
	SET @mensaje = 'Vehiculo actualizado correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoVehiculo
	@vehiculo_id INT,
	@estadoVehiculo BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM VEHICULOS WHERE vehiculo_id = @vehiculo_id)
	BEGIN
		SET @mensaje = 'El vehiculo no existe.';
		RETURN;
	END

	UPDATE VEHICULOS
	SET estadoVehiculo = @estadoVehiculo
	WHERE vehiculo_id = @vehiculo_id;

	SET @resultado = 1;
	SET @mensaje = CASE WHEN @estadoVehiculo = 1
						THEN 'Vehiculo activado correctamente.'
						ELSE 'Vehiculo desactivado correctamente.' END;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarVehiculos
	@texto VARCHAR(50) = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT v.vehiculo_id, v.placa, v.marcaVehiculo, v.modeloVehiculo, v.estadoVehiculo,
		   CASE WHEN v.estadoVehiculo = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM VEHICULOS v
	WHERE (@estado IS NULL OR v.estadoVehiculo = @estado)
	  AND (@texto = ''
		   OR v.placa LIKE '%' + @texto + '%'
		   OR v.marcaVehiculo LIKE '%' + @texto + '%'
		   OR v.modeloVehiculo LIKE '%' + @texto + '%')
	ORDER BY v.placa;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerVehiculo
	@vehiculo_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT v.vehiculo_id, v.placa, v.marcaVehiculo, v.modeloVehiculo, v.estadoVehiculo,
		   CASE WHEN v.estadoVehiculo = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
	FROM VEHICULOS v
	WHERE v.vehiculo_id = @vehiculo_id;
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
		   CASE WHEN ISNULL(frecuencia, '') = '' THEN '' ELSE ' (' + frecuencia + ')' END AS texto,
		   ISNULL(descripcion, '') AS nombre,
		   ISNULL(unidad, 'unidad') AS unidad
	FROM ACTIVIDADES
	ORDER BY descripcion;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEmpleadosOrden
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id,
		   LTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '')) AS nombreCompleto
	FROM EMPLEADOS e
	WHERE e.estadoEmpleado = 1
	  AND NOT EXISTS (SELECT 1
					  FROM ORDEN_EMPLEADOS oe
					  INNER JOIN ORDENES o ON o.orden_id = oe.orden_id
					  INNER JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
					  WHERE oe.empleado_id = e.empleado_id AND eo.nombreEstadoOrden = 'En proceso')
	ORDER BY e.nombreEmpleado, e.apellidoEmpleado;
END
GO

-- Costeo de actividades
-- Precio = (mano de obra + insumos + equipo + transporte + supervision) + gastos administrativos,
-- dividido entre (1 - margen). El ISV se suma al total del documento.
-- Los valores (salario minimo, cargas, margen, etc.) estan en la tabla PARAMETROS_COSTEO.

CREATE OR ALTER FUNCTION dbo.fn_CotizarLineas (@lineas dbo.tipo_lista_medidas READONLY)
RETURNS TABLE
AS
RETURN
(
	WITH par AS
	(
		SELECT CAST(MAX(CASE WHEN clave = 'SALARIO_MINIMO' THEN valor END) AS FLOAT) AS salario,
			   CAST(MAX(CASE WHEN clave = 'CARGAS_SOCIALES' THEN valor END) AS FLOAT) AS cargas,
			   CAST(MAX(CASE WHEN clave = 'HORAS_MES' THEN valor END) AS FLOAT) AS horasMes,
			   CAST(MAX(CASE WHEN clave = 'FACTOR_SUPERVISOR' THEN valor END) AS FLOAT) AS factorSup,
			   CAST(MAX(CASE WHEN clave = 'CUADRILLAS_POR_SUPERVISOR' THEN valor END) AS FLOAT) AS cuadrillas,
			   CAST(MAX(CASE WHEN clave = 'LITROS_GASOLINA_VIAJE' THEN valor END) AS FLOAT)
			   * CAST(MAX(CASE WHEN clave = 'PRECIO_LITRO_GASOLINA' THEN valor END) AS FLOAT) AS transporte,
			   CAST(MAX(CASE WHEN clave = 'VIDA_UTIL_EQUIPO_MESES' THEN valor END) AS FLOAT) AS vidaUtil,
			   CAST(MAX(CASE WHEN clave = 'MANTENIMIENTO_EQUIPO_MENSUAL' THEN valor END) AS FLOAT) AS mantenimiento,
			   CAST(MAX(CASE WHEN clave = 'GASTOS_ADMINISTRATIVOS' THEN valor END) AS FLOAT) AS admin,
			   CAST(MAX(CASE WHEN clave = 'MARGEN_UTILIDAD' THEN valor END) AS FLOAT) AS margen
		FROM PARAMETROS_COSTEO
	),
	pedido AS
	(
		SELECT actividad_id, SUM(medida) AS medida
		FROM @lineas
		GROUP BY actividad_id
	),
	base AS
	(
		SELECT a.actividad_id,
			   a.descripcion,
			   ISNULL(a.unidad, 'unidad') AS unidad,
			   CAST(l.medida AS FLOAT) AS medida,
			   CAST(a.rendimiento AS FLOAT) AS rendimiento,
			   CAST(ISNULL(a.precio, 0) AS FLOAT) AS precioFijo,
			   ISNULL(r.personas, 1) AS personas
		FROM pedido l
		INNER JOIN ACTIVIDADES a ON a.actividad_id = l.actividad_id
		OUTER APPLY (SELECT TOP 1 ar.personas
					 FROM ACTIVIDAD_RANGOS ar
					 WHERE ar.actividad_id = a.actividad_id AND ar.desde <= l.medida
					 ORDER BY ar.desde DESC) r
	),
	horas AS
	(
		SELECT b.*,
			   CASE WHEN ISNULL(b.rendimiento, 0) > 0
					THEN b.medida / (b.rendimiento * b.personas) ELSE 0 END AS horasTrabajo
		FROM base b
	),
	costos AS
	(
		SELECT h.*,
			   CASE WHEN ISNULL(h.rendimiento, 0) > 0
					THEN h.medida / h.rendimiento * (p.salario * (1 + p.cargas) / p.horasMes)
					ELSE 0 END AS manoObra,
			   h.medida * CAST(ISNULL((SELECT SUM(ap.cantidad_por_unidad * ISNULL(pr.precio, 0))
									   FROM ACTIVIDAD_PRODUCTOS ap
									   OUTER APPLY (SELECT TOP 1 cp.precio_unitario AS precio
													FROM COMPRAS_PRODUCTOS cp
													WHERE cp.producto_id = ap.producto_id
													ORDER BY cp.compra_producto_id DESC) pr
									   WHERE ap.actividad_id = h.actividad_id), 0) AS FLOAT) AS insumos,
			   h.horasTrabajo * CAST(ISNULL((SELECT SUM(eq.precio)
											 FROM ACTIVIDAD_EQUIPOS ae
											 CROSS APPLY (SELECT AVG(ce.precio_unitario) AS precio
														  FROM COMPRAS_EQUIPOS ce
														  INNER JOIN EQUIPOS e ON e.equipo_id = ce.equipo_id
														  WHERE e.nombreEquipo = ae.nombreEquipo) eq
											 WHERE ae.actividad_id = h.actividad_id), 0) AS FLOAT)
			   * (1 / p.vidaUtil + p.mantenimiento) / p.horasMes AS equipo,
			   h.horasTrabajo * (p.salario * p.factorSup * (1 + p.cargas) / p.horasMes) / p.cuadrillas AS supervision,
			   CASE WHEN SUM(h.horasTrabajo) OVER () > 0
					THEN p.transporte * h.horasTrabajo / SUM(h.horasTrabajo) OVER ()
					ELSE p.transporte / COUNT(*) OVER () END AS transporte,
			   p.admin, p.margen
		FROM horas h
		CROSS JOIN par p
	),
	total AS
	(
		SELECT c.*,
			   c.manoObra + c.insumos + c.equipo + c.transporte + c.supervision AS directo
		FROM costos c
	)
	SELECT t.actividad_id,
		   LEFT(t.descripcion + ' (' +
				CASE WHEN t.medida = FLOOR(t.medida)
					 THEN CAST(CAST(t.medida AS BIGINT) AS VARCHAR(20))
					 ELSE CAST(CAST(t.medida AS DECIMAL(12, 2)) AS VARCHAR(20)) END +
				' ' + t.unidad + ')', 150) AS concepto,
		   t.descripcion,
		   t.unidad,
		   CAST(t.medida AS DECIMAL(12, 2)) AS medida,
		   t.personas,
		   CAST(t.horasTrabajo AS DECIMAL(12, 2)) AS horas,
		   CAST(t.manoObra AS DECIMAL(12, 2)) AS mano_obra,
		   CAST(t.insumos AS DECIMAL(12, 2)) AS insumos,
		   CAST(t.equipo AS DECIMAL(12, 2)) AS equipo,
		   CAST(t.transporte AS DECIMAL(12, 2)) AS transporte,
		   CAST(t.supervision AS DECIMAL(12, 2)) AS supervision,
		   CAST(t.directo * t.admin AS DECIMAL(12, 2)) AS gastos_admin,
		   CAST(t.directo * (1 + t.admin) AS DECIMAL(12, 2)) AS costo_total,
		   CAST(CASE WHEN ISNULL(t.rendimiento, 0) > 0
					 THEN t.directo * (1 + t.admin) / (1 - t.margen)
					 ELSE t.precioFijo * t.medida END AS DECIMAL(12, 2)) AS precio
	FROM total t
);
GO

CREATE OR ALTER PROCEDURE sp_CotizarLineas
	@lineas dbo.tipo_lista_medidas READONLY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT actividad_id, concepto, unidad, medida, personas, horas,
		   mano_obra, insumos, equipo, transporte, supervision, gastos_admin, costo_total, precio
	FROM dbo.fn_CotizarLineas(@lineas)
	ORDER BY actividad_id;
END
GO

-- Lo que necesita una cotizacion: productos (cantidad redondeada hacia arriba) y un equipo de cada tipo.
CREATE OR ALTER FUNCTION dbo.fn_NecesidadesCotizacion (@cotizacion_id VARCHAR(50))
RETURNS TABLE
AS
RETURN
(
	SELECT 'P' AS tipo,
		   p.producto_id,
		   p.nombreProducto AS nombre,
		   CAST(CEILING(SUM(d.medida * ap.cantidad_por_unidad)) AS INT) AS cantidad
	FROM COTIZACION_DETALLE d
	INNER JOIN ACTIVIDAD_PRODUCTOS ap ON ap.actividad_id = d.actividad_id
	INNER JOIN PRODUCTOS p ON p.producto_id = ap.producto_id
	WHERE d.cotizacion_id = @cotizacion_id
	GROUP BY p.producto_id, p.nombreProducto

	UNION ALL

	SELECT 'E', NULL, ae.nombreEquipo, 1
	FROM COTIZACION_DETALLE d
	INNER JOIN ACTIVIDAD_EQUIPOS ae ON ae.actividad_id = d.actividad_id
	WHERE d.cotizacion_id = @cotizacion_id
	GROUP BY ae.nombreEquipo
);
GO

CREATE OR ALTER PROCEDURE sp_ResumenCotizacion
	@cotizacion_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 'Personas' AS tipo,
		   'Empleados necesarios' AS nombre,
		   ISNULL(MAX(d.personas), 0) AS cantidad,
		   (SELECT COUNT(1) FROM EMPLEADOS e
			WHERE e.estadoEmpleado = 1
			  AND NOT EXISTS (SELECT 1 FROM ORDEN_EMPLEADOS oe
							  INNER JOIN ORDENES o ON o.orden_id = oe.orden_id
							  INNER JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
							  WHERE oe.empleado_id = e.empleado_id AND eo.nombreEstadoOrden = 'En proceso')) AS disponible
	FROM COTIZACION_DETALLE d
	WHERE d.cotizacion_id = @cotizacion_id

	UNION ALL

	SELECT 'Producto', n.nombre, n.cantidad, ISNULL(p.stock, 0)
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	INNER JOIN PRODUCTOS p ON p.producto_id = n.producto_id
	WHERE n.tipo = 'P'

	UNION ALL

	SELECT 'Equipo', n.nombre, n.cantidad,
		   (SELECT COUNT(1) FROM EQUIPOS e
			INNER JOIN ESTADO_EQUIPOS ee ON ee.estado_equipo_id = e.estado_equipo_id
			WHERE e.nombreEquipo = n.nombre AND ee.nombreEstadoEquipo = 'Disponible'
			  AND NOT EXISTS (SELECT 1 FROM ORDEN_EQUIPOS oe
							  WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0))
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	WHERE n.tipo = 'E';
END
GO

CREATE OR ALTER PROCEDURE sp_ListarCotizacionesOrden
	@texto VARCHAR(50) = '',
	@identificacionCliente VARCHAR(30) = ''
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @identificacionCliente = LTRIM(RTRIM(ISNULL(@identificacionCliente, '')));

	SELECT q.cotizacion_id,
		   q.fechaCotizacion,
		   q.valida_hasta,
		   ISNULL(q.identificacionCliente, '') AS identificacionCliente,
		   COALESCE(NULLIF(q.nombreEmpresa, ''),
					LTRIM(ISNULL(q.nombreCliente, '') + ' ' + ISNULL(q.apellidoCliente, ''))) AS cliente,
		   q.locacion_id,
		   ISNULL(q.observaciones, '') AS observaciones,
		   q.precio_cotizado,
		   CASE WHEN c.identificacionCliente IS NULL THEN 'Cliente no registrado'
				WHEN c.estadoCliente = 0 THEN 'Cliente inactivo'
				WHEN q.valida_hasta < CAST(GETDATE() AS DATE) THEN 'Vencida'
				WHEN q.servicio_id IS NULL OR NOT EXISTS (SELECT 1 FROM COTIZACION_DETALLE d
														  WHERE d.cotizacion_id = q.cotizacion_id
															AND d.actividad_id IS NOT NULL) THEN 'Sin actividades'
				ELSE 'Disponible' END AS estado,
		   CASE WHEN c.identificacionCliente IS NULL
				THEN 'El cliente de esta cotización no está registrado. Regístrelo en Clientes antes de crear la orden.'
				WHEN c.estadoCliente = 0
				THEN 'El cliente de esta cotización está inactivo.'
				WHEN q.valida_hasta < CAST(GETDATE() AS DATE)
				THEN 'Esta cotización ya venció. Haga una cotización nueva.'
				WHEN q.servicio_id IS NULL OR NOT EXISTS (SELECT 1 FROM COTIZACION_DETALLE d
														  WHERE d.cotizacion_id = q.cotizacion_id
															AND d.actividad_id IS NOT NULL)
				THEN 'Esta cotización es antigua y no tiene actividades. Haga una cotización nueva.'
				ELSE '' END AS motivo,
		   ISNULL(q.servicio_id, 0) AS servicio_id,
		   ISNULL((SELECT MAX(d.personas) FROM COTIZACION_DETALLE d
				   WHERE d.cotizacion_id = q.cotizacion_id), 0) AS personas
	FROM COTIZACIONES q
	LEFT JOIN CLIENTES c ON c.identificacionCliente = q.identificacionCliente
	WHERE NOT EXISTS (SELECT 1 FROM ORDENES o WHERE o.cotizacion_id = q.cotizacion_id)
	  AND (@identificacionCliente = '' OR q.identificacionCliente = @identificacionCliente)
	  AND (@texto = ''
		   OR q.cotizacion_id LIKE '%' + @texto + '%'
		   OR q.identificacionCliente LIKE '%' + @texto + '%'
		   OR q.nombreEmpresa LIKE '%' + @texto + '%'
		   OR ISNULL(q.nombreCliente, '') + ' ' + ISNULL(q.apellidoCliente, '') LIKE '%' + @texto + '%')
	ORDER BY q.fechaCotizacion DESC, q.cotizacion_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleCotizacion
	@cotizacion_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT linea, concepto, cantidad, precio_unitario,
		   CAST(cantidad * precio_unitario AS MONEY) AS subtotal,
		   ISNULL(actividad_id, 0) AS actividad_id,
		   ISNULL(medida, 0) AS medida,
		   ISNULL(personas, 0) AS personas
	FROM COTIZACION_DETALLE
	WHERE cotizacion_id = @cotizacion_id
	ORDER BY linea;
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarOrden
	@identificacionCliente VARCHAR(30),
	@locacion_id INT,
	@supervisor_id INT,
	@vehiculo_id INT,
	@fecha_solicitud DATE,
	@fecha_servicio DATE,
	@estado_orden_id INT,
	@observaciones VARCHAR(200),
	@turno VARCHAR(20),
	@piso_area VARCHAR(20),
	@empleados dbo.tipo_lista_empleados READONLY,
	@cotizacion_id VARCHAR(50),
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
	SET @cotizacion_id = LTRIM(RTRIM(ISNULL(@cotizacion_id, '')));

	DECLARE @servicio_id INT, @personas INT, @nombre VARCHAR(100);

	IF @cotizacion_id = ''
	BEGIN
		SET @mensaje = 'Seleccione una cotizacion.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM COTIZACIONES WHERE cotizacion_id = @cotizacion_id)
	BEGIN
		SET @mensaje = 'La cotizacion no existe.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM ORDENES WHERE cotizacion_id = @cotizacion_id)
	BEGIN
		SET @mensaje = 'Esa cotizacion ya tiene una orden.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM COTIZACIONES
			   WHERE cotizacion_id = @cotizacion_id AND valida_hasta < CAST(GETDATE() AS DATE))
	BEGIN
		SET @mensaje = 'La cotizacion ya vencio.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM COTIZACIONES
				   WHERE cotizacion_id = @cotizacion_id AND identificacionCliente = @identificacionCliente)
	BEGIN
		SET @mensaje = 'El cliente no coincide con el de la cotizacion.';
		RETURN;
	END

	SELECT @servicio_id = servicio_id FROM COTIZACIONES WHERE cotizacion_id = @cotizacion_id;

	IF @servicio_id IS NULL
	   OR NOT EXISTS (SELECT 1 FROM COTIZACION_DETALLE WHERE cotizacion_id = @cotizacion_id AND actividad_id IS NOT NULL)
	BEGIN
		SET @mensaje = 'La cotizacion no tiene actividades. Haga una nueva.';
		RETURN;
	END

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

	IF NOT EXISTS (SELECT 1 FROM EMPLEADOS WHERE empleado_id = @supervisor_id AND estadoEmpleado = 1)
	BEGIN
		SET @mensaje = 'El supervisor no existe o esta inactivo.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM VEHICULOS WHERE vehiculo_id = @vehiculo_id AND estadoVehiculo = 1)
	BEGIN
		SET @mensaje = 'El vehiculo no existe o esta inactivo.';
		RETURN;
	END

	SELECT @personas = ISNULL(MAX(personas), 0) FROM COTIZACION_DETALLE WHERE cotizacion_id = @cotizacion_id;

	IF (SELECT COUNT(1) FROM @empleados) < @personas
	BEGIN
		SET @mensaje = 'La cotizacion necesita ' + CAST(@personas AS VARCHAR(10)) + ' empleados.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @empleados x
			   WHERE NOT EXISTS (SELECT 1 FROM EMPLEADOS e WHERE e.empleado_id = x.empleado_id AND e.estadoEmpleado = 1))
	BEGIN
		SET @mensaje = 'Uno de los empleados no existe o esta inactivo.';
		RETURN;
	END

	SELECT TOP 1 @nombre = LTRIM(RTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '')))
	FROM @empleados x
	INNER JOIN EMPLEADOS e ON e.empleado_id = x.empleado_id
	INNER JOIN ORDEN_EMPLEADOS oe ON oe.empleado_id = x.empleado_id
	INNER JOIN ORDENES o ON o.orden_id = oe.orden_id
	INNER JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE eo.nombreEstadoOrden = 'En proceso';

	IF @nombre IS NOT NULL
	BEGIN
		SET @mensaje = LEFT(@nombre + ' esta en una orden en proceso.', 100);
		RETURN;
	END

	SET @nombre = NULL;

	SELECT TOP 1 @nombre = n.nombre
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	INNER JOIN PRODUCTOS p ON p.producto_id = n.producto_id
	WHERE n.tipo = 'P' AND n.cantidad > ISNULL(p.stock, 0);

	IF @nombre IS NOT NULL
	BEGIN
		SET @mensaje = LEFT('No hay stock suficiente de ' + @nombre + '.', 100);
		RETURN;
	END

	SELECT TOP 1 @nombre = n.nombre
	FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
	WHERE n.tipo = 'E'
	  AND NOT EXISTS (SELECT 1 FROM EQUIPOS e
					  INNER JOIN ESTADO_EQUIPOS ee ON ee.estado_equipo_id = e.estado_equipo_id
					  WHERE e.nombreEquipo = n.nombre AND ee.nombreEstadoEquipo = 'Disponible'
						AND NOT EXISTS (SELECT 1 FROM ORDEN_EQUIPOS oe
										WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0));

	IF @nombre IS NOT NULL
	BEGIN
		SET @mensaje = LEFT('No hay equipo disponible: ' + @nombre + '.', 100);
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
							 conformidad_cliente, identificacionCliente, cotizacion_id)
		VALUES (@orden_id, @locacion_id, @servicio_id, @supervisor_id, @vehiculo_id,
				@fecha_solicitud, @fecha_servicio, @estado_orden_id, @observaciones,
				0, @identificacionCliente, @cotizacion_id);

		INSERT INTO ORDEN_ACTIVIDADES (orden_id, actividad_id, estado, duracion_min)
		SELECT DISTINCT @orden_id, actividad_id, 0, NULL
		FROM COTIZACION_DETALLE
		WHERE cotizacion_id = @cotizacion_id AND actividad_id IS NOT NULL;

		INSERT INTO ORDEN_EMPLEADOS (orden_id, empleado_id, turno, piso_area)
		SELECT @orden_id, empleado_id, @turno, @piso_area FROM @empleados;

		INSERT INTO ORDEN_PRODUCTOS (orden_id, producto_id, cantidad)
		SELECT @orden_id, producto_id, cantidad
		FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id)
		WHERE tipo = 'P';

		UPDATE p
		SET p.stock = ISNULL(p.stock, 0) - n.cantidad
		FROM PRODUCTOS p
		INNER JOIN dbo.fn_NecesidadesCotizacion(@cotizacion_id) n ON n.producto_id = p.producto_id
		WHERE n.tipo = 'P';

		INSERT INTO ORDEN_EQUIPOS (orden_id, equipo_id, fecha_asignacion, devuelto)
		SELECT @orden_id,
			   (SELECT MIN(e.equipo_id) FROM EQUIPOS e
				INNER JOIN ESTADO_EQUIPOS ee ON ee.estado_equipo_id = e.estado_equipo_id
				WHERE e.nombreEquipo = n.nombre AND ee.nombreEstadoEquipo = 'Disponible'
				  AND NOT EXISTS (SELECT 1 FROM ORDEN_EQUIPOS oe
								  WHERE oe.equipo_id = e.equipo_id AND ISNULL(oe.devuelto, 0) = 0)),
			   @fecha_servicio, 0
		FROM dbo.fn_NecesidadesCotizacion(@cotizacion_id) n
		WHERE n.tipo = 'E';

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

-- Gestión de órdenes

CREATE OR ALTER PROCEDURE sp_ListarOrdenesGestion
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.identificacionCliente,
		   ISNULL(c.nombreEmpresa, '') AS nombreEmpresa,
		   LTRIM(RTRIM(ISNULL(c.nombreCliente, '') + ' ' + ISNULL(c.apellidoCliente, ''))) AS contacto,
		   ISNULL(s.nombreServicio, '') AS servicio,
		   ISNULL(o.supervisor_id, 0) AS supervisor_id,
		   LTRIM(RTRIM(ISNULL(sup.nombreEmpleado, '') + ' ' + ISNULL(sup.apellidoEmpleado, ''))) AS supervisor,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   ISNULL(o.estado_orden_id, 0) AS estado_orden_id,
		   ISNULL(eo.nombreEstadoOrden, '') AS estado
	FROM ORDENES o
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	LEFT JOIN EMPLEADOS sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	ORDER BY o.fecha_servicio DESC, o.orden_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleOrdenGestion
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.identificacionCliente,
		   ISNULL(c.nombreEmpresa, '') AS nombreEmpresa,
		   LTRIM(RTRIM(ISNULL(c.nombreCliente, '') + ' ' + ISNULL(c.apellidoCliente, ''))) AS contacto,
		   ISNULL(l.nombreLocacion, '') AS locacion,
		   ISNULL(l.tipoLocacion, '') AS tipoLocacion,
		   ISNULL(l.direccion, '') AS direccion,
		   ISNULL(oe.piso_area, '') AS piso_area,
		   ISNULL(oe.turno, '') AS turno,
		   CASE WHEN v.vehiculo_id IS NULL THEN ''
				ELSE LTRIM(RTRIM(ISNULL(v.placa, '') + ' (' + ISNULL(v.marcaVehiculo, '') + ' '
								 + ISNULL(v.modeloVehiculo, '') + ')')) END AS vehiculo,
		   ISNULL(o.observaciones, '') AS observaciones,
		   ISNULL(eo.nombreEstadoOrden, '') AS estado,
		   o.conformidad_cliente,
		   f.factura_id,
		   f.estado_pago,
		   f.precio_facturado,
		   f.fechaFactura
	FROM ORDENES o
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN LOCACIONES l ON l.locacion_id = o.locacion_id
	LEFT JOIN VEHICULOS v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	LEFT JOIN FACTURAS f ON f.orden_id = o.orden_id
	OUTER APPLY (SELECT TOP 1 x.turno, x.piso_area
				 FROM ORDEN_EMPLEADOS x
				 WHERE x.orden_id = o.orden_id
				 ORDER BY x.empleado_id) oe
	WHERE o.orden_id = @orden_id;
END
GO

-- seccion: 1 actividades, 2 empleados, 3 productos, 4 equipos
CREATE OR ALTER PROCEDURE sp_ListasOrdenGestion
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT seccion, texto
	FROM
	(
		SELECT 1 AS seccion,
			   CAST(CASE WHEN ISNULL(oa.estado, 0) = 1 THEN NCHAR(10003) + N' ' ELSE NCHAR(8226) + N' ' END
					+ ISNULL(a.descripcion, '')
					+ CASE WHEN oa.duracion_min IS NOT NULL
						   THEN N' (' + CAST(oa.duracion_min AS NVARCHAR(10)) + N' min)' ELSE N'' END
					AS NVARCHAR(200)) AS texto
		FROM ORDEN_ACTIVIDADES oa
		INNER JOIN ACTIVIDADES a ON a.actividad_id = oa.actividad_id
		WHERE oa.orden_id = @orden_id

		UNION ALL

		SELECT 2,
			   CAST(LTRIM(RTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '')))
					+ CASE WHEN ca.nombreCargo IS NOT NULL THEN N' (' + ca.nombreCargo + N')' ELSE N'' END
					AS NVARCHAR(200))
		FROM ORDEN_EMPLEADOS oe
		INNER JOIN EMPLEADOS e ON e.empleado_id = oe.empleado_id
		LEFT JOIN CARGOS ca ON ca.cargo_id = e.cargo_id
		WHERE oe.orden_id = @orden_id

		UNION ALL

		SELECT 3,
			   CAST(N'Producto: ' + ISNULL(p.nombreProducto, '') + N' ' + NCHAR(215) + N' '
					+ CAST(ISNULL(op.cantidad, 0) AS NVARCHAR(10)) AS NVARCHAR(200))
		FROM ORDEN_PRODUCTOS op
		INNER JOIN PRODUCTOS p ON p.producto_id = op.producto_id
		WHERE op.orden_id = @orden_id

		UNION ALL

		SELECT 4,
			   CAST(N'Equipo: ' + LTRIM(RTRIM(ISNULL(q.nombreEquipo, '') + ' ' + ISNULL(q.marcaEquipo, '')
											   + ' ' + ISNULL(q.modeloEquipo, '')))
					+ CASE WHEN ISNULL(oq.devuelto, 0) = 1 THEN N' (devuelto)' ELSE N' (en uso)' END
					AS NVARCHAR(200))
		FROM ORDEN_EQUIPOS oq
		INNER JOIN EQUIPOS q ON q.equipo_id = oq.equipo_id
		WHERE oq.orden_id = @orden_id
	) listas
	ORDER BY seccion, texto;
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarOrdenes
	@texto VARCHAR(50) = NULL,
	@estado_orden_id INT = NULL,
	@desde DATE = NULL,
	@hasta DATE = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));

	SELECT o.orden_id,
		   o.identificacionCliente,
		   ISNULL(c.nombreEmpresa, o.identificacionCliente) AS cliente,
		   s.nombreServicio,
		   l.nombreLocacion,
		   v.placa,
		   LTRIM(RTRIM(ISNULL(sup.nombreEmpleado, '') + ' ' + ISNULL(sup.apellidoEmpleado, ''))) AS supervisor,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   o.estado_orden_id,
		   eo.nombreEstadoOrden AS estado,
		   o.observaciones,
		   o.conformidad_cliente,
		   c.nombreEmpresa,
		   c.nombreCliente,
		   c.apellidoCliente,
		   l.direccion,
		   v.marcaVehiculo,
		   v.modeloVehiculo
	FROM ORDENES o
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	LEFT JOIN LOCACIONES l ON l.locacion_id = o.locacion_id
	LEFT JOIN VEHICULOS v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN EMPLEADOS sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE (@texto = '' OR o.orden_id LIKE '%' + @texto + '%'
		   OR o.identificacionCliente LIKE '%' + @texto + '%'
		   OR c.nombreEmpresa LIKE '%' + @texto + '%')
	  AND (@estado_orden_id IS NULL OR o.estado_orden_id = @estado_orden_id)
	  AND (@desde IS NULL OR o.fecha_servicio >= @desde)
	  AND (@hasta IS NULL OR o.fecha_servicio <= @hasta)
	ORDER BY o.fecha_servicio DESC, o.orden_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.identificacionCliente,
		   ISNULL(c.nombreEmpresa, o.identificacionCliente) AS cliente,
		   o.locacion_id,
		   l.nombreLocacion,
		   o.servicio_id,
		   s.nombreServicio,
		   o.supervisor_id,
		   LTRIM(RTRIM(ISNULL(sup.nombreEmpleado, '') + ' ' + ISNULL(sup.apellidoEmpleado, ''))) AS supervisor,
		   o.vehiculo_id,
		   v.placa,
		   o.fecha_solicitud,
		   o.fecha_servicio,
		   o.estado_orden_id,
		   eo.nombreEstadoOrden AS estado,
		   o.observaciones,
		   ISNULL(o.conformidad_cliente, 0) AS conformidad_cliente,
		   (SELECT TOP 1 oe.turno FROM ORDEN_EMPLEADOS oe WHERE oe.orden_id = o.orden_id) AS turno,
		   (SELECT TOP 1 oe.piso_area FROM ORDEN_EMPLEADOS oe WHERE oe.orden_id = o.orden_id) AS piso_area
	FROM ORDENES o
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN LOCACIONES l ON l.locacion_id = o.locacion_id
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	LEFT JOIN EMPLEADOS sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN VEHICULOS v ON v.vehiculo_id = o.vehiculo_id
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarActividadesPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.actividad_id,
		   a.descripcion,
		   a.frecuencia,
		   a.precio,
		   ISNULL(oa.estado, 0) AS estado,
		   oa.duracion_min
	FROM ORDEN_ACTIVIDADES oa
	INNER JOIN ACTIVIDADES a ON a.actividad_id = oa.actividad_id
	WHERE oa.orden_id = @orden_id
	ORDER BY a.descripcion;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEmpleadosPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id,
		   LTRIM(RTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, ''))) AS nombreCompleto,
		   ca.nombreCargo,
		   oe.turno,
		   oe.piso_area
	FROM ORDEN_EMPLEADOS oe
	INNER JOIN EMPLEADOS e ON e.empleado_id = oe.empleado_id
	LEFT JOIN CARGOS ca ON ca.cargo_id = e.cargo_id
	WHERE oe.orden_id = @orden_id
	ORDER BY e.nombreEmpleado, e.apellidoEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarProductosPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT p.producto_id,
		   p.nombreProducto,
		   p.tipoProducto,
		   op.cantidad
	FROM ORDEN_PRODUCTOS op
	INNER JOIN PRODUCTOS p ON p.producto_id = op.producto_id
	WHERE op.orden_id = @orden_id
	ORDER BY p.nombreProducto;
END
GO

CREATE OR ALTER PROCEDURE sp_ListarEquiposPorOrden
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.equipo_id,
		   e.nombreEquipo,
		   e.tipoEquipo,
		   e.marcaEquipo,
		   e.modeloEquipo,
		   oe.fecha_asignacion,
		   ISNULL(oe.devuelto, 0) AS devuelto
	FROM ORDEN_EQUIPOS oe
	INNER JOIN EQUIPOS e ON e.equipo_id = oe.equipo_id
	WHERE oe.orden_id = @orden_id
	ORDER BY e.nombreEquipo, e.equipo_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarOrden
	@orden_id VARCHAR(50),
	@locacion_id INT,
	@servicio_id INT,
	@supervisor_id INT,
	@vehiculo_id INT,
	@fecha_solicitud DATE,
	@fecha_servicio DATE,
	@observaciones VARCHAR(200),
	@turno VARCHAR(20),
	@piso_area VARCHAR(20),
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @cliente VARCHAR(30), @estado VARCHAR(25);

	SELECT @cliente = o.identificacionCliente, @estado = eo.nombreEstadoOrden
	FROM ORDENES o
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;

	IF @cliente IS NULL
	BEGIN
		SET @mensaje = 'La orden no existe.';
		RETURN;
	END

	IF ISNULL(@estado, '') <> 'Pendiente'
	BEGIN
		SET @mensaje = 'Solo se pueden editar ordenes pendientes.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM LOCACIONES WHERE locacion_id = @locacion_id AND identificacionCliente = @cliente)
	BEGIN
		SET @mensaje = 'La locacion no pertenece al cliente.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM SERVICIOS WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'El servicio no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM EMPLEADOS WHERE empleado_id = @supervisor_id AND estadoEmpleado = 1)
	BEGIN
		SET @mensaje = 'El supervisor no existe o esta inactivo.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM VEHICULOS WHERE vehiculo_id = @vehiculo_id AND estadoVehiculo = 1)
	BEGIN
		SET @mensaje = 'El vehiculo no existe o esta inactivo.';
		RETURN;
	END

	IF @fecha_servicio < @fecha_solicitud
	BEGIN
		SET @mensaje = 'La fecha de limpieza no puede ser anterior a la solicitud.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE ORDENES
		SET locacion_id = @locacion_id,
			servicio_id = @servicio_id,
			supervisor_id = @supervisor_id,
			vehiculo_id = @vehiculo_id,
			fecha_solicitud = @fecha_solicitud,
			fecha_servicio = @fecha_servicio,
			observaciones = LTRIM(RTRIM(ISNULL(@observaciones, '')))
		WHERE orden_id = @orden_id;

		UPDATE ORDEN_EMPLEADOS
		SET turno = LTRIM(RTRIM(ISNULL(@turno, ''))),
			piso_area = LTRIM(RTRIM(ISNULL(@piso_area, '')))
		WHERE orden_id = @orden_id;

		UPDATE ORDEN_EQUIPOS
		SET fecha_asignacion = @fecha_servicio
		WHERE orden_id = @orden_id AND ISNULL(devuelto, 0) = 0;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'La orden se actualizo correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_CambiarEstadoOrden
	@orden_id VARCHAR(50),
	@estado_orden_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @actual VARCHAR(25), @nuevo VARCHAR(25);

	SELECT @actual = eo.nombreEstadoOrden
	FROM ORDENES o
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;

	IF NOT EXISTS (SELECT 1 FROM ORDENES WHERE orden_id = @orden_id)
	BEGIN
		SET @mensaje = 'La orden no existe.';
		RETURN;
	END

	SELECT @nuevo = nombreEstadoOrden FROM ESTADO_ORDENES WHERE estado_orden_id = @estado_orden_id;

	IF @nuevo IS NULL
	BEGIN
		SET @mensaje = 'El estado no existe.';
		RETURN;
	END

	IF ISNULL(@actual, '') IN ('Completada', 'Cancelada')
	BEGIN
		SET @mensaje = 'La orden ya esta ' + LOWER(@actual) + ' y no se puede cambiar.';
		RETURN;
	END

	IF @nuevo = 'En proceso' AND ISNULL(@actual, '') <> 'En proceso'
	BEGIN
		DECLARE @ocupado VARCHAR(100);

		SELECT TOP 1 @ocupado = LTRIM(RTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '')))
		FROM ORDEN_EMPLEADOS mi
		INNER JOIN ORDEN_EMPLEADOS otro ON otro.empleado_id = mi.empleado_id AND otro.orden_id <> mi.orden_id
		INNER JOIN ORDENES oo ON oo.orden_id = otro.orden_id
		INNER JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = oo.estado_orden_id
		INNER JOIN EMPLEADOS e ON e.empleado_id = mi.empleado_id
		WHERE mi.orden_id = @orden_id AND eo.nombreEstadoOrden = 'En proceso';

		IF @ocupado IS NOT NULL
		BEGIN
			SET @mensaje = LEFT(@ocupado + ' ya esta en otra orden en proceso.', 100);
			RETURN;
		END
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE ORDENES SET estado_orden_id = @estado_orden_id WHERE orden_id = @orden_id;

		-- Al cancelar se devuelve el stock de los productos
		IF @nuevo = 'Cancelada'
		BEGIN
			UPDATE p
			SET p.stock = ISNULL(p.stock, 0) + op.cantidad
			FROM PRODUCTOS p
			INNER JOIN ORDEN_PRODUCTOS op ON op.producto_id = p.producto_id
			WHERE op.orden_id = @orden_id;
		END

		-- Al cerrar la orden (completada o cancelada) los equipos quedan libres
		IF @nuevo IN ('Completada', 'Cancelada')
		BEGIN
			UPDATE ORDEN_EQUIPOS SET devuelto = 1 WHERE orden_id = @orden_id;
		END

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'La orden ahora esta ' + LOWER(@nuevo) + '.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_MarcarActividadOrden
	@orden_id VARCHAR(50),
	@actividad_id INT,
	@estado BIT,
	@duracion_min INT = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM ORDEN_ACTIVIDADES WHERE orden_id = @orden_id AND actividad_id = @actividad_id)
	BEGIN
		SET @mensaje = 'La actividad no pertenece a la orden.';
		RETURN;
	END

	IF @duracion_min IS NOT NULL AND @duracion_min < 0
	BEGIN
		SET @mensaje = 'La duracion no puede ser negativa.';
		RETURN;
	END

	UPDATE ORDEN_ACTIVIDADES
	SET estado = @estado,
		duracion_min = CASE WHEN @estado = 1 THEN @duracion_min ELSE NULL END
	WHERE orden_id = @orden_id AND actividad_id = @actividad_id;

	SET @resultado = 1;
	SET @mensaje = 'La actividad se actualizo correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_DevolverEquipoOrden
	@orden_id VARCHAR(50),
	@equipo_id INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	IF NOT EXISTS (SELECT 1 FROM ORDEN_EQUIPOS WHERE orden_id = @orden_id AND equipo_id = @equipo_id)
	BEGIN
		SET @mensaje = 'El equipo no esta asignado a la orden.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM ORDEN_EQUIPOS
			   WHERE orden_id = @orden_id AND equipo_id = @equipo_id AND ISNULL(devuelto, 0) = 1)
	BEGIN
		SET @mensaje = 'El equipo ya fue devuelto.';
		RETURN;
	END

	UPDATE ORDEN_EQUIPOS SET devuelto = 1 WHERE orden_id = @orden_id AND equipo_id = @equipo_id;

	SET @resultado = 1;
	SET @mensaje = 'El equipo se devolvio correctamente.';
END
GO

CREATE OR ALTER PROCEDURE sp_RegistrarConformidadOrden
	@orden_id VARCHAR(50),
	@conformidad BIT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';

	DECLARE @estado VARCHAR(25);

	SELECT @estado = eo.nombreEstadoOrden
	FROM ORDENES o
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE o.orden_id = @orden_id;

	IF NOT EXISTS (SELECT 1 FROM ORDENES WHERE orden_id = @orden_id)
	BEGIN
		SET @mensaje = 'La orden no existe.';
		RETURN;
	END

	IF ISNULL(@estado, '') <> 'Completada'
	BEGIN
		SET @mensaje = 'La conformidad solo se registra en ordenes completadas.';
		RETURN;
	END

	UPDATE ORDENES SET conformidad_cliente = @conformidad WHERE orden_id = @orden_id;

	SET @resultado = 1;
	SET @mensaje = 'La conformidad del cliente se registro correctamente.';
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

-- Facturación y cotizaciones

CREATE OR ALTER PROCEDURE sp_ListarFormasPago
AS
BEGIN
	SET NOCOUNT ON;

	SELECT forma_pago_id, nombreFormaPago
	FROM FORMAS_PAGOS
	ORDER BY forma_pago_id;
END
GO

-- Las líneas de la orden salen de su cotización (una por actividad). Sin cotización, de las actividades.
CREATE OR ALTER PROCEDURE sp_DetalleOrdenFactura
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT 1 FROM ORDENES o
			   INNER JOIN COTIZACION_DETALLE d ON d.cotizacion_id = o.cotizacion_id
			   WHERE o.orden_id = @orden_id)
	BEGIN
		SELECT d.concepto,
			   d.cantidad,
			   CAST(d.precio_unitario AS DECIMAL(12, 2)) AS precio_unitario
		FROM ORDENES o
		INNER JOIN COTIZACION_DETALLE d ON d.cotizacion_id = o.cotizacion_id
		WHERE o.orden_id = @orden_id
		ORDER BY d.linea;
	END
	ELSE
	BEGIN
		SELECT ISNULL(a.descripcion, '') +
			   CASE WHEN ISNULL(a.frecuencia, '') = '' THEN '' ELSE ' (' + a.frecuencia + ')' END AS concepto,
			   1 AS cantidad,
			   CAST(ISNULL(a.precio, 0) AS DECIMAL(12, 2)) AS precio_unitario
		FROM ORDEN_ACTIVIDADES oa
		INNER JOIN ACTIVIDADES a ON a.actividad_id = oa.actividad_id
		WHERE oa.orden_id = @orden_id
		ORDER BY a.descripcion;
	END
END
GO

CREATE OR ALTER PROCEDURE sp_InsertarCotizacion
	@identificacionCliente VARCHAR(30),
	@nombreEmpresa VARCHAR(50),
	@nombreCliente VARCHAR(50),
	@apellidoCliente VARCHAR(50),
	@telefonoCliente VARCHAR(50),
	@correoCliente VARCHAR(50),
	@locacion_id INT = NULL,
	@direccion VARCHAR(80),
	@fecha_cotizacion DATE,
	@valida_hasta DATE,
	@observaciones VARCHAR(200),
	@servicio_id INT,
	@lineas dbo.tipo_lista_medidas READONLY,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT,
	@cotizacion_id VARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @cotizacion_id = '';

	SET @nombreCliente = LTRIM(RTRIM(ISNULL(@nombreCliente, '')));

	IF @nombreCliente = ''
	BEGIN
		SET @mensaje = 'El nombre del contacto es obligatorio.';
		RETURN;
	END

	IF @locacion_id IS NOT NULL AND NOT EXISTS (SELECT 1 FROM LOCACIONES WHERE locacion_id = @locacion_id)
	BEGIN
		SET @mensaje = 'La locacion no existe.';
		RETURN;
	END

	IF @valida_hasta < @fecha_cotizacion
	BEGIN
		SET @mensaje = 'La fecha de validez no puede ser anterior a la cotizacion.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM SERVICIOS WHERE servicio_id = @servicio_id)
	BEGIN
		SET @mensaje = 'El servicio no existe.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM @lineas)
	BEGIN
		SET @mensaje = 'La cotizacion necesita al menos una actividad.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @lineas WHERE medida <= 0)
	BEGIN
		SET @mensaje = 'Todas las actividades necesitan una medida mayor que 0.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM @lineas x WHERE NOT EXISTS (SELECT 1 FROM ACTIVIDADES a WHERE a.actividad_id = x.actividad_id))
	BEGIN
		SET @mensaje = 'Una de las actividades no existe.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @siguiente INT;
		DECLARE @tasa DECIMAL(6, 4) = ISNULL((SELECT valor FROM PARAMETROS_COSTEO WHERE clave = 'ISV'), 0.15);
		DECLARE @subtotal DECIMAL(12, 2) = (SELECT ISNULL(SUM(precio), 0) FROM dbo.fn_CotizarLineas(@lineas));
		DECLARE @total DECIMAL(12, 2) = @subtotal + ROUND(@subtotal * @tasa, 2);

		SELECT @siguiente = ISNULL(MAX(TRY_CAST(SUBSTRING(cotizacion_id, 5, 20) AS INT)), 0) + 1
		FROM COTIZACIONES WITH (UPDLOCK, HOLDLOCK)
		WHERE cotizacion_id LIKE 'COT-%';

		SET @cotizacion_id = 'COT-' + RIGHT('000000' + CAST(@siguiente AS VARCHAR(10)), 6);

		INSERT INTO COTIZACIONES (cotizacion_id, identificacionCliente, nombreEmpresa, nombreCliente,
								  apellidoCliente, telefonoCliente, correoCliente, locacion_id, direccion,
								  fechaCotizacion, valida_hasta, observaciones, precio_cotizado, servicio_id)
		VALUES (@cotizacion_id, NULLIF(LTRIM(RTRIM(@identificacionCliente)), ''), NULLIF(@nombreEmpresa, ''),
				@nombreCliente, NULLIF(@apellidoCliente, ''), NULLIF(@telefonoCliente, ''),
				NULLIF(@correoCliente, ''), @locacion_id, @direccion,
				@fecha_cotizacion, @valida_hasta, NULLIF(@observaciones, ''), @total, @servicio_id);

		INSERT INTO COTIZACION_DETALLE (cotizacion_id, linea, concepto, cantidad, precio_unitario,
										actividad_id, medida, personas)
		SELECT @cotizacion_id, ROW_NUMBER() OVER (ORDER BY actividad_id), concepto, 1, precio,
			   actividad_id, medida, personas
		FROM dbo.fn_CotizarLineas(@lineas);

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Cotizacion registrada correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @cotizacion_id = '';
		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

-- Contado (1 cuota): factura pagada. De 6 a 24 cuotas: crédito con cuenta por cobrar y cuotas mensuales.
CREATE OR ALTER PROCEDURE sp_InsertarFactura
	@orden_id VARCHAR(50),
	@fecha_factura DATE,
	@forma_pago_id INT,
	@numero_cuotas INT,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT,
	@factura_id INT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @factura_id = NULL;

	IF NOT EXISTS (SELECT 1 FROM ORDENES o
				   INNER JOIN ESTADO_ORDENES e ON e.estado_orden_id = o.estado_orden_id
				   WHERE o.orden_id = @orden_id AND e.nombreEstadoOrden = 'Completada')
	BEGIN
		SET @mensaje = 'La orden no existe o no esta completada.';
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM FACTURAS WHERE orden_id = @orden_id)
	BEGIN
		SET @mensaje = 'Esta orden ya fue facturada.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM FORMAS_PAGOS WHERE forma_pago_id = @forma_pago_id)
	BEGIN
		SET @mensaje = 'El metodo de pago no existe.';
		RETURN;
	END

	IF @numero_cuotas < 1 OR @numero_cuotas > 24
	BEGIN
		SET @mensaje = 'El numero de cuotas debe estar entre 1 y 24.';
		RETURN;
	END

	CREATE TABLE #lineas (concepto VARCHAR(150), cantidad INT, precio_unitario DECIMAL(12, 2));
	INSERT INTO #lineas EXEC sp_DetalleOrdenFactura @orden_id;

	IF NOT EXISTS (SELECT 1 FROM #lineas)
	BEGIN
		SET @mensaje = 'La orden no tiene servicios para facturar.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		DECLARE @tasa DECIMAL(6, 4) = ISNULL((SELECT valor FROM PARAMETROS_COSTEO WHERE clave = 'ISV'), 0.15);
		DECLARE @subtotal DECIMAL(12, 2) = (SELECT ISNULL(SUM(cantidad * precio_unitario), 0) FROM #lineas);
		DECLARE @total DECIMAL(12, 2) = @subtotal + ROUND(@subtotal * @tasa, 2);
		DECLARE @contado BIT = CASE WHEN @numero_cuotas = 1 THEN 1 ELSE 0 END;

		INSERT INTO FACTURAS (orden_id, fechaFactura, tipo_pago, estado_pago, fecha_pago,
							  forma_pago_id, precio_facturado)
		VALUES (@orden_id, @fecha_factura,
				CASE WHEN @contado = 1 THEN 'Contado' ELSE 'Credito' END,
				CASE WHEN @contado = 1 THEN 'Pagada' ELSE 'Pendiente' END,
				CASE WHEN @contado = 1 THEN @fecha_factura ELSE NULL END,
				@forma_pago_id, @total);

		SET @factura_id = CAST(SCOPE_IDENTITY() AS INT);

		INSERT INTO FACTURA_DETALLE (factura_id, linea, concepto, cantidad, precio_unitario, adicional)
		SELECT @factura_id, ROW_NUMBER() OVER (ORDER BY (SELECT 1)), concepto, cantidad, precio_unitario, 0
		FROM #lineas;

		IF @contado = 0
		BEGIN
			INSERT INTO CUENTAS_POR_COBRAR (factura_id, monto_total, prima, numero_cuotas,
											frecuencia, fecha_inicio, estado_cuenta)
			VALUES (@factura_id, @total, 0, @numero_cuotas, 'Mensual', @fecha_factura, 'Vigente');

			DECLARE @cuenta_id INT = CAST(SCOPE_IDENTITY() AS INT);
			DECLARE @cuota DECIMAL(12, 2) = ROUND(@total / @numero_cuotas, 2);

			-- La última cuota absorbe la diferencia por redondeo
			;WITH n AS
			(
				SELECT 1 AS num
				UNION ALL
				SELECT num + 1 FROM n WHERE num < @numero_cuotas
			)
			INSERT INTO CUOTAS (cuenta_id, numero_cuota, fecha_vencimiento, monto_cuota, monto_pagado, estado_cuota)
			SELECT @cuenta_id, num, DATEADD(MONTH, num, @fecha_factura),
				   CASE WHEN num = @numero_cuotas THEN @total - @cuota * (@numero_cuotas - 1) ELSE @cuota END,
				   0, 'Pendiente'
			FROM n;
		END

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'Factura registrada correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @factura_id = NULL;
		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_BuscarFacturas
	@texto VARCHAR(50) = NULL,
	@estado_pago VARCHAR(50) = NULL,
	@desde DATE = NULL,
	@hasta DATE = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @estado_pago = LTRIM(RTRIM(ISNULL(@estado_pago, '')));

	SELECT f.factura_id,
		   f.orden_id,
		   o.identificacionCliente,
		   ISNULL(c.nombreEmpresa, o.identificacionCliente) AS cliente,
		   f.fechaFactura,
		   f.tipo_pago,
		   f.estado_pago,
		   f.fecha_pago,
		   fp.nombreFormaPago,
		   f.precio_facturado
	FROM FACTURAS f
	INNER JOIN ORDENES o ON o.orden_id = f.orden_id
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN FORMAS_PAGOS fp ON fp.forma_pago_id = f.forma_pago_id
	WHERE (@texto = '' OR CAST(f.factura_id AS VARCHAR(10)) = @texto
		   OR f.orden_id LIKE '%' + @texto + '%'
		   OR o.identificacionCliente LIKE '%' + @texto + '%'
		   OR c.nombreEmpresa LIKE '%' + @texto + '%')
	  AND (@estado_pago = '' OR f.estado_pago = @estado_pago)
	  AND (@desde IS NULL OR f.fechaFactura >= @desde)
	  AND (@hasta IS NULL OR f.fechaFactura <= @hasta)
	ORDER BY f.fechaFactura DESC, f.factura_id DESC;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT f.factura_id,
		   f.orden_id,
		   f.fechaFactura,
		   f.tipo_pago,
		   f.estado_pago,
		   f.fecha_pago,
		   ISNULL(fp.nombreFormaPago, '') AS nombreFormaPago,
		   ISNULL(f.precio_facturado, 0) AS precio_facturado,
		   ISNULL(o.identificacionCliente, '') AS identificacionCliente,
		   ISNULL(c.nombreEmpresa, '') AS nombreEmpresa,
		   ISNULL(c.nombreCliente, '') AS nombreCliente,
		   ISNULL(c.apellidoCliente, '') AS apellidoCliente,
		   ISNULL(c.telefonoCliente, '') AS telefonoCliente,
		   ISNULL(c.correoCliente, '') AS correoCliente,
		   ISNULL(l.nombreLocacion, '') AS nombreLocacion,
		   ISNULL(l.tipoLocacion, '') AS tipoLocacion,
		   ISNULL(l.direccion, '') AS direccion,
		   pg.pagado,
		   ISNULL(f.precio_facturado, 0) - pg.pagado AS saldo
	FROM FACTURAS f
	INNER JOIN ORDENES o ON o.orden_id = f.orden_id
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN LOCACIONES l ON l.locacion_id = o.locacion_id
	LEFT JOIN FORMAS_PAGOS fp ON fp.forma_pago_id = f.forma_pago_id
	CROSS APPLY (SELECT CASE WHEN f.tipo_pago = 'Credito'
							 THEN ISNULL((SELECT SUM(ISNULL(q.monto_pagado, 0))
										  FROM CUENTAS_POR_COBRAR cc
										  INNER JOIN CUOTAS q ON q.cuenta_id = cc.cuenta_id
										  WHERE cc.factura_id = f.factura_id), 0)
							 WHEN f.estado_pago = 'Pagada' THEN ISNULL(f.precio_facturado, 0)
							 ELSE 0 END AS pagado) pg
	WHERE f.factura_id = @factura_id;
END
GO

CREATE OR ALTER PROCEDURE sp_DetalleFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT linea,
		   CASE WHEN adicional = 1 THEN 'Adicional' ELSE 'Servicio' END AS tipo,
		   concepto,
		   cantidad,
		   precio_unitario,
		   CAST(cantidad * precio_unitario AS DECIMAL(12, 2)) AS subtotal
	FROM FACTURA_DETALLE
	WHERE factura_id = @factura_id
	ORDER BY linea;
END
GO

CREATE OR ALTER PROCEDURE sp_CuotasFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT q.numero_cuota,
		   q.fecha_vencimiento,
		   q.monto_cuota,
		   ISNULL(q.monto_pagado, 0) AS monto_pagado,
		   q.monto_cuota - ISNULL(q.monto_pagado, 0) AS saldo,
		   q.estado_cuota
	FROM CUENTAS_POR_COBRAR cpc
	INNER JOIN CUOTAS q ON q.cuenta_id = cpc.cuenta_id
	WHERE cpc.factura_id = @factura_id
	ORDER BY q.numero_cuota;
END
GO

CREATE OR ALTER PROCEDURE sp_ObtenerOrdenFactura
	@orden_id VARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT o.orden_id,
		   o.fecha_servicio,
		   ISNULL(o.cotizacion_id, '') AS cotizacion_id,
		   ISNULL(o.identificacionCliente, '') AS identificacionCliente,
		   ISNULL(c.nombreEmpresa, '') AS nombreEmpresa,
		   ISNULL(c.nombreCliente, '') AS nombreCliente,
		   ISNULL(c.apellidoCliente, '') AS apellidoCliente,
		   ISNULL(c.telefonoCliente, '') AS telefonoCliente,
		   ISNULL(c.correoCliente, '') AS correoCliente,
		   ISNULL(l.nombreLocacion, '') AS nombreLocacion,
		   ISNULL(l.tipoLocacion, '') AS tipoLocacion,
		   ISNULL(l.direccion, '') AS direccion,
		   ISNULL(s.nombreServicio, '') AS nombreServicio,
		   CASE WHEN eo.nombreEstadoOrden = 'Completada'
				 AND NOT EXISTS (SELECT 1 FROM FACTURAS f WHERE f.orden_id = o.orden_id)
				THEN 1 ELSE 0 END AS facturable
	FROM ORDENES o
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN LOCACIONES l ON l.locacion_id = o.locacion_id
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	WHERE o.orden_id = @orden_id;
END
GO

-- Cómo quedan las cuotas si se abona un monto: el abono cubre primero las cuotas más antiguas.
CREATE OR ALTER FUNCTION dbo.fn_AplicarAbono (@factura_id INT, @monto MONEY)
RETURNS TABLE
AS
RETURN
(
	WITH c AS
	(
		SELECT q.cuota_id, q.numero_cuota, q.fecha_vencimiento, q.monto_cuota,
			   ISNULL(q.monto_pagado, 0) AS pagado,
			   q.monto_cuota - ISNULL(q.monto_pagado, 0) AS pendiente
		FROM CUOTAS q
		INNER JOIN CUENTAS_POR_COBRAR cc ON cc.cuenta_id = q.cuenta_id
		WHERE cc.factura_id = @factura_id
	),
	a AS
	(
		SELECT c.*,
			   ISNULL(SUM(c.pendiente) OVER (ORDER BY c.numero_cuota
											 ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING), 0) AS antes
		FROM c
	),
	b AS
	(
		SELECT a.*,
			   CASE WHEN @monto - a.antes <= 0 THEN 0
					WHEN @monto - a.antes >= a.pendiente THEN a.pendiente
					ELSE @monto - a.antes END AS aplicado
		FROM a
	)
	SELECT b.cuota_id, b.numero_cuota, b.fecha_vencimiento, b.monto_cuota, b.pagado,
		   b.aplicado,
		   b.pagado + b.aplicado AS nuevo_pagado,
		   b.monto_cuota - b.pagado - b.aplicado AS nuevo_saldo,
		   CASE WHEN b.pagado + b.aplicado >= b.monto_cuota THEN 'Pagada'
				WHEN b.fecha_vencimiento < CAST(GETDATE() AS DATE) THEN 'Vencida'
				WHEN b.pagado + b.aplicado > 0 THEN 'Parcial'
				ELSE 'Pendiente' END AS nuevo_estado
	FROM b
);
GO

CREATE OR ALTER PROCEDURE sp_SimularAbono
	@factura_id INT,
	@monto MONEY
AS
BEGIN
	SET NOCOUNT ON;

	SELECT numero_cuota, fecha_vencimiento, monto_cuota, pagado, aplicado, nuevo_pagado, nuevo_saldo, nuevo_estado
	FROM dbo.fn_AplicarAbono(@factura_id, @monto)
	ORDER BY numero_cuota;
END
GO

CREATE OR ALTER PROCEDURE sp_RegistrarAbono
	@factura_id INT,
	@monto MONEY,
	@forma_pago_id INT,
	@fecha_abono DATE = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @fecha_abono = ISNULL(@fecha_abono, CAST(GETDATE() AS DATE));

	DECLARE @tipo VARCHAR(10), @estado VARCHAR(50), @cuenta_id INT, @saldo MONEY;

	SELECT @tipo = tipo_pago, @estado = estado_pago FROM FACTURAS WHERE factura_id = @factura_id;

	IF @tipo IS NULL
	BEGIN
		SET @mensaje = 'La factura no existe.';
		RETURN;
	END

	IF @tipo <> 'Credito'
	BEGIN
		SET @mensaje = 'Solo las facturas a credito reciben abonos.';
		RETURN;
	END

	IF @estado = 'Pagada'
	BEGIN
		SET @mensaje = 'La factura ya esta pagada.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM FORMAS_PAGOS WHERE forma_pago_id = @forma_pago_id)
	BEGIN
		SET @mensaje = 'La forma de pago no existe.';
		RETURN;
	END

	SELECT @cuenta_id = cc.cuenta_id,
		   @saldo = SUM(q.monto_cuota - ISNULL(q.monto_pagado, 0))
	FROM CUENTAS_POR_COBRAR cc
	INNER JOIN CUOTAS q ON q.cuenta_id = cc.cuenta_id
	WHERE cc.factura_id = @factura_id
	GROUP BY cc.cuenta_id;

	IF ISNULL(@monto, 0) <= 0
	BEGIN
		SET @mensaje = 'El abono debe ser mayor que 0.';
		RETURN;
	END

	IF @monto > @saldo
	BEGIN
		SET @mensaje = 'El abono supera el saldo pendiente: ' + CAST(@saldo AS VARCHAR(20)) + '.';
		RETURN;
	END

	DECLARE @nuevas TABLE (cuota_id INT, aplicado MONEY, nuevo_pagado MONEY, nuevo_estado VARCHAR(20));

	INSERT INTO @nuevas
	SELECT cuota_id, aplicado, nuevo_pagado, nuevo_estado
	FROM dbo.fn_AplicarAbono(@factura_id, @monto);

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE q
		SET q.monto_pagado = n.nuevo_pagado,
			q.estado_cuota = n.nuevo_estado,
			q.fecha_pago = @fecha_abono,
			q.forma_pago_id = @forma_pago_id
		FROM CUOTAS q
		INNER JOIN @nuevas n ON n.cuota_id = q.cuota_id
		WHERE n.aplicado > 0;

		INSERT INTO ABONOS (factura_id, fecha_abono, monto, forma_pago_id, saldo_despues)
		VALUES (@factura_id, @fecha_abono, @monto, @forma_pago_id, @saldo - @monto);

		IF NOT EXISTS (SELECT 1 FROM CUOTAS WHERE cuenta_id = @cuenta_id AND estado_cuota <> 'Pagada')
		BEGIN
			UPDATE CUENTAS_POR_COBRAR SET estado_cuenta = 'Pagada' WHERE cuenta_id = @cuenta_id;

			UPDATE FACTURAS
			SET estado_pago = 'Pagada', fecha_pago = @fecha_abono, forma_pago_id = @forma_pago_id
			WHERE factura_id = @factura_id;
		END
		ELSE IF EXISTS (SELECT 1 FROM CUOTAS WHERE cuenta_id = @cuenta_id AND estado_cuota = 'Vencida')
			UPDATE CUENTAS_POR_COBRAR SET estado_cuenta = 'Vencida' WHERE cuenta_id = @cuenta_id;
		ELSE
			UPDATE CUENTAS_POR_COBRAR SET estado_cuenta = 'Vigente' WHERE cuenta_id = @cuenta_id;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'El abono se registro correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ListarAbonosFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.fecha_abono, a.monto, ISNULL(fp.nombreFormaPago, '') AS nombreFormaPago, a.saldo_despues
	FROM ABONOS a
	LEFT JOIN FORMAS_PAGOS fp ON fp.forma_pago_id = a.forma_pago_id
	WHERE a.factura_id = @factura_id
	ORDER BY a.abono_id;
END
GO

CREATE OR ALTER PROCEDURE sp_PagarFactura
	@factura_id INT,
	@forma_pago_id INT,
	@fecha_pago DATE = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @fecha_pago = ISNULL(@fecha_pago, CAST(GETDATE() AS DATE));

	DECLARE @tipo VARCHAR(10), @estado VARCHAR(50);

	SELECT @tipo = tipo_pago, @estado = estado_pago FROM FACTURAS WHERE factura_id = @factura_id;

	IF @tipo IS NULL
	BEGIN
		SET @mensaje = 'La factura no existe.';
		RETURN;
	END

	IF @tipo = 'Credito'
	BEGIN
		SET @mensaje = 'Las facturas a credito se pagan por cuotas.';
		RETURN;
	END

	IF @estado = 'Pagada'
	BEGIN
		SET @mensaje = 'La factura ya esta pagada.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM FORMAS_PAGOS WHERE forma_pago_id = @forma_pago_id)
	BEGIN
		SET @mensaje = 'La forma de pago no existe.';
		RETURN;
	END

	UPDATE FACTURAS
	SET estado_pago = 'Pagada',
		fecha_pago = @fecha_pago,
		forma_pago_id = @forma_pago_id
	WHERE factura_id = @factura_id;

	SET @resultado = 1;
	SET @mensaje = 'El pago se registro correctamente.';
END
GO

-- Cuentas por cobrar y cuotas

CREATE OR ALTER PROCEDURE sp_ListarCuotasPorFactura
	@factura_id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT q.cuota_id,
		   q.numero_cuota,
		   q.fecha_vencimiento,
		   q.monto_cuota,
		   ISNULL(q.monto_pagado, 0) AS monto_pagado,
		   q.monto_cuota - ISNULL(q.monto_pagado, 0) AS saldo,
		   q.fecha_pago,
		   fp.nombreFormaPago,
		   q.estado_cuota
	FROM CUOTAS q
	INNER JOIN CUENTAS_POR_COBRAR cc ON cc.cuenta_id = q.cuenta_id
	LEFT JOIN FORMAS_PAGOS fp ON fp.forma_pago_id = q.forma_pago_id
	WHERE cc.factura_id = @factura_id
	ORDER BY q.numero_cuota;
END
GO

CREATE OR ALTER PROCEDURE sp_ActualizarCuotasVencidas
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @hoy DATE = CAST(GETDATE() AS DATE);

	UPDATE CUOTAS
	SET estado_cuota = 'Vencida'
	WHERE estado_cuota IN ('Pendiente', 'Parcial')
	  AND fecha_vencimiento < @hoy;

	UPDATE CUENTAS_POR_COBRAR
	SET estado_cuenta = 'Vencida'
	WHERE estado_cuenta = 'Vigente'
	  AND EXISTS (SELECT 1 FROM CUOTAS q WHERE q.cuenta_id = CUENTAS_POR_COBRAR.cuenta_id
				  AND q.estado_cuota = 'Vencida');
END
GO

CREATE OR ALTER PROCEDURE sp_RegistrarPagoCuota
	@cuota_id INT,
	@monto MONEY,
	@forma_pago_id INT,
	@fecha_pago DATE = NULL,
	@resultado INT OUTPUT,
	@mensaje VARCHAR(100) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SET @resultado = 0;
	SET @mensaje = '';
	SET @fecha_pago = ISNULL(@fecha_pago, CAST(GETDATE() AS DATE));

	DECLARE @cuenta_id INT, @factura_id INT, @saldo MONEY;

	SELECT @cuenta_id = q.cuenta_id,
		   @factura_id = cc.factura_id,
		   @saldo = q.monto_cuota - ISNULL(q.monto_pagado, 0)
	FROM CUOTAS q
	INNER JOIN CUENTAS_POR_COBRAR cc ON cc.cuenta_id = q.cuenta_id
	WHERE q.cuota_id = @cuota_id;

	IF @cuenta_id IS NULL
	BEGIN
		SET @mensaje = 'La cuota no existe.';
		RETURN;
	END

	IF @saldo <= 0
	BEGIN
		SET @mensaje = 'La cuota ya esta pagada.';
		RETURN;
	END

	IF ISNULL(@monto, 0) <= 0
	BEGIN
		SET @mensaje = 'El monto debe ser mayor que 0.';
		RETURN;
	END

	IF @monto > @saldo
	BEGIN
		SET @mensaje = 'El monto supera el saldo de la cuota: ' + CAST(@saldo AS VARCHAR(20)) + '.';
		RETURN;
	END

	IF NOT EXISTS (SELECT 1 FROM FORMAS_PAGOS WHERE forma_pago_id = @forma_pago_id)
	BEGIN
		SET @mensaje = 'La forma de pago no existe.';
		RETURN;
	END

	BEGIN TRY
		BEGIN TRANSACTION;

		UPDATE CUOTAS
		SET monto_pagado = ISNULL(monto_pagado, 0) + @monto,
			fecha_pago = @fecha_pago,
			forma_pago_id = @forma_pago_id,
			estado_cuota = CASE WHEN ISNULL(monto_pagado, 0) + @monto >= monto_cuota THEN 'Pagada' ELSE 'Parcial' END
		WHERE cuota_id = @cuota_id;

		IF NOT EXISTS (SELECT 1 FROM CUOTAS WHERE cuenta_id = @cuenta_id AND estado_cuota <> 'Pagada')
		BEGIN
			UPDATE CUENTAS_POR_COBRAR SET estado_cuenta = 'Pagada' WHERE cuenta_id = @cuenta_id;

			UPDATE FACTURAS
			SET estado_pago = 'Pagada', fecha_pago = @fecha_pago, forma_pago_id = @forma_pago_id
			WHERE factura_id = @factura_id;
		END
		ELSE IF EXISTS (SELECT 1 FROM CUOTAS WHERE cuenta_id = @cuenta_id AND estado_cuota = 'Vencida')
			UPDATE CUENTAS_POR_COBRAR SET estado_cuenta = 'Vencida' WHERE cuenta_id = @cuenta_id;
		ELSE
			UPDATE CUENTAS_POR_COBRAR SET estado_cuenta = 'Vigente' WHERE cuenta_id = @cuenta_id;

		COMMIT TRANSACTION;

		SET @resultado = 1;
		SET @mensaje = 'El pago de la cuota se registro correctamente.';
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0
			ROLLBACK TRANSACTION;

		SET @mensaje = LEFT(ERROR_MESSAGE(), 100);
	END CATCH
END
GO

CREATE OR ALTER PROCEDURE sp_ListarCuentasPorCobrar
	@texto VARCHAR(50) = NULL,
	@estado_cuenta VARCHAR(20) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SET @texto = LTRIM(RTRIM(ISNULL(@texto, '')));
	SET @estado_cuenta = LTRIM(RTRIM(ISNULL(@estado_cuenta, '')));

	SELECT cc.cuenta_id,
		   cc.factura_id,
		   f.orden_id,
		   ISNULL(c.nombreEmpresa, o.identificacionCliente) AS cliente,
		   cc.monto_total,
		   ISNULL(cc.prima, 0) AS prima,
		   cc.numero_cuotas,
		   cc.frecuencia,
		   cc.estado_cuenta,
		   ISNULL((SELECT SUM(q.monto_pagado) FROM CUOTAS q WHERE q.cuenta_id = cc.cuenta_id), 0) AS pagado_cuotas,
		   (SELECT SUM(q.monto_cuota - ISNULL(q.monto_pagado, 0)) FROM CUOTAS q WHERE q.cuenta_id = cc.cuenta_id) AS saldo,
		   (SELECT MIN(q.fecha_vencimiento) FROM CUOTAS q
			WHERE q.cuenta_id = cc.cuenta_id AND q.estado_cuota <> 'Pagada') AS proximo_vencimiento
	FROM CUENTAS_POR_COBRAR cc
	INNER JOIN FACTURAS f ON f.factura_id = cc.factura_id
	INNER JOIN ORDENES o ON o.orden_id = f.orden_id
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	WHERE (@texto = '' OR f.orden_id LIKE '%' + @texto + '%'
		   OR o.identificacionCliente LIKE '%' + @texto + '%'
		   OR c.nombreEmpresa LIKE '%' + @texto + '%')
	  AND (@estado_cuenta = '' OR cc.estado_cuenta = @estado_cuenta)
	ORDER BY cc.cuenta_id DESC;
END
GO

-- Bitácora

CREATE OR ALTER PROCEDURE sp_ListarBitacora
	@desde DATE = NULL,
	@hasta DATE = NULL,
	@usuario_id INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT b.bitacora_id,
		   b.fechaBitacora,
		   u.usuario,
		   b.accion,
		   b.tabla,
		   b.equipoSistema
	FROM BITACORAS b
	LEFT JOIN USUARIOS u ON u.usuario_id = b.usuario_id
	WHERE (@desde IS NULL OR b.fechaBitacora >= @desde)
	  AND (@hasta IS NULL OR b.fechaBitacora < DATEADD(DAY, 1, @hasta))
	  AND (@usuario_id IS NULL OR b.usuario_id = @usuario_id)
	ORDER BY b.fechaBitacora DESC, b.bitacora_id DESC;
END
GO

-- Reportes

CREATE OR ALTER PROCEDURE sp_ReporteOrdenes
	@desde DATE,
	@hasta DATE,
	@estado_orden_id INT = NULL,
	@servicio_id INT = NULL,
	@cliente VARCHAR(50) = ''
AS
BEGIN
	SET NOCOUNT ON;

	SET @cliente = LTRIM(RTRIM(ISNULL(@cliente, '')));

	SELECT o.orden_id,
		   ISNULL(c.nombreEmpresa, o.identificacionCliente) AS cliente,
		   ISNULL(s.nombreServicio, 'Sin servicio') AS nombreServicio,
		   LTRIM(ISNULL(sup.nombreEmpleado, '') + ' ' + ISNULL(sup.apellidoEmpleado, '')) AS supervisor,
		   COALESCE(o.fecha_servicio, o.fecha_solicitud) AS fecha,
		   ISNULL(eo.nombreEstadoOrden, 'Sin estado') AS nombreEstadoOrden,
		   o.conformidad_cliente,
		   CASE WHEN o.conformidad_cliente IS NULL THEN 'Sin evaluar'
				WHEN o.conformidad_cliente = 1 THEN 'Conforme'
				ELSE 'No conforme' END AS conformidad
	FROM ORDENES o
	LEFT JOIN CLIENTES c ON c.identificacionCliente = o.identificacionCliente
	LEFT JOIN SERVICIOS s ON s.servicio_id = o.servicio_id
	LEFT JOIN EMPLEADOS sup ON sup.empleado_id = o.supervisor_id
	LEFT JOIN ESTADO_ORDENES eo ON eo.estado_orden_id = o.estado_orden_id
	WHERE COALESCE(o.fecha_servicio, o.fecha_solicitud) BETWEEN @desde AND @hasta
	  AND (@estado_orden_id IS NULL OR o.estado_orden_id = @estado_orden_id)
	  AND (@servicio_id IS NULL OR o.servicio_id = @servicio_id)
	  AND (@cliente = ''
		   OR c.nombreEmpresa LIKE '%' + @cliente + '%'
		   OR (c.nombreCliente + ' ' + c.apellidoCliente) LIKE '%' + @cliente + '%'
		   OR o.identificacionCliente LIKE '%' + @cliente + '%')
	ORDER BY COALESCE(o.fecha_servicio, o.fecha_solicitud), o.orden_id;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteAsistencia
	@desde DATE,
	@hasta DATE,
	@empleado_id INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT a.empleado_id,
		   LTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '')) AS empleado,
		   ISNULL(c.nombreCargo, 'Sin cargo') AS nombreCargo,
		   a.fechaAsistencia,
		   CONVERT(VARCHAR(5), a.hora_entrada, 108) AS hora_entrada,
		   CONVERT(VARCHAR(5), a.hora_salida, 108) AS hora_salida,
		   CASE WHEN a.hora_entrada IS NOT NULL AND a.hora_salida IS NOT NULL
				THEN CAST(DATEDIFF(MINUTE, a.hora_entrada, a.hora_salida) / 60.0 AS DECIMAL(6, 2))
				ELSE 0 END AS horas
	FROM ASISTENCIAS a
	INNER JOIN EMPLEADOS e ON e.empleado_id = a.empleado_id
	LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
	WHERE a.fechaAsistencia BETWEEN @desde AND @hasta
	  AND (@empleado_id IS NULL OR a.empleado_id = @empleado_id)
	ORDER BY a.fechaAsistencia, e.nombreEmpleado, e.apellidoEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteEmpleados
	@desde DATE,
	@hasta DATE,
	@cargo_id INT = NULL,
	@estado BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT e.empleado_id,
		   LTRIM(ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '')) AS nombreCompleto,
		   ISNULL(c.nombreCargo, 'Sin cargo') AS nombreCargo,
		   CASE WHEN e.estadoEmpleado = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado,
		   (SELECT COUNT(*)
			FROM ORDEN_EMPLEADOS oe
			INNER JOIN ORDENES o ON o.orden_id = oe.orden_id
			WHERE oe.empleado_id = e.empleado_id
			  AND o.fecha_servicio BETWEEN @desde AND @hasta) AS ordenes,
		   (SELECT COUNT(*)
			FROM ASISTENCIAS a
			WHERE a.empleado_id = e.empleado_id
			  AND a.fechaAsistencia BETWEEN @desde AND @hasta) AS dias,
		   (SELECT CAST(ISNULL(SUM(CASE WHEN a.hora_entrada IS NOT NULL AND a.hora_salida IS NOT NULL
										 THEN DATEDIFF(MINUTE, a.hora_entrada, a.hora_salida)
										 ELSE 0 END), 0) / 60.0 AS DECIMAL(10, 2))
			FROM ASISTENCIAS a
			WHERE a.empleado_id = e.empleado_id
			  AND a.fechaAsistencia BETWEEN @desde AND @hasta) AS horas
	FROM EMPLEADOS e
	LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
	WHERE (@cargo_id IS NULL OR e.cargo_id = @cargo_id)
	  AND (@estado IS NULL OR e.estadoEmpleado = @estado)
	ORDER BY e.nombreEmpleado, e.apellidoEmpleado;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteFacturacion
	@desde DATE,
	@hasta DATE
AS
BEGIN
	SET NOCOUNT ON;

	SELECT f.tipo_pago,
		   f.estado_pago,
		   COUNT(1) AS facturas,
		   ISNULL(SUM(f.precio_facturado), 0) AS total
	FROM FACTURAS f
	WHERE f.fechaFactura BETWEEN @desde AND @hasta
	GROUP BY f.tipo_pago, f.estado_pago
	ORDER BY f.tipo_pago, f.estado_pago;
END
GO

CREATE OR ALTER PROCEDURE sp_ReporteInventarioBajo
	@minimo INT = 5
AS
BEGIN
	SET NOCOUNT ON;

	SELECT producto_id, nombreProducto, tipoProducto, ISNULL(stock, 0) AS stock
	FROM PRODUCTOS
	WHERE ISNULL(stock, 0) <= @minimo
	ORDER BY stock, nombreProducto;
END
GO

-- Bitácora automática (triggers)
-- Cada INSERT, UPDATE o DELETE sobre estas tablas deja un registro en BITACORAS.
-- El usuario sale de SESSION_CONTEXT('usuario_id'), que la aplicación escribe al abrir cada conexión
-- (ver Conexion.cs). Si el cambio se hace directo en SSMS, usuario_id queda en NULL.
-- Se guarda una fila por instrucción, no una por fila afectada.

CREATE OR ALTER TRIGGER trg_bitacora_CLIENTES
ON CLIENTES
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'CLIENTES',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_LOCACIONES
ON LOCACIONES
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'LOCACIONES',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_EMPLEADOS
ON EMPLEADOS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'EMPLEADOS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_USUARIOS
ON USUARIOS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'USUARIOS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_VEHICULOS
ON VEHICULOS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'VEHICULOS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_ORDENES
ON ORDENES
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'ORDENES',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_FACTURAS
ON FACTURAS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'FACTURAS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_CUENTAS_POR_COBRAR
ON CUENTAS_POR_COBRAR
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'CUENTAS_POR_COBRAR',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_CUOTAS
ON CUOTAS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'CUOTAS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_PRODUCTOS
ON PRODUCTOS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'PRODUCTOS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_EQUIPOS
ON EQUIPOS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'EQUIPOS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_PROVEEDORES
ON PROVEEDORES
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'PROVEEDORES',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

CREATE OR ALTER TRIGGER trg_bitacora_COMPRAS
ON COMPRAS
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM inserted) AND NOT EXISTS (SELECT 1 FROM deleted)
		RETURN;

	INSERT INTO BITACORAS (usuario_id, accion, tabla, equipoSistema, fechaBitacora)
	VALUES (TRY_CAST(SESSION_CONTEXT(N'usuario_id') AS INT),
			CASE WHEN EXISTS (SELECT 1 FROM inserted) AND EXISTS (SELECT 1 FROM deleted) THEN 'UPDATE'
				 WHEN EXISTS (SELECT 1 FROM inserted) THEN 'INSERT'
				 ELSE 'DELETE' END,
			'COMPRAS',
			LEFT(HOST_NAME(), 50),
			GETDATE());
END
GO

SET NOEXEC OFF;
GO
