using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using static Codelex.BDD.Conexion;

namespace Codelex.BDD
{
    public class Usuario


    {
        public bool ValidarLogin(string usuario, string password)
        {
            Conexion cn = new Conexion();
            bool esValido = false;

            string query = "SELECT COUNT(1) FROM USUARIOS WHERE usuario = @usuario AND contrasena_hash = @clave";

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@usuario", usuario);
                    comando.Parameters.AddWithValue("@clave", password);

                    int conteo = Convert.ToInt32(comando.ExecuteScalar());
                    if (conteo > 0)
                    {
                        esValido = true;
                    }
                }
            }

            return esValido;
        }





    }
}
