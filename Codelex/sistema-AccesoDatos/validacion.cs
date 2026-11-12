namespace Codelex.AccesoDatos
{
    /// <summary>
    /// Reglas de validación compartidas por todos los módulos, pensadas para datos reales de Honduras:
    /// identidad (13 dígitos), RTN (14 dígitos), teléfono de 8 dígitos, correo, nombres, direcciones y placas.
    /// Todos los métodos devuelven el texto del error, o texto vacío si el dato es válido.
    /// </summary>
    public static class Validacion
    {
        public const string PREFIJO_TELEFONO = "+504";
        public const int EDAD_MINIMA_EMPLEADO = 18;
        public const int ANIO_MINIMO = 1900;

        // Cantidad de municipios que tiene cada uno de los 18 departamentos de Honduras (01 a 18).
        private static readonly int[] MUNICIPIOS_POR_DEPARTAMENTO =
            { 8, 10, 21, 23, 12, 16, 19, 28, 6, 17, 4, 19, 28, 16, 23, 28, 9, 11 };

        // ---------- Texto básico ----------

        /// <summary>
        /// Deja solamente los dígitos de un texto.
        /// </summary>
        /// <param name="texto">Texto a limpiar.</param>
        /// <returns>Texto con solo números.</returns>
        public static string soloDigitos(string texto)
        {
            string resultado = "";

            foreach (char c in texto)
            {
                if (c >= '0' && c <= '9')
                    resultado += c;
            }

            return resultado;
        }

        /// <summary>
        /// Indica si todos los caracteres son dígitos de 0 a 9.
        /// </summary>
        /// <param name="texto">Texto a revisar.</param>
        /// <returns>true si todos son dígitos; false en caso contrario.</returns>
        public static bool sonDigitos(string texto)
        {
            foreach (char c in texto)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Indica si todos los caracteres del texto son iguales (por ejemplo 0000 o aaaa).
        /// </summary>
        /// <param name="texto">Texto a revisar.</param>
        /// <returns>true si todos son iguales; false en caso contrario.</returns>
        public static bool todosIguales(string texto)
        {
            if (texto.Length == 0)
                return false;

            foreach (char c in texto)
            {
                if (char.ToLower(c) != char.ToLower(texto[0]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Indica si el texto repite la misma letra seguida la cantidad de veces indicada (los números no cuentan).
        /// </summary>
        /// <param name="texto">Texto a revisar.</param>
        /// <param name="veces">Cantidad de repeticiones seguidas que se consideran sospechosas.</param>
        /// <returns>true si hay una repetición así; false en caso contrario.</returns>
        public static bool repiteCaracter(string texto, int veces)
        {
            int seguidos = 1;

            for (int i = 1; i < texto.Length; i++)
            {
                if (char.IsLetter(texto[i]) && char.ToLower(texto[i]) == char.ToLower(texto[i - 1]))
                    seguidos++;
                else
                    seguidos = 1;

                if (seguidos >= veces)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Cuenta las letras que tiene un texto.
        /// </summary>
        /// <param name="texto">Texto a revisar.</param>
        /// <returns>Cantidad de letras.</returns>
        public static int contarLetras(string texto)
        {
            int total = 0;

            foreach (char c in texto)
            {
                if (char.IsLetter(c))
                    total++;
            }

            return total;
        }

        // ---------- Teléfono ----------

        /// <summary>
        /// Convierte lo que escribió el usuario en los 8 dígitos que se guardan (quita +504, guiones y espacios).
        /// </summary>
        /// <param name="texto">Teléfono escrito por el usuario.</param>
        /// <returns>Teléfono solo con números, sin el 504 del país.</returns>
        public static string normalizarTelefono(string texto)
        {
            string digitos = soloDigitos(texto);

            if (digitos.Length == 11 && digitos.StartsWith("504"))
                digitos = digitos.Substring(3);

            return digitos;
        }

        /// <summary>
        /// Da formato +504 XXXX-XXXX a un teléfono guardado de 8 dígitos.
        /// </summary>
        /// <param name="guardado">Teléfono guardado en la base de datos.</param>
        /// <returns>Teléfono con formato para mostrar, o el mismo texto si no tiene 8 dígitos.</returns>
        public static string formatoTelefono(string guardado)
        {
            string digitos = normalizarTelefono(guardado);

            if (digitos.Length != 8)
                return guardado.Trim();

            return PREFIJO_TELEFONO + " " + digitos.Substring(0, 4) + "-" + digitos.Substring(4);
        }

        /// <summary>
        /// Valida un teléfono de Honduras: 8 dígitos, empieza con 2, 3, 7, 8 o 9 y no es un número repetido.
        /// </summary>
        /// <param name="texto">Teléfono escrito por el usuario.</param>
        /// <param name="obligatorio">true si el teléfono no se puede dejar vacío.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarTelefono(string texto, bool obligatorio)
        {
            texto = texto.Trim();

            if (texto == "")
            {
                if (obligatorio)
                    return "El teléfono es obligatorio.";

                return "";
            }

            foreach (char c in texto)
            {
                bool permitido = (c >= '0' && c <= '9') || c == ' ' || c == '-' || c == '+' || c == '(' || c == ')';
                if (!permitido)
                    return "El teléfono solo puede tener números. Escríbalo sin letras ni símbolos.";
            }

            string digitos = normalizarTelefono(texto);

            if (digitos.Length != 8)
                return "El teléfono debe tener 8 dígitos. Tiene " + digitos.Length + ".";

            if (todosIguales(digitos))
                return "El teléfono no es válido: no puede ser todo ceros ni repetir el mismo número.";

            char primero = digitos[0];
            if (primero != '2' && primero != '3' && primero != '7' && primero != '8' && primero != '9')
                return "El teléfono debe empezar con 2 (fijo) o con 3, 7, 8 o 9 (celular).";

            if (todosIguales(digitos.Substring(2)) || digitos == "12345678" || digitos == "23456789")
                return "El teléfono no parece real. Revise los números.";

            return "";
        }

        // ---------- Correo ----------

        /// <summary>
        /// Valida un correo electrónico: mínimo 6 caracteres, una sola @, dominio con punto y extensión de 2 o más letras.
        /// </summary>
        /// <param name="correo">Correo escrito por el usuario.</param>
        /// <param name="obligatorio">true si el correo no se puede dejar vacío.</param>
        /// <param name="maximo">Cantidad máxima de caracteres que se pueden guardar.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarCorreo(string correo, bool obligatorio, int maximo)
        {
            correo = correo.Trim();
            string ejemplo = "Ejemplo: nombre@empresa.com";

            if (correo == "")
            {
                if (obligatorio)
                    return "El correo es obligatorio.";

                return "";
            }

            if (correo.Length < 6)
                return "El correo es demasiado corto. " + ejemplo;
            if (correo.Length > maximo)
                return "El correo no puede tener más de " + maximo + " caracteres.";
            if (correo.Contains(" "))
                return "El correo no puede llevar espacios.";

            int arroba = correo.IndexOf('@');
            if (arroba <= 0 || arroba != correo.LastIndexOf('@') || arroba == correo.Length - 1)
                return "El correo no es válido: debe tener una sola @ con texto antes y después. " + ejemplo;

            string local = correo.Substring(0, arroba);
            string dominio = correo.Substring(arroba + 1);

            if (local.Length > 64)
                return "El correo no es válido: la parte antes de la @ es demasiado larga.";
            if (!caracteresDeCorreo(local, "._%+-'"))
                return "El correo tiene caracteres que no se permiten. " + ejemplo;
            if (local.StartsWith(".") || local.EndsWith(".") || local.Contains(".."))
                return "El correo no es válido: la parte antes de la @ no puede empezar ni terminar con punto. " + ejemplo;

            if (!dominio.Contains("."))
                return "El correo no es válido: el dominio necesita un punto. " + ejemplo;
            if (!caracteresDeCorreo(dominio, ".-"))
                return "El correo tiene caracteres que no se permiten. " + ejemplo;
            if (dominio.StartsWith(".") || dominio.EndsWith(".") || dominio.Contains("..")
                || dominio.StartsWith("-") || dominio.Contains("-.") || dominio.Contains(".-"))
                return "El correo no es válido: revise el dominio. " + ejemplo;

            string extension = dominio.Substring(dominio.LastIndexOf('.') + 1);
            if (extension.Length < 2 || contarLetras(extension) != extension.Length)
                return "El correo no es válido: la extensión (por ejemplo .com o .hn) debe tener al menos 2 letras.";

            return "";
        }

        /// <summary>
        /// Indica si un texto solo tiene letras sin tilde, números y los símbolos permitidos.
        /// </summary>
        /// <param name="texto">Texto a revisar.</param>
        /// <param name="simbolos">Símbolos que se permiten además de letras y números.</param>
        /// <returns>true si todos los caracteres son permitidos; false en caso contrario.</returns>
        private static bool caracteresDeCorreo(string texto, string simbolos)
        {
            foreach (char c in texto)
            {
                bool letra = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
                bool numero = c >= '0' && c <= '9';

                if (!letra && !numero && !simbolos.Contains(c))
                    return false;
            }

            return true;
        }

        // ---------- Nombres ----------

        /// <summary>
        /// Valida el nombre o apellido de una persona: solo letras, mínimo 2, sin números ni repeticiones raras.
        /// </summary>
        /// <param name="valor">Texto escrito por el usuario.</param>
        /// <param name="etiqueta">Cómo se llama el dato en el mensaje, por ejemplo "El nombre".</param>
        /// <param name="obligatorio">true si no se puede dejar vacío.</param>
        /// <param name="maximo">Cantidad máxima de caracteres que se pueden guardar.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarNombrePersona(string valor, string etiqueta, bool obligatorio, int maximo = 50)
        {
            valor = valor.Trim();

            if (valor == "")
            {
                if (obligatorio)
                    return etiqueta + " es obligatorio.";

                return "";
            }

            if (valor.Length < 2)
                return etiqueta + " debe tener al menos 2 letras.";
            if (valor.Length > maximo)
                return etiqueta + " no puede tener más de " + maximo + " caracteres.";

            foreach (char c in valor)
            {
                bool permitido = char.IsLetter(c) || c == ' ' || c == '\'' || c == '-' || c == '.';
                if (!permitido)
                    return etiqueta + " solo puede tener letras. No se permiten números ni símbolos.";
            }

            if (!char.IsLetter(valor[0]) || !(char.IsLetter(valor[valor.Length - 1]) || valor[valor.Length - 1] == '.'))
                return etiqueta + " debe empezar con una letra y terminar con una letra o un punto.";
            if (valor.Contains("  "))
                return etiqueta + " no puede tener espacios seguidos.";
            if (!tieneVocal(valor) || repiteCaracter(valor, 3))
                return etiqueta + " no parece un nombre real. Revíselo.";

            return "";
        }

        /// <summary>
        /// Indica si el texto tiene al menos una vocal (los nombres reales siempre la tienen).
        /// </summary>
        /// <param name="texto">Texto a revisar.</param>
        /// <returns>true si tiene una vocal; false en caso contrario.</returns>
        private static bool tieneVocal(string texto)
        {
            string vocales = "aeiouyáéíóúü";

            foreach (char c in texto.ToLower())
            {
                if (vocales.Contains(c))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Valida el nombre de una empresa o proveedor: mínimo 3 caracteres, con letras, sin símbolos raros.
        /// </summary>
        /// <param name="valor">Texto escrito por el usuario.</param>
        /// <param name="etiqueta">Cómo se llama el dato en el mensaje, por ejemplo "La empresa".</param>
        /// <param name="obligatorio">true si no se puede dejar vacío.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarEmpresa(string valor, string etiqueta, bool obligatorio)
        {
            valor = valor.Trim();

            if (valor == "")
            {
                if (obligatorio)
                    return etiqueta + " es obligatoria.";

                return "";
            }

            if (valor.Length < 3)
                return etiqueta + " debe tener al menos 3 caracteres.";
            if (valor.Length > 50)
                return etiqueta + " no puede tener más de 50 caracteres.";
            if (contarLetras(valor) < 2)
                return etiqueta + " debe tener un nombre con letras, no solo números o símbolos.";

            string permitidos = " &.,-'()/#+";
            foreach (char c in valor)
            {
                if (!char.IsLetterOrDigit(c) && !permitidos.Contains(c))
                    return etiqueta + " tiene símbolos que no se permiten.";
            }

            if (repiteCaracter(valor, 4))
                return etiqueta + " no parece un nombre real. Revíselo.";

            return "";
        }

        /// <summary>
        /// Valida un texto descriptivo (producto, equipo, servicio, locación, marca...): con letras y largo mínimo.
        /// </summary>
        /// <param name="valor">Texto escrito por el usuario.</param>
        /// <param name="etiqueta">Cómo se llama el dato en el mensaje, por ejemplo "El nombre del producto".</param>
        /// <param name="minimo">Cantidad mínima de caracteres.</param>
        /// <param name="maximo">Cantidad máxima de caracteres.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarTexto(string valor, string etiqueta, int minimo, int maximo)
        {
            valor = valor.Trim();

            if (valor == "")
                return etiqueta + " es obligatorio.";
            if (valor.Length < minimo)
                return etiqueta + " debe tener al menos " + minimo + " caracteres.";
            if (valor.Length > maximo)
                return etiqueta + " no puede tener más de " + maximo + " caracteres.";
            if (contarLetras(valor) == 0)
                return etiqueta + " debe tener letras, no solo números o símbolos.";
            if (repiteCaracter(valor, 4))
                return etiqueta + " no parece real. Revíselo.";

            return "";
        }

        /// <summary>
        /// Valida un modelo (de vehículo o equipo): puede ser solo números, como Mazda 3 o Peugeot 208.
        /// </summary>
        /// <param name="valor">Texto escrito por el usuario.</param>
        /// <param name="etiqueta">Cómo se llama el dato en el mensaje, por ejemplo "El modelo".</param>
        /// <param name="maximo">Cantidad máxima de caracteres.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarModelo(string valor, string etiqueta, int maximo)
        {
            valor = valor.Trim();

            if (valor == "")
                return etiqueta + " es obligatorio.";
            if (valor.Length > maximo)
                return etiqueta + " no puede tener más de " + maximo + " caracteres.";

            bool tieneLetraONumero = false;
            foreach (char c in valor)
            {
                if (char.IsLetterOrDigit(c))
                    tieneLetraONumero = true;
            }

            if (!tieneLetraONumero)
                return etiqueta + " debe tener letras o números.";

            return "";
        }

        /// <summary>
        /// Valida una dirección: mínimo 5 caracteres y con letras.
        /// </summary>
        /// <param name="valor">Dirección escrita por el usuario.</param>
        /// <param name="maximo">Cantidad máxima de caracteres.</param>
        /// <returns>Texto del error, o texto vacío si es válida.</returns>
        public static string validarDireccion(string valor, int maximo)
        {
            valor = valor.Trim();

            if (valor == "")
                return "La dirección es obligatoria.";
            if (valor.Length < 5)
                return "La dirección debe tener al menos 5 caracteres. Ejemplo: Col. Kennedy, bloque 5.";
            if (valor.Length > maximo)
                return "La dirección no puede tener más de " + maximo + " caracteres.";
            if (contarLetras(valor) < 3)
                return "La dirección debe incluir el nombre de la colonia, barrio o calle.";
            if (repiteCaracter(valor, 4))
                return "La dirección no parece real. Revísela.";

            return "";
        }

        /// <summary>
        /// Valida el motivo que se escribe al anular o deshacer algo.
        /// </summary>
        /// <param name="motivo">Motivo escrito por el usuario.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarMotivo(string motivo)
        {
            motivo = motivo.Trim();

            if (motivo == "")
                return "El motivo es obligatorio.";
            if (motivo.Length < 5)
                return "El motivo debe tener al menos 5 caracteres.";
            if (motivo.Length > 100)
                return "El motivo no puede tener más de 100 caracteres.";
            if (contarLetras(motivo) < 3 || repiteCaracter(motivo, 4))
                return "Escriba un motivo claro, con palabras.";

            return "";
        }

        // ---------- Identidad y RTN ----------

        /// <summary>
        /// Valida la identidad hondureña: 13 dígitos (departamento, municipio, año de nacimiento y consecutivo).
        /// </summary>
        /// <param name="numero">Identidad sin guiones.</param>
        /// <param name="esEmpleado">true para exigir que la persona sea mayor de 18 años.</param>
        /// <returns>Texto del error, o texto vacío si es válida.</returns>
        public static string validarIdentidad(string numero, bool esEmpleado)
        {
            numero = soloDigitosDeIdentificacion(numero);

            if (!sonDigitos(numero))
                return "La identidad solo puede tener números, sin guiones ni espacios.";
            if (numero.Length != 13)
                return "La identidad debe tener 13 dígitos. Tiene " + numero.Length + ".";

            string problema = validarSecuencia(numero, 5, "La identidad", false);
            if (problema != "")
                return problema;

            int anio = int.Parse(numero.Substring(4, 4));
            if (esEmpleado && anio > DateTime.Today.Year - EDAD_MINIMA_EMPLEADO)
                return "La identidad indica que la persona es menor de " + EDAD_MINIMA_EMPLEADO + " años. Solo se registran empleados mayores de edad.";

            return "";
        }

        /// <summary>
        /// Valida el RTN hondureño: 14 dígitos (departamento, municipio, año y consecutivo).
        /// </summary>
        /// <param name="numero">RTN sin guiones.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarRtn(string numero)
        {
            numero = soloDigitosDeIdentificacion(numero);

            if (!sonDigitos(numero))
                return "El RTN solo puede tener números, sin guiones ni espacios.";
            if (numero.Length != 14)
                return "El RTN debe tener 14 dígitos. Tiene " + numero.Length + ".";

            return validarSecuencia(numero, 6, "El RTN", true);
        }

        /// <summary>
        /// Quita guiones y espacios de una identidad o RTN.
        /// </summary>
        /// <param name="numero">Número escrito por el usuario.</param>
        /// <returns>Número sin guiones ni espacios.</returns>
        private static string soloDigitosDeIdentificacion(string numero)
        {
            return numero.Replace("-", "").Replace(" ", "").Trim();
        }

        /// <summary>
        /// Revisa la secuencia de una identidad o RTN: departamento 01 a 18, municipio que exista, año válido y consecutivo distinto de cero.
        /// </summary>
        /// <param name="numero">Número solo con dígitos.</param>
        /// <param name="largoConsecutivo">Cantidad de dígitos del consecutivo final (5 en identidad, 6 en RTN).</param>
        /// <param name="etiqueta">Cómo se llama el dato en el mensaje, por ejemplo "La identidad".</param>
        /// <param name="esRtn">true si es un RTN: las empresas también pueden llevar un año que empieza con 9 (9000 a 9999).</param>
        /// <returns>Texto del error, o texto vacío si la secuencia es válida.</returns>
        private static string validarSecuencia(string numero, int largoConsecutivo, string etiqueta, bool esRtn)
        {
            int departamento = int.Parse(numero.Substring(0, 2));
            int municipio = int.Parse(numero.Substring(2, 2));
            int anio = int.Parse(numero.Substring(4, 4));
            string consecutivo = numero.Substring(8, largoConsecutivo);

            if (departamento < 1 || departamento > 18)
                return etiqueta + " no es válida: los primeros 2 dígitos (" + numero.Substring(0, 2) + ") deben ser un departamento entre 01 y 18.";

            int maximoMunicipio = MUNICIPIOS_POR_DEPARTAMENTO[departamento - 1];
            if (municipio < 1 || municipio > maximoMunicipio)
                return etiqueta + " no es válida: el departamento " + numero.Substring(0, 2) + " tiene municipios del 01 al "
                       + maximoMunicipio.ToString("00") + " y se escribió " + numero.Substring(2, 2) + ".";

            bool anioDeEmpresa = esRtn && anio >= 9000 && anio <= 9999;
            if (!anioDeEmpresa && (anio < ANIO_MINIMO || anio > DateTime.Today.Year))
                return etiqueta + " no es válida: el año (" + anio + ") debe estar entre " + ANIO_MINIMO + " y " + DateTime.Today.Year + ".";

            if (consecutivo.Trim('0') == "")
                return etiqueta + " no es válida: los últimos " + largoConsecutivo + " dígitos no pueden ser todos ceros.";

            return "";
        }

        // ---------- Vehículos y usuarios ----------

        /// <summary>
        /// Valida una placa hondureña: 3 letras y 3 o 4 números, con guion opcional (por ejemplo HAB-1234).
        /// </summary>
        /// <param name="placa">Placa escrita por el usuario.</param>
        /// <returns>Texto del error, o texto vacío si es válida.</returns>
        public static string validarPlaca(string placa)
        {
            placa = placa.Trim().ToUpper();
            string ejemplo = "Ejemplo: HAB-1234.";

            if (placa == "")
                return "La placa es obligatoria.";

            string sinGuion = placa.Replace("-", "").Replace(" ", "");
            if (placa.Length - placa.Replace("-", "").Length > 1)
                return "La placa solo puede tener un guion. " + ejemplo;
            if (sinGuion.Length < 6 || sinGuion.Length > 7)
                return "La placa debe tener 3 letras y 3 o 4 números. " + ejemplo;

            for (int i = 0; i < sinGuion.Length; i++)
            {
                bool letra = sinGuion[i] >= 'A' && sinGuion[i] <= 'Z';
                bool numero = sinGuion[i] >= '0' && sinGuion[i] <= '9';

                if (i < 3 && !letra)
                    return "La placa debe empezar con 3 letras. " + ejemplo;
                if (i >= 3 && !numero)
                    return "Después de las 3 letras la placa lleva solo números. " + ejemplo;
            }

            if (sinGuion.Substring(3).Trim('0') == "")
                return "Los números de la placa no pueden ser todos ceros.";

            return "";
        }

        /// <summary>
        /// Deja la placa en mayúsculas y sin guion ni espacios (HAB1234), para que HAB-1234, hab 1234 y HAB1234 sean la misma.
        /// </summary>
        /// <param name="placa">Placa escrita por el usuario.</param>
        /// <returns>Placa normalizada.</returns>
        public static string normalizarPlaca(string placa)
        {
            return placa.Trim().ToUpper().Replace("-", "").Replace(" ", "");
        }

        /// <summary>
        /// Valida el nombre de usuario para iniciar sesión: 4 a 30 caracteres, empieza con letra, sin espacios.
        /// </summary>
        /// <param name="usuario">Nombre de usuario escrito.</param>
        /// <returns>Texto del error, o texto vacío si es válido.</returns>
        public static string validarNombreUsuario(string usuario)
        {
            usuario = usuario.Trim();

            if (usuario == "")
                return "El usuario es obligatorio.";
            if (usuario.Contains(" "))
                return "El usuario no puede llevar espacios.";
            if (usuario.Length < 4)
                return "El usuario debe tener al menos 4 caracteres.";
            if (usuario.Length > 30)
                return "El usuario no puede tener más de 30 caracteres.";
            if (!char.IsLetter(usuario[0]))
                return "El usuario debe empezar con una letra.";

            foreach (char c in usuario)
            {
                bool permitido = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')
                                 || c == '.' || c == '_' || c == '-';
                if (!permitido)
                    return "El usuario solo puede tener letras sin tilde, números, punto, guion y guion bajo.";
            }

            return "";
        }

        /// <summary>
        /// Valida una contraseña: 8 o más caracteres con mayúscula, minúscula y número, sin espacios.
        /// </summary>
        /// <param name="contrasena">Contraseña escrita.</param>
        /// <param name="usuario">Nombre de usuario (la contraseña no puede ser igual); puede ir vacío.</param>
        /// <returns>Texto del error, o texto vacío si es válida.</returns>
        public static string validarContrasena(string contrasena, string usuario)
        {
            bool mayuscula = false;
            bool minuscula = false;
            bool numero = false;

            foreach (char c in contrasena)
            {
                if (char.IsUpper(c))
                    mayuscula = true;
                if (char.IsLower(c))
                    minuscula = true;
                if (char.IsDigit(c))
                    numero = true;
            }

            if (contrasena.Length < 8)
                return "Tu contraseña debe tener al menos 8 caracteres";
            if (contrasena.Length > 50)
                return "La contraseña no puede tener más de 50 caracteres.";
            if (contrasena.Contains(" "))
                return "La contraseña no puede llevar espacios.";
            if (!mayuscula)
                return "Por favor utiliza al menos una mayúscula.";
            if (!minuscula)
                return "Por favor utiliza al menos una minúscula.";
            if (!numero)
                return "Tu contraseña debe utilizar por lo menos un dígito";
            if (usuario.Trim() != "" && contrasena.ToLower() == usuario.Trim().ToLower())
                return "La contraseña no puede ser igual al usuario.";

            return "";
        }
    }
}
