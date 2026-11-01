using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class FotoPerfilBD : BaseDatos
    {
        public byte[]? Obtener(int usuarioId)
        {
            SqlCommand comando = Procedimiento("sp_ObtenerFotoUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);

            DataTable tabla = Consultar(comando);
            if (tabla.Rows.Count == 0 || tabla.Rows[0][0] == DBNull.Value)
                return null;

            return (byte[])tabla.Rows[0][0];
        }

        // foto = null borra la foto del usuario.
        public void Guardar(int usuarioId, byte[]? foto)
        {
            SqlCommand comando = Procedimiento("sp_GuardarFotoUsuario");
            comando.Parameters.AddWithValue("@usuario_id", usuarioId);

            SqlParameter parametro = comando.Parameters.Add("@foto", SqlDbType.VarBinary, -1);
            parametro.Value = (object?)foto ?? DBNull.Value;

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                comando.Connection = conexion;
                comando.ExecuteNonQuery();
            }
        }
    }
}