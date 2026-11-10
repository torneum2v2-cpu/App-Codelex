using Codelex.AccesoDatos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class Cliente
    {
        public const string TIPO_RTN = "RTN";
        public const string TIPO_IDENTIDAD = "Identidad";

        public List<string> tiposIdentificacion()
        {
            List<string> tipos = new List<string>();
            tipos.Add(TIPO_RTN);
            tipos.Add(TIPO_IDENTIDAD);
            return tipos;
        }

        // RTN: 14 dígitos. Identidad: 13 dígitos.
        public int largoIdentificacion(string tipo)
        {
            if (tipo == TIPO_IDENTIDAD)
                return 13;

            return 14;
        }

        public string tipoPorLongitud(string numero)
        {
            if (limpiarNumero(numero).Length == 13)
                return TIPO_IDENTIDAD;

            return TIPO_RTN;
        }

        // Los números de identidad y RTN se guardan sin guiones ni espacios.
        public static string limpiarNumero(string texto)
        {
            return texto.Replace("-", "").Replace(" ", "").Trim();
        }

        public string validarIdentificacion(string tipo, string numero)
        {
            numero = limpiarNumero(numero);

            if (numero == "")
                return "El RTN / identidad es obligatorio.";
            if (tipo != TIPO_RTN && tipo != TIPO_IDENTIDAD)
                return "Seleccione si es RTN o identidad.";
            if (!soloNumeros(numero))
                return "El " + tipo + " solo puede tener números, sin guiones ni espacios.";
            if (numero.Trim('0') == "")
                return "El " + tipo + " no puede ser todo ceros.";

            int largo = largoIdentificacion(tipo);
            if (numero.Length != largo)
                return "El " + tipo + " debe tener " + largo + " dígitos. Tiene " + numero.Length + ".";

            return "";
        }

        public DataTable buscarClientes(string texto, string buscarPor, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = procedimiento("sp_BuscarClientes");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@buscar_por", buscarPor);
            comando.Parameters.AddWithValue("@estado", estado);
            return consultar(comando);
        }

        public DataTable obtenerCliente(string rtn)
        {
            return consultarPorCliente("sp_ObtenerCliente", rtn);
        }

        public DataTable locacionesPorCliente(string rtn)
        {
            return consultarPorCliente("sp_ListarLocacionesPorCliente", rtn);
        }

        public DataTable ordenesPorCliente(string rtn)
        {
            return consultarPorCliente("sp_ListarOrdenesPorCliente", rtn);
        }

        public DataTable facturasPorCliente(string rtn)
        {
            return consultarPorCliente("sp_ListarFacturasPorCliente", rtn);
        }

        public bool insertarCliente(string tipo, string rtn, string empresa, string nombre, string apellido,
                                    string telefono, string correo, out string mensaje)
        {
            return guardarCliente("sp_InsertarCliente", 0, tipo, rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        public bool actualizarCliente(int clienteId, string tipo, string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo, out string mensaje)
        {
            return guardarCliente("sp_ActualizarCliente", clienteId, tipo, rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        public bool cambiarEstadoCliente(string rtn, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoCliente");
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            comando.Parameters.AddWithValue("@estado_cliente", activo);

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

        private string validarCliente(string tipo, string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo)
        {
            telefono = telefono.Trim();
            correo = correo.Trim();

            string problema = validarIdentificacion(tipo, rtn);
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
            if (!soloNumeros(telefono))
                return "El teléfono solo puede tener números, sin guiones ni espacios.";
            if (telefono.Length != 8)
                return "El teléfono debe tener 8 dígitos. Tiene " + telefono.Length + ".";

            if (correo == "")
                return "El correo es obligatorio.";
            if (!correo.Contains("@") || !correo.Contains("."))
                return "El correo no es válido. Ejemplo: nombre@empresa.com";

            return "";
        }

        private bool guardarCliente(string nombreProcedimiento, int clienteId, string tipo, string rtn, string empresa,
                                    string nombre, string apellido, string telefono, string correo, out string mensaje)
        {
            mensaje = validarCliente(tipo, rtn, empresa, nombre, apellido, telefono, correo);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento(nombreProcedimiento);

            if (clienteId > 0)
                comando.Parameters.AddWithValue("@cliente_id", clienteId);

            comando.Parameters.AddWithValue("@identificacion_cliente", limpiarNumero(rtn));
            comando.Parameters.AddWithValue("@tipo_identificacion", tipo);
            comando.Parameters.AddWithValue("@nombre_empresa", empresa.Trim());
            comando.Parameters.AddWithValue("@nombre_cliente", nombre.Trim());
            comando.Parameters.AddWithValue("@apellido_cliente", apellido.Trim());
            comando.Parameters.AddWithValue("@telefono_cliente", telefono.Trim());
            comando.Parameters.AddWithValue("@correo_cliente", correo.Trim());

            return ejecutar(comando, out mensaje);
        }

        private SqlCommand procedimiento(string nombre)
        {
            SqlCommand comando = new SqlCommand(nombre);
            comando.CommandType = CommandType.StoredProcedure;
            return comando;
        }

        private DataTable consultarPorCliente(string nombreProcedimiento, string rtn)
        {
            SqlCommand comando = procedimiento(nombreProcedimiento);
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            return consultar(comando);
        }

        private DataTable consultar(SqlCommand comando)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.obtenerConexion())
            {
                comando.Connection = conexion;
                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                adaptador.Fill(tabla);
            }

            return tabla;
        }

        private bool ejecutar(SqlCommand comando, out string mensaje)
        {
            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;
            comando.Parameters.Add(resultado);

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;
            comando.Parameters.Add(textoMensaje);

            using (SqlConnection conexion = Conexion.obtenerConexion())
            {
                comando.Connection = conexion;
                comando.ExecuteNonQuery();
            }

            mensaje = "" + textoMensaje.Value;
            return Convert.ToInt32(resultado.Value) == 1;
        }
    }
}
