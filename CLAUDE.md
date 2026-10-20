# Reglas del proyecto Codelex

Proyecto WPF (.NET 10) con SQL Server. La solución es `Codelex.slnx` y el proyecto está en `Codelex/`.

## Base de datos
- Usar **solo procedimientos almacenados**. Nada de consultas SQL escritas en el código (`SELECT`, `INSERT`, `UPDATE`, `DELETE`).
- Si falta un procedimiento, crearlo en un script `.sql` dentro de `BaseDeDatos/` y avisar para que lo ejecuten en la base.
- El acceso a la base va en las clases de `Codelex/Clases_BDD/` (por ejemplo `Cliente.cs`), usando `Conexion.ObtenerConexion()`.

## Validaciones
- **Las validaciones van en las clases** de `Clases_BDD`, no en los formularios. Por ejemplo, `InsertarCliente` y `ActualizarCliente` validan antes de llamar al procedimiento y devuelven el mensaje de error.
- En el formulario solo queda lo que se usa una sola vez en esa pantalla (llenar o limpiar los campos, habilitar botones, abrir otra ventana, bloquear teclas en un `TextBox`).

## Estilo del código
- Código sencillo y fácil de entender: `if/else`, `foreach`, `using`, `AddWithValue`, `SqlDataAdapter`. Evitar LINQ, `var`, `=>`, `switch` de expresión, `$"..."` y cosas avanzadas si no hacen falta.
- No repetir código: si algo se repite, pasarlo a un método.
- Sin comentarios en el código.
