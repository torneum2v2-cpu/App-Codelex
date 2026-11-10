using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Codelex.Clases_BDD
{
    class DatosDocumento
    {
        // Cliente
        public string Rtn = "";
        public string Empresa = "";
        public string Contacto = "";
        public string Apellido = "";
        public string Telefono = "";
        public string Correo = "";

        // Lugar
        public int LocacionId = 0;
        public string Direccion = "";

        // Documento
        public DateTime? Fecha = null;
        public DateTime? ValidaHasta = null;
        public string Observaciones = "";

        // Solo factura
        public string OrdenId = "";
        public int FormaPagoId = 0;
        public int Cuotas = 1;

        // Cotización: servicio y actividades (listas paralelas; la medida llega como la escribió el usuario)
        public int ServicioId = 0;
        public List<int> ActividadesId = new List<int>();
        public List<string> ActividadesNombre = new List<string>();
        public List<string> ActividadesMedida = new List<string>();

        // Factura: detalle (listas paralelas, igual que productos en DatosOrden)
        public List<string> LineasConcepto = new List<string>();
        public List<int> LineasCantidad = new List<int>();
        public List<decimal> LineasPrecio = new List<decimal>();
        public List<bool> LineasAdicional = new List<bool>();
    }

    class Facturacion : BaseDatos
    {
        public const decimal TasaIsv = 0.15m;

        public DataTable ListarServicios()
        {
            return Consultar(Procedimiento("sp_ListarServicios"));
        }

        public DataTable ListarActividades()
        {
            return Consultar(Procedimiento("sp_ListarActividades"));
        }

        public DataTable ListarFormasPago()
        {
            return Consultar(Procedimiento("sp_ListarFormasPago"));
        }

        public DataTable ListarOrdenesFacturables(string texto)
        {
            SqlCommand comando = Procedimiento("sp_ListarOrdenesFacturables");
            comando.Parameters.AddWithValue("@texto", texto.Trim());
            return Consultar(comando);
        }

        public DataTable DetalleOrden(string ordenId)
        {
            SqlCommand comando = Procedimiento("sp_DetalleOrdenFactura");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return Consultar(comando);
        }

        public DataTable ObtenerFactura(int facturaId)
        {
            SqlCommand comando = Procedimiento("sp_ObtenerFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return Consultar(comando);
        }

        public DataTable DetalleFactura(int facturaId)
        {
            SqlCommand comando = Procedimiento("sp_DetalleFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return Consultar(comando);
        }

        public DataTable CuotasFactura(int facturaId)
        {
            SqlCommand comando = Procedimiento("sp_CuotasFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return Consultar(comando);
        }

        // Calcula el precio de cada actividad con la fórmula de costeo (la fórmula vive en la base de datos).
        // Las medidas vacías se ignoran; si alguna no es válida, mensaje dice cuál.
        public DataTable CotizarActividades(List<int> ids, List<string> medidas, out string mensaje)
        {
            mensaje = "";

            DataTable tabla = new DataTable();
            tabla.Columns.Add("actividad_id", typeof(int));
            tabla.Columns.Add("medida", typeof(decimal));

            for (int i = 0; i < ids.Count; i++)
            {
                if (medidas[i].Trim() == "")
                    continue;

                decimal medida;
                string error = ValidarMedida(medidas[i], out medida);

                if (error != "")
                {
                    mensaje = error;
                    continue;
                }

                tabla.Rows.Add(ids[i], medida);
            }

            if (tabla.Rows.Count == 0)
                return new DataTable();

            SqlCommand comando = Procedimiento("sp_CotizarLineas");
            AgregarLista(comando, "@lineas", "dbo.tipo_lista_medidas", tabla);
            return Consultar(comando);
        }

        // ---------- Cálculos ----------

        public decimal CalcularIsv(decimal subtotal)
        {
            return Math.Round(subtotal * TasaIsv, 2, MidpointRounding.AwayFromZero);
        }

        public decimal CalcularCuota(decimal total, int cuotas)
        {
            if (cuotas <= 0)
                cuotas = 1;

            return Math.Round(total / cuotas, 2, MidpointRounding.AwayFromZero);
        }

        public string Dinero(decimal valor)
        {
            return "L " + valor.ToString("N2", CultureInfo.InvariantCulture);
        }

        // ---------- Validaciones ----------

        public string ValidarLinea(string concepto, string cantidadTexto, string precioTexto,
                                   out int cantidad, out decimal precio)
        {
            cantidad = 0;
            precio = 0;

            concepto = concepto.Trim();
            cantidadTexto = cantidadTexto.Trim();
            precioTexto = precioTexto.Trim();

            if (concepto == "")
                return "Escriba o seleccione el concepto.";
            if (concepto.Length > 150)
                return "El concepto no puede tener más de 150 caracteres.";

            if (cantidadTexto == "")
                return "Escriba la cantidad.";
            if (!int.TryParse(cantidadTexto, out cantidad))
                return "La cantidad no es válida.";
            if (cantidad <= 0)
                return "La cantidad debe ser mayor que 0.";

            if (precioTexto == "")
                return "Escriba el precio.";
            if (!decimal.TryParse(precioTexto, NumberStyles.Number, CultureInfo.InvariantCulture, out precio))
                return "El precio no es válido.";
            if (precio <= 0)
                return "El precio debe ser mayor que 0.";

            return "";
        }

        public string ValidarMedida(string texto, out decimal medida)
        {
            medida = 0;
            texto = texto.Trim().Replace(',', '.');

            if (texto == "")
                return "Escriba la cantidad (m² o unidades).";
            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out medida))
                return "La cantidad \"" + texto + "\" no es válida.";
            if (medida <= 0)
                return "La cantidad debe ser mayor que 0.";
            if (medida > 1000000)
                return "La cantidad es demasiado grande.";

            return "";
        }

        private string ValidarActividades(DatosDocumento datos)
        {
            if (datos.ServicioId <= 0)
                return "Seleccione el servicio.";
            if (datos.ActividadesId.Count == 0)
                return "Agregue al menos una actividad.";

            for (int i = 0; i < datos.ActividadesId.Count; i++)
            {
                decimal medida;
                string error = ValidarMedida(datos.ActividadesMedida[i], out medida);

                if (error != "")
                    return datos.ActividadesNombre[i] + ": " + error;
            }

            return "";
        }

        private string ValidarCotizacion(DatosDocumento datos)
        {
            if (datos.Contacto.Trim() == "")
                return "Escriba el nombre del contacto.";
            if (datos.Rtn.Trim().Length > 30)
                return "El RTN no puede tener más de 30 caracteres.";
            if (datos.Empresa.Trim().Length > 50 || datos.Contacto.Trim().Length > 50
                || datos.Apellido.Trim().Length > 50)
                return "La empresa y el nombre no pueden tener más de 50 caracteres.";
            if (datos.Telefono.Trim() == "" && datos.Correo.Trim() == "")
                return "Escriba un teléfono o un correo del cliente.";
            if (datos.Telefono.Trim().Length > 50 || datos.Correo.Trim().Length > 50)
                return "El teléfono y el correo no pueden tener más de 50 caracteres.";
            if (datos.Direccion.Trim() == "")
                return "Escriba la dirección del servicio.";
            if (datos.Direccion.Trim().Length > 80)
                return "La dirección no puede tener más de 80 caracteres.";

            if (!datos.Fecha.HasValue)
                return "Seleccione la fecha.";
            if (!datos.ValidaHasta.HasValue)
                return "Seleccione hasta cuándo es válida la cotización.";
            if (datos.ValidaHasta.Value < datos.Fecha.Value)
                return "La fecha de validez no puede ser anterior a la fecha de la cotización.";

            if (datos.Observaciones.Trim().Length > 200)
                return "Las observaciones no pueden tener más de 200 caracteres.";

            return ValidarActividades(datos);
        }

        private string ValidarFactura(DatosDocumento datos)
        {
            if (datos.OrdenId.Trim() == "")
                return "Seleccione una orden de la lista inferior.";
            if (!datos.Fecha.HasValue)
                return "Seleccione la fecha.";
            if (datos.FormaPagoId <= 0)
                return "Seleccione el método de pago.";
            if (datos.Cuotas <= 0 || datos.Cuotas > 24)
                return "Seleccione el número de cuotas.";

            return ValidarDetalle(datos);
        }

        private string ValidarDetalle(DatosDocumento datos)
        {
            if (datos.LineasConcepto.Count == 0)
                return "Agregue al menos un servicio al detalle.";

            return "";
        }

        // ---------- Guardado ----------

        public bool GuardarCotizacion(DatosDocumento datos, out string mensaje, out string cotizacionId)
        {
            cotizacionId = "";

            mensaje = ValidarCotizacion(datos);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento("sp_InsertarCotizacion");
            comando.Parameters.AddWithValue("@identificacionCliente", datos.Rtn.Trim());
            comando.Parameters.AddWithValue("@nombreEmpresa", datos.Empresa.Trim());
            comando.Parameters.AddWithValue("@nombreCliente", datos.Contacto.Trim());
            comando.Parameters.AddWithValue("@apellidoCliente", datos.Apellido.Trim());
            comando.Parameters.AddWithValue("@telefonoCliente", datos.Telefono.Trim());
            comando.Parameters.AddWithValue("@correoCliente", datos.Correo.Trim());

            if (datos.LocacionId > 0)
                comando.Parameters.AddWithValue("@locacion_id", datos.LocacionId);
            else
                comando.Parameters.AddWithValue("@locacion_id", DBNull.Value);

            comando.Parameters.AddWithValue("@direccion", datos.Direccion.Trim());
            comando.Parameters.AddWithValue("@fecha_cotizacion", datos.Fecha.Value);
            comando.Parameters.AddWithValue("@valida_hasta", datos.ValidaHasta.Value);
            comando.Parameters.AddWithValue("@observaciones", datos.Observaciones.Trim());

            comando.Parameters.AddWithValue("@servicio_id", datos.ServicioId);
            AgregarLista(comando, "@lineas", "dbo.tipo_lista_medidas", TablaDeMedidas(datos));

            SqlParameter idParametro = new SqlParameter("@cotizacion_id", SqlDbType.VarChar, 50);
            idParametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(idParametro);

            bool guardada = Ejecutar(comando, out mensaje);

            if (guardada)
            {
                cotizacionId = "" + idParametro.Value;
                mensaje = "Cotización " + cotizacionId + " registrada correctamente.";
            }

            return guardada;
        }

        public bool GuardarFactura(DatosDocumento datos, out string mensaje, out string facturaId)
        {
            facturaId = "";

            mensaje = ValidarFactura(datos);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento("sp_InsertarFactura");
            comando.Parameters.AddWithValue("@orden_id", datos.OrdenId.Trim());
            comando.Parameters.AddWithValue("@fecha_factura", datos.Fecha.Value);
            comando.Parameters.AddWithValue("@forma_pago_id", datos.FormaPagoId);
            comando.Parameters.AddWithValue("@numero_cuotas", datos.Cuotas);

            AgregarLista(comando, "@lineas", "dbo.tipo_lista_lineas", TablaDeLineas(datos));

            SqlParameter idParametro = new SqlParameter("@factura_id", SqlDbType.Int);
            idParametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(idParametro);

            bool guardada = Ejecutar(comando, out mensaje);

            if (guardada)
            {
                facturaId = "FAC-" + ("" + idParametro.Value).PadLeft(6, '0');
                mensaje = "Factura " + facturaId + " registrada correctamente.";
            }

            return guardada;
        }

        // ---------- Auxiliares ----------

        private void AgregarLista(SqlCommand comando, string nombre, string tipo, DataTable tabla)
        {
            SqlParameter parametro = comando.Parameters.AddWithValue(nombre, tabla);
            parametro.SqlDbType = SqlDbType.Structured;
            parametro.TypeName = tipo;
        }

        private DataTable TablaDeMedidas(DatosDocumento datos)
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add("actividad_id", typeof(int));
            tabla.Columns.Add("medida", typeof(decimal));

            for (int i = 0; i < datos.ActividadesId.Count; i++)
            {
                decimal medida;
                ValidarMedida(datos.ActividadesMedida[i], out medida);
                tabla.Rows.Add(datos.ActividadesId[i], medida);
            }

            return tabla;
        }

        private DataTable TablaDeLineas(DatosDocumento datos)
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add("concepto", typeof(string));
            tabla.Columns.Add("cantidad", typeof(int));
            tabla.Columns.Add("precio_unitario", typeof(decimal));
            tabla.Columns.Add("adicional", typeof(bool));

            for (int i = 0; i < datos.LineasConcepto.Count; i++)
            {
                tabla.Rows.Add(datos.LineasConcepto[i], datos.LineasCantidad[i],
                               datos.LineasPrecio[i], datos.LineasAdicional[i]);
            }

            return tabla;
        }
    }
}