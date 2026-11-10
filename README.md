# Codelex

Sistema de escritorio de Codelex (WPF, .NET 10 y SQL Server): clientes, empleados, órdenes de limpieza, cotizaciones con costeo por actividad, facturación con cuotas e inventario.

## Módulos

Cada módulo tiene su propio `README.md` con su propósito, dependencias y forma de ejecutarlo.

| Carpeta | Contenido |
|---|---|
| `Codelex/sistema-Login` | Inicio de sesión, verificación con el teléfono (passkeys) y recuperación de contraseña |
| `Codelex/sistema-Clientes` | Clientes (RTN o identidad) y detalle del cliente |
| `Codelex/sistema-Locaciones` | Locaciones de los clientes |
| `Codelex/sistema-Empleados` | Empleados, reporte de asistencia y órdenes, y "Mis órdenes" |
| `Codelex/sistema-Usuarios` | Usuarios del sistema |
| `Codelex/sistema-Vehiculos` | Registro y gestión de vehículos |
| `Codelex/sistema-Ordenes` | Ingreso y gestión de órdenes |
| `Codelex/sistema-FacturasCotizaciones` | Cotizaciones, facturas, cuotas y abonos |
| `Codelex/sistema-Inventario` | Productos, equipos, proveedores y compras |
| `Codelex/sistema-Reportes` | Reportes de órdenes, asistencia y empleados |
| `Codelex/sistema-Comunes` | Ventanas de mensaje y de carga, foto de perfil |
| `Codelex/sistema-AccesoDatos` | Acceso a la base de datos (solo procedimientos almacenados) |
| `sistema-BaseDatos` | Scripts de SQL Server |

## Base de datos

| Archivo | Contenido | Cuándo usarlo |
|---|---|---|
| `sistema-BaseDatos/codelex-tablas.sql` | Creación de la base, las tablas, los tipos de tabla y los datos iniciales | Para crear la base desde cero |
| `sistema-BaseDatos/codelex-procedimientos.sql` | Solo los procedimientos almacenados | Después de `codelex-tablas.sql`, o cada vez que cambie un procedimiento |
| `sistema-BaseDatos/codelex-base-completa.sql` | Todo junto: tablas, cambios para bases de versiones anteriores y procedimientos | Para crear la base o para poner al día una base que ya existe |

Los tres se pueden ejecutar varias veces sin errores y sin duplicar datos. Cada cambio en la base se agrega en los tres archivos.

## Estándares de nombres

| Elemento | Regla | Ejemplo |
|---|---|---|
| Variables, funciones y métodos | camelCase en español | `obtenerEmpleadoDisponible`, `fechaOrden` |
| Manejadores de eventos | `nombreEvento` | `guardarClic`, `ventanaCargada` |
| Clases y ventanas | PascalCase | `OrdenLimpieza`, `GestionOrdenes` |
| Constantes | MAYÚSCULAS con guion bajo | `TASA_ISV`, `MAX_INTENTOS_HUELLA` |
| Tablas | plural, minúscula | `empleados`, `detalles_factura` |
| Campos | singular, minúscula con guion bajo | `fecha_registro`, `nombre_cliente` |
| Carpetas | sin acentos ni espacios | `sistema-Clientes`, `sistema-Ordenes` |
| Archivos | minúscula, con guiones | `gestion-ordenes.xaml`, `cliente-detalle.xaml.cs` |

Las propiedades públicas que usa el enlace de datos de WPF (`{Binding ...}`) conservan PascalCase, como pide el propio .NET. `App.xaml`, `AssemblyInfo.cs`, `Codelex.csproj` y `Codelex.slnx` conservan el nombre que exige la plantilla de .NET.

Cada método lleva un comentario breve con su propósito, parámetros y valor de retorno.

## Verificación con el teléfono

Después de la contraseña, Codelex pide confirmar con el teléfono (iPhone o Android) usando una llave de acceso (passkey). El teléfono siempre pide Face ID, huella o reconocimiento facial.

- La computadora necesita Windows 11 con Bluetooth encendido, y el teléfono Bluetooth e internet.
- La primera vez cada usuario registra su teléfono escaneando el código QR que muestra Windows.
- Si alguien pierde el teléfono: `EXEC sp_EliminarPasskey 'usuario', NULL, NULL;` y la próxima vez registra uno nuevo.

## Borradores

| Archivo | Contenido |
|---|---|
| `borradores/codelex-google-authenticator.zip` | Versión con verificación en dos pasos por código de 6 dígitos (Google Authenticator). También está en la rama `borrador-google-authenticator`. |

## Ejecutar

1. Abrir `Codelex.slnx` en Visual Studio.
2. En `Codelex/sistema-AccesoDatos/conexion.cs`, poner el nombre del servidor de SQL Server.
3. Ejecutar `sistema-BaseDatos/codelex-base-completa.sql` en SQL Server.
4. Presionar **F5**.
