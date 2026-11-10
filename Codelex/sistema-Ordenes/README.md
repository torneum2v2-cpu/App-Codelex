# Módulo de órdenes

## Propósito

Crear órdenes de limpieza a partir de una cotización (personal, vehículo, fechas y turno), ver y administrar el estado de las órdenes, y facturar las completadas. Los productos se descuentan del inventario, los equipos se asignan solos y los empleados en una orden "En proceso" no se pueden asignar a otra.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `ingresar-ordenes.xaml(.cs)` | Asistente de 3 pasos para crear una orden. |
| `gestion-ordenes.xaml(.cs)` | Tabla de órdenes, detalle, cambio de estado, factura y conformidad. |
| `seleccionar-cotizacion.xaml(.cs)` | Cuadro para elegir (o crear) la cotización de la orden. |

## Dependencias

- Clases `Orden`, `DatosOrden`, `GestionOrdenesBD`, `Cliente` y `Vehiculo` de `sistema-AccesoDatos`.
- Procedimientos: `sp_ListarCotizacionesOrden`, `sp_DetalleCotizacion`, `sp_ResumenCotizacion`, `sp_ListarEmpleadosOrden`, `sp_InsertarOrden`, `sp_ListarOrdenesGestion`, `sp_DetalleOrdenGestion`, `sp_ListasOrdenGestion`, `sp_CambiarEstadoOrden`.
- Tablas: `ordenes`, `orden_empleados`, `orden_productos`, `orden_equipos`, `orden_actividades`, `estados_orden`.
- Módulos: `sistema-FacturasCotizaciones`, `sistema-Clientes`, `sistema-Vehiculos`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Órdenes → Ingresar orden** o **Gestión de órdenes**.
