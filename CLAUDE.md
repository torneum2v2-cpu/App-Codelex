# Reglas del proyecto Codelex

Proyecto WPF (.NET 10) con SQL Server. La solución es `Codelex.slnx` y el proyecto está en `Codelex/`.

## Base de datos
- Usar **solo procedimientos almacenados**. Nada de consultas SQL escritas en el código (`SELECT`, `INSERT`, `UPDATE`, `DELETE`).
- Si falta un procedimiento, crearlo en un script `.sql` dentro de `BaseDeDatos/` y avisar para que lo ejecuten en la base.
- El acceso a la base va en las clases de `Codelex/Clases_BDD/` (por ejemplo `Cliente.cs`), usando `Conexion.ObtenerConexion()`.
- Orden de los scripts: `CodelexBD2.sql`, `procedimientos_avance_semana_1.sql`, `BaseDeDatos/cambios_base_de_datos.sql`. Los cambios nuevos se agregan al final de `cambios_base_de_datos.sql`, de forma que se pueda volver a ejecutar sin duplicar nada.
- La llave de `CLIENTES` es `identificacionCliente` (RTN). Las locaciones y las órdenes apuntan al cliente con `identificacionCliente`; ya no existe `cliente_id` ni `CLIENTES.locacion_id`.

## Diseño
- Todas las pantallas usan el mismo diseño del login y del menú: ventana sin bordes con esquinas redondeadas, encabezado o panel con el degradado verde oscuro (`BgGradient`), fondo `#F4F8F8`, tarjetas blancas con borde `#DDEAE8`, botones con el degradado `#1FAFA5` → `#2CC5B4` y fuente Segoe UI.

## Validaciones
- **Las validaciones van en las clases** de `Clases_BDD`, no en los formularios. Por ejemplo, `InsertarCliente` y `ActualizarCliente` validan antes de llamar al procedimiento y devuelven el mensaje de error.
- En el formulario solo queda lo que se usa una sola vez en esa pantalla (llenar o limpiar los campos, habilitar botones, abrir otra ventana, bloquear teclas en un `TextBox`).

## Estilo del código
- Código sencillo y fácil de entender: `if/else`, `foreach`, `using`, `AddWithValue`, `SqlDataAdapter`. Evitar LINQ, `var`, `=>`, `switch` de expresión, `$"..."` y cosas avanzadas si no hacen falta.
- No repetir código: si algo se repite, pasarlo a un método.
- Sin comentarios en el código.
