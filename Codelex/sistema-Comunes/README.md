# Módulo de elementos comunes

## Propósito

Ventanas y utilidades que usan todos los módulos: cuadro de mensajes y de confirmación con el diseño de Codelex, impresión de documentos, ventana de carga y manejo de la foto de perfil.

## Archivos

| Archivo | Para qué sirve |
|---|---|
| `mensaje-codelex.xaml(.cs)` | Cuadro de mensaje de éxito o error, y de pregunta Sí/No (`preguntar`), con el estilo de la aplicación. |
| `impresion-documento.cs` | Arma facturas y cotizaciones para imprimir; con "Microsoft Print to PDF" se guardan como PDF. |
| `cargando.xaml(.cs)` | Ventana de carga que se muestra mientras se abre otra pantalla. |
| `foto-perfil.cs` | Convierte la foto de perfil en imagen y la reduce para guardarla. |

## Dependencias

- Ninguna base de datos propia; la foto se guarda con `FotoPerfilBD` de `sistema-AccesoDatos`.

## Cómo ejecutarlo

1. Tener SQL Server y crear la base ejecutando `sistema-BaseDatos/codelex-base-completa.sql` (se puede repetir sin problema).
2. Poner el nombre del servidor en `Codelex/sistema-AccesoDatos/conexion.cs`.
3. Desde la raíz del repositorio: `dotnet run --project Codelex/Codelex.csproj` (Windows, .NET 10).
4. Iniciar sesión y entrar por el menú: no tiene una entrada propia; se muestra desde los demás módulos.
