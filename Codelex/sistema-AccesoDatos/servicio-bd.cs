using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Codelex.AccesoDatos
{
    // Datos de una actividad tal como se capturan en la pantalla.
    class DatosActividad
    {
        public int actividadId = 0;
        public int servicioId = 0;
        public string descripcion = "";
        public string unidad = "";
        public decimal rendimiento = 0;

        // desde, hasta (puede ir vacío), cantidad_personas
        public DataTable rangos = new DataTable();

        // producto_id (0 si el producto es nuevo y se crea al guardar), nombre_producto, tipo_producto, cantidad_por_unidad
        public DataTable productos = new DataTable();

        // nombre_equipo
        public DataTable equipos = new DataTable();
    }

    class ServicioBD : BaseDatos
    {
        /// <summary>
        /// Lista los servicios con la cantidad de actividades de cada uno.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarServicios()
        {
            return consultar(procedimiento("sp_ListarServiciosGestion"));
        }

        /// <summary>
        /// Lista las actividades de un servicio (0 trae todas).
        /// </summary>
        /// <param name="servicioId">Identificador del servicio.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarActividades(int servicioId)
        {
            SqlCommand comando = procedimiento("sp_ListarActividadesGestion");
            comando.Parameters.AddWithValue("@servicio_id", servicioId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista los rangos de personas de una actividad.
        /// </summary>
        /// <param name="actividadId">Identificador de la actividad.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarRangos(int actividadId)
        {
            SqlCommand comando = procedimiento("sp_ListarRangosActividad");
            comando.Parameters.AddWithValue("@actividad_id", actividadId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista los productos que usa una actividad.
        /// </summary>
        /// <param name="actividadId">Identificador de la actividad.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarProductosActividad(int actividadId)
        {
            SqlCommand comando = procedimiento("sp_ListarProductosActividad");
            comando.Parameters.AddWithValue("@actividad_id", actividadId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista los equipos que usa una actividad.
        /// </summary>
        /// <param name="actividadId">Identificador de la actividad.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarEquiposActividad(int actividadId)
        {
            SqlCommand comando = procedimiento("sp_ListarEquiposActividad");
            comando.Parameters.AddWithValue("@actividad_id", actividadId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista todos los productos registrados.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarProductosCatalogo()
        {
            return consultar(procedimiento("sp_ListarProductosCatalogo"));
        }

        /// <summary>
        /// Lista los nombres de los equipos registrados.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarEquiposCatalogo()
        {
            return consultar(procedimiento("sp_ListarEquiposCatalogo"));
        }

        // ---------- Servicios ----------

        /// <summary>
        /// Valida el nombre de un servicio.
        /// </summary>
        /// <param name="nombre">Nombre del servicio.</param>
        /// <returns>Mensaje de error, o texto vacío si es válido.</returns>
        public string validarServicio(string nombre)
        {
            nombre = nombre.Trim();

            return Validacion.validarTexto(nombre, "El nombre del servicio", 3, 30);
        }

        /// <summary>
        /// Crea un servicio (servicioId = 0) o cambia el nombre de uno existente.
        /// </summary>
        /// <param name="servicioId">Identificador del servicio, 0 si es nuevo.</param>
        /// <param name="nombre">Nombre del servicio.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <param name="idGuardado">(salida) Identificador del servicio guardado.</param>
        /// <returns>true si se guardó; false en caso contrario.</returns>
        public bool guardarServicio(int servicioId, string nombre, out string mensaje, out int idGuardado)
        {
            idGuardado = servicioId;

            mensaje = validarServicio(nombre);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_GuardarServicio");
            comando.Parameters.AddWithValue("@servicio_id", servicioId);
            comando.Parameters.AddWithValue("@nombre_servicio", nombre.Trim());

            SqlParameter id = new SqlParameter("@id_guardado", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = ejecutar(comando, out mensaje);
            if (guardado)
                idGuardado = Convert.ToInt32(id.Value);

            return guardado;
        }

        /// <summary>
        /// Elimina un servicio que no tiene actividades ni se ha usado.
        /// </summary>
        /// <param name="servicioId">Identificador del servicio.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si se eliminó; false en caso contrario.</returns>
        public bool eliminarServicio(int servicioId, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_EliminarServicio");
            comando.Parameters.AddWithValue("@servicio_id", servicioId);
            return ejecutar(comando, out mensaje);
        }

        // ---------- Actividades ----------

        /// <summary>
        /// Crea las tablas vacías donde se capturan los rangos, productos y equipos de una actividad.
        /// </summary>
        /// <returns>Datos de actividad con sus tablas listas.</returns>
        public DatosActividad crearDatosActividad()
        {
            DatosActividad datos = new DatosActividad();

            datos.rangos.Columns.Add("desde", typeof(decimal));
            datos.rangos.Columns.Add("hasta", typeof(decimal));
            datos.rangos.Columns.Add("cantidad_personas", typeof(int));

            datos.productos.Columns.Add("producto_id", typeof(int));
            datos.productos.Columns.Add("nombre_producto", typeof(string));
            datos.productos.Columns.Add("tipo_producto", typeof(string));
            datos.productos.Columns.Add("cantidad_por_unidad", typeof(decimal));

            datos.equipos.Columns.Add("nombre_equipo", typeof(string));

            return datos;
        }

        /// <summary>
        /// Convierte un texto a número decimal aceptando punto o coma.
        /// </summary>
        /// <param name="texto">Texto escrito por el usuario.</param>
        /// <param name="valor">(salida) Número obtenido.</param>
        /// <returns>true si el texto es un número; false en caso contrario.</returns>
        public bool leerDecimal(string texto, out decimal valor)
        {
            texto = texto.Trim().Replace(',', '.');
            return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
        }

        /// <summary>
        /// Valida y agrega un rango de personas a la tabla de rangos.
        /// </summary>
        /// <param name="rangos">Tabla de rangos de la actividad.</param>
        /// <param name="desdeTexto">Medida desde la que aplica el rango.</param>
        /// <param name="hastaTexto">Medida hasta la que aplica (vacío si no tiene tope).</param>
        /// <param name="personasTexto">Cantidad de personas del rango.</param>
        /// <param name="mensaje">(salida) Mensaje de error, vacío si se agregó.</param>
        /// <returns>true si se agregó; false en caso contrario.</returns>
        public bool agregarRango(DataTable rangos, string desdeTexto, string hastaTexto, string personasTexto, out string mensaje)
        {
            decimal desde;
            decimal hasta = 0;
            int personas;
            bool tieneHasta = hastaTexto.Trim() != "";

            if (!leerDecimal(desdeTexto, out desde) || desde < 0)
            {
                mensaje = "Escriba desde qué medida aplica el rango (0 o más).";
                return false;
            }

            if (tieneHasta && (!leerDecimal(hastaTexto, out hasta) || hasta <= desde))
            {
                mensaje = "El \"hasta\" debe ser mayor que el \"desde\", o déjelo vacío si no tiene tope.";
                return false;
            }

            if (!int.TryParse(personasTexto.Trim(), out personas) || personas < 1)
            {
                mensaje = "Escriba cuántas personas trabajan en ese rango (1 o más).";
                return false;
            }

            foreach (DataRow fila in rangos.Rows)
            {
                if (Convert.ToDecimal(fila["desde"]) == desde)
                {
                    mensaje = "Ya hay un rango que empieza en esa medida.";
                    return false;
                }
            }

            if (tieneHasta)
                rangos.Rows.Add(desde, hasta, personas);
            else
                rangos.Rows.Add(desde, DBNull.Value, personas);

            mensaje = "";
            return true;
        }

        /// <summary>
        /// Valida y agrega un producto a la tabla de productos de la actividad.
        /// </summary>
        /// <param name="productos">Tabla de productos de la actividad.</param>
        /// <param name="productoId">Identificador del producto, 0 si es un producto nuevo.</param>
        /// <param name="nombre">Nombre del producto.</param>
        /// <param name="tipo">Tipo del producto.</param>
        /// <param name="cantidadTexto">Cantidad que se gasta por cada unidad de la actividad.</param>
        /// <param name="mensaje">(salida) Mensaje de error, vacío si se agregó.</param>
        /// <returns>true si se agregó; false en caso contrario.</returns>
        public bool agregarProducto(DataTable productos, int productoId, string nombre, string tipo, string cantidadTexto, out string mensaje)
        {
            decimal cantidad;

            if (nombre.Trim() == "")
            {
                mensaje = "Elija un producto.";
                return false;
            }

            if (productoId == 0)
            {
                mensaje = new InventarioBD().validarProducto(nombre, tipo);
                if (mensaje != "")
                    return false;
            }

            if (!leerDecimal(cantidadTexto, out cantidad) || cantidad <= 0)
            {
                mensaje = "Escriba la cantidad por unidad (mayor que cero).";
                return false;
            }

            foreach (DataRow fila in productos.Rows)
            {
                bool mismoId = productoId != 0 && Convert.ToInt32(fila["producto_id"]) == productoId;
                bool mismoNuevo = productoId == 0 && Convert.ToInt32(fila["producto_id"]) == 0 &&
                                  "" + fila["nombre_producto"] == nombre.Trim() && "" + fila["tipo_producto"] == tipo.Trim();

                if (mismoId || mismoNuevo)
                {
                    mensaje = "Ese producto ya está en la actividad. Quítelo si quiere cambiar la cantidad.";
                    return false;
                }
            }

            productos.Rows.Add(productoId, nombre.Trim(), tipo.Trim(), cantidad);
            mensaje = "";
            return true;
        }

        /// <summary>
        /// Valida y agrega un equipo a la tabla de equipos de la actividad.
        /// </summary>
        /// <param name="equipos">Tabla de equipos de la actividad.</param>
        /// <param name="nombre">Nombre del equipo.</param>
        /// <param name="mensaje">(salida) Mensaje de error, vacío si se agregó.</param>
        /// <returns>true si se agregó; false en caso contrario.</returns>
        public bool agregarEquipo(DataTable equipos, string nombre, out string mensaje)
        {
            nombre = nombre.Trim();

            if (nombre == "")
            {
                mensaje = "Elija un equipo.";
                return false;
            }

            foreach (DataRow fila in equipos.Rows)
            {
                if ("" + fila["nombre_equipo"] == nombre)
                {
                    mensaje = "Ese equipo ya está en la actividad.";
                    return false;
                }
            }

            equipos.Rows.Add(nombre);
            mensaje = "";
            return true;
        }

        /// <summary>
        /// Valida los datos de una actividad antes de guardarla.
        /// </summary>
        /// <param name="datos">Datos de la actividad.</param>
        /// <returns>Mensaje de error, o texto vacío si es válida.</returns>
        public string validarActividad(DatosActividad datos)
        {
            string descripcion = datos.descripcion.Trim();
            string unidad = datos.unidad.Trim();

            if (datos.servicioId == 0)
                return "Elija el servicio de la actividad.";

            string problema = Validacion.validarTexto(descripcion, "El nombre de la actividad", 5, 50);
            if (problema != "")
                return problema;

            problema = Validacion.validarTexto(unidad, "La unidad de medida", 1, 20);
            if (problema != "")
                return problema;
            if (datos.rendimiento <= 0)
                return "El rendimiento debe ser mayor que cero.";
            if (datos.rangos.Rows.Count == 0)
                return "Agregue al menos un rango de personas.";
            if (datos.productos.Rows.Count == 0)
                return "La actividad necesita al menos un producto.";
            if (datos.equipos.Rows.Count == 0)
                return "La actividad necesita al menos un equipo.";

            return validarRangos(datos.rangos);
        }

        /// <summary>
        /// Revisa que los rangos de personas empiecen en 0 y no se crucen.
        /// </summary>
        /// <param name="rangos">Tabla de rangos de la actividad.</param>
        /// <returns>Mensaje de error, o texto vacío si son válidos.</returns>
        private string validarRangos(DataTable rangos)
        {
            DataView vista = new DataView(rangos);
            vista.Sort = "desde ASC";

            if (Convert.ToDecimal(vista[0]["desde"]) > 0.01m)
                return "El primer rango de personas debe empezar en 0.";

            for (int i = 0; i < vista.Count - 1; i++)
            {
                if (vista[i]["hasta"] != DBNull.Value &&
                    Convert.ToDecimal(vista[i]["hasta"]) > Convert.ToDecimal(vista[i + 1]["desde"]))
                {
                    return "Los rangos de personas se cruzan. Revise los valores.";
                }
            }

            return "";
        }

        /// <summary>
        /// Guarda una actividad nueva (actividadId = 0) o los cambios de una existente.
        /// </summary>
        /// <param name="datos">Datos de la actividad.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <param name="idGuardado">(salida) Identificador de la actividad guardada.</param>
        /// <returns>true si se guardó; false en caso contrario.</returns>
        public bool guardarActividad(DatosActividad datos, out string mensaje, out int idGuardado)
        {
            idGuardado = datos.actividadId;

            mensaje = validarActividad(datos);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_GuardarActividad");
            comando.Parameters.AddWithValue("@actividad_id", datos.actividadId);
            comando.Parameters.AddWithValue("@servicio_id", datos.servicioId);
            comando.Parameters.AddWithValue("@descripcion", datos.descripcion.Trim());
            comando.Parameters.AddWithValue("@unidad", datos.unidad.Trim());
            comando.Parameters.AddWithValue("@rendimiento", datos.rendimiento);
            agregarLista(comando, "@rangos", "dbo.tipo_actividad_rangos", datos.rangos);
            agregarLista(comando, "@productos", "dbo.tipo_actividad_productos", tablaDeProductos(datos.productos, false));
            agregarLista(comando, "@productos_nuevos", "dbo.tipo_actividad_productos_nuevos", tablaDeProductos(datos.productos, true));
            agregarLista(comando, "@equipos", "dbo.tipo_actividad_equipos", datos.equipos);

            SqlParameter id = new SqlParameter("@id_guardado", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = ejecutar(comando, out mensaje);
            if (guardado)
                idGuardado = Convert.ToInt32(id.Value);

            return guardado;
        }

        /// <summary>
        /// Elimina una actividad que no se ha usado en cotizaciones ni órdenes.
        /// </summary>
        /// <param name="actividadId">Identificador de la actividad.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si se eliminó; false en caso contrario.</returns>
        public bool eliminarActividad(int actividadId, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_EliminarActividad");
            comando.Parameters.AddWithValue("@actividad_id", actividadId);
            return ejecutar(comando, out mensaje);
        }

        // ---------- Productos y equipos nuevos ----------

        /// <summary>
        /// Crea la tabla donde se eligen las actividades de un producto o equipo nuevo.
        /// </summary>
        /// <returns>Tabla con las columnas actividad_id y cantidad_por_unidad.</returns>
        public DataTable crearVinculos()
        {
            DataTable vinculos = new DataTable();
            vinculos.Columns.Add("actividad_id", typeof(int));
            vinculos.Columns.Add("cantidad_por_unidad", typeof(decimal));
            return vinculos;
        }

        /// <summary>
        /// Crea la tabla donde se marcan las actividades de un producto o equipo nuevo.
        /// </summary>
        /// <returns>Tabla con las columnas usar, actividad_id, servicio, actividad y cantidad.</returns>
        public DataTable crearActividadesParaElegir()
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add("usar", typeof(bool));
            tabla.Columns.Add("actividad_id", typeof(int));
            tabla.Columns.Add("servicio", typeof(string));
            tabla.Columns.Add("actividad", typeof(string));
            tabla.Columns.Add("cantidad", typeof(string));

            foreach (DataRow fila in listarActividades(0).Rows)
                tabla.Rows.Add(false, fila["actividad_id"], fila["nombre_servicio"], fila["descripcion"], "1");

            return tabla;
        }

        /// <summary>
        /// Toma de la tabla las actividades marcadas y valida sus cantidades.
        /// </summary>
        /// <param name="actividades">Tabla con las actividades que el usuario marcó.</param>
        /// <param name="conCantidad">true si cada actividad lleva cantidad por unidad (productos).</param>
        /// <param name="vinculos">(salida) Actividades marcadas con su cantidad.</param>
        /// <returns>Mensaje de error, o texto vacío si todo es válido.</returns>
        public string tomarVinculos(DataTable actividades, bool conCantidad, DataTable vinculos)
        {
            foreach (DataRow fila in actividades.Rows)
            {
                if (!Convert.ToBoolean(fila["usar"]))
                    continue;

                decimal cantidad = 1;

                if (conCantidad && (!leerDecimal("" + fila["cantidad"], out cantidad) || cantidad <= 0))
                    return "Escriba una cantidad válida en la actividad " + fila["actividad"] + ".";

                vinculos.Rows.Add(fila["actividad_id"], cantidad);
            }

            string articulo = "el equipo";
            if (conCantidad)
                articulo = "el producto";

            return validarVinculos(vinculos, articulo);
        }

        /// <summary>
        /// Registra un producto nuevo ligado a las actividades donde se usa (mínimo una).
        /// </summary>
        /// <param name="nombre">Nombre del producto.</param>
        /// <param name="tipo">Tipo de producto.</param>
        /// <param name="vinculos">Actividades y cantidad por unidad de cada una.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <param name="productoId">(salida) Identificador del producto creado.</param>
        /// <returns>true si se registró; false en caso contrario.</returns>
        public bool insertarProductoConActividades(string nombre, string tipo, DataTable vinculos, out string mensaje, out int productoId)
        {
            productoId = 0;

            InventarioBD inventarioBD = new InventarioBD();
            mensaje = inventarioBD.validarProducto(nombre, tipo);
            if (mensaje != "")
                return false;

            mensaje = validarVinculos(vinculos, "el producto");
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_InsertarProductoConActividades");
            comando.Parameters.AddWithValue("@nombre_producto", nombre.Trim());
            comando.Parameters.AddWithValue("@tipo_producto", tipo.Trim());
            agregarLista(comando, "@actividades", "dbo.tipo_producto_actividades", vinculos);

            SqlParameter id = new SqlParameter("@producto_id", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = ejecutar(comando, out mensaje);
            if (guardado)
                productoId = Convert.ToInt32(id.Value);

            return guardado;
        }

        /// <summary>
        /// Liga un equipo nuevo a las actividades donde se usa (mínimo una).
        /// </summary>
        /// <param name="nombre">Nombre del equipo.</param>
        /// <param name="vinculos">Actividades elegidas.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si se ligó; false en caso contrario.</returns>
        public bool vincularEquipo(string nombre, DataTable vinculos, out string mensaje)
        {
            mensaje = validarVinculos(vinculos, "el equipo");
            if (mensaje != "")
                return false;

            DataTable actividades = new DataTable();
            actividades.Columns.Add("actividad_id", typeof(int));
            foreach (DataRow fila in vinculos.Rows)
                actividades.Rows.Add(fila["actividad_id"]);

            SqlCommand comando = procedimiento("sp_VincularEquipoActividades");
            comando.Parameters.AddWithValue("@nombre_equipo", nombre.Trim());
            agregarLista(comando, "@actividades", "dbo.tipo_equipo_actividades", actividades);
            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Revisa que haya al menos una actividad elegida y que las cantidades sean válidas.
        /// </summary>
        /// <param name="vinculos">Actividades elegidas.</param>
        /// <param name="articulo">Texto para el mensaje, por ejemplo "el producto".</param>
        /// <returns>Mensaje de error, o texto vacío si es válido.</returns>
        public string validarVinculos(DataTable vinculos, string articulo)
        {
            if (vinculos.Rows.Count == 0)
                return "Elija al menos una actividad donde se usa " + articulo + ".";

            foreach (DataRow fila in vinculos.Rows)
            {
                if (Convert.ToDecimal(fila["cantidad_por_unidad"]) <= 0)
                    return "La cantidad por unidad debe ser mayor que cero.";
            }

            return "";
        }

        // ---------- Listas para los procedimientos ----------

        /// <summary>
        /// Envía una tabla como parámetro de tipo tabla.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <param name="nombre">Nombre del parámetro.</param>
        /// <param name="tipo">Nombre del tipo de tabla en SQL Server.</param>
        /// <param name="tabla">Tabla de datos.</param>
        private void agregarLista(SqlCommand comando, string nombre, string tipo, DataTable tabla)
        {
            SqlParameter parametro = comando.Parameters.AddWithValue(nombre, tabla);
            parametro.SqlDbType = SqlDbType.Structured;
            parametro.TypeName = tipo;
        }

        /// <summary>
        /// Separa los productos de la actividad en la tabla que recibe el procedimiento:
        /// los que ya existen (producto_id y cantidad) o los nuevos (nombre, tipo y cantidad).
        /// </summary>
        /// <param name="productos">Tabla de productos de la actividad.</param>
        /// <param name="nuevos">true para armar la tabla de productos nuevos; false para la de existentes.</param>
        /// <returns>Tabla lista para enviar al procedimiento.</returns>
        private DataTable tablaDeProductos(DataTable productos, bool nuevos)
        {
            DataTable tabla = new DataTable();

            if (nuevos)
            {
                tabla.Columns.Add("nombre_producto", typeof(string));
                tabla.Columns.Add("tipo_producto", typeof(string));
                tabla.Columns.Add("cantidad_por_unidad", typeof(decimal));
            }
            else
            {
                tabla.Columns.Add("producto_id", typeof(int));
                tabla.Columns.Add("cantidad_por_unidad", typeof(decimal));
            }

            foreach (DataRow fila in productos.Rows)
            {
                bool esNuevo = Convert.ToInt32(fila["producto_id"]) == 0;

                if (nuevos && esNuevo)
                    tabla.Rows.Add(fila["nombre_producto"], fila["tipo_producto"], fila["cantidad_por_unidad"]);
                else if (!nuevos && !esNuevo)
                    tabla.Rows.Add(fila["producto_id"], fila["cantidad_por_unidad"]);
            }

            return tabla;
        }
    }
}
