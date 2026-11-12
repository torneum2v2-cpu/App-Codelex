using Codelex.AccesoDatos;

namespace Codelex.Pruebas
{
    public class PruebasNegocio
    {
        Facturacion facturaBD = new Facturacion();
        InventarioBD inventarioBD = new InventarioBD();
        Cliente clienteBD = new Cliente();

        [Fact]
        public void isvEsQuinceComaPorCiento()
        {
            Assert.Equal(150m, facturaBD.calcularIsv(1000m));
        }

        [Fact]
        public void descuentoSeCalculaSobreElSubtotal()
        {
            Assert.Equal(330.20m, facturaBD.calcularDescuento(3302.02m, 10m));
            Assert.Equal(0m, facturaBD.calcularDescuento(3302.02m, 0m));
        }

        [Theory]
        [InlineData("", true)]
        [InlineData("0", true)]
        [InlineData("10", true)]
        [InlineData("99.99", true)]
        [InlineData("100", false)]
        [InlineData("-5", false)]
        [InlineData("abc", false)]
        public void descuentoValido(string texto, bool esValido)
        {
            decimal porcentaje;
            Assert.Equal(esValido, facturaBD.validarDescuento(texto, out porcentaje) == "");
        }

        [Fact]
        public void cuotasSumanElTotal()
        {
            decimal cuota = facturaBD.calcularCuota(2377.27m, 4);
            Assert.True(cuota > 594m && cuota < 595m);
        }

        [Fact]
        public void abonoNoPuedeSuperarElSaldo()
        {
            decimal monto;
            Assert.Equal("", facturaBD.validarAbono("100", 500m, out monto));
            Assert.NotEqual("", facturaBD.validarAbono("600", 500m, out monto));
            Assert.NotEqual("", facturaBD.validarAbono("0", 500m, out monto));
            Assert.NotEqual("", facturaBD.validarAbono("-10", 500m, out monto));
            Assert.NotEqual("", facturaBD.validarAbono("abc", 500m, out monto));
        }

        [Fact]
        public void dineroTieneFormatoDeLempiras()
        {
            Assert.Equal("L 1,234.50", facturaBD.dinero(1234.5m));
            Assert.Equal("000-001-01-00000008", Facturacion.numeroFactura(8));
        }

        [Fact]
        public void motivoDeAnulacion()
        {
            Assert.Equal("", facturaBD.validarAnulacion("Error en el precio"));
            Assert.NotEqual("", facturaBD.validarAnulacion("ab"));
        }

        [Fact]
        public void ajusteDeInventarioValidaTodo()
        {
            int cantidad;
            Assert.Equal("", inventarioBD.validarAjuste(5, "Merma", "3", "Se dañó el envase", out cantidad));
            Assert.Equal(3, cantidad);
            Assert.NotEqual("", inventarioBD.validarAjuste(0, "Merma", "3", "Se dañó el envase", out cantidad));
            Assert.NotEqual("", inventarioBD.validarAjuste(5, "", "3", "Se dañó el envase", out cantidad));
            Assert.NotEqual("", inventarioBD.validarAjuste(5, "Merma", "0", "Se dañó el envase", out cantidad));
            Assert.Equal("", inventarioBD.validarAjuste(5, "Conteo", "0", "Conteo mensual", out cantidad));
            Assert.NotEqual("", inventarioBD.validarAjuste(5, "Merma", "3", "abc", out cantidad));
            Assert.NotEqual("", inventarioBD.validarAjuste(5, "Merma", "-3", "Se dañó el envase", out cantidad));
            Assert.NotEqual("", inventarioBD.validarAjuste(5, "Merma", "3.5", "Se dañó el envase", out cantidad));
        }

        [Fact]
        public void tiposDeAjuste()
        {
            Assert.Equal("Merma", inventarioBD.valorTipoAjuste("Merma (se dañó o venció)"));
            Assert.Equal("Perdida", inventarioBD.valorTipoAjuste("Pérdida (se extravió)"));
            Assert.Equal("Conteo", inventarioBD.valorTipoAjuste("Conteo físico (stock real)"));
            Assert.Equal("", inventarioBD.valorTipoAjuste(""));
        }

        [Fact]
        public void productoYEquipoNecesitanNombresReales()
        {
            Assert.Equal("", inventarioBD.validarProducto("Cloro (litro)", "Químico"));
            Assert.NotEqual("", inventarioBD.validarProducto("12", "Químico"));
            Assert.NotEqual("", inventarioBD.validarProducto("Cloro", "x"));
            Assert.Equal("", inventarioBD.validarEquipo("Aspiradora industrial", "Limpieza", "Karcher", "NT 70"));
            Assert.NotEqual("", inventarioBD.validarEquipo("A", "Limpieza", "Karcher", "NT 70"));
        }

        [Fact]
        public void clienteConRtnOIdentidadReal()
        {
            Assert.Equal("", clienteBD.validarIdentificacion("RTN", "08011990123456"));
            Assert.Equal("", clienteBD.validarIdentificacion("Identidad", "0801-1990-12345"));
            Assert.NotEqual("", clienteBD.validarIdentificacion("RTN", "00000000000000"));
            Assert.NotEqual("", clienteBD.validarIdentificacion("RTN", "99991990123456"));
            Assert.NotEqual("", clienteBD.validarIdentificacion("Identidad", "08011990123456"));
            Assert.NotEqual("", clienteBD.validarIdentificacion("", "08011990123456"));
        }

        [Fact]
        public void soloUnUsuarioAvanzadoManejaAdministradores()
        {
            UsuarioBD usuarioBD = new UsuarioBD();

            Assert.NotEqual("", usuarioBD.validarPermisoRol("Usuario Avanzado"));
            Assert.Equal("", usuarioBD.validarPermisoRol("Usuario Medio"));
            Assert.Equal("", usuarioBD.validarPermisoRol("Usuario Básico"));
        }

        [Fact]
        public void usuarioYContrasenaDeUnUsuarioNuevo()
        {
            UsuarioBD usuarioBD = new UsuarioBD();

            Assert.Equal("", usuarioBD.validarAcceso("jperez", "Codelex2026", true));
            Assert.NotEqual("", usuarioBD.validarAcceso("jperez", "", true));
            Assert.Equal("", usuarioBD.validarAcceso("jperez", "", false));
            Assert.NotEqual("", usuarioBD.validarAcceso("j p", "Codelex2026", true));
        }
    }
}
