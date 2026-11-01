namespace Codelex.BDD
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

                return NombreUsuario.Length > 0 ? NombreUsuario[0].ToString().ToUpper() : "?";
            }
        }

        public static void Iniciar(Usuario usuario)
        {
            UsuarioId = usuario.UsuarioId;
            EmpleadoId = usuario.EmpleadoId;
            NombreUsuario = usuario.NombreUsuario;
            NombreCompleto = usuario.NombreCompleto;
            Rol = usuario.Rol;
            Nivel = NivelDe(usuario.Rol);
        }

        public static void Cerrar()
        {
            UsuarioId = 0;
            EmpleadoId = 0;
            NombreUsuario = "";
            NombreCompleto = "";
            Rol = "";
            Nivel = NivelAcceso.SinRol;
        }

        // Verdadero si el usuario tiene al menos el nivel pedido.
        public static bool Permite(NivelAcceso minimo)
        {
            return Nivel != NivelAcceso.SinRol && Nivel >= minimo;
        }

        private static NivelAcceso NivelDe(string rol)
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