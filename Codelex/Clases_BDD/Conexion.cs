using Microsoft.Data.SqlClient;

namespace Codelex.BDD
{
    public class Conexion
    {
        private static string cadenaConexion = "Data Source=localhost;Initial Catalog=Codelex;Integrated Security=True;Encrypt=False";

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
                throw new Exception("Error al conectar a la base de datos: " + ex.Message);
            }
        }
    }
}
