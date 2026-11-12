# Módulo de vehículos

## Propósito

Registrar vehículos (placa, marca y modelo) y administrarlos: buscar, editar y activar o desactivar (no se desactiva un vehículo que está en una orden en proceso).

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `registrar-vehiculo.xaml(.cs)` | Alta de un vehículo nuevo. |
| `gestion-vehiculos.xaml(.cs)` | Tabla de vehículos con edición y cambio de estado. |

## Dependencias

- Clase `Vehiculo` de `sistema-AccesoDatos`.
- Procedimientos: `sp_InsertarVehiculo`, `sp_ActualizarVehiculo`, `sp_BuscarVehiculos`, `sp_ObtenerVehiculo`, `sp_CambiarEstadoVehiculo`.
- Tabla: `vehiculos`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Vehículos**.
