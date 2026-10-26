using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class Empleado : BaseDatos
    {
        public bool ExisteCorreo(string correo)
        {
            if (correo.Trim() == "")
                return false;

            SqlCommand comando = Procedimiento("sp_ExisteCorreoEmpleado");
            comando.Parameters.AddWithValue("@correo", correo.Trim());

            DataTable tabla = Consultar(comando);
            return Convert.ToInt32(tabla.Rows[0]["cantidad"]) > 0;
        }

        public DataTable ListarCargos()
        {
            return Consultar(Procedimiento("sp_ListarCargos"));
        }

        public DataTable BuscarEmpleados(string texto, string buscarPor, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = Procedimiento("sp_BuscarEmpleados");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@buscarPor", buscarPor);
            comando.Parameters.AddWithValue("@estado", estado);
            return Consultar(comando);
        }

        public DataTable ObtenerEmpleado(int empleadoId)
        {
            SqlCommand comando = Procedimiento("sp_ObtenerEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return Consultar(comando);
        }

        public DataTable AsistenciasPorEmpleado(int empleadoId, DateTime desde, DateTime hasta)
        {
            return ConsultarPorRango("sp_AsistenciasPorEmpleado", empleadoId, desde, hasta);
        }

        public DataTable OrdenesPorEmpleado(int empleadoId, DateTime desde, DateTime hasta)
        {
            return ConsultarPorRango("sp_OrdenesPorEmpleado", empleadoId, desde, hasta);
        }

        public bool InsertarEmpleado(int cargoId, string nombre, string apellido, string identidad,
                                     string telefono, string correo, string huella,
                                     out int empleadoId, out string mensaje)
        {
            empleadoId = 0;
            mensaje = ValidarEmpleado(cargoId, nombre, apellido, identidad, telefono, correo, huella);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento("sp_InsertarEmpleado");
            AgregarParametrosEmpleado(comando, cargoId, nombre, apellido, identidad, telefono, correo, huella);

            SqlParameter id = new SqlParameter("@empleado_id", SqlDbType.Int);
            id.Direction = ParameterDirection.Output;
            comando.Parameters.Add(id);

            bool guardado = Ejecutar(comando, out mensaje);
            if (guardado && id.Value != DBNull.Value)
                empleadoId = Convert.ToInt32(id.Value);
            return guardado;
        }

        public bool ActualizarEmpleado(int empleadoId, int cargoId, string nombre, string apellido, string identidad,
                                       string telefono, string correo, string huella, out string mensaje)
        {
            mensaje = ValidarEmpleado(cargoId, nombre, apellido, identidad, telefono, correo, huella);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento("sp_ActualizarEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            AgregarParametrosEmpleado(comando, cargoId, nombre, apellido, identidad, telefono, correo, huella);

            return Ejecutar(comando, out mensaje);
        }

        public bool CambiarEstadoEmpleado(int empleadoId, bool activo, out string mensaje)
        {
            SqlCommand comando = Procedimiento("sp_CambiarEstadoEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            comando.Parameters.AddWithValue("@estadoEmpleado", activo);

            return Ejecutar(comando, out mensaje);
        }

        public bool SoloNumeros(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        private string ValidarEmpleado(int cargoId, string nombre, string apellido, string identidad,
                                       string telefono, string correo, string huella)
        {
            identidad = identidad.Trim();
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
            if (!SoloNumeros(identidad))
                return "La identidad solo puede tener números, sin guiones ni espacios.";
            if (identidad.Length != 13)
                return "La identidad debe tener 13 dígitos. Tiene " + identidad.Length + ".";

            if (telefono != "")
            {
                if (!SoloNumeros(telefono))
                    return "El teléfono solo puede tener números, sin guiones ni espacios.";
                if (telefono.Length != 8)
                    return "El teléfono debe tener 8 dígitos. Tiene " + telefono.Length + ".";
            }

            if (correo != "" && (!correo.Contains("@") || !correo.Contains(".")))
                return "El correo no es válido. Ejemplo: nombre@empresa.com";

            if (huella.Trim() == "")
                return "El ID de huella es obligatorio.";

            return "";
        }

        private void AgregarParametrosEmpleado(SqlCommand comando, int cargoId, string nombre, string apellido,
                                               string identidad, string telefono, string correo, string huella)
        {
            comando.Parameters.AddWithValue("@cargo_id", cargoId);
            comando.Parameters.AddWithValue("@nombreEmpleado", nombre.Trim());
            comando.Parameters.AddWithValue("@apellidoEmpleado", apellido.Trim());
            comando.Parameters.AddWithValue("@identidad", identidad.Trim());
            comando.Parameters.AddWithValue("@telefonoEmpleado", telefono.Trim());
            comando.Parameters.AddWithValue("@correoEmpleado", correo.Trim());
            comando.Parameters.AddWithValue("@huella_id", huella.Trim());
        }

        private DataTable ConsultarPorRango(string procedimiento, int empleadoId, DateTime desde, DateTime hasta)
        {
            SqlCommand comando = Procedimiento(procedimiento);
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            comando.Parameters.Add("@desde", SqlDbType.Date).Value = desde.Date;
            comando.Parameters.Add("@hasta", SqlDbType.Date).Value = hasta.Date;
            return Consultar(comando);
        }
    }
}
