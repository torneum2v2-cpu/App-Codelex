# Codelex

Sistema de escritorio de Codelex (WPF, .NET 10 y SQL Server).

## Módulos

| Carpeta | Contenido |
|---|---|
| `Codelex/Login` | Recuperación de contraseña (correo, código y nueva contraseña) |
| `Codelex/Clientes` | Clientes y detalle del cliente |
| `Codelex/Empleados` | Empleados y reporte de asistencia y órdenes |
| `Codelex/Inventario` | Consulta de inventario, ingreso de compras, productos, equipos y proveedores |
| `Codelex/Órdenes` | Ingreso de órdenes |
| `Codelex/Usuarios` | Usuarios del sistema |
| `Codelex/Vehículos` | Registro y gestión de vehículos |
| `Codelex/Comunes` | Ventanas de mensaje y de carga |
| `Codelex/Clases_BDD` | Acceso a la base de datos (solo procedimientos almacenados) |

## Base de datos

| Archivo | Contenido | Cuándo usarlo |
|---|---|---|
| `BaseDeDatos/Codelex_tablas.sql` | Creación de la base, las tablas, los tipos de tabla y los datos iniciales | Para crear la base desde cero |
| `BaseDeDatos/Codelex_procedimientos.sql` | Solo los procedimientos almacenados | Después de `Codelex_tablas.sql`, o cada vez que cambie un procedimiento |
| `BaseDeDatos/Codelex_base_completa.sql` | Todo junto: tablas, cambios para bases de versiones anteriores y procedimientos | Para crear la base o para poner al día una base que ya existe |

Los tres se pueden ejecutar varias veces sin errores y sin duplicar datos. Cada cambio en la base se agrega en los tres archivos.

## Ejecutar

1. Abrir `Codelex.slnx` en Visual Studio.
2. En `Codelex/Clases_BDD/Conexion.cs`, poner el nombre del servidor de SQL Server.
3. Presionar **F5**.
