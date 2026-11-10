using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class MiTrabajoBD : BaseDatos
    {
        public DataTable ordenes(int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_MisOrdenes");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }

        public DataTable detalle(string ordenId, int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_DetalleMiOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }

        public DataTable listas(string ordenId, int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_ListasMiOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }
    }
}