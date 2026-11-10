# Módulo de inicio de sesión

## Propósito

Iniciar sesión con usuario y contraseña, confirmar con el teléfono (llave de acceso o passkey), recordar cuentas en la computadora y recuperar la contraseña con un código enviado por correo.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `ventana-principal.xaml(.cs)` | (en `Codelex/`) Pantalla de inicio de sesión. |
| `verificar-telefono.xaml(.cs)` | Verificación con el teléfono (Face ID, huella o reconocimiento facial). |
| `seleccionar-cuenta.xaml(.cs)` | Elegir una cuenta recordada. |
| `ingresar-correo.xaml(.cs)` | Pedir el correo para recuperar la contraseña. |
| `ingresar-codigo.xaml(.cs)` | Escribir el código de 6 dígitos recibido. |
| `nueva-contra.xaml(.cs)` | Escribir la contraseña nueva. |

## Dependencias

- Clases `Usuario`, `Empleado`, `CuentasRecordadas` y `Sesion` de `sistema-AccesoDatos`.
- Paquetes NuGet: `DSInternals.Win32.WebAuthn` (passkeys) y `System.Security.Cryptography.ProtectedData` (cuentas recordadas).
- Procedimientos: `sp_ObtenerUsuarioLogin`, `sp_GuardarPasskey`, `sp_EliminarPasskey`, `sp_BuscarUsuarioRecuperacion`, `sp_CambiarContrasenaPorCorreo`, `sp_ExisteCorreoEmpleado`.
- Windows 11 con Bluetooth para registrar el teléfono la primera vez.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: es la primera ventana al abrir el programa.
