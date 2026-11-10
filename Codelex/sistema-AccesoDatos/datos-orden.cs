using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class DatosOrden
    {
        public string cotizacionId = "";
        public string rtn = "";
        public int locacionId = 0;
        public int supervisorId = 0;
        public int vehiculoId = 0;
        public DateTime? fechaSolicitud = null;
        public DateTime? fechaServicio = null;
        public int estadoId = 0;
        public string observaciones = "";
        public string turno = "";
        public string pisoArea = "";
        public int personasNecesarias = 0;

        public List<int> empleados = new List<int>();
    }

    class Orden : BaseDatos
    {
        /// <summary>
        /// Lista estados orden.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarEstadosOrden()
        {
            return consultar(procedimiento("sp_ListarEstadosOrden"));
        }

        /// <summary>
        /// Lista servicios.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarServicios()
        {
            return consultar(procedimiento("sp_ListarServicios"));
        }

        /// <summary>
        /// Lista empleados.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarEmpleados()
        {
            return consultar(procedimiento("sp_ListarEmpleadosOrden"));
        }

        /// <summary>
        /// Lista cotizaciones.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarCotizaciones(string texto, string rtn)
        {
            SqlCommand comando = procedimiento("sp_ListarCotizacionesOrden");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            return consultar(comando);
        }

        /// <summary>
        /// Lista las líneas de una cotización.
        /// </summary>
        /// <param name="cotizacionId">Identificador de cotización.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable detalleCotizacion(string cotizacionId)
        {
            SqlCommand comando = procedimiento("sp_DetalleCotizacion");
            comando.Parameters.AddWithValue("@cotizacion_id", cotizacionId);
            return consultar(comando);
        }

        /// <summary>
        /// Resume lo que necesita una cotización (personas, productos y equipos).
        /// </summary>
        /// <param name="cotizacionId">Identificador de cotización.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable resumenCotizacion(string cotizacionId)
        {
            SqlCommand comando = procedimiento("sp_ResumenCotizacion");
            comando.Parameters.AddWithValue("@cotizacion_id", cotizacionId);
            return consultar(comando);
        }

        // Una línea del resumen: lo que necesita la orden y lo que hay disponible.
        /// <summary>
        /// Arma el texto de una necesidad de la orden e indica si falta (FALTA - ...).
        /// </summary>
        /// <param name="fila">Fila con los datos.</param>
        /// <returns>Texto resultante.</returns>
        public string textoNecesidad(DataRow fila)
        {
            string tipo = "" + fila["tipo"];
            string nombre = "" + fila["nombre"];
            int cantidad = Convert.ToInt32(fila["cantidad"]);
            int disponible = Convert.ToInt32(fila["disponible"]);

            string texto;

            if (tipo == "Personas")
                texto = nombre + ": " + cantidad + " (hay " + disponible + " libres)";
            else if (tipo == "Producto")
                texto = nombre + ": " + cantidad + " (stock " + disponible + ")";
            else
                texto = "Equipo: " + nombre + " (libres " + disponible + ")";

            if (disponible < cantidad)
                texto = "FALTA - " + texto;

            return texto;
        }

        /// <summary>
        /// Guarda orden.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <param name="ordenId">(salida) Identificador de orden.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool guardarOrden(DatosOrden datos, out string mensaje, out string ordenId)
        {
            ordenId = "";

            mensaje = validarOrden(datos);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_InsertarOrden");
            comando.Parameters.AddWithValue("@identificacion_cliente", datos.rtn.Trim());
            comando.Parameters.AddWithValue("@locacion_id", datos.locacionId);
            comando.Parameters.AddWithValue("@supervisor_id", datos.supervisorId);
            comando.Parameters.AddWithValue("@vehiculo_id", datos.vehiculoId);
            comando.Parameters.AddWithValue("@fecha_solicitud", datos.fechaSolicitud.Value);
            comando.Parameters.AddWithValue("@fecha_servicio", datos.fechaServicio.Value);
            comando.Parameters.AddWithValue("@estado_orden_id", datos.estadoId);
            comando.Parameters.AddWithValue("@observacion", datos.observaciones.Trim());
            comando.Parameters.AddWithValue("@turno", datos.turno.Trim());
            comando.Parameters.AddWithValue("@piso_area", datos.pisoArea.Trim());
            comando.Parameters.AddWithValue("@cotizacion_id", datos.cotizacionId.Trim());

            agregarLista(comando, "@empleados", "dbo.tipo_lista_empleados",
                         tablaDeEnteros("empleado_id", datos.empleados));

            SqlParameter ordenParametro = new SqlParameter("@orden_id", SqlDbType.VarChar, 50);
            ordenParametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(ordenParametro);

            bool guardada = ejecutar(comando, out mensaje);

            if (guardada)
            {
                ordenId = "" + ordenParametro.Value;
                mensaje = "Orden " + ordenId + " registrada correctamente.";
            }

            return guardada;
        }

        /// <summary>
        /// Valida orden.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <returns>Texto resultante.</returns>
        private string validarOrden(DatosOrden datos)
        {
            if (datos.cotizacionId.Trim() == "")
                return "Seleccione una cotización.";
            if (datos.rtn.Trim() == "")
                return "La cotización no tiene un cliente registrado.";
            if (datos.locacionId <= 0)
                return "Seleccione la locación.";
            if (!datos.fechaSolicitud.HasValue)
                return "Seleccione la fecha de solicitud.";
            if (!datos.fechaServicio.HasValue)
                return "Seleccione la fecha de limpieza.";
            if (datos.fechaServicio.Value < datos.fechaSolicitud.Value)
                return "La fecha de limpieza no puede ser anterior a la fecha de solicitud.";

            if (datos.estadoId <= 0)
                return "Seleccione el estado de la orden.";
            if (datos.supervisorId <= 0)
                return "Seleccione el supervisor.";

            if (datos.empleados.Count == 0)
                return "Asigne al menos un empleado.";
            if (datos.empleados.Count < datos.personasNecesarias)
                return "La cotización necesita " + datos.personasNecesarias + " empleados. Asigne más.";
            if (datos.turno.Trim() == "")
                return "Seleccione el turno.";
            if (datos.pisoArea.Trim() == "")
                return "Escriba el piso o área.";
            if (datos.pisoArea.Trim().Length > 20)
                return "El piso o área no puede tener más de 20 caracteres.";

            if (datos.vehiculoId <= 0)
                return "Seleccione el vehículo.";

            if (datos.observaciones.Trim().Length > 200)
                return "La descripción no puede tener más de 200 caracteres.";

            return "";
        }

        /// <summary>
        /// Agrega lista.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="tabla">Tabla de datos.</param>
        private void agregarLista(SqlCommand comando, string nombre, string tipo, DataTable tabla)
        {
            SqlParameter parametro = comando.Parameters.AddWithValue(nombre, tabla);
            parametro.SqlDbType = SqlDbType.Structured;
            parametro.TypeName = tipo;
        }

        /// <summary>
        /// Arma una tabla de un solo campo entero para enviarla al procedimiento almacenado.
        /// </summary>
        /// <param name="columna">Nombre de la columna.</param>
        /// <param name="valores">Valores a procesar.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        private DataTable tablaDeEnteros(string columna, List<int> valores)
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add(columna, typeof(int));

            foreach (int valor in valores)
            {
                tabla.Rows.Add(valor);
            }

            return tabla;
        }
    }
}
