# Módulo de locaciones

## Propósito

Registrar las locaciones (lugares de servicio) de un cliente: nombre, tipo y dirección.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `ingresar-locacion.xaml(.cs)` | Formulario para agregar una locación a un cliente. |

## Dependencias

- Clases `Cliente` y `Locacion` de `sistema-AccesoDatos`.
- Procedimientos: `sp_InsertarLocacion`, `sp_ListarLocacionesPorCliente`.
- Tablas: `locaciones`, `clientes`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: **Clientes → Locaciones**.
