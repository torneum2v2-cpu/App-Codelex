using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Codelex.AccesoDatos
{
    class InventarioBD : BaseDatos
    {
        /// <summary>
        /// Busca inventario.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable buscarInventario(string texto, string tipo)
        {
            SqlCommand comando = procedimiento("sp_BuscarInventario");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            comando.Parameters.AddWithValue("@tipo", tipo);
            return consultar(comando);
        }

        /// <summary>
        /// Lista productos.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarProductos(string texto)
        {
            SqlCommand comando = procedimiento("sp_ListarProductosInventario");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            return consultar(comando);
        }

        /// <summary>
        /// Lista equipos.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarEquipos(string texto)
        {
            SqlCommand comando = procedimiento("sp_ListarEquiposInventario");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            return consultar(comando);
        }

        /// <summary>
        /// Lista proveedores.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarProveedores()
        {
            return consultar(procedimiento("sp_ListarProveedores"));
        }

        /// <summary>
        /// Lista empleados.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarEmpleados()
        {
            return consultar(procedimiento("sp_ListarEmpleadosInventario"));
        }

        /// <summary>
        /// Inserta proveedor.
        /// </summary>
        /// <param name="nombre">Nombre.</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <param name="proveedorId">(salida) Identificador de proveedor.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool insertarProveedor(string nombre, string telefono, string correo, out string mensaje, out int proveedorId)
        {
            proveedorId = 0;

            mensaje = validarProveedor(nombre, telefono, correo);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_InsertarProveedor");
            comando.Parameters.AddWithValue("@nombre_proveedor", nombre.Trim());
            comando.Parameters.AddWithValue("@telefono_proveedor", Validacion.normalizarTelefono(telefono));
            comando.Parameters.AddWithValue("@correo_proveedor", correo.Trim());

            SqlParameter id = new SqlParameter("@proveedor_id", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = ejecutar(comando, out mensaje);
            if (guardado)
                proveedorId = Convert.ToInt32(id.Value);

            return guardado;
        }

        /// <summary>
        /// Valida producto.
        /// </summary>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <returns>Texto resultante.</returns>
        public string validarProducto(string nombre, string tipo)
        {
            nombre = nombre.Trim();
            tipo = tipo.Trim();

            string problema = Validacion.validarTexto(nombre, "El nombre del producto", 3, 50);
            if (problema != "")
                return problema;

            return Validacion.validarTexto(tipo, "El tipo de producto", 3, 20);
        }

        /// <summary>
        /// Valida equipo.
        /// </summary>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="marca">Marca.</param>
        /// <param name="modelo">Modelo.</param>
        /// <returns>Texto resultante.</returns>
        public string validarEquipo(string nombre, string tipo, string marca, string modelo)
        {
            nombre = nombre.Trim();
            tipo = tipo.Trim();
            marca = marca.Trim();
            modelo = modelo.Trim();

            string problema = Validacion.validarTexto(nombre, "El nombre del equipo", 3, 50);
            if (problema != "")
                return problema;

            problema = Validacion.validarTexto(tipo, "El tipo de equipo", 3, 50);
            if (problema != "")
                return problema;

            problema = Validacion.validarTexto(marca, "La marca", 2, 50);
            if (problema != "")
                return problema;

            return Validacion.validarModelo(modelo, "El modelo", 50);
        }

        /// <summary>
        /// Tipos de ajuste que el usuario puede elegir.
        /// </summary>
        public string[] tiposAjuste = { "Merma (se dañó o venció)", "Pérdida (se extravió)", "Conteo físico (stock real)" };

        /// <summary>
        /// Convierte el texto elegido en la pantalla al valor que guarda la base de datos.
        /// </summary>
        /// <param name="texto">Texto elegido en la lista de tipos de ajuste.</param>
        /// <returns>Merma, Perdida, Conteo o texto vacío si no se eligió ninguno.</returns>
        public string valorTipoAjuste(string texto)
        {
            if (texto.StartsWith("Merma"))
                return "Merma";
            if (texto.StartsWith("Pérdida"))
                return "Perdida";
            if (texto.StartsWith("Conteo"))
                return "Conteo";

            return "";
        }

        /// <summary>
        /// Valida los datos de un ajuste de inventario.
        /// </summary>
        /// <param name="productoId">Identificador del producto (0 si no se eligió).</param>
        /// <param name="tipo">Tipo de ajuste elegido (Merma, Perdida o Conteo).</param>
        /// <param name="cantidadTexto">Cantidad escrita por el usuario.</param>
        /// <param name="motivo">Motivo del ajuste.</param>
        /// <param name="cantidad">(salida) Cantidad ya convertida a número.</param>
        /// <returns>Texto del error, o texto vacío si todo es válido.</returns>
        public string validarAjuste(int productoId, string tipo, string cantidadTexto, string motivo, out int cantidad)
        {
            cantidad = 0;
            cantidadTexto = cantidadTexto.Trim();
            motivo = motivo.Trim();

            if (productoId == 0)
                return "Seleccione un producto de la tabla.";
            if (tipo == "")
                return "Seleccione el tipo de ajuste.";

            if (cantidadTexto == "")
                return "La cantidad es obligatoria.";
            if (!soloNumeros(cantidadTexto))
                return "La cantidad solo puede tener números enteros.";
            if (cantidadTexto.Length > 7)
                return "La cantidad no puede tener más de 7 dígitos.";

            cantidad = int.Parse(cantidadTexto);
            if (cantidad == 0 && tipo != "Conteo")
                return "La cantidad debe ser mayor que cero.";

            if (motivo == "")
                return "El motivo es obligatorio.";
            if (motivo.Length < 5)
                return "El motivo debe tener al menos 5 caracteres.";
            if (motivo.Length > 100)
                return "El motivo no puede tener más de 100 caracteres.";

            return "";
        }

        /// <summary>
        /// Registra un ajuste de inventario (merma, pérdida o conteo físico) de un producto.
        /// </summary>
        /// <param name="productoId">Identificador del producto.</param>
        /// <param name="tipo">Tipo de ajuste elegido (Merma, Perdida o Conteo).</param>
        /// <param name="cantidadTexto">Cantidad escrita por el usuario.</param>
        /// <param name="motivo">Motivo del ajuste.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool ajustarInventario(int productoId, string tipo, string cantidadTexto, string motivo, out string mensaje)
        {
            int cantidad;
            mensaje = validarAjuste(productoId, tipo, cantidadTexto, motivo, out cantidad);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_AjustarInventario");
            comando.Parameters.AddWithValue("@producto_id", productoId);
            comando.Parameters.AddWithValue("@tipo_ajuste", tipo);
            comando.Parameters.AddWithValue("@cantidad", cantidad);
            comando.Parameters.AddWithValue("@motivo", motivo.Trim());

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Lista los últimos ajustes de inventario.
        /// </summary>
        /// <param name="productoId">Identificador del producto (0 para ver los de todos).</param>
        /// <returns>Tabla con los ajustes.</returns>
        public DataTable listarAjustes(int productoId)
        {
            SqlCommand comando = procedimiento("sp_ListarAjustesInventario");
            if (productoId > 0)
                comando.Parameters.AddWithValue("@producto_id", productoId);

            return consultar(comando);
        }

        /// <summary>
        /// Impide escribir caracteres que no sean números.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool soloNumeros(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Crea lista.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
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

        /// <summary>
        /// Agrega producto a lista.
        /// </summary>
        /// <param name="lista">Lista de datos.</param>
        /// <param name="productoId">Identificador de producto.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="cantidadTexto">Cantidad texto.</param>
        /// <param name="precioTexto">Precio texto.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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

        /// <summary>
        /// Agrega equipo a lista.
        /// </summary>
        /// <param name="lista">Lista de datos.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="marca">Marca.</param>
        /// <param name="modelo">Modelo.</param>
        /// <param name="cantidadTexto">Cantidad texto.</param>
        /// <param name="precioTexto">Precio texto.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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

        /// <summary>
        /// Suma el valor total del inventario.
        /// </summary>
        /// <param name="inventario">Tabla de inventario.</param>
        /// <returns>Valor decimal resultante.</returns>
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

        /// <summary>
        /// Suma las unidades de la lista de inventario.
        /// </summary>
        /// <param name="lista">Lista de datos.</param>
        /// <returns>Número entero resultante.</returns>
        public int totalUnidades(DataTable lista)
        {
            int total = 0;

            foreach (DataRow fila in lista.Rows)
            {
                total = total + Convert.ToInt32(fila["cantidad"]);
            }

            return total;
        }

        /// <summary>
        /// Registra ingreso.
        /// </summary>
        /// <param name="proveedorId">Identificador de proveedor.</param>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="factura">Factura.</param>
        /// <param name="fecha">Fecha.</param>
        /// <param name="lista">Lista de datos.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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

        /// <summary>
        /// Valida ingreso.
        /// </summary>
        /// <param name="proveedorId">Identificador de proveedor.</param>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="factura">Factura.</param>
        /// <param name="fecha">Fecha.</param>
        /// <param name="lista">Lista de datos.</param>
        /// <returns>Texto resultante.</returns>
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

        /// <summary>
        /// Valida cantidad y precio.
        /// </summary>
        /// <param name="cantidadTexto">Cantidad texto.</param>
        /// <param name="precioTexto">Precio texto.</param>
        /// <param name="cantidad">(salida) Cantidad.</param>
        /// <param name="precio">(salida) Precio.</param>
        /// <returns>Texto resultante.</returns>
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

        /// <summary>
        /// Valida proveedor.
        /// </summary>
        /// <param name="nombre">Nombre.</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <returns>Texto resultante.</returns>
        private string validarProveedor(string nombre, string telefono, string correo)
        {
            nombre = nombre.Trim();
            telefono = telefono.Trim();
            correo = correo.Trim();

            string problema = Validacion.validarEmpresa(nombre, "El nombre del proveedor", true);
            if (problema != "")
                return problema;

            problema = Validacion.validarTelefono(telefono, true);
            if (problema != "")
                return problema;

            return Validacion.validarCorreo(correo, true, 50);
        }

        /// <summary>
        /// Agrega tabla.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="tabla">Tabla de datos.</param>
        private void agregarTabla(SqlCommand comando, string nombre, string tipo, DataTable tabla)
        {
            SqlParameter parametro = comando.Parameters.AddWithValue(nombre, tabla);
            parametro.SqlDbType = SqlDbType.Structured;
            parametro.TypeName = tipo;
        }
    }
}
