# Módulo de inventario

## Propósito

Consultar productos y equipos, registrar compras (ingresos) con su proveedor, ajustar existencias (mermas, pérdidas y conteo físico) y crear productos, equipos y proveedores nuevos. Los precios de compra alimentan el costo de las cotizaciones.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `inventario.xaml(.cs)` | Consulta de productos y equipos con búsqueda. |
| `agregar-inventario.xaml(.cs)` | Ingreso de una compra con productos y equipos. |
| `ajustar-inventario.xaml(.cs)` | Ajuste de existencias de un producto con motivo obligatorio; muestra el historial de ajustes. |
| `nuevo-producto-equipo.xaml(.cs)` | Alta de un producto o equipo nuevo; obliga a elegir al menos una actividad donde se usa. |
| `nuevo-proveedor.xaml(.cs)` | Alta de un proveedor nuevo. |

## Dependencias

- Clase `InventarioBD` de `sistema-AccesoDatos`.
- Procedimientos: `sp_BuscarInventario`, `sp_ListarProductosInventario`, `sp_ListarEquiposInventario`, `sp_ListarProveedores`, `sp_InsertarProductoConActividades`, `sp_VincularEquipoActividades`, `sp_InsertarProveedor`, `sp_RegistrarIngresoInventario`, `sp_AjustarInventario`, `sp_ListarAjustesInventario`.
- Tablas: `productos`, `equipos`, `proveedores`, `compras`, `compras_productos`, `compras_equipos`, `ajustes_inventario`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Inventario**.
