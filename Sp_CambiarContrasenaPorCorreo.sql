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