using Codelex.AccesoDatos;
using System.Data;
using System.Windows;

namespace Codelex.FacturasCotizaciones
{
    /// <summary>
    /// Imprime una cotización con los datos guardados en la base (encabezado, líneas y totales).
    /// Se usa al guardar una cotización y desde la pantalla de seleccionar cotización.
    /// </summary>
    public static class ImpresionCotizacion
    {
        /// <summary>
        /// Lee la cotización de la base, arma el documento y abre el cuadro de impresión.
        /// </summary>
        /// <param name="dueno">Ventana dueña del cuadro.</param>
        /// <param name="cotizacionId">Número de la cotización (COT-000001).</param>
        public static void imprimir(Window dueno, string cotizacionId)
        {
            Orden ordenBD = new Orden();
            Facturacion facturaBD = new Facturacion();

            DataTable cabecera = ordenBD.obtenerCotizacion(cotizacionId);
            if (cabecera.Rows.Count == 0)
            {
                MensajeCodelex.mostrar(dueno, "Cotización", "La cotización " + cotizacionId + " no existe.", false);
                return;
            }

            DataRow c = cabecera.Rows[0];
            DataTable detalle = ordenBD.detalleCotizacion(cotizacionId);

            List<string> encabezado = new List<string>();
            encabezado.Add("Fecha: " + fecha(c["fecha_cotizacion"]) + "     Válida hasta: " + fecha(c["valida_hasta"]));

            string servicio = ("" + c["servicio"]).Trim();
            if (servicio != "")
                encabezado.Add("Servicio: " + servicio);

            encabezado.Add("Cliente: " + textoCliente(c));

            string contacto = ("" + c["contacto"]).Trim();
            string telefono = ("" + c["telefono_cliente"]).Trim();
            string lineaContacto = "Contacto: " + contacto;
            if (telefono != "")
                lineaContacto += "     Teléfono: " + Validacion.formatoTelefono(telefono);
            encabezado.Add(lineaContacto);

            string correo = ("" + c["correo_cliente"]).Trim();
            if (correo != "")
                encabezado.Add("Correo: " + correo);

            string locacion = ("" + c["locacion"]).Trim();
            string direccion = ("" + c["direccion"]).Trim();
            if (locacion != "" && direccion != "")
                encabezado.Add("Locación: " + locacion + " - " + direccion);
            else if (locacion != "")
                encabezado.Add("Locación: " + locacion);
            else if (direccion != "")
                encabezado.Add("Dirección: " + direccion);

            encabezado.Add("Personas necesarias: " + c["cantidad_personas"]);

            string observaciones = ("" + c["observacion"]).Trim();
            if (observaciones != "")
                encabezado.Add("Observaciones: " + observaciones);

            DataTable lineas = ImpresionDocumento.crearLineas();
            foreach (DataRow fila in detalle.Rows)
            {
                // El descuento no va en la tabla: se muestra en los totales.
                if (Convert.ToInt32(fila["actividad_id"]) == 0)
                    continue;

                string cantidad = Convert.ToDecimal(fila["medida"]).ToString("0.##") + " " + fila["unidad"];

                ImpresionDocumento.agregarLinea(lineas, "" + fila["concepto"], cantidad.Trim(),
                    facturaBD.dinero(Convert.ToDecimal(fila["precio_unitario"])),
                    facturaBD.dinero(Convert.ToDecimal(fila["subtotal"])));
            }

            TotalesCotizacion totales = ordenBD.calcularTotalesCotizacion(detalle, Convert.ToDecimal(c["precio_cotizado"]));

            List<string> textoTotales = new List<string>();
            textoTotales.Add("Subtotal: " + facturaBD.dinero(totales.subtotal));
            if (totales.descuento > 0)
                textoTotales.Add(totales.textoDescuento + ": - " + facturaBD.dinero(totales.descuento));
            textoTotales.Add("ISV: " + facturaBD.dinero(totales.isv));
            textoTotales.Add("Total: " + facturaBD.dinero(totales.total));

            ImpresionDocumento.imprimir(dueno, "Cotización", cotizacionId, encabezado, lineas, textoTotales);
        }

        /// <summary>
        /// Arma el texto del cliente: empresa (o contacto) y su RTN o identidad.
        /// </summary>
        /// <param name="c">Fila del encabezado de la cotización.</param>
        /// <returns>Texto del cliente.</returns>
        private static string textoCliente(DataRow c)
        {
            string empresa = ("" + c["nombre_empresa"]).Trim();
            string rtn = ("" + c["identificacion_cliente"]).Trim();

            string nombre = empresa;
            if (nombre == "")
                nombre = ("" + c["contacto"]).Trim();

            if (rtn == "")
                return nombre;

            return nombre + " (RTN/Identidad: " + rtn + ")";
        }

        /// <summary>
        /// Da formato dd/MM/yyyy a una fecha de la base.
        /// </summary>
        /// <param name="valor">Valor leído de la base.</param>
        /// <returns>Fecha con formato, o vacío si no hay fecha.</returns>
        private static string fecha(object valor)
        {
            if (valor == DBNull.Value)
                return "";

            return Convert.ToDateTime(valor).ToString("dd/MM/yyyy");
        }
    }
}
