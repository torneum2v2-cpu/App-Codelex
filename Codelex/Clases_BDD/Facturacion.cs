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

        // Cotización: servicio y actividades (listas paralelas; la medida llega como la escribió el usuario)
        public int ServicioId = 0;
        public List<int> ActividadesId = new List<int>();
        public List<string> ActividadesNombre = new List<string>();
        public List<string> ActividadesMedida = new List<string>();

    }

    // Lo único que el usuario decide al facturar: cómo se paga.
    class DatosFactura
    {
        public string OrdenId = "";
        public string TipoPago = "";
        public int FormaPagoId = 0;
        public int Cuotas = 0;
        public DateTime Fecha = DateTime.Today;
    }

    class Facturacion : BaseDatos
    {
        public const decimal TasaIsv = 0.15m;
        public const string Contado = "Contado";
        public const string Credito = "Crédito";

        public List<string> TiposPago()
        {
            List<string> tipos = new List<string>();
            tipos.Add(Contado);
            tipos.Add(Credito);
            return tipos;
        }

        public List<int> OpcionesCuotas()
        {
            List<int> cuotas = new List<int>();

            for (int i = 2; i <= 24; i++)
            {
                cuotas.Add(i);
            }

            return cuotas;
        }

        // Número de factura con formato profesional: 000-001-01-00000001
        public static string NumeroFactura(int facturaId)
        {
            return "000-001-01-" + facturaId.ToString().PadLeft(8, '0');
        }

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

        public DataTable ObtenerOrdenFactura(string ordenId)
        {
            SqlCommand comando = Procedimiento("sp_ObtenerOrdenFactura");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return Consultar(comando);
        }

        public DataTable DetalleOrden(string ordenId)
        {
            SqlCommand comando = Procedimiento("sp_DetalleOrdenFactura");
            comando.Parameters.AddWithValue("@orden_id", ordenId);

            DataTable tabla = Consultar(comando);
            tabla.Columns.Add("tipo", typeof(string), "'Servicio'");
            tabla.Columns.Add("subtotal", typeof(decimal), "cantidad * precio_unitario");
            return tabla;
        }

        public string ValidarOrdenFacturable(DataTable orden)
        {
            if (orden.Rows.Count == 0)
                return "No se encontró la orden.";
            if (Convert.ToInt32(orden.Rows[0]["facturable"]) == 0)
                return "Esta orden no se puede facturar: debe estar completada y sin factura.";

            return "";
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

        public void ActualizarCuotasVencidas()
        {
            Consultar(Procedimiento("sp_ActualizarCuotasVencidas"));
        }

        public DataTable ListarAbonos(int facturaId)
        {
            SqlCommand comando = Procedimiento("sp_ListarAbonosFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return Consultar(comando);
        }

        // Cómo quedarían las cuotas si se abona ese monto.
        public DataTable SimularAbono(int facturaId, string montoTexto, decimal saldo, out string mensaje)
        {
            decimal monto;
            mensaje = ValidarAbono(montoTexto, saldo, out monto);

            if (mensaje != "")
                return new DataTable();

            SqlCommand comando = Procedimiento("sp_SimularAbono");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            comando.Parameters.AddWithValue("@monto", monto);
            return Consultar(comando);
        }

        // El abono usa el método de pago que se eligió al crear la factura.
        public bool RegistrarAbono(int facturaId, string montoTexto, decimal saldo, out string mensaje)
        {
            decimal monto;
            mensaje = ValidarAbono(montoTexto, saldo, out monto);

            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento("sp_RegistrarAbono");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            comando.Parameters.AddWithValue("@monto", monto);
            comando.Parameters.AddWithValue("@fecha_abono", DateTime.Today);
            return Ejecutar(comando, out mensaje);
        }

        // La cuota elegida; si no se eligió ninguna, la primera que todavía tiene saldo.
        public DataRow? ElegirCuota(DataTable cuotas, DataRowView? seleccion, out string mensaje)
        {
            mensaje = "";

            if (seleccion != null)
            {
                if (Convert.ToDecimal(seleccion["saldo"]) <= 0)
                {
                    mensaje = "Esa cuota ya está pagada.";
                    return null;
                }

                return seleccion.Row;
            }

            foreach (DataRow cuota in cuotas.Rows)
            {
                if (Convert.ToDecimal(cuota["saldo"]) > 0)
                    return cuota;
            }

            mensaje = "No hay cuotas pendientes.";
            return null;
        }

        public bool SaldarCuota(int cuotaId, out string mensaje)
        {
            SqlCommand comando = Procedimiento("sp_SaldarCuota");
            comando.Parameters.AddWithValue("@cuota_id", cuotaId);
            comando.Parameters.AddWithValue("@fecha_pago", DateTime.Today);
            return Ejecutar(comando, out mensaje);
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
            if (Cliente.LimpiarNumero(datos.Rtn) != "")
            {
                Cliente clienteBD = new Cliente();
                string rtn = Cliente.LimpiarNumero(datos.Rtn);
                string problema = clienteBD.ValidarIdentificacion(clienteBD.TipoPorLongitud(rtn), rtn);

                if (problema != "")
                    return problema;
            }
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

        public string ValidarAbono(string texto, decimal saldo, out decimal monto)
        {
            monto = 0;
            texto = texto.Trim().Replace(",", "");

            if (texto == "")
                return "Escriba el monto del abono.";
            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out monto))
                return "El monto no es válido.";
            if (monto <= 0)
                return "El abono debe ser mayor que 0.";
            if (monto > saldo)
                return "El abono no puede ser mayor que el saldo pendiente (" + Dinero(saldo) + ").";

            return "";
        }

        private string ValidarFactura(DatosFactura datos)
        {
            if (datos.OrdenId.Trim() == "")
                return "No se indicó la orden a facturar.";
            if (datos.TipoPago != Contado && datos.TipoPago != Credito)
                return "Seleccione si es al contado o a crédito.";
            if (datos.FormaPagoId <= 0)
                return "Seleccione el método de pago.";
            if (datos.TipoPago == Credito && (datos.Cuotas < 2 || datos.Cuotas > 24))
                return "Seleccione el número de cuotas.";

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
            comando.Parameters.AddWithValue("@identificacionCliente", Cliente.LimpiarNumero(datos.Rtn));
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

        public bool GuardarFactura(DatosFactura datos, out string mensaje, out int facturaId)
        {
            facturaId = 0;

            mensaje = ValidarFactura(datos);
            if (mensaje != "")
                return false;

            int cuotas = 1;
            if (datos.TipoPago == Credito)
                cuotas = datos.Cuotas;

            SqlCommand comando = Procedimiento("sp_InsertarFactura");
            comando.Parameters.AddWithValue("@orden_id", datos.OrdenId.Trim());
            comando.Parameters.AddWithValue("@fecha_factura", datos.Fecha);
            comando.Parameters.AddWithValue("@forma_pago_id", datos.FormaPagoId);
            comando.Parameters.AddWithValue("@numero_cuotas", cuotas);

            SqlParameter idParametro = new SqlParameter("@factura_id", SqlDbType.Int);
            idParametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(idParametro);

            bool guardada = Ejecutar(comando, out mensaje);

            if (guardada)
            {
                facturaId = Convert.ToInt32(idParametro.Value);
                mensaje = "Factura " + NumeroFactura(facturaId) + " registrada correctamente.";
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
    }
}