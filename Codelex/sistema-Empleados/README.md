# Módulo de empleados

## Propósito

Registrar y administrar empleados (cargo, que define el rol de su usuario; identidad de 13 dígitos sin guiones, contacto y estado), ver el reporte de asistencia y órdenes de cada empleado, y la pantalla "Mis órdenes" para el empleado que inició sesión.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `empleados.xaml(.cs)` | Formulario y tabla de empleados. |
| `empleado-reporte.xaml(.cs)` | Reporte de un empleado: asistencia y órdenes por rango de fechas, con exportación a CSV. |
| `mis-ordenes.xaml(.cs)` | Órdenes asignadas al empleado que inició sesión. |

## Dependencias

- Clases `Empleado` y `MiTrabajoBD` de `sistema-AccesoDatos`.
- Procedimientos: `sp_BuscarEmpleados`, `sp_ObtenerEmpleado`, `sp_InsertarEmpleado`, `sp_ActualizarEmpleado`, `sp_CambiarEstadoEmpleado`, `sp_ListarCargos`, `sp_AsistenciasPorEmpleado`, `sp_OrdenesPorEmpleado`, `sp_MisOrdenes`, `sp_DetalleMiOrden`, `sp_ListasMiOrden`.
- Tablas: `empleados`, `cargos`, `asistencias`, `ordenes`, `orden_empleados`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Empleados** (y **Mi perfil** para "Mis órdenes").
