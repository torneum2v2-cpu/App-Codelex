using Codelex.AccesoDatos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class FotoPerfilBD : BaseDatos
    {
        /// <summary>
        /// Obtiene la foto de perfil guardada de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <returns>Resultado de tipo .</returns>
        public byte[]? obtener(int usuarioId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerFotoUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);

            DataTable tabla = consultar(comando);
            if (tabla.Rows.Count == 0 || tabla.Rows[0][0] == DBNull.Value)
                return null;

            return (byte[])tabla.Rows[0][0];
        }

        // foto = null borra la foto del usuario.
        /// <summary>
        /// Guarda la foto de perfil de un usuario.
        /// </summary>
        /// <param name="usuarioId">Identificador del usuario.</param>
        /// <param name="foto">Bytes de la foto.</param>
        public void guardar(int usuarioId, byte[]? foto)
        {
            SqlCommand comando = procedimiento("sp_GuardarFotoUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);

            SqlParameter parametro = comando.Parameters.Add("@foto", SqlDbType.VarBinary, -1);
            parametro.Value = (object?)foto ?? DBNull.Value;

            using (SqlConnection conexion = Conexion.obtenerConexion())
            {
                comando.Connection = conexion;
                comando.ExecuteNonQuery();
            }
        }
    }
}