using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class MiTrabajoBD : BaseDatos
    {
        public DataTable Ordenes(int empleadoId)
        {
            SqlCommand comando = Procedimiento("sp_MisOrdenes");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return Consultar(comando);
        }

        public DataTable Detalle(string ordenId, int empleadoId)
        {
            SqlCommand comando = Procedimiento("sp_DetalleMiOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return Consultar(comando);
        }

        public DataTable Listas(string ordenId, int empleadoId)
        {
            SqlCommand comando = Procedimiento("sp_ListasMiOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return Consultar(comando);
        }
    }
}