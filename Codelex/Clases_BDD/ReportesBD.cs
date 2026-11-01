using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class ReporteBD : BaseDatos
    {
        public DataTable ListarEstadosOrden()
        {
            return Consultar(Procedimiento("sp_ListarEstadosOrden"));
        }

        public DataTable ListarServicios()
        {
            return Consultar(Procedimiento("sp_ListarServicios"));
        }

        public DataTable Ordenes(DateTime desde, DateTime hasta, int estadoId, int servicioId, string cliente)
        {
            SqlCommand comando = ConRango("sp_ReporteOrdenes", desde, hasta);
            comando.Parameters.AddWithValue("@estado_orden_id", Nulo(estadoId));
            comando.Parameters.AddWithValue("@servicio_id", Nulo(servicioId));
            comando.Parameters.AddWithValue("@cliente", cliente);
            return Consultar(comando);
        }

        public DataTable Asistencia(DateTime desde, DateTime hasta, int empleadoId)
        {
            SqlCommand comando = ConRango("sp_ReporteAsistencia", desde, hasta);
            comando.Parameters.AddWithValue("@empleado_id", Nulo(empleadoId));
            return Consultar(comando);
        }

        // estadoEmpleado: 0 = todos, 1 = activos, 2 = inactivos
        public DataTable Empleados(DateTime desde, DateTime hasta, int cargoId, int estadoEmpleado)
        {
            object estado = DBNull.Value;
            if (estadoEmpleado == 1)
                estado = true;
            else if (estadoEmpleado == 2)
                estado = false;

            SqlCommand comando = ConRango("sp_ReporteEmpleados", desde, hasta);
            comando.Parameters.AddWithValue("@cargo_id", Nulo(cargoId));
            comando.Parameters.AddWithValue("@estado", estado);
            return Consultar(comando);
        }

        private SqlCommand ConRango(string procedimiento, DateTime desde, DateTime hasta)
        {
            SqlCommand comando = Procedimiento(procedimiento);
            comando.Parameters.Add("@desde", SqlDbType.Date).Value = desde.Date;
            comando.Parameters.Add("@hasta", SqlDbType.Date).Value = hasta.Date;
            return comando;
        }

        private object Nulo(int valor)
        {
            if (valor > 0)
                return valor;

            return DBNull.Value;
        }
    }
}