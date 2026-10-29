using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Codelex.Clases_BDD
{
    class UsuarioBD
    {

        public UsuarioBD()
        {

        }

        public DataTable ListarRoles()
        {
            return Consultar(Procedimiento("sp_ListarRoles"));
        }

        public DataTable BuscarUsuarios(string texto, string buscarPor, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = Procedimiento("sp_BuscarUsuarios");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@buscarPor", buscarPor);
            comando.Parameters.AddWithValue("@estado", estado);
            return Consultar(comando);
        }

        public DataTable BuscarEmpleadosSinUsuario(string texto)
        {
            SqlCommand comando = Procedimiento("sp_BuscarEmpleadosSinUsuario");
            comando.Parameters.AddWithValue("@texto", texto);
            return Consultar(comando);
        }

        public bool ExisteUsuario(string usuario, int excluirUsuarioId)
        {
            SqlCommand comando = Procedimiento("sp_ExisteUsuario");
            comando.Parameters.AddWithValue("@usuario", usuario.Trim());
            comando.Parameters.AddWithValue("@excluirUsuarioId", excluirUsuarioId);

            DataTable tabla = Consultar(comando);
            return Convert.ToInt32(tabla.Rows[0]["cantidad"]) > 0;
        }

        public string UsuarioDisponible(string usuarioBase)
        {
            string candidato = usuarioBase;
            int numero = 2;

            while (candidato != "" && ExisteUsuario(candidato, 0))
            {
                candidato = usuarioBase + numero;
                numero++;
            }

            return candidato;
        }

        public bool CrearUsuario(int empleadoId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            return GuardarUsuario(true, empleadoId, usuario, contrasena, rolId, out mensaje);
        }

        public bool ActualizarUsuario(int usuarioId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            return GuardarUsuario(false, usuarioId, usuario, contrasena, rolId, out mensaje);
        }

        public bool CambiarEstadoUsuario(int usuarioId, bool activo, out string mensaje)
        {
            SqlCommand comando = Procedimiento("sp_CambiarEstadoUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);
            comando.Parameters.AddWithValue("@estadoUsuario", activo);

            return Ejecutar(comando, out mensaje);
        }

        public bool EliminarUsuario(int usuarioId, out string mensaje)
        {
            SqlCommand comando = Procedimiento("sp_EliminarUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);

            return Ejecutar(comando, out mensaje);
        }

        private string ValidarUsuario(string usuario, string contrasena, int rolId, bool esNuevo)
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
        private string PrepararContrasena(string contrasena)
        {
            return contrasena;
        }

        private bool GuardarUsuario(bool esNuevo, int id, string usuario, string contrasena, int rolId, out string mensaje)
        {
            mensaje = ValidarUsuario(usuario, contrasena, rolId, esNuevo);
            if (mensaje != "")
                return false;

            SqlCommand comando;
            if (esNuevo)
            {
                comando = Procedimiento("sp_InsertarUsuario");
                comando.Parameters.AddWithValue("@empleado_id", id);
            }
            else
            {
                comando = Procedimiento("sp_ActualizarUsuario");
                comando.Parameters.AddWithValue("@usuario_id", id);
            }

            comando.Parameters.AddWithValue("@usuario", usuario.Trim());
            comando.Parameters.AddWithValue("@contrasena_hash", PrepararContrasena(contrasena));
            comando.Parameters.AddWithValue("@rol_id", rolId);

            return Ejecutar(comando, out mensaje);
        }

        private SqlCommand Procedimiento(string nombre)
        {
            SqlCommand comando = new SqlCommand(nombre);
            comando.CommandType = CommandType.StoredProcedure;
            return comando;
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