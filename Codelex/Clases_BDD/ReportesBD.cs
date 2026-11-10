using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class ReporteBD : BaseDatos
    {
        public DataTable listarEstadosOrden()
        {
            return consultar(procedimiento("sp_ListarEstadosOrden"));
        }

        public DataTable listarServicios()
        {
            return consultar(procedimiento("sp_ListarServicios"));
        }

        public DataTable ordenes(DateTime desde, DateTime hasta, int estadoId, int servicioId, string cliente)
        {
            SqlCommand comando = conRango("sp_ReporteOrdenes", desde, hasta);
            comando.Parameters.AddWithValue("@estado_orden_id", nulo(estadoId));
            comando.Parameters.AddWithValue("@servicio_id", nulo(servicioId));
            comando.Parameters.AddWithValue("@cliente", cliente);
            return consultar(comando);
        }

        public DataTable asistencia(DateTime desde, DateTime hasta, int empleadoId)
        {
            SqlCommand comando = conRango("sp_ReporteAsistencia", desde, hasta);
            comando.Parameters.AddWithValue("@empleado_id", nulo(empleadoId));
            return consultar(comando);
        }

        // estadoEmpleado: 0 = todos, 1 = activos, 2 = inactivos
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

        private SqlCommand conRango(string nombreProcedimiento, DateTime desde, DateTime hasta)
        {
            SqlCommand comando = procedimiento(nombreProcedimiento);
            comando.Parameters.Add("@desde", SqlDbType.Date).Value = desde.Date;
            comando.Parameters.Add("@hasta", SqlDbType.Date).Value = hasta.Date;
            return comando;
        }

        private object nulo(int valor)
        {
            if (valor > 0)
                return valor;

            return DBNull.Value;
        }
    }
}