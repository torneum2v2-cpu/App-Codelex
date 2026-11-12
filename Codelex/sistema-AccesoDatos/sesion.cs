namespace Codelex.AccesoDatos
{
    public enum NivelAcceso
    {
        SinRol = 0,
        Basico = 1,
        Medio = 2,
        Avanzado = 3
    }

    public static class Sesion
    {
        public static int UsuarioId { get; private set; }
        public static int EmpleadoId { get; private set; }
        public static string NombreUsuario { get; private set; } = "";
        public static string NombreCompleto { get; private set; } = "";
        public static string Rol { get; private set; } = "";
        public static NivelAcceso Nivel { get; private set; } = NivelAcceso.SinRol;

        public static string Iniciales
        {
            get
            {
                string[] partes = NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (partes.Length >= 2)
                    return (partes[0][0].ToString() + partes[1][0]).ToUpper();
                if (partes.Length == 1)
                    return partes[0][0].ToString().ToUpper();

                if (NombreUsuario.Length > 0)
                    return NombreUsuario[0].ToString().ToUpper();

                return "?";
            }
        }

        /// <summary>
        /// Guarda los datos del usuario que inició sesión.
        /// </summary>
        /// <param name="usuario">Nombre de usuario.</param>
        public static void iniciar(Usuario usuario)
        {
            UsuarioId = usuario.UsuarioId;
            EmpleadoId = usuario.EmpleadoId;
            NombreUsuario = usuario.NombreUsuario;
            NombreCompleto = usuario.NombreCompleto;
            Rol = usuario.Rol;
            Nivel = nivelDe(usuario.Rol);
        }

        /// <summary>
        /// Cierra la sesión actual.
        /// </summary>
        public static void cerrar()
        {
            UsuarioId = 0;
            EmpleadoId = 0;
            NombreUsuario = "";
            NombreCompleto = "";
            Rol = "";
            Nivel = NivelAcceso.SinRol;
        }

        // Verdadero si el usuario tiene al menos el nivel pedido.
        /// <summary>
        /// Indica si el usuario tiene al menos el nivel de acceso pedido.
        /// </summary>
        /// <param name="minimo">Nivel mínimo requerido.</param>
        /// <returns>true si el nivel alcanza; false en caso contrario.</returns>
        public static bool permite(NivelAcceso minimo)
        {
            return Nivel != NivelAcceso.SinRol && Nivel >= minimo;
        }

        /// <summary>
        /// Devuelve el nivel de acceso que corresponde a un rol.
        /// </summary>
        /// <param name="rol">Nombre del rol.</param>
        /// <returns>Nivel de acceso del rol.</returns>
        private static NivelAcceso nivelDe(string rol)
        {
            string texto = rol.Trim().ToLowerInvariant();

            if (texto.Contains("avanzado"))
                return NivelAcceso.Avanzado;
            if (texto.Contains("medio"))
                return NivelAcceso.Medio;
            if (texto.Contains("básico") || texto.Contains("basico"))
                return NivelAcceso.Basico;

            return NivelAcceso.SinRol;
        }
    }
}