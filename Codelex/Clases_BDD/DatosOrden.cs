using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class DatosOrden
    {
        public string CotizacionId = "";
        public string Rtn = "";
        public int LocacionId = 0;
        public int SupervisorId = 0;
        public int VehiculoId = 0;
        public DateTime? FechaSolicitud = null;
        public DateTime? FechaServicio = null;
        public int EstadoId = 0;
        public string Observaciones = "";
        public string Turno = "";
        public string PisoArea = "";
        public int PersonasNecesarias = 0;

        public List<int> Empleados = new List<int>();
    }

    class Orden : BaseDatos
    {
        public DataTable ListarEstadosOrden()
        {
            return Consultar(Procedimiento("sp_ListarEstadosOrden"));
        }

        public DataTable ListarServicios()
        {
            return Consultar(Procedimiento("sp_ListarServicios"));
        }

        public DataTable ListarEmpleados()
        {
            return Consultar(Procedimiento("sp_ListarEmpleadosOrden"));
        }

        public DataTable ListarCotizaciones(string texto, string rtn)
        {
            SqlCommand comando = Procedimiento("sp_ListarCotizacionesOrden");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            return Consultar(comando);
        }

        public DataTable DetalleCotizacion(string cotizacionId)
        {
            SqlCommand comando = Procedimiento("sp_DetalleCotizacion");
            comando.Parameters.AddWithValue("@cotizacion_id", cotizacionId);
            return Consultar(comando);
        }

        public DataTable ResumenCotizacion(string cotizacionId)
        {
            SqlCommand comando = Procedimiento("sp_ResumenCotizacion");
            comando.Parameters.AddWithValue("@cotizacion_id", cotizacionId);
            return Consultar(comando);
        }

        // Una línea del resumen: lo que necesita la orden y lo que hay disponible.
        public string TextoNecesidad(DataRow fila)
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

        public bool GuardarOrden(DatosOrden datos, out string mensaje, out string ordenId)
        {
            ordenId = "";

            mensaje = ValidarOrden(datos);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento("sp_InsertarOrden");
            comando.Parameters.AddWithValue("@identificacion_cliente", datos.Rtn.Trim());
            comando.Parameters.AddWithValue("@locacion_id", datos.LocacionId);
            comando.Parameters.AddWithValue("@supervisor_id", datos.SupervisorId);
            comando.Parameters.AddWithValue("@vehiculo_id", datos.VehiculoId);
            comando.Parameters.AddWithValue("@fecha_solicitud", datos.FechaSolicitud.Value);
            comando.Parameters.AddWithValue("@fecha_servicio", datos.FechaServicio.Value);
            comando.Parameters.AddWithValue("@estado_orden_id", datos.EstadoId);
            comando.Parameters.AddWithValue("@observacion", datos.Observaciones.Trim());
            comando.Parameters.AddWithValue("@turno", datos.Turno.Trim());
            comando.Parameters.AddWithValue("@piso_area", datos.PisoArea.Trim());
            comando.Parameters.AddWithValue("@cotizacion_id", datos.CotizacionId.Trim());

            AgregarLista(comando, "@empleados", "dbo.tipo_lista_empleados",
                         TablaDeEnteros("empleado_id", datos.Empleados));

            SqlParameter ordenParametro = new SqlParameter("@orden_id", SqlDbType.VarChar, 50);
            ordenParametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(ordenParametro);

            bool guardada = Ejecutar(comando, out mensaje);

            if (guardada)
            {
                ordenId = "" + ordenParametro.Value;
                mensaje = "Orden " + ordenId + " registrada correctamente.";
            }

            return guardada;
        }

        private string ValidarOrden(DatosOrden datos)
        {
            if (datos.CotizacionId.Trim() == "")
                return "Seleccione una cotización.";
            if (datos.Rtn.Trim() == "")
                return "La cotización no tiene un cliente registrado.";
            if (datos.LocacionId <= 0)
                return "Seleccione la locación.";
            if (!datos.FechaSolicitud.HasValue)
                return "Seleccione la fecha de solicitud.";
            if (!datos.FechaServicio.HasValue)
                return "Seleccione la fecha de limpieza.";
            if (datos.FechaServicio.Value < datos.FechaSolicitud.Value)
                return "La fecha de limpieza no puede ser anterior a la fecha de solicitud.";

            if (datos.EstadoId <= 0)
                return "Seleccione el estado de la orden.";
            if (datos.SupervisorId <= 0)
                return "Seleccione el supervisor.";

            if (datos.Empleados.Count == 0)
                return "Asigne al menos un empleado.";
            if (datos.Empleados.Count < datos.PersonasNecesarias)
                return "La cotización necesita " + datos.PersonasNecesarias + " empleados. Asigne más.";
            if (datos.Turno.Trim() == "")
                return "Seleccione el turno.";
            if (datos.PisoArea.Trim() == "")
                return "Escriba el piso o área.";
            if (datos.PisoArea.Trim().Length > 20)
                return "El piso o área no puede tener más de 20 caracteres.";

            if (datos.VehiculoId <= 0)
                return "Seleccione el vehículo.";

            if (datos.Observaciones.Trim().Length > 200)
                return "La descripción no puede tener más de 200 caracteres.";

            return "";
        }

        private void AgregarLista(SqlCommand comando, string nombre, string tipo, DataTable tabla)
        {
            SqlParameter parametro = comando.Parameters.AddWithValue(nombre, tabla);
            parametro.SqlDbType = SqlDbType.Structured;
            parametro.TypeName = tipo;
        }

        private DataTable TablaDeEnteros(string columna, List<int> valores)
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
