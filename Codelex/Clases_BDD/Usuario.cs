using DSInternals.Win32.WebAuthn;
using DSInternals.Win32.WebAuthn.COSE;
using DSInternals.Win32.WebAuthn.FIDO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Formats.Cbor;
using System.Security.Cryptography;
using System.Text;

namespace Codelex.BDD
{
    public class Usuario
    {
        private const string SitioCodelex = "codelex.hn";

        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public bool TieneLlave { get; set; }
        public byte[] LlaveId { get; set; } = new byte[0];
        public byte[] LlavePublica { get; set; } = new byte[0];

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

            DataRow fila = tabla.Rows[0];
            string claveGuardada = "" + fila["contrasena_hash"];
            if (claveGuardada != password)
                return false;

            UsuarioId = Convert.ToInt32(fila["usuario_id"]);
            NombreUsuario = "" + fila["usuario"];
            NombreCompleto = fila["nombreEmpleado"] + " " + fila["apellidoEmpleado"];
            TieneLlave = fila["passkey_id"] != DBNull.Value;

            if (TieneLlave)
            {
                LlaveId = (byte[])fila["passkey_id"];
                LlavePublica = (byte[])fila["passkey_clave"];
            }

            return true;
        }

        public async Task<string> RegistrarLlave(IntPtr ventana)
        {
            RelyingPartyInformation sitio = new RelyingPartyInformation()
            {
                Id = SitioCodelex,
                Name = "Codelex"
            };

            UserInformation datosUsuario = new UserInformation()
            {
                Id = BitConverter.GetBytes(UsuarioId),
                Name = NombreUsuario,
                DisplayName = NombreCompleto
            };

            byte[] reto = RandomNumberGenerator.GetBytes(32);
            WebAuthnApi api = new WebAuthnApi();
            AttestationPublicKeyCredential respuesta;

            try
            {
                respuesta = await api.AuthenticatorMakeCredentialAsync(sitio, datosUsuario, reto,
                    UserVerificationRequirement.Required,
                    AuthenticatorAttachment.CrossPlatform,
                    pubKeyCredParams: new Algorithm[] { Algorithm.ES256 },
                    credentialHints: PedirTelefono(),
                    windowHandle: new WindowHandle(ventana));
            }
            catch (Exception ex)
            {
                return "No se pudo registrar el teléfono. " + ex.Message;
            }

            string error = ComprobarRegistro(respuesta, reto);
            if (error != "")
                return error;

            AttestedCredentialData llave = LeerDatosRegistro(respuesta)!.AttestedCredentialData!;
            string mensaje;
            if (!GuardarLlave(llave.CredentialId.ToArray(), llave.CredentialPublicKey.ToArray(), out mensaje))
                return mensaje;

            return "";
        }

        public async Task<string> VerificarLlave(IntPtr ventana)
        {
            List<PublicKeyCredentialDescriptor> permitidas = new List<PublicKeyCredentialDescriptor>();
            permitidas.Add(new PublicKeyCredentialDescriptor(LlaveId));

            byte[] reto = RandomNumberGenerator.GetBytes(32);
            WebAuthnApi api = new WebAuthnApi();
            AssertionPublicKeyCredential respuesta;

            try
            {
                respuesta = await api.AuthenticatorGetAssertionAsync(SitioCodelex, reto,
                    UserVerificationRequirement.Required,
                    AuthenticatorAttachment.CrossPlatform,
                    allowCredentials: permitidas,
                    credentialHints: PedirTelefono(),
                    windowHandle: new WindowHandle(ventana));
            }
            catch (Exception ex)
            {
                return "No se pudo verificar con el teléfono. " + ex.Message;
            }

            return ComprobarInicio(respuesta, reto);
        }

        private string ComprobarRegistro(AttestationPublicKeyCredential respuesta, byte[] reto)
        {
            AuthenticatorData? datos = LeerDatosRegistro(respuesta);
            if (respuesta.Response == null || datos == null)
                return "La respuesta del teléfono no es válida.";

            string error = ComprobarDatos(datos, respuesta.Response.ClientDataJson, "webauthn.create", reto);
            if (error != "")
                return error;

            if (datos.AttestedCredentialData == null)
                return "El teléfono no envió la llave de acceso.";

            CredentialPublicKey clave = new CredentialPublicKey(datos.AttestedCredentialData.CredentialPublicKey.ToArray());
            if (clave.Algorithm != Algorithm.ES256)
                return "El tipo de llave del teléfono no es compatible.";

            return "";
        }

