using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class Empleado : BaseDatos
    {
        public bool existeCorreo(string correo)
        {
            if (correo.Trim() == "")
                return false;

            SqlCommand comando = procedimiento("sp_ExisteCorreoEmpleado");
            comando.Parameters.AddWithValue("@correo", correo.Trim());

            DataTable tabla = consultar(comando);
            return Convert.ToInt32(tabla.Rows[0]["cantidad"]) > 0;
        }

        public DataTable listarCargos()
        {
            return consultar(procedimiento("sp_ListarCargos"));
        }

        public DataTable buscarEmpleados(string texto, string buscarPor, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = procedimiento("sp_BuscarEmpleados");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@buscar_por", buscarPor);
            comando.Parameters.AddWithValue("@estado", estado);
            return consultar(comando);
        }

        public DataTable obtenerEmpleado(int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }

        public DataTable asistenciasPorEmpleado(int empleadoId, DateTime desde, DateTime hasta)
        {
            return consultarPorRango("sp_AsistenciasPorEmpleado", empleadoId, desde, hasta);
        }

        public DataTable ordenesPorEmpleado(int empleadoId, DateTime desde, DateTime hasta)
        {
            return consultarPorRango("sp_OrdenesPorEmpleado", empleadoId, desde, hasta);
        }

        public bool insertarEmpleado(int cargoId, string nombre, string apellido, string identidad,
                                     string telefono, string correo,
                                     out int empleadoId, out string mensaje)
        {
            empleadoId = 0;
            mensaje = validarEmpleado(cargoId, nombre, apellido, identidad, telefono, correo);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_InsertarEmpleado");
            agregarParametrosEmpleado(comando, cargoId, nombre, apellido, identidad, telefono, correo);

            SqlParameter id = new SqlParameter("@empleado_id", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = ejecutar(comando, out mensaje);
            if (guardado && id.Value != DBNull.Value)
                empleadoId = Convert.ToInt32(id.Value);
            return guardado;
        }

        public bool actualizarEmpleado(int empleadoId, int cargoId, string nombre, string apellido, string identidad,
                                       string telefono, string correo, out string mensaje)
        {
            mensaje = validarEmpleado(cargoId, nombre, apellido, identidad, telefono, correo);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_ActualizarEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            agregarParametrosEmpleado(comando, cargoId, nombre, apellido, identidad, telefono, correo);

            return ejecutar(comando, out mensaje);
        }

        public bool cambiarEstadoEmpleado(int empleadoId, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            comando.Parameters.AddWithValue("@estado_empleado", activo);

            return ejecutar(comando, out mensaje);
        }

        public bool soloNumeros(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        private string validarEmpleado(int cargoId, string nombre, string apellido, string identidad,
                                       string telefono, string correo)
        {
            identidad = Cliente.limpiarNumero(identidad);
            telefono = telefono.Trim();
            correo = correo.Trim();

            if (cargoId <= 0)
                return "Seleccione el cargo del empleado.";
            if (nombre.Trim() == "")
                return "El nombre es obligatorio.";
            if (apellido.Trim() == "")
                return "El apellido es obligatorio.";

            if (identidad == "")
                return "La identidad es obligatoria.";
            if (!soloNumeros(identidad))
                return "La identidad solo puede tener números, sin guiones ni espacios.";
            if (identidad.Length != 13)
                return "La identidad debe tener 13 dígitos. Tiene " + identidad.Length + ".";

            if (telefono != "")
            {
                if (!soloNumeros(telefono))
                    return "El teléfono solo puede tener números, sin guiones ni espacios.";
                if (telefono.Length != 8)
                    return "El teléfono debe tener 8 dígitos. Tiene " + telefono.Length + ".";
            }

            if (correo != "" && (!correo.Contains("@") || !correo.Contains(".")))
                return "El correo no es válido. Ejemplo: nombre@empresa.com";

            return "";
        }

        private void agregarParametrosEmpleado(SqlCommand comando, int cargoId, string nombre, string apellido,
                                               string identidad, string telefono, string correo)
        {
            comando.Parameters.AddWithValue("@cargo_id", cargoId);
            comando.Parameters.AddWithValue("@nombre_empleado", nombre.Trim());
            comando.Parameters.AddWithValue("@apellido_empleado", apellido.Trim());
            comando.Parameters.AddWithValue("@identidad", Cliente.limpiarNumero(identidad));
            comando.Parameters.AddWithValue("@telefono_empleado", telefono.Trim());
            comando.Parameters.AddWithValue("@correo_empleado", correo.Trim());
        }

        private DataTable consultarPorRango(string nombreProcedimiento, int empleadoId, DateTime desde, DateTime hasta)
        {
            SqlCommand comando = procedimiento(nombreProcedimiento);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            comando.Parameters.Add("@desde", SqlDbType.Date).Value = desde.Date;
            comando.Parameters.Add("@hasta", SqlDbType.Date).Value = hasta.Date;
            return consultar(comando);
        }
    }
}
