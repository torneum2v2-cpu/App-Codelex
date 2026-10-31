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
        private static readonly string Carpeta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Codelex");

        private static readonly string Archivo = Path.Combine(Carpeta, "cuentas.json");

        private static readonly JsonSerializerOptions Opciones = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static List<CuentaRecordada> Listar()
        {
            try
            {
                if (!File.Exists(Archivo))
                    return new List<CuentaRecordada>();

                string json = File.ReadAllText(Archivo);
                return JsonSerializer.Deserialize<List<CuentaRecordada>>(json) ?? new List<CuentaRecordada>();
            }
            catch
            {
                return new List<CuentaRecordada>();
            }
        }

        public static bool Existe(string usuario)
        {
            return Listar().Any(c => string.Equals(c.Usuario, usuario, StringComparison.OrdinalIgnoreCase));
        }

        public static void Guardar(string usuario, string nombreCompleto, string contrasena)
        {
            try
            {
                List<CuentaRecordada> cuentas = Listar();

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

                Escribir(cuentas);
            }
            catch
            {
                // Si no se puede guardar, el inicio de sesión sigue funcionando normal.
            }
        }

        public static void Quitar(string usuario)
        {
            try
            {
                List<CuentaRecordada> cuentas = Listar();
                cuentas.RemoveAll(c => string.Equals(c.Usuario, usuario, StringComparison.OrdinalIgnoreCase));
                Escribir(cuentas);
            }
            catch
            {
            }
        }

        public static string ObtenerContrasena(CuentaRecordada cuenta)
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

        private static void Escribir(List<CuentaRecordada> cuentas)
        {
            Directory.CreateDirectory(Carpeta);
            File.WriteAllText(Archivo, JsonSerializer.Serialize(cuentas, Opciones));
        }
    }
}