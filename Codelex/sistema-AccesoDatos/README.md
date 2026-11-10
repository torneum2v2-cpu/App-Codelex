# Módulo de acceso a datos

## Propósito

Todas las llamadas a SQL Server. Las pantallas no escriben consultas: solo llaman a procedimientos almacenados a través de estas clases, que también concentran las validaciones.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `conexion.cs` | Abre la conexión con SQL Server y envía el usuario de la sesión para la bitácora. |
| `base-datos.cs` | Clase base: crea comandos de procedimiento, consulta y ejecuta. |
| `sesion.cs` | Datos del usuario que inició sesión y su nivel de acceso. |
| `cliente.cs, empleado.cs, vehiculo.cs, locacion.cs` | Clientes, empleados, vehículos y locaciones. |
| `datos-orden.cs, gestion-ordenes-bd.cs, mi-trabajo-bd.cs` | Órdenes, gestión de órdenes y órdenes del empleado. |
| `facturacion.cs` | Cotizaciones, facturas, cuotas y abonos. |
| `inventario-bd.cs, reportes-bd.cs` | Inventario y reportes. |
| `usuario.cs, usuario-bd.cs, foto-perfil-bd.cs, cuentas-recordadas.cs` | Inicio de sesión, usuarios, foto de perfil y cuentas recordadas. |

## Dependencias

- Paquete NuGet `Microsoft.Data.SqlClient`.
- Base de datos creada con `sistema-BaseDatos/codelex-base-completa.sql`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: no se abre desde el menú; lo usan todos los módulos.
