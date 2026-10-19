using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.Data.SqlClient;


namespace Codelex
{
    public class Conexion
    {
        private static string cadenaConexion = "Server=DESKTOP-G6UO5OF\\SQLEXPRESS; Database=Codelex; Integrated Security=true; TrustServerCertificate=True;";

        public class Abrir_conexion()
        {
            SqlConnection conexion = new SqlConnection(cadenaConexion);

            public static SqlConnection ObtenerConexion()
            {
                SqlConnection conexion = new SqlConnection(cadenaConexion);
                try
                {
                    conexion.Open();
                    MessageBox.Show("Conexion Correcta");
                    return conexion; // Retorna la conexión lista para usarse
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al conectar a la base de datos: " + ex.Message);
                }






            }
        }
    }
}
