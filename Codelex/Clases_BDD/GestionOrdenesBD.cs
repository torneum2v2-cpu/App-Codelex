using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class GestionOrdenesBD : BaseDatos
    {
        public DataTable listarOrdenes()
        {
            return consultar(procedimiento("sp_ListarOrdenesGestion"));
        }

        public DataTable obtenerDetalle(string ordenId)
        {
            SqlCommand comando = procedimiento("sp_DetalleOrdenGestion");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return consultar(comando);
        }

        // Columnas: seccion (1 actividades, 2 empleados, 3 productos, 4 equipos) y texto.
        public DataTable listarDetalle(string ordenId)
        {
            SqlCommand comando = procedimiento("sp_ListasOrdenGestion");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return consultar(comando);
        }

        public bool cambiarEstado(string ordenId, int estadoId, out string mensaje)
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

            SqlCommand comando = procedimiento("sp_CambiarEstadoOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@estado_orden_id", estadoId);

            return ejecutar(comando, out mensaje);
        }
    }
}