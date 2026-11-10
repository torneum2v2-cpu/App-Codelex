# Módulo de usuarios

## Propósito

Crear y administrar las cuentas de acceso: elegir un empleado (con la lupa), definir usuario, contraseña y rol, y activar o desactivar la cuenta.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `usuarios.xaml(.cs)` | Formulario y tabla de usuarios. |
| `elegir-empleado.xaml(.cs)` | Cuadro de búsqueda (lupa) para elegir un empleado sin usuario. |

## Dependencias

- Clase `UsuarioBD` de `sistema-AccesoDatos`.
- Procedimientos: `sp_BuscarUsuarios`, `sp_BuscarEmpleadosSinUsuario`, `sp_InsertarUsuario`, `sp_ActualizarUsuario`, `sp_CambiarEstadoUsuario`, `sp_EliminarUsuario`, `sp_ExisteUsuario`, `sp_ListarRoles`.
- Tablas: `usuarios`, `roles`, `empleados`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Usuarios**.
