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


        public UsuarioBD()
        {

        }

        public DataTable listarRoles()
        {
            return consultar(procedimiento("sp_ListarRoles"));
        }

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

        public DataTable buscarEmpleadosSinUsuario(string texto)
        {
            SqlCommand comando = procedimiento("sp_BuscarEmpleadosSinUsuario");
            comando.Parameters.AddWithValue("@texto", texto);
            return consultar(comando);
        }

        public bool existeUsuario(string usuario, int excluirUsuarioId)
        {
            SqlCommand comando = procedimiento("sp_ExisteUsuario");
            comando.Parameters.AddWithValue("@usuario", usuario.Trim());
            comando.Parameters.AddWithValue("@excluir_usuario_id", excluirUsuarioId);

            DataTable tabla = consultar(comando);
            return Convert.ToInt32(tabla.Rows[0]["cantidad"]) > 0;
        }

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

        public bool crearUsuario(int empleadoId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            return guardarUsuario(true, empleadoId, usuario, contrasena, rolId, out mensaje);
        }

        public bool actualizarUsuario(int usuarioId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            return guardarUsuario(false, usuarioId, usuario, contrasena, rolId, out mensaje);
        }

        public bool cambiarEstadoUsuario(int usuarioId, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);
            comando.Parameters.AddWithValue("@estado_usuario", activo);

            return ejecutar(comando, out mensaje);
        }

        public bool eliminarUsuario(int usuarioId, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_EliminarUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);

            return ejecutar(comando, out mensaje);
        }

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
        private string prepararContrasena(string contrasena)
        {
            return contrasena;
        }

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

        private SqlCommand procedimiento(string nombre)
        {
            SqlCommand comando = new SqlCommand(nombre);
            comando.CommandType = CommandType.StoredProcedure;
            return comando;
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