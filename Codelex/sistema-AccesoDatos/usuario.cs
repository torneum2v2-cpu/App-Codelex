using DSInternals.Win32.WebAuthn;
using DSInternals.Win32.WebAuthn.COSE;
using DSInternals.Win32.WebAuthn.FIDO;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Formats.Cbor;
using System.Security.Cryptography;
using System.Text;

namespace Codelex.AccesoDatos
{
    public class Usuario
    {
        private const string SITIO_CODELEX = "codelex.hn";
        public int EmpleadoId { get; set; }
        public string Rol { get; set; } = "";

        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public bool TieneLlave { get; set; }
        public byte[] LlaveId { get; set; } = new byte[0];
        public byte[] LlavePublica { get; set; } = new byte[0];

        /// <summary>
        /// Valida login.
        /// </summary>
        /// <param name="usuario">Nombre de usuario.</param>
        /// <param name="password">Password.</param>
        /// <returns>true si todo es válido; false si hay algun problema.</returns>
        public bool validarLogin(string usuario, string password)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.obtenerConexion())
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
            if (!Contrasenas.verificar(password, claveGuardada))
                return false;

            int idEncontrado = Convert.ToInt32(fila["usuario_id"]);
            if (!Contrasenas.esHash(claveGuardada))
                guardarHash(idEncontrado, password);

            UsuarioId = idEncontrado;
            EmpleadoId = Convert.ToInt32(fila["empleado_id"]);
            Rol = "" + fila["rol"];
            NombreUsuario = "" + fila["usuario"];
            NombreCompleto = fila["nombre_empleado"] + " " + fila["apellido_empleado"];
            TieneLlave = fila["passkey_id"] != DBNull.Value;

            if (TieneLlave)
            {
                LlaveId = (byte[])fila["passkey_id"];
                LlavePublica = (byte[])fila["passkey_clave"];
            }

