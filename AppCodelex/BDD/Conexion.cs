using Microsoft.Data.SqlClient;

namespace AppCodelex.BDD;

public static class Conexion
{
    // Si la base de datos está en otra computadora, cambiar el nombre del servidor (Server=...).
    private static string cadenaConexion = "Server=DESKTOP-G6UO5OF\\SQLEXPRESS; Database=Codelex; Integrated Security=true; TrustServerCertificate=True;";

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
            throw new Exception("Error al conectar a la base de datos: " + ex.Message);
        }
    }
}
