# Módulo de reportes

## Propósito

Reportes de órdenes, asistencia y empleados con filtros, gráficos y exportación a CSV.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `reportes.xaml(.cs)` | Pantalla con filtros, indicadores, gráficos y tabla del reporte elegido. |

## Dependencias

- Clases `ReporteBD` y `Empleado` de `sistema-AccesoDatos`.
- Procedimientos: `sp_ReporteOrdenes`, `sp_ReporteAsistencia`, `sp_ReporteEmpleados`, `sp_ListarEstadosOrden`, `sp_ListarServicios`, `sp_ListarCargos`.
- Tablas: `ordenes`, `asistencias`, `empleados`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Reportes**.
