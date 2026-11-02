using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class GestionOrdenesBD : BaseDatos
    {
        public DataTable ListarOrdenes()
        {
            return Consultar(Procedimiento("sp_ListarOrdenesGestion"));
        }

        public DataTable ObtenerDetalle(string ordenId)
        {
            SqlCommand comando = Procedimiento("sp_DetalleOrdenGestion");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return Consultar(comando);
        }

        // Columnas: seccion (1 actividades, 2 empleados, 3 productos, 4 equipos) y texto.
        public DataTable ListarDetalle(string ordenId)
        {
            SqlCommand comando = Procedimiento("sp_ListasOrdenGestion");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return Consultar(comando);
        }

        public bool CambiarEstado(string ordenId, int estadoId, out string mensaje)
        {
            if (ordenId.Trim() == "")
            {
                mensaje = "Seleccione una orden.";
                return false;
            }
            if (estadoId <= 0)
            {
                mensaje = "Seleccione el estado.";
                return false;
            }

            SqlCommand comando = Procedimiento("sp_CambiarEstadoOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@estado_orden_id", estadoId);

            return Ejecutar(comando, out mensaje);
        }
    }
}