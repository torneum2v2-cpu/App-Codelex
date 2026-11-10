using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class GestionOrdenesBD : BaseDatos
    {
        /// <summary>
        /// Lista órdenes.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarOrdenes()
        {
            return consultar(procedimiento("sp_ListarOrdenesGestion"));
        }

        /// <summary>
        /// Obtiene detalle.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable obtenerDetalle(string ordenId)
        {
            SqlCommand comando = procedimiento("sp_DetalleOrdenGestion");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return consultar(comando);
        }

        // Columnas: seccion (1 actividades, 2 empleados, 3 productos, 4 equipos) y texto.
        /// <summary>
        /// Lista detalle.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarDetalle(string ordenId)
        {
            SqlCommand comando = procedimiento("sp_ListasOrdenGestion");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return consultar(comando);
        }

        /// <summary>
        /// Cambia estado.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <param name="estadoId">Identificador del estado.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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