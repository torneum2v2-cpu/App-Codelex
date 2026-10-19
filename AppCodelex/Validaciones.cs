namespace AppCodelex;

public class Validaciones
{
    public static bool SoloNumeros(string texto)
    {
        foreach (char c in texto)
        {
            if (!char.IsDigit(c))
                return false;
        }

        return true;
    }

    public static string Obligatorio(string valor, string campo)
    {
        if (valor.Trim() == "")
            return "El campo " + campo + " es obligatorio.";

        return "";
    }

    public static string ValidarIdentidad(string rtn)
    {
        rtn = rtn.Trim();

        if (rtn == "")
            return "El RTN / identidad es obligatorio.";

        if (!SoloNumeros(rtn))
            return "El RTN / identidad solo puede tener números, sin guiones ni espacios.";

        if (rtn.Length < 13 || rtn.Length > 14)
            return "El RTN / identidad debe tener 13 o 14 dígitos. Tiene " + rtn.Length + ".";

        return "";
    }

    public static string ValidarTelefono(string telefono)
    {
        telefono = telefono.Trim();

        if (telefono == "")
            return "El teléfono es obligatorio.";

        if (!SoloNumeros(telefono))
            return "El teléfono solo puede tener números, sin guiones ni espacios.";

        if (telefono.Length != 8)
            return "El teléfono debe tener 8 dígitos. Tiene " + telefono.Length + ".";

        return "";
    }

    public static string ValidarCorreo(string correo)
    {
        correo = correo.Trim();

        if (correo == "")
            return "El correo es obligatorio.";

        if (!correo.Contains("@") || !correo.Contains("."))
            return "El correo no es válido. Ejemplo: nombre@empresa.com";

        return "";
    }
}
