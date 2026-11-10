# Módulo de servicios

## Propósito

Crear y gestionar los servicios de Codelex y las actividades de cada uno. Una actividad define cómo se mide (unidad y rendimiento), cuántas personas trabajan según la medida (rangos), qué productos gasta (mínimo uno) y qué equipos usa (mínimo uno). Con eso se calcula el costo de las cotizaciones.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `servicios.xaml(.cs)` | Lista de servicios (crear, renombrar, eliminar) y sus actividades (nueva, editar, eliminar). |
| `actividad-editar.xaml(.cs)` | Alta y edición de una actividad con sus rangos de personas, productos y equipos. Desde aquí se pueden crear productos y equipos nuevos, que se guardan junto con la actividad. |

Los productos y equipos nuevos que se crean en Inventario (`nuevo-producto-equipo`) obligan a elegir al menos una actividad, para que nunca queden sin ligar.

## Dependencias

- Clase `ServicioBD` de `sistema-AccesoDatos` (donde están las validaciones).
- Procedimientos: `sp_ListarServiciosGestion`, `sp_GuardarServicio`, `sp_EliminarServicio`, `sp_ListarActividadesGestion`, `sp_GuardarActividad`, `sp_EliminarActividad`, `sp_ListarRangosActividad`, `sp_ListarProductosActividad`, `sp_ListarEquiposActividad`, `sp_ListarProductosCatalogo`, `sp_ListarEquiposCatalogo`, `sp_InsertarProductoConActividades`, `sp_VincularEquipoActividades`.
- Tablas: `servicios`, `actividades`, `actividad_rangos`, `actividad_productos`, `actividad_equipos`, `productos`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Servicios**.
