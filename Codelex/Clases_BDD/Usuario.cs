using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Codelex.BDD
{
    public class Usuario
    {
        public bool ValidarLogin(string usuario, string password)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_ObtenerUsuarioLogin", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@usuario", usuario);

                    SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                    adaptador.Fill(tabla);
                }
            }

            if (tabla.Rows.Count == 0)
                return false;

            string claveGuardada = "" + tabla.Rows[0]["contrasena_hash"];
            return claveGuardada == password;
        }
    }
}
