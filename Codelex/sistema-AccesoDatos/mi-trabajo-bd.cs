using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class MiTrabajoBD : BaseDatos
    {
        /// <summary>
        /// Lista las órdenes asignadas a un empleado.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable ordenes(int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_MisOrdenes");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }

        /// <summary>
        /// Obtiene el detalle de una orden asignada al empleado.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable detalle(string ordenId, int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_DetalleMiOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }

        /// <summary>
        /// Obtiene las listas (actividades, productos y equipos) de una orden del empleado.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listas(string ordenId, int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_ListasMiOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }
    }
}