using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.BDD
{
    public class Conexion
    {
        private static string cadenaConexion = "Data Source=BERTARTX\\SQLEXPRESS;Initial Catalog=Codelex;Integrated Security=True;Encrypt=False";

        public static SqlConnection ObtenerConexion()
        {
            SqlConnection conexion = new SqlConnection(cadenaConexion);
            try
            {
                conexion.Open();

                if (Sesion.UsuarioId > 0)
                {
                    SqlCommand contexto = new SqlCommand("sp_set_session_context", conexion);
                    contexto.CommandType = CommandType.StoredProcedure;
                    contexto.Parameters.AddWithValue("@key", "usuario_id");
                    contexto.Parameters.AddWithValue("@value", Sesion.UsuarioId);
                    contexto.ExecuteNonQuery();
                }

                return conexion;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al conectar a la base de datos: " + ex.Message);
            }
        }
    }
}
