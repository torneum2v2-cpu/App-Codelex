using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class Cliente
    {
        public DataTable BuscarClientes(string texto, string buscarPor, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = Procedimiento("sp_BuscarClientes");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@buscarPor", buscarPor);
            comando.Parameters.AddWithValue("@estado", estado);
            return Consultar(comando);
        }

        public DataTable ObtenerCliente(string rtn)
        {
            return ConsultarPorCliente("sp_ObtenerCliente", rtn);
        }

        public DataTable LocacionesPorCliente(string rtn)
        {
            return ConsultarPorCliente("sp_ListarLocacionesPorCliente", rtn);
        }

        public DataTable OrdenesPorCliente(string rtn)
        {
            return ConsultarPorCliente("sp_ListarOrdenesPorCliente", rtn);
        }

        public DataTable FacturasPorCliente(string rtn)
        {
            return ConsultarPorCliente("sp_ListarFacturasPorCliente", rtn);
        }

        public bool InsertarCliente(string rtn, string empresa, string nombre, string apellido,
                                    string telefono, string correo, out string mensaje)
        {
            return GuardarCliente("sp_InsertarCliente", rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        public bool ActualizarCliente(string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo, out string mensaje)
        {
            return GuardarCliente("sp_ActualizarCliente", rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        public bool CambiarEstadoCliente(string rtn, bool activo, out string mensaje)
        {
            SqlCommand comando = Procedimiento("sp_CambiarEstadoCliente");
            comando.Parameters.AddWithValue("@identificacionCliente", rtn.Trim());
            comando.Parameters.AddWithValue("@estadoCliente", activo);

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

        private string ValidarCliente(string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo)
        {
            rtn = rtn.Trim();
            telefono = telefono.Trim();
            correo = correo.Trim();

            if (rtn == "")
                return "El RTN / identidad es obligatorio.";
            if (!SoloNumeros(rtn))
                return "El RTN / identidad solo puede tener números, sin guiones ni espacios.";
            if (rtn.Length < 13 || rtn.Length > 14)
                return "El RTN / identidad debe tener 13 o 14 dígitos. Tiene " + rtn.Length + ".";

            if (empresa.Trim() == "")
                return "La empresa es obligatoria.";
            if (nombre.Trim() == "")
                return "El nombre del contacto es obligatorio.";
            if (apellido.Trim() == "")
                return "El apellido del contacto es obligatorio.";

            if (telefono == "")
                return "El teléfono es obligatorio.";
            if (!SoloNumeros(telefono))
                return "El teléfono solo puede tener números, sin guiones ni espacios.";
            if (telefono.Length != 8)
                return "El teléfono debe tener 8 dígitos. Tiene " + telefono.Length + ".";

            if (correo == "")
                return "El correo es obligatorio.";
            if (!correo.Contains("@") || !correo.Contains("."))
                return "El correo no es válido. Ejemplo: nombre@empresa.com";

            return "";
        }

        private bool GuardarCliente(string procedimiento, string rtn, string empresa, string nombre, string apellido,
                                    string telefono, string correo, out string mensaje)
        {
            mensaje = ValidarCliente(rtn, empresa, nombre, apellido, telefono, correo);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento(procedimiento);
            comando.Parameters.AddWithValue("@identificacionCliente", rtn.Trim());
            comando.Parameters.AddWithValue("@nombreEmpresa", empresa.Trim());
            comando.Parameters.AddWithValue("@nombreCliente", nombre.Trim());
            comando.Parameters.AddWithValue("@apellidoCliente", apellido.Trim());
            comando.Parameters.AddWithValue("@telefonoCliente", telefono.Trim());
            comando.Parameters.AddWithValue("@correoCliente", correo.Trim());

            return Ejecutar(comando, out mensaje);
        }

        private SqlCommand Procedimiento(string nombre)
        {
            SqlCommand comando = new SqlCommand(nombre);
            comando.CommandType = CommandType.StoredProcedure;
            return comando;
        }

        private DataTable ConsultarPorCliente(string procedimiento, string rtn)
        {
            SqlCommand comando = Procedimiento(procedimiento);
            comando.Parameters.AddWithValue("@identificacionCliente", rtn.Trim());
            return Consultar(comando);
        }

        private DataTable Consultar(SqlCommand comando)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                comando.Connection = conexion;
                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                adaptador.Fill(tabla);
            }

            return tabla;
        }

        private bool Ejecutar(SqlCommand comando, out string mensaje)
        {
            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;
            comando.Parameters.Add(resultado);

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;
            comando.Parameters.Add(textoMensaje);

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                comando.Connection = conexion;
                comando.ExecuteNonQuery();
            }

            mensaje = "" + textoMensaje.Value;
            return Convert.ToInt32(resultado.Value) == 1;
        }
    }
}
