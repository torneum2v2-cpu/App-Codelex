using Codelex.AccesoDatos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class Cliente
    {
        public const string TIPO_RTN = "RTN";
        public const string TIPO_IDENTIDAD = "Identidad";

        /// <summary>
        /// Lista los tipos de identificación disponibles (RTN e identidad).
        /// </summary>
        /// <returns>Lista con los elementos obtenidos.</returns>
        public List<string> tiposIdentificacion()
        {
            List<string> tipos = new List<string>();
            tipos.Add(TIPO_RTN);
            tipos.Add(TIPO_IDENTIDAD);
            return tipos;
        }

        // RTN: 14 dígitos. Identidad: 13 dígitos.
        /// <summary>
        /// Indica cuántos dígitos debe tener el número según el tipo (RTN 14, identidad 13).
        /// </summary>
        /// <param name="tipo">Tipo de dato.</param>
        /// <returns>Cantidad de dígitos que debe tener (14 para RTN, 13 para identidad).</returns>
        public int largoIdentificacion(string tipo)
        {
            if (tipo == TIPO_IDENTIDAD)
                return 13;

            return 14;
        }

        /// <summary>
        /// Deduce si el número es RTN o identidad según su cantidad de dígitos.
        /// </summary>
        /// <param name="numero">Número a revisar.</param>
        /// <returns>RTN o Identidad, según la cantidad de dígitos.</returns>
        public string tipoPorLongitud(string numero)
        {
            if (limpiarNumero(numero).Length == 13)
                return TIPO_IDENTIDAD;

            return TIPO_RTN;
        }

        // Los números de identidad y RTN se guardan sin guiones ni espacios.
        /// <summary>
        /// Limpia número.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>Texto resultante.</returns>
        public static string limpiarNumero(string texto)
        {
            return texto.Replace("-", "").Replace(" ", "").Trim();
        }

        /// <summary>
        /// Valida identificación.
        /// </summary>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="numero">Número a revisar.</param>
        /// <returns>Texto resultante.</returns>
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

            if (tipo == TIPO_RTN)
                return Validacion.validarRtn(numero);

            return Validacion.validarIdentidad(numero, false);
        }

        /// <summary>
        /// Busca clientes.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="buscarPor">Campo por el que se busca.</param>
        /// <param name="filtroEstado">Filtro de estado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
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

        /// <summary>
        /// Obtiene cliente.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable obtenerCliente(string rtn)
        {
            return consultarPorCliente("sp_ObtenerCliente", rtn);
        }

        /// <summary>
        /// Lista las locaciones de un cliente.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable locacionesPorCliente(string rtn)
        {
            return consultarPorCliente("sp_ListarLocacionesPorCliente", rtn);
        }

        /// <summary>
        /// Lista las órdenes de un cliente.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable ordenesPorCliente(string rtn)
        {
            return consultarPorCliente("sp_ListarOrdenesPorCliente", rtn);
        }

        /// <summary>
        /// Lista las facturas de un cliente.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable facturasPorCliente(string rtn)
        {
            return consultarPorCliente("sp_ListarFacturasPorCliente", rtn);
        }

        /// <summary>
        /// Inserta cliente.
        /// </summary>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="empresa">Nombre de la empresa.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool insertarCliente(string tipo, string rtn, string empresa, string nombre, string apellido,
                                    string telefono, string correo, out string mensaje)
        {
            return guardarCliente("sp_InsertarCliente", 0, tipo, rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        /// <summary>
        /// Actualiza cliente.
        /// </summary>
        /// <param name="clienteId">Identificador de cliente.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="empresa">Nombre de la empresa.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool actualizarCliente(int clienteId, string tipo, string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo, out string mensaje)
        {
            return guardarCliente("sp_ActualizarCliente", clienteId, tipo, rtn, empresa, nombre, apellido, telefono, correo, out mensaje);
        }

        /// <summary>
        /// Cambia estado cliente.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="activo">true para activar, false para desactivar.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool cambiarEstadoCliente(string rtn, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoCliente");
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            comando.Parameters.AddWithValue("@estado_cliente", activo);

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Impide escribir caracteres que no sean números.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool soloNumeros(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Valida cliente.
        /// </summary>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="empresa">Nombre de la empresa.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <returns>Texto resultante.</returns>
        private string validarCliente(string tipo, string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo)
        {
            telefono = telefono.Trim();
            correo = correo.Trim();

            string problema = validarIdentificacion(tipo, rtn);
            if (problema != "")
                return problema;

            problema = Validacion.validarEmpresa(empresa, "La empresa", true);
            if (problema != "")
                return problema;

            problema = Validacion.validarNombrePersona(nombre, "El nombre del contacto", true);
            if (problema != "")
                return problema;

            problema = Validacion.validarNombrePersona(apellido, "El apellido del contacto", true);
            if (problema != "")
                return problema;

            problema = Validacion.validarTelefono(telefono, true);
            if (problema != "")
                return problema;

            return Validacion.validarCorreo(correo, true, 50);
        }

        /// <summary>
        /// Guarda cliente.
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado.</param>
        /// <param name="clienteId">Identificador de cliente.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="empresa">Nombre de la empresa.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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
            comando.Parameters.AddWithValue("@telefono_cliente", Validacion.normalizarTelefono(telefono));
            comando.Parameters.AddWithValue("@correo_cliente", correo.Trim());

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Crea un comando listo para llamar a un procedimiento almacenado.
        /// </summary>
        /// <param name="nombre">Nombre.</param>
        /// <returns>Comando listo para ejecutar.</returns>
        private SqlCommand procedimiento(string nombre)
        {
            SqlCommand comando = new SqlCommand(nombre);
            comando.CommandType = CommandType.StoredProcedure;
            return comando;
        }

        /// <summary>
        /// Consulta por cliente.
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Tabla con los datos consultados.</returns>
        private DataTable consultarPorCliente(string nombreProcedimiento, string rtn)
        {
            SqlCommand comando = procedimiento(nombreProcedimiento);
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            return consultar(comando);
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado y devuelve el resultado en una tabla.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <returns>Tabla con los datos consultados.</returns>
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

        /// <summary>
        /// Ejecuta un procedimiento almacenado que responde con un resultado y un mensaje.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si el procedimiento respondió resultado 1; false en caso contrario.</returns>
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
