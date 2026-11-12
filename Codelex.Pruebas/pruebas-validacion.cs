using Codelex.AccesoDatos;

namespace Codelex.Pruebas
{
    public class PruebasValidacion
    {
        [Theory]
        [InlineData("32449022")]
        [InlineData("+504 3244-9022")]
        [InlineData("22345678")]
        [InlineData("98765430")]
        public void telefonoValido(string telefono)
        {
            Assert.Equal("", Validacion.validarTelefono(telefono, true));
        }

        [Theory]
        [InlineData("00000000")]
        [InlineData("99999999")]
        [InlineData("3244902")]
        [InlineData("324490221")]
        [InlineData("12449022")]
        [InlineData("3244abcd")]
        [InlineData("12345678")]
        [InlineData("33000000")]
        [InlineData("")]
        public void telefonoInvalido(string telefono)
        {
            Assert.NotEqual("", Validacion.validarTelefono(telefono, true));
        }

        [Fact]
        public void telefonoVacioOpcional()
        {
            Assert.Equal("", Validacion.validarTelefono("", false));
        }

        [Fact]
        public void telefonoSeMuestraConFormato()
        {
            Assert.Equal("+504 3244-9022", Validacion.formatoTelefono("32449022"));
            Assert.Equal("32449022", Validacion.normalizarTelefono("+504 3244-9022"));
        }

        [Theory]
        [InlineData("juan.perez@gmail.com")]
        [InlineData("ventas@empresa.com.hn")]
        [InlineData("a_b-c@dominio.hn")]
        public void correoValido(string correo)
        {
            Assert.Equal("", Validacion.validarCorreo(correo, true, 50));
        }

        [Theory]
        [InlineData("a@b.c")]
        [InlineData("juangmail.com")]
        [InlineData("a@@gmail.com")]
        [InlineData("juan@gmail")]
        [InlineData("ju an@gmail.com")]
        [InlineData("juan@gmail.")]
        [InlineData("ju..an@gmail.com")]
        [InlineData("juan@gmail.c")]
        [InlineData("")]
        public void correoInvalido(string correo)
        {
            Assert.NotEqual("", Validacion.validarCorreo(correo, true, 50));
        }

        [Fact]
        public void correoMasLargoQueElMaximo()
        {
            string largo = new string('a', 45) + "@gmail.com";
            Assert.NotEqual("", Validacion.validarCorreo(largo, true, 50));
        }

        [Theory]
        [InlineData("José Luis")]
        [InlineData("Nuñez")]
        [InlineData("De la Cruz-Oliva")]
        [InlineData("O'Connor")]
        public void nombreValido(string nombre)
        {
            Assert.Equal("", Validacion.validarNombrePersona(nombre, "El nombre", true));
        }

        [Theory]
        [InlineData("Juan2")]
        [InlineData("J")]
        [InlineData("Aaaa")]
        [InlineData("Bcdfg")]
        [InlineData("Juan@")]
        [InlineData("   ")]
        public void nombreInvalido(string nombre)
        {
            Assert.NotEqual("", Validacion.validarNombrePersona(nombre, "El nombre", true));
        }

        [Theory]
        [InlineData("Limpieza Total S. de R.L.")]
        [InlineData("A&B Servicios")]
        public void empresaValida(string empresa)
        {
            Assert.Equal("", Validacion.validarEmpresa(empresa, "La empresa", true));
        }

        [Theory]
        [InlineData("A")]
        [InlineData("AB")]
        [InlineData("12345")]
        [InlineData("xxxx")]
        public void empresaInvalida(string empresa)
        {
            Assert.NotEqual("", Validacion.validarEmpresa(empresa, "La empresa", true));
        }

        [Theory]
        [InlineData("0801199012345")]
        [InlineData("0801-1990-12345")]
        [InlineData("1104198512345")]
        public void identidadValida(string identidad)
        {
            Assert.Equal("", Validacion.validarIdentidad(identidad, true));
        }

        [Theory]
        [InlineData("1901199012345")]
        [InlineData("0001199012345")]
        [InlineData("0829199012345")]
        [InlineData("0907199012345")]
        [InlineData("0800199012345")]
        [InlineData("0801185012345")]
        [InlineData("0801209912345")]
        [InlineData("0801199000000")]
        [InlineData("080119901234")]
        [InlineData("0801199A12345")]
        public void identidadInvalida(string identidad)
        {
            Assert.NotEqual("", Validacion.validarIdentidad(identidad, true));
        }

        [Fact]
        public void identidadDeMenorSoloSirveParaClientes()
        {
            string menor = "0801" + (DateTime.Today.Year - 10) + "12345";

            Assert.NotEqual("", Validacion.validarIdentidad(menor, true));
            Assert.Equal("", Validacion.validarIdentidad(menor, false));
        }

        [Theory]
        [InlineData("08011990123456", true)]
        [InlineData("08012015000123", true)]
        [InlineData("99011990123456", false)]
        [InlineData("0801199012345", false)]
        [InlineData("08011990000000", false)]
        [InlineData("08011800123456", false)]
        public void rtn(string rtn, bool esValido)
        {
            Assert.Equal(esValido, Validacion.validarRtn(rtn) == "");
        }

        [Theory]
        [InlineData("HAB-1234", true)]
        [InlineData("hab1234", true)]
        [InlineData("MAB123", true)]
        [InlineData("HA-1234", false)]
        [InlineData("1234567", false)]
        [InlineData("HAB 1234", false)]
        [InlineData("HAB-0000", false)]
        public void placa(string placa, bool esValida)
        {
            Assert.Equal(esValida, Validacion.validarPlaca(placa) == "");
        }

        [Fact]
        public void placaSeNormaliza()
        {
            Assert.Equal("HAB-1234", Validacion.normalizarPlaca("hab1234"));
        }

        [Theory]
        [InlineData("jperez", true)]
        [InlineData("jp", false)]
        [InlineData("juan perez", false)]
        [InlineData("1juan", false)]
        public void nombreDeUsuario(string usuario, bool esValido)
        {
            Assert.Equal(esValido, Validacion.validarNombreUsuario(usuario) == "");
        }

        [Theory]
        [InlineData("Codelex2026", "jperez", true)]
        [InlineData("codelex2026", "", false)]
        [InlineData("CODELEX2026", "", false)]
        [InlineData("CodelexHn", "", false)]
        [InlineData("Cod1", "", false)]
        [InlineData("Jperez123", "jperez123", false)]
        public void contrasena(string contrasena, string usuario, bool esValida)
        {
            Assert.Equal(esValida, Validacion.validarContrasena(contrasena, usuario) == "");
        }

        [Fact]
        public void direccionYMotivo()
        {
            Assert.Equal("", Validacion.validarDireccion("Col. Kennedy, bloque 5", 80));
            Assert.NotEqual("", Validacion.validarDireccion("abc", 80));
            Assert.NotEqual("", Validacion.validarDireccion("12345 678", 80));
            Assert.Equal("", Validacion.validarMotivo("Monto mal digitado"));
            Assert.NotEqual("", Validacion.validarMotivo("xxxxxxx"));
            Assert.NotEqual("", Validacion.validarMotivo("abc"));
        }
    }
}
