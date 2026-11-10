using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class Empleado : BaseDatos
    {
        /// <summary>
        /// Indica si existe correo.
        /// </summary>
        /// <param name="correo">Correo electrónico.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool existeCorreo(string correo)
        {
            if (correo.Trim() == "")
                return false;

            SqlCommand comando = procedimiento("sp_ExisteCorreoEmpleado");
            comando.Parameters.AddWithValue("@correo", correo.Trim());

            DataTable tabla = consultar(comando);
            return Convert.ToInt32(tabla.Rows[0]["cantidad"]) > 0;
        }

        /// <summary>
        /// Lista cargos.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarCargos()
        {
            return consultar(procedimiento("sp_ListarCargos"));
        }

        /// <summary>
        /// Busca empleados.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="buscarPor">Campo por el que se busca.</param>
        /// <param name="filtroEstado">Filtro de estado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
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

        /// <summary>
        /// Obtiene empleado.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable obtenerEmpleado(int empleadoId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista las asistencias de un empleado en un rango de fechas.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="desde">Fecha inicial.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable asistenciasPorEmpleado(int empleadoId, DateTime desde, DateTime hasta)
        {
            return consultarPorRango("sp_AsistenciasPorEmpleado", empleadoId, desde, hasta);
        }

        /// <summary>
        /// Lista las órdenes de un empleado en un rango de fechas.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="desde">Fecha inicial.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable ordenesPorEmpleado(int empleadoId, DateTime desde, DateTime hasta)
        {
            return consultarPorRango("sp_OrdenesPorEmpleado", empleadoId, desde, hasta);
        }

        /// <summary>
        /// Inserta empleado.
        /// </summary>
        /// <param name="cargoId">Identificador del cargo.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="identidad">Número de identidad (sin guiones).</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="empleadoId">(salida) Identificador de empleado.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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

        /// <summary>
        /// Actualiza empleado.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="cargoId">Identificador del cargo.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="identidad">Número de identidad (sin guiones).</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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

        /// <summary>
        /// Cambia estado empleado.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="activo">true para activar, false para desactivar.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool cambiarEstadoEmpleado(int empleadoId, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoEmpleado");
            comando.Parameters.AddWithValue("@empleado_id", empleadoId);
            comando.Parameters.AddWithValue("@estado_empleado", activo);

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
        /// Valida empleado.
        /// </summary>
        /// <param name="cargoId">Identificador del cargo.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="identidad">Número de identidad (sin guiones).</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <returns>Texto resultante.</returns>
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

        /// <summary>
        /// Agrega parametros empleado.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <param name="cargoId">Identificador del cargo.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="apellido">Apellido.</param>
        /// <param name="identidad">Número de identidad (sin guiones).</param>
        /// <param name="telefono">Teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
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

        /// <summary>
        /// Consulta por rango.
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado.</param>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="desde">Fecha inicial.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <returns>Tabla con los datos consultados.</returns>
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
