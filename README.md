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

Ejecutar `BaseDeDatos/Codelex_base_completa.sql` en SQL Server. Sirve para crear la base desde cero y también para actualizar una base que ya existe: se puede ejecutar varias veces sin duplicar nada.

## Ejecutar

1. Abrir `Codelex.slnx` en Visual Studio.
2. En `Codelex/Clases_BDD/Conexion.cs`, poner el nombre del servidor de SQL Server.
3. Presionar **F5**.
