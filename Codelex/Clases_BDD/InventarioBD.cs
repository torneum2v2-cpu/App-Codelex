using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Codelex.Clases_BDD
{
    class InventarioBD : BaseDatos
    {
        public DataTable buscarInventario(string texto, string tipo)
        {
            SqlCommand comando = procedimiento("sp_BuscarInventario");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            comando.Parameters.AddWithValue("@tipo", tipo);
            return consultar(comando);
        }

        public DataTable listarProductos(string texto)
        {
            SqlCommand comando = procedimiento("sp_ListarProductosInventario");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            return consultar(comando);
        }

        public DataTable listarEquipos(string texto)
        {
            SqlCommand comando = procedimiento("sp_ListarEquiposInventario");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            return consultar(comando);
        }

        public DataTable listarProveedores()
        {
            return consultar(procedimiento("sp_ListarProveedores"));
        }

        public DataTable listarEmpleados()
        {
            return consultar(procedimiento("sp_ListarEmpleadosInventario"));
        }

        public bool insertarProveedor(string nombre, string telefono, string correo, out string mensaje, out int proveedorId)
        {
            proveedorId = 0;

            mensaje = validarProveedor(nombre, telefono, correo);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_InsertarProveedor");
            comando.Parameters.AddWithValue("@nombre_proveedor", nombre.Trim());
            comando.Parameters.AddWithValue("@telefono_proveedor", telefono.Trim());
            comando.Parameters.AddWithValue("@correo_proveedor", correo.Trim());

            SqlParameter id = new SqlParameter("@proveedor_id", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = ejecutar(comando, out mensaje);
            if (guardado)
                proveedorId = Convert.ToInt32(id.Value);

            return guardado;
        }

        public bool insertarProducto(string nombre, string tipo, out string mensaje, out int productoId)
        {
            productoId = 0;

            mensaje = validarProducto(nombre, tipo);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_InsertarProducto");
            comando.Parameters.AddWithValue("@nombre_producto", nombre.Trim());
            comando.Parameters.AddWithValue("@tipo_producto", tipo.Trim());

            SqlParameter id = new SqlParameter("@producto_id", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = ejecutar(comando, out mensaje);
            if (guardado)
                productoId = Convert.ToInt32(id.Value);

            return guardado;
        }

        public string validarProducto(string nombre, string tipo)
        {
            nombre = nombre.Trim();
            tipo = tipo.Trim();

            if (nombre == "")
                return "El nombre del producto es obligatorio.";
            if (nombre.Length > 50)
                return "El nombre no puede tener más de 50 caracteres.";
            if (tipo == "")
                return "El tipo de producto es obligatorio.";
            if (tipo.Length > 20)
                return "El tipo de producto no puede tener más de 20 caracteres.";

            return "";
        }

        public string validarEquipo(string nombre, string tipo, string marca, string modelo)
        {
            nombre = nombre.Trim();
            tipo = tipo.Trim();
            marca = marca.Trim();
            modelo = modelo.Trim();

            if (nombre == "")
                return "El nombre del equipo es obligatorio.";
            if (tipo == "")
                return "El tipo de equipo es obligatorio.";
            if (marca == "")
                return "La marca es obligatoria.";
            if (modelo == "")
                return "El modelo es obligatorio.";
            if (nombre.Length > 50 || tipo.Length > 50 || marca.Length > 50 || modelo.Length > 50)
                return "Los datos del equipo no pueden tener más de 50 caracteres.";

            return "";
        }

        public bool soloNumeros(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        public bool caracteresDePrecio(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c) && c != '.' && c != ',')
                    return false;
            }

            return true;
        }

        public DataTable crearLista()
        {
            DataTable lista = new DataTable();
            lista.Columns.Add("tipo", typeof(string));
            lista.Columns.Add("nombre", typeof(string));
            lista.Columns.Add("detalle", typeof(string));
            lista.Columns.Add("cantidad", typeof(int));
            lista.Columns.Add("precio_unitario", typeof(decimal));
            lista.Columns.Add("producto_id", typeof(int));
            lista.Columns.Add("tipo_equipo", typeof(string));
            lista.Columns.Add("marca_equipo", typeof(string));
            lista.Columns.Add("modelo_equipo", typeof(string));
            return lista;
        }

        public bool agregarProductoALista(DataTable lista, int productoId, string nombre, string tipo,
                                          string cantidadTexto, string precioTexto, out string mensaje)
        {
            int cantidad;
            decimal precio;

            mensaje = validarCantidadYPrecio(cantidadTexto, precioTexto, out cantidad, out precio);
            if (mensaje != "")
                return false;

            foreach (DataRow fila in lista.Rows)
            {
                if ("" + fila["tipo"] == "Producto" && Convert.ToInt32(fila["producto_id"]) == productoId)
                {
                    mensaje = "Ese producto ya está en la lista. Quítelo si quiere cambiar la cantidad.";
                    return false;
                }
            }

            lista.Rows.Add("Producto", nombre, tipo, cantidad, precio, productoId, "", "", "");
            return true;
        }

        public bool agregarEquipoALista(DataTable lista, string nombre, string tipo, string marca, string modelo,
                                        string cantidadTexto, string precioTexto, out string mensaje)
        {
            int cantidad;
            decimal precio;

            mensaje = validarCantidadYPrecio(cantidadTexto, precioTexto, out cantidad, out precio);
            if (mensaje != "")
                return false;

            if (cantidad > 200)
            {
                mensaje = "No se pueden ingresar más de 200 unidades de un mismo equipo a la vez.";
                return false;
            }

            foreach (DataRow fila in lista.Rows)
            {
                if ("" + fila["tipo"] == "Equipo" && "" + fila["nombre"] == nombre && "" + fila["tipo_equipo"] == tipo
                    && "" + fila["marca_equipo"] == marca && "" + fila["modelo_equipo"] == modelo)
                {
                    mensaje = "Ese equipo ya está en la lista. Quítelo si quiere cambiar la cantidad.";
                    return false;
                }
            }

            string detalle = tipo + " · " + marca + " " + modelo;
            lista.Rows.Add("Equipo", nombre, detalle, cantidad, precio, 0, tipo, marca, modelo);
            return true;
        }

        public decimal valorTotal(DataTable inventario)
        {
            decimal total = 0;

            foreach (DataRow fila in inventario.Rows)
            {
                if (fila["valor"] != DBNull.Value)
                    total = total + Convert.ToDecimal(fila["valor"]);
            }

            return total;
        }

        public int totalUnidades(DataTable lista)
        {
            int total = 0;

            foreach (DataRow fila in lista.Rows)
            {
                total = total + Convert.ToInt32(fila["cantidad"]);
            }

            return total;
        }

        public bool registrarIngreso(int proveedorId, int empleadoId, string factura, DateTime? fecha,
                                     DataTable lista, out string mensaje)
        {
            mensaje = validarIngreso(proveedorId, empleadoId, factura, fecha, lista);
            if (mensaje != "")
                return false;

            DataTable productos = new DataTable();
            productos.Columns.Add("producto_id", typeof(int));
            productos.Columns.Add("cantidad", typeof(int));
            productos.Columns.Add("precio_unitario", typeof(decimal));

            DataTable equipos = new DataTable();
            equipos.Columns.Add("nombre_equipo", typeof(string));
            equipos.Columns.Add("tipo_equipo", typeof(string));
            equipos.Columns.Add("marca_equipo", typeof(string));
            equipos.Columns.Add("modelo_equipo", typeof(string));
            equipos.Columns.Add("cantidad", typeof(int));
            equipos.Columns.Add("precio_unitario", typeof(decimal));

            foreach (DataRow fila in lista.Rows)
            {
                if ("" + fila["tipo"] == "Producto")
                    productos.Rows.Add(fila["producto_id"], fila["cantidad"], fila["precio_unitario"]);
                else
                    equipos.Rows.Add(fila["nombre"], fila["tipo_equipo"], fila["marca_equipo"], fila["modelo_equipo"],
                                     fila["cantidad"], fila["precio_unitario"]);
            }

            DateTime fechaCompra = DateTime.Today;
            if (fecha.HasValue)
                fechaCompra = fecha.Value.Date;

            SqlCommand comando = procedimiento("sp_RegistrarIngresoInventario");
            comando.Parameters.AddWithValue("@proveedor_id", proveedorId);
            comando.Parameters.AddWithValue("@firma_empleado_id", empleadoId);
            comando.Parameters.AddWithValue("@numero_factura", factura.Trim());
            comando.Parameters.AddWithValue("@fecha_compra", fechaCompra);
            agregarTabla(comando, "@productos", "dbo.tipo_ingreso_productos", productos);
            agregarTabla(comando, "@equipos", "dbo.tipo_ingreso_equipos", equipos);

            SqlParameter compra = new SqlParameter("@compra_id", SqlDbType.Int);
            compra.Direction = ParameterDirection.Output;
            comando.Parameters.Add(compra);

            return ejecutar(comando, out mensaje);
        }

        private string validarIngreso(int proveedorId, int empleadoId, string factura, DateTime? fecha, DataTable lista)
        {
            if (proveedorId <= 0)
                return "Seleccione el proveedor.";
            if (factura.Trim() == "")
                return "Escriba el número de factura.";
            if (factura.Trim().Length > 50)
                return "El número de factura no puede tener más de 50 caracteres.";
            if (!fecha.HasValue)
                return "Seleccione la fecha de la compra.";
            if (fecha.Value.Date > DateTime.Today)
                return "La fecha de la compra no puede ser futura.";
            if (empleadoId <= 0)
                return "Seleccione el empleado que recibe.";
            if (lista.Rows.Count == 0)
                return "Agregue al menos un producto o equipo a la lista.";

            return "";
        }

        private string validarCantidadYPrecio(string cantidadTexto, string precioTexto, out int cantidad, out decimal precio)
        {
            cantidad = 0;
            precio = 0;

            cantidadTexto = cantidadTexto.Trim();
            precioTexto = precioTexto.Trim().Replace(',', '.');

            if (cantidadTexto == "")
                return "Escriba la cantidad.";
            if (!int.TryParse(cantidadTexto, out cantidad) || cantidad <= 0)
                return "La cantidad debe ser un número mayor que 0.";

            if (precioTexto == "")
                return "Escriba el precio unitario.";
            if (!decimal.TryParse(precioTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out precio) || precio < 0)
                return "El precio unitario no es válido.";

            return "";
        }

        private string validarProveedor(string nombre, string telefono, string correo)
        {
            nombre = nombre.Trim();
            telefono = telefono.Trim();
            correo = correo.Trim();

            if (nombre == "")
                return "El nombre del proveedor es obligatorio.";
            if (nombre.Length > 50)
                return "El nombre no puede tener más de 50 caracteres.";

            if (telefono == "")
                return "El teléfono es obligatorio.";
            if (!soloNumeros(telefono))
                return "El teléfono solo puede tener números, sin guiones ni espacios.";
            if (telefono.Length != 8)
                return "El teléfono debe tener 8 dígitos. Tiene " + telefono.Length + ".";

            if (correo == "")
                return "El correo es obligatorio.";
            if (!correo.Contains("@") || !correo.Contains("."))
                return "El correo no es válido. Ejemplo: ventas@proveedor.com";
            if (correo.Length > 50)
                return "El correo no puede tener más de 50 caracteres.";

            return "";
        }

        private void agregarTabla(SqlCommand comando, string nombre, string tipo, DataTable tabla)
        {
            SqlParameter parametro = comando.Parameters.AddWithValue(nombre, tabla);
            parametro.SqlDbType = SqlDbType.Structured;
            parametro.TypeName = tipo;
        }
    }
}
