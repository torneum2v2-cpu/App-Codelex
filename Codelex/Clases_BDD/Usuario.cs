using Microsoft.Data.SqlClient;
using System.Data;

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

        public string ValidarContras(string contra, string contra2)
        {
            if (contra != contra2)
                return "Las contraseñas ingresadas deben de coincidir entre sí";
            if (contra.Length < 8)
                return "Tu contraseña debe tener al menos 8 caracteres";
            if (!TieneMayuscula(contra))
                return "Por favor utiliza al menos una mayúscula.";
            if (!TieneNumero(contra))
                return "Tu contraseña debe utilizar por lo menos un dígito";

            return "";
        }

        public bool CambiarContrasena(string correo, string contra, string contra2, out string mensaje)
        {
            mensaje = ValidarContras(contra, contra2);
            if (mensaje != "")
                return false;

            if (correo.Trim() == "")
            {
                mensaje = "No se encontró el correo de la cuenta.";
                return false;
            }

            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_CambiarContrasenaPorCorreo", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@correo", correo.Trim());
                    comando.Parameters.AddWithValue("@contrasena_hash", contra);
                    comando.Parameters.Add(resultado);
                    comando.Parameters.Add(textoMensaje);
                    comando.ExecuteNonQuery();
                }
            }

            mensaje = "" + textoMensaje.Value;
            return Convert.ToInt32(resultado.Value) == 1;
        }

        private bool TieneMayuscula(string texto)
        {
            foreach (char c in texto)
            {
                if (char.IsUpper(c))
                    return true;
            }
            return false;
        }

        private bool TieneNumero(string texto)
        {
            foreach (char c in texto)
            {
                if (char.IsDigit(c))
                    return true;
            }
            return false;
        }
    }
}
