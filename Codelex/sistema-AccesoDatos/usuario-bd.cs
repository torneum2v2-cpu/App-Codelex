using Codelex.AccesoDatos;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Codelex.AccesoDatos
{
    class UsuarioBD
    {


        /// <summary>
        /// Crea el objeto con sus valores iniciales.
        /// </summary>
        public UsuarioBD()
        {

        }

        /// <summary>
        /// Lista roles.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarRoles()
        {
            return consultar(procedimiento("sp_ListarRoles"));
        }

        /// <summary>
        /// Busca usuarios.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="buscarPor">Campo por el que se busca.</param>
        /// <param name="filtroEstado">Filtro de estado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable buscarUsuarios(string texto, string buscarPor, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = procedimiento("sp_BuscarUsuarios");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@buscar_por", buscarPor);
            comando.Parameters.AddWithValue("@estado", estado);
            return consultar(comando);
        }

        /// <summary>
        /// Busca empleados sin usuario.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable buscarEmpleadosSinUsuario(string texto)
        {
            SqlCommand comando = procedimiento("sp_BuscarEmpleadosSinUsuario");
            comando.Parameters.AddWithValue("@texto", texto);
            return consultar(comando);
        }

        /// <summary>
        /// Indica si existe usuario.
        /// </summary>
        /// <param name="usuario">Nombre de usuario.</param>
        /// <param name="excluirUsuarioId">Identificador de excluir usuario.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool existeUsuario(string usuario, int excluirUsuarioId)
        {
            SqlCommand comando = procedimiento("sp_ExisteUsuario");
            comando.Parameters.AddWithValue("@usuario", usuario.Trim());
            comando.Parameters.AddWithValue("@excluir_usuario_id", excluirUsuarioId);

            DataTable tabla = consultar(comando);
            return Convert.ToInt32(tabla.Rows[0]["cantidad"]) > 0;
        }

        /// <summary>
        /// Busca un nombre de usuario que todavía no esté usado.
        /// </summary>
        /// <param name="usuarioBase">Nombre de usuario base.</param>
        /// <returns>Nombre de usuario libre.</returns>
        public string usuarioDisponible(string usuarioBase)
        {
            string candidato = usuarioBase;
            int numero = 2;

            while (candidato != "" && existeUsuario(candidato, 0))
            {
                candidato = usuarioBase + numero;
                numero++;
            }

            return candidato;
        }

        /// <summary>
        /// Crea usuario.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        /// <param name="usuario">Nombre de usuario.</param>
        /// <param name="contrasena">Contraseña.</param>
        /// <param name="rolId">Identificador de rol.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool crearUsuario(int empleadoId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            return guardarUsuario(true, empleadoId, usuario, contrasena, rolId, out mensaje);
        }

        /// <summary>
        /// Actualiza usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <param name="usuario">Nombre de usuario.</param>
        /// <param name="contrasena">Contraseña.</param>
        /// <param name="rolId">Identificador de rol.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool actualizarUsuario(int usuarioId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            return guardarUsuario(false, usuarioId, usuario, contrasena, rolId, out mensaje);
        }

        /// <summary>
        /// Cambia estado usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <param name="activo">true para activar, false para desactivar.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool cambiarEstadoUsuario(int usuarioId, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);
            comando.Parameters.AddWithValue("@estado_usuario", activo);

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Elimina usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool eliminarUsuario(int usuarioId, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_EliminarUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Valida usuario.
        /// </summary>
        /// <param name="usuario">Nombre de usuario.</param>
        /// <param name="contrasena">Contraseña.</param>
        /// <param name="rolId">Identificador de rol.</param>
        /// <param name="esNuevo">Es nuevo.</param>
        /// <returns>Texto resultante.</returns>
        private string validarUsuario(string usuario, string contrasena, int rolId, bool esNuevo)
        {
            usuario = usuario.Trim();

            if (usuario == "")
                return "El usuario es obligatorio.";
            if (usuario.Contains(" "))
                return "El usuario no puede llevar espacios.";

            if (rolId <= 0)
                return "Seleccione un rol.";

            if (esNuevo && contrasena == "")
                return "La contraseña es obligatoria.";

            return "";
        }

        // Hoy el login compara la contrasena tal cual. Si el equipo usa hash, se cambia aqui y en ValidarLogin.
        /// <summary>
        /// Prepara contraseña.
        /// </summary>
        /// <param name="contrasena">Contraseña.</param>
        /// <returns>Texto resultante.</returns>
        private string prepararContrasena(string contrasena)
        {
            // Una contraseña vacía significa "no cambiarla": se deja vacía para que la base conserve la anterior.
            if (contrasena == "")
                return "";

            return Contrasenas.hashear(contrasena);
        }

        /// <summary>
        /// Guarda usuario.
        /// </summary>
        /// <param name="esNuevo">Es nuevo.</param>
        /// <param name="id">Identificador.</param>
        /// <param name="usuario">Nombre de usuario.</param>
        /// <param name="contrasena">Contraseña.</param>
        /// <param name="rolId">Identificador de rol.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        private bool guardarUsuario(bool esNuevo, int id, string usuario, string contrasena, int rolId, out string mensaje)
        {
            mensaje = validarUsuario(usuario, contrasena, rolId, esNuevo);
            if (mensaje != "")
                return false;

            SqlCommand comando;
            if (esNuevo)
            {
                comando = procedimiento("sp_InsertarUsuario");
                comando.Parameters.AddWithValue("@empleado_id", id);
            }
            else
            {
                comando = procedimiento("sp_ActualizarUsuario");
                comando.Parameters.AddWithValue("@usuario_id", id);
            }

            comando.Parameters.AddWithValue("@usuario", usuario.Trim());
            comando.Parameters.AddWithValue("@contrasena_hash", prepararContrasena(contrasena));
            comando.Parameters.AddWithValue("@rol_id", rolId);

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