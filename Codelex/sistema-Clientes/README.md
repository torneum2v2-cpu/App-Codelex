# Módulo de clientes

## Propósito

Registrar, buscar, editar y activar o desactivar clientes. Cada cliente tiene un RTN (14 dígitos) o una identidad (13 dígitos), siempre sin guiones, y el número se puede corregir. También muestra el detalle de un cliente con sus locaciones, órdenes y facturas.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `clientes.xaml(.cs)` | Pantalla principal: formulario, filtros y tabla de clientes. |
| `cliente-detalle.xaml(.cs)` | Detalle de un cliente con sus locaciones, órdenes y facturas. |
| `elegir-cliente.xaml(.cs)` | Cuadro de búsqueda (lupa) que usan la cotización y otras pantallas para elegir un cliente. |

## Dependencias

- Clase `Cliente` de `sistema-AccesoDatos` (validaciones y llamadas a la base).
- Procedimientos: `sp_BuscarClientes`, `sp_ObtenerCliente`, `sp_InsertarCliente`, `sp_ActualizarCliente`, `sp_CambiarEstadoCliente`, `sp_ListarLocacionesPorCliente`, `sp_ListarOrdenesPorCliente`, `sp_ListarFacturasPorCliente`.
- Tablas: `clientes`, `locaciones`, `ordenes`, `facturas`.
- Módulos: `sistema-Comunes` (mensajes), `sistema-Locaciones`, `sistema-Ordenes`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Clientes**.
