USE Codelex
GO

/* =====================================================================
   Procedimientos para el login y la recuperación de contraseña.
   Ejecutar después de procedimientos_avance_semana_1.sql.
   Se puede volver a ejecutar sin problema.
   ===================================================================== */

-- Revisa si existe un empleado con ese correo (pantalla Ingresar correo).
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
