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
        public int cantidadActividades = 0;

        public List<int> empleados = new List<int>();
    }

    class TotalesCotizacion
    {
        public decimal subtotal = 0;
        public decimal descuento = 0;
        public string textoDescuento = "Descuento";
        public decimal isv = 0;
        public decimal total = 0;
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
        /// Lista los empleados que pueden supervisar una orden.
        /// El administrador ve todos los supervisores libres; un supervisor solo se ve a sí mismo.
        /// </summary>
        /// <returns>Tabla con empleado_id y nombre_completo.</returns>
        public DataTable listarSupervisores()
        {
            DataTable supervisores = consultar(procedimiento("sp_ListarSupervisoresOrden"));

            if (Sesion.esAdministrador())
                return supervisores;

            DataTable soloYo = supervisores.Clone();
            foreach (DataRow fila in supervisores.Rows)
            {
                if (Convert.ToInt32(fila["empleado_id"]) == Sesion.EmpleadoId)
                    soloYo.ImportRow(fila);
            }

            return soloYo;
        }

        /// <summary>
        /// Lee el encabezado de una cotización (cliente, fechas, servicio y total).
        /// </summary>
        /// <param name="cotizacionId">Identificador de cotización.</param>
        /// <returns>Tabla con una fila, o vacía si la cotización no existe.</returns>
        public DataTable obtenerCotizacion(string cotizacionId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerCotizacion");
            comando.Parameters.AddWithValue("@cotizacion_id", cotizacionId.Trim());
            return consultar(comando);
        }

        /// <summary>
        /// Calcula los totales de una cotización a partir de sus líneas guardadas.
        /// Las líneas sin actividad son el descuento (precio negativo).
        /// El ISV es lo que falta del subtotal con descuento para llegar al total guardado.
        /// </summary>
        /// <param name="detalle">Líneas de la cotización (sp_DetalleCotizacion).</param>
        /// <param name="totalGuardado">Total con ISV guardado en la cotización.</param>
        /// <returns>Totales listos para mostrar o imprimir.</returns>
        public TotalesCotizacion calcularTotalesCotizacion(DataTable detalle, decimal totalGuardado)
        {
            TotalesCotizacion totales = new TotalesCotizacion();

            foreach (DataRow fila in detalle.Rows)
            {
                decimal subtotal = Convert.ToDecimal(fila["subtotal"]);

                if (Convert.ToInt32(fila["actividad_id"]) != 0)
                {
                    totales.subtotal += subtotal;
                }
                else
                {
                    totales.descuento += -subtotal;
                    totales.textoDescuento = "" + fila["concepto"];
                }
            }

            totales.total = totalGuardado;
            totales.isv = totalGuardado - (totales.subtotal - totales.descuento);

            if (totales.isv < 0)
                totales.isv = 0;

            return totales;
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

            // El supervisor solo ve las cotizaciones que él hizo; el administrador ve todas.
            if (!Sesion.esAdministrador())
                comando.Parameters.AddWithValue("@usuario_id", Sesion.UsuarioId);

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
        /// Valida un paso del asistente de orden.
        /// Paso 1: cotización. Paso 2: fechas, estado, supervisor y locación. Paso 3: empleados, turno y vehículo.
        /// </summary>
        /// <param name="datos">Datos escritos en el asistente.</param>
        /// <param name="paso">Número del paso (1, 2 o 3).</param>
        /// <returns>Mensaje de error, o vacío si el paso está bien.</returns>
        public string validarPaso(DatosOrden datos, int paso)
        {
            if (paso == 1)
            {
                if (datos.cotizacionId.Trim() == "")
                    return "Seleccione una cotización.";
                if (datos.rtn.Trim() == "")
                    return "La cotización no tiene un cliente registrado.";
            }
            else if (paso == 2)
            {
                if (datos.cantidadActividades == 0)
                    return "La orden necesita al menos una actividad.";
                if (!datos.fechaSolicitud.HasValue)
                    return "Seleccione la fecha de solicitud.";
                if (!datos.fechaServicio.HasValue)
                    return "Seleccione la fecha de limpieza.";
                if (datos.fechaServicio.Value < datos.fechaSolicitud.Value)
                    return "La fecha de limpieza no puede ser anterior a la fecha de solicitud.";
                if (datos.estadoId <= 0)
                    return "Seleccione el estado de la orden.";
                if (datos.supervisorId <= 0)
                {
                    if (!Sesion.esAdministrador())
                        return "Usted ya tiene una orden en proceso o su usuario no tiene cargo de supervisor. Termine la orden en proceso para crear otra.";

                    return "Seleccione el supervisor.";
                }
                if (!Sesion.esAdministrador() && datos.supervisorId != Sesion.EmpleadoId)
                    return "Solo puede crear órdenes a su nombre.";
                if (datos.locacionId <= 0)
                    return "Seleccione la locación.";
                if (datos.observaciones.Trim().Length > 200)
                    return "La descripción no puede tener más de 200 caracteres.";
            }
            else if (paso == 3)
            {
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
            }

            return "";
        }

        /// <summary>
        /// Valida todos los pasos de la orden antes de guardarla.
        /// </summary>
        /// <param name="datos">Datos de la orden.</param>
        /// <returns>Mensaje de error, o vacío si todo está bien.</returns>
        private string validarOrden(DatosOrden datos)
        {
            for (int paso = 1; paso <= 3; paso++)
            {
                string problema = validarPaso(datos, paso);
                if (problema != "")
                    return problema;
            }

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
