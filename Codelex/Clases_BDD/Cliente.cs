using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class Cliente
    {
        public const string TipoRtn = "RTN";
        public const string TipoIdentidad = "Identidad";

        public List<string> TiposIdentificacion()
        {
            List<string> tipos = new List<string>();
            tipos.Add(TipoRtn);
            tipos.Add(TipoIdentidad);
            return tipos;
        }

        // RTN: 14 dígitos. Identidad: 13 dígitos.
        public int LargoIdentificacion(string tipo)
        {
            if (tipo == TipoIdentidad)
                return 13;

            return 14;
        }

        public string TipoPorLongitud(string numero)
        {
            if (LimpiarNumero(numero).Length == 13)
                return TipoIdentidad;

            return TipoRtn;
        }

        // Los números de identidad y RTN se guardan sin guiones ni espacios.
        public static string LimpiarNumero(string texto)
        {
            return texto.Replace("-", "").Replace(" ", "").Trim();
        }

        public string ValidarIdentificacion(string tipo, string numero)
        {
            numero = LimpiarNumero(numero);

            if (numero == "")
                return "El RTN / identidad es obligatorio.";
            if (tipo != TipoRtn && tipo != TipoIdentidad)
                return "Seleccione si es RTN o identidad.";
            if (!SoloNumeros(numero))
                return "El " + tipo + " solo puede tener números, sin guiones ni espacios.";
            if (numero.Trim('0') == "")
                return "El " + tipo + " no puede ser todo ceros.";

            int largo = LargoIdentificacion(tipo);
            if (numero.Length != largo)
                return "El " + tipo + " debe tener " + largo + " dígitos. Tiene " + numero.Length + ".";

            return "";
        }

        public DataTable BuscarClientes(string texto, string buscarPor, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = Procedimiento("sp_BuscarClientes");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@buscar_por", buscarPor);
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

        public bool InsertarCliente(string tipo, string rtn, string empresa, string nombre, string apellido,
                                    string telefono, string correo, out string mensaje)
        {
            return GuardarCliente("sp_InsertarCliente", 0, tipo, rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        public bool ActualizarCliente(int clienteId, string tipo, string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo, out string mensaje)
        {
            return GuardarCliente("sp_ActualizarCliente", clienteId, tipo, rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        public bool CambiarEstadoCliente(string rtn, bool activo, out string mensaje)
        {
            SqlCommand comando = Procedimiento("sp_CambiarEstadoCliente");
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            comando.Parameters.AddWithValue("@estado_cliente", activo);

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

        private string ValidarCliente(string tipo, string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo)
        {
            telefono = telefono.Trim();
            correo = correo.Trim();

            string problema = ValidarIdentificacion(tipo, rtn);
            if (problema != "")
                return problema;

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

        private bool GuardarCliente(string procedimiento, int clienteId, string tipo, string rtn, string empresa,
                                    string nombre, string apellido, string telefono, string correo, out string mensaje)
        {
            mensaje = ValidarCliente(tipo, rtn, empresa, nombre, apellido, telefono, correo);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento(procedimiento);

            if (clienteId > 0)
                comando.Parameters.AddWithValue("@cliente_id", clienteId);

            comando.Parameters.AddWithValue("@identificacion_cliente", LimpiarNumero(rtn));
            comando.Parameters.AddWithValue("@tipo_identificacion", tipo);
            comando.Parameters.AddWithValue("@nombre_empresa", empresa.Trim());
            comando.Parameters.AddWithValue("@nombre_cliente", nombre.Trim());
            comando.Parameters.AddWithValue("@apellido_cliente", apellido.Trim());
            comando.Parameters.AddWithValue("@telefono_cliente", telefono.Trim());
            comando.Parameters.AddWithValue("@correo_cliente", correo.Trim());

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
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
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
