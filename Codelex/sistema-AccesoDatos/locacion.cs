using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class Locacion : BaseDatos
    {
        public string[] tipos = { "Oficina", "Edificio", "Casa", "Local comercial", "Bodega",
                                  "Clínica / Hospital", "Centro educativo", "Industria", "Otro" };

        /// <summary>
        /// Inserta locación.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="direccion">Dirección.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool insertarLocacion(string rtn, string nombre, string tipo, string direccion, out string mensaje)
        {
            mensaje = validarLocacion(0, rtn, nombre, tipo, direccion);
            if (mensaje != "")
                return false;

            SqlParameter locacionId = new SqlParameter("@locacion_id", SqlDbType.Int);
            locacionId.Direction = ParameterDirection.Output;

            SqlCommand comando = procedimiento("sp_InsertarLocacion");
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            comando.Parameters.AddWithValue("@nombre_locacion", nombre.Trim());
            comando.Parameters.AddWithValue("@direccion", direccion.Trim());
            comando.Parameters.AddWithValue("@tipo_locacion", tipo.Trim());
            comando.Parameters.Add(locacionId);

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Actualiza los datos de una locación.
        /// </summary>
        /// <param name="locacionId">Identificador de la locación.</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="direccion">Dirección.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool actualizarLocacion(int locacionId, string rtn, string nombre, string tipo, string direccion, out string mensaje)
        {
            mensaje = validarLocacion(locacionId, rtn, nombre, tipo, direccion);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_ActualizarLocacion");
            comando.Parameters.AddWithValue("@locacion_id", locacionId);
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());
            comando.Parameters.AddWithValue("@nombre_locacion", nombre.Trim());
            comando.Parameters.AddWithValue("@direccion", direccion.Trim());
            comando.Parameters.AddWithValue("@tipo_locacion", tipo.Trim());

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Activa o desactiva una locación.
        /// </summary>
        /// <param name="locacionId">Identificador de la locación.</param>
        /// <param name="activa">true para activarla; false para desactivarla.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool cambiarEstadoLocacion(int locacionId, bool activa, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoLocacion");
            comando.Parameters.AddWithValue("@locacion_id", locacionId);
            comando.Parameters.AddWithValue("@estado_locacion", activa);

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Lista todas las locaciones de un cliente, activas e inactivas.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <returns>Tabla con las locaciones del cliente.</returns>
        public DataTable locacionesDeCliente(string rtn)
        {
            SqlCommand comando = procedimiento("sp_BuscarLocaciones");
            comando.Parameters.AddWithValue("@identificacion_cliente", rtn.Trim());

            return consultar(comando);
        }

        /// <summary>
        /// Valida locación.
        /// </summary>
        /// <param name="locacionId">Identificador de la locación que se edita (0 si es nueva).</param>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="direccion">Dirección.</param>
        /// <returns>Texto resultante.</returns>
        private string validarLocacion(int locacionId, string rtn, string nombre, string tipo, string direccion)
        {
            if (rtn.Trim() == "")
                return "Seleccione un cliente.";

            string problema = Validacion.validarTexto(nombre, "El nombre de la locación", 3, 60);
            if (problema != "")
                return problema;

            if (tipo.Trim() == "")
                return "Seleccione el tipo de locación.";

            problema = Validacion.validarDireccion(direccion, 80);
            if (problema != "")
                return problema;

            DataTable existentes = locacionesDeCliente(rtn);

            foreach (DataRow fila in existentes.Rows)
            {
                bool esOtra = Convert.ToInt32(fila["locacion_id"]) != locacionId;

                if (esOtra && ("" + fila["nombre_locacion"]).Trim().ToLower() == nombre.Trim().ToLower())
                    return "Este cliente ya tiene una locación con ese nombre.";
            }

            return "";
        }
    }
}