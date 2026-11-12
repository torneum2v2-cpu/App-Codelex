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
        /// Lista las actividades de una orden con su estado (hecha o pendiente).
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <returns>Tabla con las actividades de la orden.</returns>
        public DataTable listarActividades(string ordenId)
        {
            SqlCommand comando = procedimiento("sp_ListarActividadesPorOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return consultar(comando);
        }

        /// <summary>
        /// Marca una actividad de la orden como hecha o la deja otra vez pendiente.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <param name="actividadId">Identificador de la actividad.</param>
        /// <param name="hecha">true para marcarla como hecha; false para dejarla pendiente.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool marcarActividad(string ordenId, int actividadId, bool hecha, out string mensaje)
        {
            if (ordenId.Trim() == "" || actividadId <= 0)
            {
                mensaje = "Seleccione una actividad.";
                return false;
            }

            SqlCommand comando = procedimiento("sp_MarcarActividadOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@actividad_id", actividadId);
            comando.Parameters.AddWithValue("@estado", hecha);

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Registra si el cliente quedó conforme con el servicio de una orden completada.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <param name="conforme">true si el cliente quedó conforme; false si no.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool registrarConformidad(string ordenId, bool conforme, out string mensaje)
        {
            if (ordenId.Trim() == "")
            {
                mensaje = "Seleccione una orden.";
                return false;
            }

            SqlCommand comando = procedimiento("sp_RegistrarConformidadOrden");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@conformidad", conforme);

            return ejecutar(comando, out mensaje);
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