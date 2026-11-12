using System.Security.Cryptography;

namespace Codelex.AccesoDatos
{
    /// <summary>
    /// Guarda y comprueba contraseñas con PBKDF2 (nunca en texto plano).
    /// </summary>
    public static class Contrasenas
    {
        private const string PREFIJO = "PBKDF2";
        private const int ITERACIONES = 100000;
        private const int LARGO_SAL = 16;
        private const int LARGO_HASH = 32;

        /// <summary>
        /// Convierte una contraseña en un hash con sal para guardarlo en la base.
        /// </summary>
        /// <param name="contrasena">Contraseña en texto.</param>
        /// <returns>Texto con el formato PBKDF2$iteraciones$sal$hash.</returns>
        public static string hashear(string contrasena)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(LARGO_SAL);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, ITERACIONES, HashAlgorithmName.SHA256, LARGO_HASH);

            return PREFIJO + "$" + ITERACIONES + "$" + Convert.ToBase64String(sal) + "$" + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Indica si el texto guardado ya es un hash y no una contraseña en texto plano.
        /// </summary>
        /// <param name="guardado">Valor guardado en la base de datos.</param>
        /// <returns>true si tiene el formato de hash; false si es texto plano.</returns>
        public static bool esHash(string guardado)
        {
            return guardado.StartsWith(PREFIJO + "$");
        }

        /// <summary>
        /// Comprueba una contraseña contra lo guardado. Acepta contraseñas antiguas en texto plano.
        /// </summary>
        /// <param name="contrasena">Contraseña escrita por el usuario.</param>
        /// <param name="guardado">Valor guardado en la base de datos.</param>
        /// <returns>true si coincide; false en caso contrario.</returns>
        public static bool verificar(string contrasena, string guardado)
        {
            if (!esHash(guardado))
                return contrasena == guardado;

            string[] partes = guardado.Split('$');
            if (partes.Length != 4)
                return false;

            int iteraciones;
            if (!int.TryParse(partes[1], out iteraciones))
                return false;

            byte[] sal = Convert.FromBase64String(partes[2]);
            byte[] esperado = Convert.FromBase64String(partes[3]);
            byte[] calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
    }
}
