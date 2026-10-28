using Microsoft.Data.SqlClient;
using OtpNet;
using QRCoder;
using System.Data;
using System.IO;
using System.Windows.Media.Imaging;

namespace Codelex.BDD
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string SecretoDobleFactor { get; set; } = "";
        public bool DobleFactorActivo { get; set; }

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
            if (claveGuardada != password)
                return false;

            UsuarioId = Convert.ToInt32(tabla.Rows[0]["usuario_id"]);
            NombreUsuario = "" + tabla.Rows[0]["usuario"];
            SecretoDobleFactor = "" + tabla.Rows[0]["totp_secreto"];
            DobleFactorActivo = Convert.ToBoolean(tabla.Rows[0]["totp_activo"]);
            return true;
        }

        public string GenerarSecreto()
        {
            byte[] llave = KeyGeneration.GenerateRandomKey(20);
            return Base32Encoding.ToString(llave);
        }

        public BitmapImage CrearCodigoQR(string secreto)
        {
            string enlace = "otpauth://totp/Codelex:" + Uri.EscapeDataString(NombreUsuario)
                          + "?secret=" + secreto + "&issuer=Codelex";

            QRCodeGenerator generador = new QRCodeGenerator();
            QRCodeData datos = generador.CreateQrCode(enlace, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qr = new PngByteQRCode(datos);
            byte[] png = qr.GetGraphic(10);

            BitmapImage imagen = new BitmapImage();
            using (MemoryStream memoria = new MemoryStream(png))
            {
                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.StreamSource = memoria;
                imagen.EndInit();
            }

            return imagen;
        }

        public bool ConfirmarDobleFactor(string secreto, string codigo, out string mensaje)
        {
            mensaje = ValidarCodigoDobleFactor(secreto, codigo);
            if (mensaje != "")
                return false;

            if (DobleFactorActivo)
                return true;

            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_ActivarDobleFactor", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@usuario_id", UsuarioId);
                    comando.Parameters.AddWithValue("@totp_secreto", secreto);
                    comando.Parameters.Add(resultado);
                    comando.Parameters.Add(textoMensaje);
                    comando.ExecuteNonQuery();
                }
            }

            mensaje = "" + textoMensaje.Value;
            return Convert.ToInt32(resultado.Value) == 1;
        }

        private string ValidarCodigoDobleFactor(string secreto, string codigo)
        {
            codigo = codigo.Trim();

            if (codigo == "")
                return "Escribe el código de 6 dígitos.";
            if (codigo.Length != 6)
                return "El código debe tener 6 dígitos.";

            foreach (char c in codigo)
            {
                if (!char.IsDigit(c))
                    return "El código solo puede tener números.";
            }

            Totp totp = new Totp(Base32Encoding.ToBytes(secreto));
            long paso;
            if (!totp.VerifyTotp(codigo, out paso, new VerificationWindow(1, 1)))
                return "El código no es correcto o ya expiró. Inténtalo de nuevo.";

            return "";
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
