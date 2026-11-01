namespace Codelex.Facturas_y_Cotizaciones
{
    // Línea del detalle, compartida por las ventanas Cotizacion y Factura.
    class LineaDocumento
    {
        public string Origen { get; set; } = "";
        public string Concepto { get; set; } = "";
        public int Cantidad { get; set; } = 0;
        public decimal Precio { get; set; } = 0;

        public decimal Subtotal
        {
            get { return Cantidad * Precio; }
        }
    }
}