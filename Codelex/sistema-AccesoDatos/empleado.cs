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
        /// Revisa si quien tiene la sesión abierta puede asignar ese cargo.
        /// El cargo da el rol del usuario, y el rol avanzado (Administrador) solo lo asigna un usuario avanzado.
        /// </summary>
        /// <param name="cargoId">Identificador del cargo.</param>
        /// <returns>Texto del error, o texto vacío si puede asignarlo.</returns>
        private string validarPermisoCargo(int cargoId)
        {
            return validarPermisoCargo(cargoId, "asignar");
        }

        /// <summary>
        /// Revisa si quien tiene la sesión abierta puede asignar o quitar ese cargo (el de administrador es solo de un usuario avanzado).
        /// </summary>
        /// <param name="cargoId">Identificador del cargo.</param>
        /// <param name="accion">Texto de la acción para el mensaje: "asignar" o "quitar".</param>
        /// <returns>Texto del error, o texto vacío si puede hacerlo.</returns>
        private string validarPermisoCargo(int cargoId, string accion)
        {
            if (Sesion.Nivel == NivelAcceso.Avanzado)
                return "";

            foreach (DataRow fila in listarCargos().Rows)
            {
                bool esAvanzado = ("" + fila["nombre_rol"]).ToLower().Contains("avanzado");

                if (Convert.ToInt32(fila["cargo_id"]) == cargoId && esAvanzado)
                    return "Solo un usuario avanzado puede " + accion + " el cargo " + fila["nombre_cargo"] + ".";
            }

            return "";
        }

        /// <summary>
        /// Revisa que quien tiene la sesión abierta pueda cambiar el cargo del empleado.
        /// Si el cargo no cambia no se revisa nada; si cambia, el cargo de administrador ni se asigna ni se quita sin ser usuario avanzado.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="cargoNuevoId">Identificador del cargo que se quiere guardar.</param>
        /// <returns>Texto del error, o texto vacío si puede hacerlo.</returns>
        private string validarCambioDeCargo(int empleadoId, int cargoNuevoId)
        {
            if (Sesion.Nivel == NivelAcceso.Avanzado)
                return "";

            DataTable actual = obtenerEmpleado(empleadoId);
            if (actual.Rows.Count == 0 || actual.Rows[0]["cargo_id"] == DBNull.Value)
                return validarPermisoCargo(cargoNuevoId, "asignar");

            int cargoActualId = Convert.ToInt32(actual.Rows[0]["cargo_id"]);
            if (cargoActualId == cargoNuevoId)
                return "";

            string problema = validarPermisoCargo(cargoActualId, "quitar");
            if (problema != "")
                return problema;

            return validarPermisoCargo(cargoNuevoId, "asignar");
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
            if (mensaje == "")
                mensaje = validarPermisoCargo(cargoId);
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
            if (mensaje == "")
                mensaje = validarCambioDeCargo(empleadoId, cargoId);
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

            string problema = Validacion.validarNombrePersona(nombre, "El nombre", true, 30);
            if (problema != "")
                return problema;

            problema = Validacion.validarNombrePersona(apellido, "El apellido", true, 30);
            if (problema != "")
                return problema;

            if (identidad == "")
                return "La identidad es obligatoria.";

            problema = Validacion.validarIdentidad(identidad, true);
            if (problema != "")
                return problema;

            problema = Validacion.validarTelefono(telefono, false);
            if (problema != "")
                return problema;

            return Validacion.validarCorreo(correo, false, 50);
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
            comando.Parameters.AddWithValue("@telefono_empleado", Validacion.normalizarTelefono(telefono));
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
