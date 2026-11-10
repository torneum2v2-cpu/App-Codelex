using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class ReporteBD : BaseDatos
    {
        /// <summary>
        /// Lista estados orden.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarEstadosOrden()
        {
            return consultar(procedimiento("sp_ListarEstadosOrden"));
        }

        /// <summary>
        /// Lista servicios.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarServicios()
        {
            return consultar(procedimiento("sp_ListarServicios"));
        }

        /// <summary>
        /// Genera el reporte de órdenes según los filtros indicados.
        /// </summary>
        /// <param name="desde">Fecha inicial.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <param name="estadoId">Identificador del estado.</param>
        /// <param name="servicioId">Identificador de servicio.</param>
        /// <param name="cliente">Texto del cliente a buscar.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable ordenes(DateTime desde, DateTime hasta, int estadoId, int servicioId, string cliente)
        {
            SqlCommand comando = conRango("sp_ReporteOrdenes", desde, hasta);
            comando.Parameters.AddWithValue("@estado_orden_id", nulo(estadoId));
            comando.Parameters.AddWithValue("@servicio_id", nulo(servicioId));
            comando.Parameters.AddWithValue("@cliente", cliente);
            return consultar(comando);
        }

        /// <summary>
        /// Genera el reporte de asistencia en un rango de fechas.
        /// </summary>
        /// <param name="desde">Fecha inicial.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable asistencia(DateTime desde, DateTime hasta, int empleadoId)
        {
            SqlCommand comando = conRango("sp_ReporteAsistencia", desde, hasta);
            comando.Parameters.AddWithValue("@empleado_id", nulo(empleadoId));
            return consultar(comando);
        }

        // estadoEmpleado: 0 = todos, 1 = activos, 2 = inactivos
        /// <summary>
        /// Genera el reporte de empleados en un rango de fechas.
        /// </summary>
        /// <param name="desde">Fecha inicial.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <param name="cargoId">Identificador del cargo.</param>
        /// <param name="estadoEmpleado">Estado del empleado (1 activo, 0 inactivo, -1 todos).</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable empleados(DateTime desde, DateTime hasta, int cargoId, int estadoEmpleado)
        {
            object estado = DBNull.Value;
            if (estadoEmpleado == 1)
                estado = true;
            else if (estadoEmpleado == 2)
                estado = false;

            SqlCommand comando = conRango("sp_ReporteEmpleados", desde, hasta);
            comando.Parameters.AddWithValue("@cargo_id", nulo(cargoId));
            comando.Parameters.AddWithValue("@estado", estado);
            return consultar(comando);
        }

        /// <summary>
        /// Crea el comando con el rango de fechas ya cargado.
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado.</param>
        /// <param name="desde">Fecha inicial.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <returns>Comando listo para ejecutar.</returns>
        private SqlCommand conRango(string nombreProcedimiento, DateTime desde, DateTime hasta)
        {
            SqlCommand comando = procedimiento(nombreProcedimiento);
            comando.Parameters.Add("@desde", SqlDbType.Date).Value = desde.Date;
            comando.Parameters.Add("@hasta", SqlDbType.Date).Value = hasta.Date;
            return comando;
        }

        /// <summary>
        /// Convierte el valor 0 en NULL para los filtros opcionales.
        /// </summary>
        /// <param name="valor">Valor a usar.</param>
        /// <returns>El valor, o NULL si es 0.</returns>
        private object nulo(int valor)
        {
            if (valor > 0)
                return valor;

            return DBNull.Value;
        }
    }
}