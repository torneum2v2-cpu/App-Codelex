# Pruebas automáticas

## Propósito

Comprobar las reglas de validación y de cálculo de Codelex sin abrir la aplicación ni la base de datos: identidad y RTN de Honduras, teléfono, correo, nombres, placa, contraseña, descuento, abonos, ajustes de inventario y permisos por rol.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `Codelex.Pruebas.csproj` | Proyecto de pruebas (xUnit). Compila solo las clases de `sistema-AccesoDatos` que no usan pantallas. |
| `pruebas-validacion.cs` | Pruebas de la clase `Validacion` (casos válidos, inválidos y límite). |
| `pruebas-negocio.cs` | Pruebas de cálculos y validaciones de facturación, inventario, clientes y usuarios. |

## Dependencias

- .NET 10 y los paquetes NuGet xUnit, `Microsoft.Data.SqlClient` y `DSInternals.Win32.WebAuthn` (se restauran solos).
- No necesita SQL Server.

## Cómo ejecutarlo

1. Desde la raíz del repositorio: `dotnet test Codelex.Pruebas/Codelex.Pruebas.csproj`.
2. Cada vez que se agregue una regla nueva en `sistema-AccesoDatos`, agregar su prueba aquí.
