using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class DatosOrden
    {
        public string Rtn = "";
        public int LocacionId = 0;
        public int ServicioId = 0;
        public int SupervisorId = 0;
        public int VehiculoId = 0;
        public DateTime? FechaSolicitud = null;
        public DateTime? FechaServicio = null;
        public int EstadoId = 0;
        public string Observaciones = "";
        public string Turno = "";
        public string PisoArea = "";

        public List<int> Actividades = new List<int>();
        public List<int> Empleados = new List<int>();
        public List<int> ProductosId = new List<int>();
        public List<int> ProductosCantidad = new List<int>();
        public List<int> EquiposId = new List<int>();
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

        public DataTable ListarActividades()
        {
            return Consultar(Procedimiento("sp_ListarActividades"));
        }

        public DataTable ListarEmpleados()
        {
            return Consultar(Procedimiento("sp_ListarEmpleadosOrden"));
        }

        public DataTable ListarProductosEquipos()
        {
            return Consultar(Procedimiento("sp_ListarProductosEquipos"));
        }

        public string ValidarCantidad(string texto, int stock, out int cantidad)
        {
            cantidad = 0;
            texto = texto.Trim();

            if (texto == "")
                return "Escriba la cantidad.";
            if (!int.TryParse(texto, out cantidad))
                return "La cantidad no es válida.";
            if (cantidad <= 0)
                return "La cantidad debe ser mayor que 0.";
            if (cantidad > stock)
                return "No hay stock suficiente. Disponible: " + stock + ".";

            return "";
        }

        public bool GuardarOrden(DatosOrden datos, out string mensaje, out string ordenId)
        {
            ordenId = "";

            mensaje = ValidarOrden(datos);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento("sp_InsertarOrden");
            comando.Parameters.AddWithValue("@identificacionCliente", datos.Rtn.Trim());
            comando.Parameters.AddWithValue("@locacion_id", datos.LocacionId);
            comando.Parameters.AddWithValue("@servicio_id", datos.ServicioId);
            comando.Parameters.AddWithValue("@supervisor_id", datos.SupervisorId);
            comando.Parameters.AddWithValue("@vehiculo_id", datos.VehiculoId);
            comando.Parameters.AddWithValue("@fecha_solicitud", datos.FechaSolicitud.Value);
            comando.Parameters.AddWithValue("@fecha_servicio", datos.FechaServicio.Value);
            comando.Parameters.AddWithValue("@estado_orden_id", datos.EstadoId);
            comando.Parameters.AddWithValue("@observaciones", datos.Observaciones.Trim());
            comando.Parameters.AddWithValue("@turno", datos.Turno.Trim());
            comando.Parameters.AddWithValue("@piso_area", datos.PisoArea.Trim());

            AgregarLista(comando, "@actividades", "dbo.tipo_lista_actividades",
                         TablaDeEnteros("actividad_id", datos.Actividades));
            AgregarLista(comando, "@empleados", "dbo.tipo_lista_empleados",
                         TablaDeEnteros("empleado_id", datos.Empleados));
            AgregarLista(comando, "@productos", "dbo.tipo_lista_productos",
                         TablaDeProductos(datos.ProductosId, datos.ProductosCantidad));
            AgregarLista(comando, "@equipos", "dbo.tipo_lista_equipos",
                         TablaDeEnteros("equipo_id", datos.EquiposId));

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
            if (datos.Rtn.Trim() == "")
                return "Seleccione un cliente de la lista inferior.";
            if (datos.LocacionId <= 0)
                return "Seleccione la locación.";
            if (datos.ServicioId <= 0)
                return "Seleccione el tipo de servicio.";
            if (datos.Actividades.Count == 0)
                return "La orden necesita al menos una actividad.";

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

        private DataTable TablaDeProductos(List<int> ids, List<int> cantidades)
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add("producto_id", typeof(int));
            tabla.Columns.Add("cantidad", typeof(int));

            for (int i = 0; i < ids.Count; i++)
            {
                tabla.Rows.Add(ids[i], cantidades[i]);
            }

            return tabla;
        }
    }
}
