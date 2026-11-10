# Módulo de elementos comunes

## Propósito

Ventanas y utilidades que usan todos los módulos: cuadro de mensajes con el diseño de Codelex, ventana de carga y manejo de la foto de perfil.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `mensaje-codelex.xaml(.cs)` | Cuadro de mensaje de éxito o error con el estilo de la aplicación. |
| `cargando.xaml(.cs)` | Ventana de carga que se muestra mientras se abre otra pantalla. |
| `foto-perfil.cs` | Convierte la foto de perfil en imagen y la reduce para guardarla. |

## Dependencias

- Ninguna base de datos propia; la foto se guarda con `FotoPerfilBD` de `sistema-AccesoDatos`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: no tiene una entrada propia; se muestra desde los demás módulos.