            return true;
        }

        /// <summary>
        /// Cambia una contraseña antigua en texto plano por su hash (se hace al iniciar sesión).
        /// </summary>
        /// <param name="usuarioId">Id del usuario.</param>
        /// <param name="password">Contraseña que escribió el usuario.</param>
        private void guardarHash(int usuarioId, string password)
        {
            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;

            using (SqlConnection conexion = Conexion.obtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_CambiarContrasena", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@usuario_id", usuarioId);
                    comando.Parameters.AddWithValue("@nuevo_hash", Contrasenas.hashear(password));
                    comando.Parameters.Add(resultado);
                    comando.Parameters.Add(textoMensaje);
                    comando.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Registra llave.
        /// </summary>
        /// <param name="ventana">Ventana a usar.</param>
        /// <returns>Resultado de tipo Task.</returns>
        public async Task<string> registrarLlave(IntPtr ventana)
        {
            RelyingPartyInformation sitio = new RelyingPartyInformation()
            {
                Id = SITIO_CODELEX,
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
                    credentialHints: pedirTelefono(),
                    windowHandle: new WindowHandle(ventana));
            }
            catch (Exception ex)
            {
                return "No se pudo registrar el teléfono. " + ex.Message;
            }

            string error = comprobarRegistro(respuesta, reto);
            if (error != "")
                return error;

            AttestedCredentialData llave = leerDatosRegistro(respuesta)!.AttestedCredentialData!;
            string mensaje;
            if (!guardarLlave(llave.CredentialId.ToArray(), llave.CredentialPublicKey.ToArray(), out mensaje))
                return mensaje;

            return "";
        }

        /// <summary>
        /// Verifica llave.
        /// </summary>
        /// <param name="ventana">Ventana a usar.</param>
        /// <returns>Resultado de tipo Task.</returns>
        public async Task<string> verificarLlave(IntPtr ventana)
        {
            List<PublicKeyCredentialDescriptor> permitidas = new List<PublicKeyCredentialDescriptor>();
            permitidas.Add(new PublicKeyCredentialDescriptor(LlaveId));

            byte[] reto = RandomNumberGenerator.GetBytes(32);
            WebAuthnApi api = new WebAuthnApi();
            AssertionPublicKeyCredential respuesta;

            try
            {
                respuesta = await api.AuthenticatorGetAssertionAsync(SITIO_CODELEX, reto,
                    UserVerificationRequirement.Required,
                    AuthenticatorAttachment.CrossPlatform,
                    allowCredentials: permitidas,
                    credentialHints: pedirTelefono(),
                    windowHandle: new WindowHandle(ventana));
            }
            catch (Exception ex)
            {
                return "No se pudo verificar con el teléfono. " + ex.Message;
            }

            return comprobarInicio(respuesta, reto);
        }

        /// <summary>
        /// Comprueba registro.
        /// </summary>
        /// <param name="respuesta">Respuesta.</param>
        /// <param name="reto">Reto.</param>
        /// <returns>Texto resultante.</returns>
        private string comprobarRegistro(AttestationPublicKeyCredential respuesta, byte[] reto)
        {
            AuthenticatorData? datos = leerDatosRegistro(respuesta);
            if (respuesta.Response == null || datos == null)
                return "La respuesta del teléfono no es válida.";

            string error = comprobarDatos(datos, respuesta.Response.ClientDataJson, "webauthn.create", reto);
            if (error != "")
                return error;

            if (datos.AttestedCredentialData == null)
                return "El teléfono no envió la llave de acceso.";

            CredentialPublicKey clave = new CredentialPublicKey(datos.AttestedCredentialData.CredentialPublicKey.ToArray());
            if (clave.Algorithm != Algorithm.ES256)
                return "El tipo de llave del teléfono no es compatible.";

            return "";
        }

        /// <summary>
        /// Lee datos registro.
        /// </summary>
        /// <param name="respuesta">Respuesta.</param>
        /// <returns>Resultado de tipo AuthenticatorData.</returns>
        private AuthenticatorData? leerDatosRegistro(AttestationPublicKeyCredential respuesta)
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

        /// <summary>
        /// Comprueba inicio.
        /// </summary>
        /// <param name="respuesta">Respuesta.</param>
        /// <param name="reto">Reto.</param>
        /// <returns>Texto resultante.</returns>
        private string comprobarInicio(AssertionPublicKeyCredential respuesta, byte[] reto)
        {
            AuthenticatorAssertionResponse? datos = respuesta.Response;
            if (datos == null || datos.AuthenticatorDataParsed == null || datos.AuthenticatorData == null
                || datos.ClientData == null || datos.Signature == null || respuesta.RawId == null)
                return "La respuesta del teléfono no es válida.";

            string error = comprobarDatos(datos.AuthenticatorDataParsed, datos.ClientDataJson, "webauthn.get", reto);
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

        /// <summary>
        /// Comprueba datos.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <param name="json">Json.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="reto">Reto.</param>
        /// <returns>Texto resultante.</returns>
        private string comprobarDatos(AuthenticatorData datos, string? json, string tipo, byte[] reto)
        {
            if (json == null || datos.RelyingPartyIdHash == null)
                return "La respuesta del teléfono no es válida.";

            if (!datos.UserVerified)
                return "El teléfono no confirmó tu identidad con Face ID, huella o reconocimiento facial.";

            byte[] resumenSitio = SHA256.HashData(Encoding.UTF8.GetBytes(SITIO_CODELEX));
            if (!CryptographicOperations.FixedTimeEquals(datos.RelyingPartyIdHash, resumenSitio))
                return "La respuesta del teléfono no es para Codelex.";

            CollectedClientData cliente = CollectedClientData.FromJson(json);
            if (cliente.Type != tipo || cliente.Challenge == null || !CryptographicOperations.FixedTimeEquals(cliente.Challenge, reto))
                return "La respuesta del teléfono no es válida.";

            return "";
        }

        /// <summary>
        /// Pide teléfono.
        /// </summary>
        /// <returns>Resultado de tipo .</returns>
        private string[]? pedirTelefono()
        {
            if (WebAuthnApi.IsPublicKeyCredentialHintSupported)
                return new string[] { "hybrid" };

            return null;
        }

        /// <summary>
        /// Guarda llave.
        /// </summary>
        /// <param name="llaveId">Identificador de llave.</param>
        /// <param name="llavePublica">Llave publica.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        private bool guardarLlave(byte[] llaveId, byte[] llavePublica, out string mensaje)
        {
            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;

            using (SqlConnection conexion = Conexion.obtenerConexion())
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

        /// <summary>
        /// Valida contras.
        /// </summary>
        /// <param name="contra">Contra.</param>
        /// <param name="contra2">Contra 2.</param>
        /// <returns>Texto resultante.</returns>
        public string validarContras(string contra, string contra2)
        {
            if (contra != contra2)
                return "Las contraseñas ingresadas deben de coincidir entre sí";
            if (contra.Length < 8)
                return "Tu contraseña debe tener al menos 8 caracteres";
            if (!tieneMayuscula(contra))
                return "Por favor utiliza al menos una mayúscula.";
            if (!tieneNumero(contra))
                return "Tu contraseña debe utilizar por lo menos un dígito";

            return "";
        }

        /// <summary>
        /// Cambia contraseña.
        /// </summary>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="contra">Contra.</param>
        /// <param name="contra2">Contra 2.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool cambiarContrasena(string correo, string contra, string contra2, out string mensaje)
        {
            mensaje = validarContras(contra, contra2);
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

            using (SqlConnection conexion = Conexion.obtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_CambiarContrasenaPorCorreo", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@correo", correo.Trim());
                    comando.Parameters.AddWithValue("@contrasena_hash", Contrasenas.hashear(contra));
                    comando.Parameters.Add(resultado);
                    comando.Parameters.Add(textoMensaje);
                    comando.ExecuteNonQuery();
                }
            }

            mensaje = "" + textoMensaje.Value;
            return Convert.ToInt32(resultado.Value) == 1;
        }

        /// <summary>
        /// Indica si tiene mayuscula.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        private bool tieneMayuscula(string texto)
        {
            foreach (char c in texto)
            {
                if (char.IsUpper(c))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Indica si tiene número.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        private bool tieneNumero(string texto)
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
