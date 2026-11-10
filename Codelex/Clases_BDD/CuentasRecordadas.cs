using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Codelex.BDD
{
    public class CuentaRecordada
    {
        public string Usuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string ContrasenaProtegida { get; set; } = "";

        [JsonIgnore]
        public string Iniciales
        {
            get
            {
                string[] partes = NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length >= 2)
                    return (partes[0][0].ToString() + partes[1][0]).ToUpper();
                if (partes.Length == 1)
                    return partes[0][0].ToString().ToUpper();
                return Usuario.Length > 0 ? Usuario[0].ToString().ToUpper() : "?";
            }
        }
    }

    public static class CuentasRecordadas
    {
        private static readonly string CARPETA = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Codelex");

        private static readonly string ARCHIVO = Path.Combine(CARPETA, "cuentas.json");

        private static readonly JsonSerializerOptions OPCIONES = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static List<CuentaRecordada> listar()
        {
            try
            {
                if (!File.Exists(ARCHIVO))
                    return new List<CuentaRecordada>();

                string json = File.ReadAllText(ARCHIVO);
                return JsonSerializer.Deserialize<List<CuentaRecordada>>(json) ?? new List<CuentaRecordada>();
            }
            catch
            {
                return new List<CuentaRecordada>();
            }
        }

        public static bool existe(string usuario)
        {
            return listar().Any(c => string.Equals(c.Usuario, usuario, StringComparison.OrdinalIgnoreCase));
        }

        public static void guardar(string usuario, string nombreCompleto, string contrasena)
        {
            try
            {
                List<CuentaRecordada> cuentas = listar();

                byte[] protegida = ProtectedData.Protect(
                    Encoding.UTF8.GetBytes(contrasena), null, DataProtectionScope.CurrentUser);

                CuentaRecordada nueva = new CuentaRecordada
                {
                    Usuario = usuario,
                    NombreCompleto = nombreCompleto,
                    ContrasenaProtegida = Convert.ToBase64String(protegida)
                };

                int indice = cuentas.FindIndex(c => string.Equals(c.Usuario, usuario, StringComparison.OrdinalIgnoreCase));
                if (indice >= 0)
                    cuentas[indice] = nueva;
                else
                    cuentas.Add(nueva);

                escribir(cuentas);
            }
            catch
            {
                // Si no se puede guardar, el inicio de sesión sigue funcionando normal.
            }
        }

        public static void quitar(string usuario)
        {
            try
            {
                List<CuentaRecordada> cuentas = listar();
                cuentas.RemoveAll(c => string.Equals(c.Usuario, usuario, StringComparison.OrdinalIgnoreCase));
                escribir(cuentas);
            }
            catch
            {
            }
        }

        public static string obtenerContrasena(CuentaRecordada cuenta)
        {
            try
            {
                byte[] protegida = Convert.FromBase64String(cuenta.ContrasenaProtegida);
                byte[] datos = ProtectedData.Unprotect(protegida, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(datos);
            }
            catch
            {
                return "";
            }
        }

        private static void escribir(List<CuentaRecordada> cuentas)
        {
            Directory.CreateDirectory(CARPETA);
            File.WriteAllText(ARCHIVO, JsonSerializer.Serialize(cuentas, OPCIONES));
        }
    }
}