        private AuthenticatorData? LeerDatosRegistro(AttestationPublicKeyCredential respuesta)
        {
            if (respuesta.Response == null)
                return null;

            if (respuesta.Response.AuthenticatorData != null)
                return AuthenticatorData.Parse(respuesta.Response.AuthenticatorData);

            if (respuesta.Response.AttestationObject == null)
                return null;

            CborReader lector = new CborReader(respuesta.Response.AttestationObject);
            int? campos = lector.ReadStartMap();

            for (int i = 0; i < campos; i++)
            {
                string nombre = lector.ReadTextString();
                if (nombre == "authData")
                    return AuthenticatorData.Parse(lector.ReadByteString());

                lector.SkipValue();
            }

            return null;
        }

        private string ComprobarInicio(AssertionPublicKeyCredential respuesta, byte[] reto)
        {
            AuthenticatorAssertionResponse? datos = respuesta.Response;
            if (datos == null || datos.AuthenticatorDataParsed == null || datos.AuthenticatorData == null
                || datos.ClientData == null || datos.Signature == null || respuesta.RawId == null)
                return "La respuesta del teléfono no es válida.";

            string error = ComprobarDatos(datos.AuthenticatorDataParsed, datos.ClientDataJson, "webauthn.get", reto);
            if (error != "")
                return error;

            if (!CryptographicOperations.FixedTimeEquals(respuesta.RawId, LlaveId))
                return "Ese teléfono no es el que está registrado para este usuario.";

            byte[] resumenCliente = SHA256.HashData(datos.ClientData);
            byte[] firmado = new byte[datos.AuthenticatorData.Length + resumenCliente.Length];
            datos.AuthenticatorData.CopyTo(firmado, 0);
            resumenCliente.CopyTo(firmado, datos.AuthenticatorData.Length);

            CredentialPublicKey clave = new CredentialPublicKey(LlavePublica);
            if (clave.ECDsa == null || !clave.ECDsa.VerifyData(firmado, datos.Signature, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence))
                return "La respuesta del teléfono no es válida.";

            return "";
        }

        private string ComprobarDatos(AuthenticatorData datos, string? json, string tipo, byte[] reto)
        {
            if (json == null || datos.RelyingPartyIdHash == null)
                return "La respuesta del teléfono no es válida.";

            if (!datos.UserVerified)
                return "El teléfono no confirmó tu identidad con Face ID, huella o reconocimiento facial.";

            byte[] resumenSitio = SHA256.HashData(Encoding.UTF8.GetBytes(SitioCodelex));
            if (!CryptographicOperations.FixedTimeEquals(datos.RelyingPartyIdHash, resumenSitio))
                return "La respuesta del teléfono no es para Codelex.";

            CollectedClientData cliente = CollectedClientData.FromJson(json);
            if (cliente.Type != tipo || cliente.Challenge == null || !CryptographicOperations.FixedTimeEquals(cliente.Challenge, reto))
                return "La respuesta del teléfono no es válida.";

            return "";
        }

        private string[]? PedirTelefono()
        {
            if (WebAuthnApi.IsPublicKeyCredentialHintSupported)
                return new string[] { "hybrid" };

            return null;
        }

        private bool GuardarLlave(byte[] llaveId, byte[] llavePublica, out string mensaje)
        {
            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_GuardarPasskey", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@usuario_id", UsuarioId);
                    comando.Parameters.AddWithValue("@passkey_id", llaveId);
                    comando.Parameters.AddWithValue("@passkey_clave", llavePublica);
                    comando.Parameters.Add(resultado);
                    comando.Parameters.Add(textoMensaje);
                    comando.ExecuteNonQuery();
                }
            }

            mensaje = "" + textoMensaje.Value;
            if (Convert.ToInt32(resultado.Value) != 1)
                return false;

            TieneLlave = true;
            LlaveId = llaveId;
            LlavePublica = llavePublica;
            return true;
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
