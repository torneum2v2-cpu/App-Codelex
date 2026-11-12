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
        /// Cuenta cuántas actividades de la orden ya fueron revisadas (marcadas como hechas).
        /// </summary>
        /// <param name="actividades">Tabla de actividades de la orden (columna estado).</param>
        /// <returns>Cantidad de actividades revisadas.</returns>
        public int contarRevisadas(DataTable actividades)
        {
            int revisadas = 0;

            foreach (DataRow fila in actividades.Rows)
            {
                if (Convert.ToBoolean(fila["estado"]))
                    revisadas++;
            }

            return revisadas;
        }

        /// <summary>
        /// Valida el control de calidad: la orden solo se aprueba si todas sus actividades fueron revisadas.
        /// </summary>
        /// <param name="actividades">Tabla de actividades de la orden (columna estado).</param>
        /// <param name="nota">Nota opcional escrita por quien hace el control.</param>
        /// <returns>Texto del error, o texto vacío si ya se puede aprobar.</returns>
        public string validarControl(DataTable actividades, string nota)
        {
            if (actividades.Rows.Count == 0)
                return "La orden no tiene actividades para revisar.";

            int pendientes = actividades.Rows.Count - contarRevisadas(actividades);
            if (pendientes > 0)
                return "Faltan " + pendientes + " actividades por revisar. La orden sigue en proceso.";

            if (nota.Trim().Length > 200)
                return "La nota del control no puede tener más de 200 caracteres.";

            return "";
        }

        /// <summary>
        /// Aprueba el control de calidad y completa la orden (solo si todas las actividades fueron revisadas).
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <param name="actividades">Tabla de actividades de la orden (columna estado).</param>
        /// <param name="nota">Nota opcional escrita por quien hace el control.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool completarConControl(string ordenId, DataTable actividades, string nota, out string mensaje)
        {
            mensaje = validarControl(actividades, nota);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_CompletarOrdenConControl");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            comando.Parameters.AddWithValue("@nota", nota.Trim());

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