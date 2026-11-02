using Microsoft.Data.SqlClient;

namespace Codelex.BDD
{
    public class Conexion
    {
        private static string cadenaConexion = "Data Source=BERTARTX\\SQLEXPRESS;Initial Catalog=Codelex;Integrated Security=True;Encrypt=False;Connect Timeout=5";

        public static SqlConnection ObtenerConexion()
        {
            SqlConnection conexion = new SqlConnection(cadenaConexion);
            try
            {
                conexion.Open();
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