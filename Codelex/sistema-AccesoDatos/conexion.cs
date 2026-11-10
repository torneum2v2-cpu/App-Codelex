using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    public class Conexion
    {
        private static string cadenaConexion = "Data Source=BERTARTX\\SQLEXPRESS;Initial Catalog=Codelex;Integrated Security=True;Encrypt=False;Connect Timeout=5";

        /// <summary>
        /// Obtiene conexión.
        /// </summary>
        /// <returns>Resultado de tipo SqlConnection.</returns>
        public static SqlConnection obtenerConexion()
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
                conexion.Dispose();
                throw new Exception("No se pudo conectar a la base de datos. Verifica que el servidor esté encendido y que tengas conexión.\n\nDetalle: " + ex.Message, ex);
            }
        }
    }
}