using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;

namespace Codelex.AccesoDatos
{
    class DatosDocumento
    {
        // Cliente
        public string rtn = "";
        public string empresa = "";
        public string contacto = "";
        public string apellido = "";
        public string telefono = "";
        public string correo = "";

        // Lugar
        public int locacionId = 0;
        public string direccion = "";

        // Documento
        public DateTime? fecha = null;
        public DateTime? validaHasta = null;
        public string observaciones = "";
        public string descuento = "0";

        // Cotización: servicio y actividades (listas paralelas; la medida llega como la escribió el usuario)
        public int servicioId = 0;
        public List<int> actividadesId = new List<int>();
        public List<string> actividadesNombre = new List<string>();
        public List<string> actividadesMedida = new List<string>();

    }

    // Lo único que el usuario decide al facturar: cómo se paga.
    class DatosFactura
    {
        public string ordenId = "";
        public string tipoPago = "";
        public int formaPagoId = 0;
        public int cuotas = 0;
        public DateTime fecha = DateTime.Today;
    }

    class Facturacion : BaseDatos
    {
        public const decimal TASA_ISV = 0.15m;
        public const string CONTADO = "Contado";
        public const string CREDITO = "Crédito";

        /// <summary>
        /// Lista los tipos de pago (Contado y Crédito).
        /// </summary>
        /// <returns>Lista con los elementos obtenidos.</returns>
        public List<string> tiposPago()
        {
            List<string> tipos = new List<string>();
            tipos.Add(CONTADO);
            tipos.Add(CREDITO);
            return tipos;
        }

        /// <summary>
        /// Lista las cantidades de cuotas disponibles (de 2 a 24).
        /// </summary>
        /// <returns>Lista con los elementos obtenidos.</returns>
        public List<int> opcionesCuotas()
        {
            List<int> cuotas = new List<int>();

            for (int i = 2; i <= 24; i++)
            {
                cuotas.Add(i);
            }

            return cuotas;
        }

        // Número de factura con formato profesional: 000-001-01-00000001
        /// <summary>
        /// Arma el número de factura con el formato 000-001-01-00000001.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <returns>Número de factura, por ejemplo 000-001-01-00000008.</returns>
        public static string numeroFactura(int facturaId)
        {
            return "000-001-01-" + facturaId.ToString().PadLeft(8, '0');
        }

        /// <summary>
        /// Lista servicios.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarServicios()
        {
            return consultar(procedimiento("sp_ListarServiciosConActividades"));
        }

        /// <summary>
        /// Lista actividades.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarActividades()
        {
            return consultar(procedimiento("sp_ListarActividades"));
        }

        /// <summary>
        /// Lista formas pago.
        /// </summary>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarFormasPago()
        {
            return consultar(procedimiento("sp_ListarFormasPago"));
        }

        /// <summary>
        /// Obtiene orden factura.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable obtenerOrdenFactura(string ordenId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerOrdenFactura");
            comando.Parameters.AddWithValue("@orden_id", ordenId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista las líneas de la cotización de una orden, con su subtotal.
        /// </summary>
        /// <param name="ordenId">Identificador de orden.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable detalleOrden(string ordenId)
        {
            SqlCommand comando = procedimiento("sp_DetalleOrdenFactura");
            comando.Parameters.AddWithValue("@orden_id", ordenId);

            DataTable tabla = consultar(comando);
            tabla.Columns.Add("tipo", typeof(string), "'Servicio'");
            tabla.Columns.Add("subtotal", typeof(decimal), "cantidad * precio_unitario");
            return tabla;
        }

        /// <summary>
        /// Valida orden facturable.
        /// </summary>
        /// <param name="orden">Orden.</param>
        /// <returns>Texto resultante.</returns>
        public string validarOrdenFacturable(DataTable orden)
        {
            if (orden.Rows.Count == 0)
                return "No se encontró la orden.";
            if (Convert.ToInt32(orden.Rows[0]["facturable"]) == 0)
                return "Esta orden no se puede facturar: debe estar completada y sin factura.";

            return "";
        }

        /// <summary>
        /// Obtiene factura.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable obtenerFactura(int facturaId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista las líneas de una factura.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable detalleFactura(int facturaId)
        {
            SqlCommand comando = procedimiento("sp_DetalleFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return consultar(comando);
        }

        /// <summary>
        /// Lista las cuotas de una factura.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable cuotasFactura(int facturaId)
        {
            SqlCommand comando = procedimiento("sp_CuotasFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return consultar(comando);
        }

        /// <summary>
        /// Actualiza cuotas vencidas.
        /// </summary>
        public void actualizarCuotasVencidas()
        {
            consultar(procedimiento("sp_ActualizarCuotasVencidas"));
        }

        /// <summary>
        /// Lista abonos.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable listarAbonos(int facturaId)
        {
            SqlCommand comando = procedimiento("sp_ListarAbonosFactura");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            return consultar(comando);
        }

        // Cómo quedarían las cuotas si se abona ese monto.
        /// <summary>
        /// Simula abono.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <param name="montoTexto">Monto texto.</param>
        /// <param name="saldo">Saldo pendiente en lempiras.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable simularAbono(int facturaId, string montoTexto, decimal saldo, out string mensaje)
        {
            decimal monto;
            mensaje = validarAbono(montoTexto, saldo, out monto);

            if (mensaje != "")
                return new DataTable();

            SqlCommand comando = procedimiento("sp_SimularAbono");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            comando.Parameters.AddWithValue("@monto", monto);
            return consultar(comando);
        }

        // El abono usa el método de pago que se eligió al crear la factura.
        /// <summary>
        /// Registra abono.
        /// </summary>
        /// <param name="facturaId">Identificador de factura.</param>
        /// <param name="montoTexto">Monto texto.</param>
        /// <param name="saldo">Saldo pendiente en lempiras.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool registrarAbono(int facturaId, string montoTexto, decimal saldo, out string mensaje)
        {
            decimal monto;
            mensaje = validarAbono(montoTexto, saldo, out monto);

            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_RegistrarAbono");
            comando.Parameters.AddWithValue("@factura_id", facturaId);
            comando.Parameters.AddWithValue("@monto", monto);
            comando.Parameters.AddWithValue("@fecha_abono", DateTime.Today);
            return ejecutar(comando, out mensaje);
        }

        // La cuota elegida; si no se eligió ninguna, la primera que todavía tiene saldo.
        /// <summary>
        /// Elige cuota.
        /// </summary>
        /// <param name="cuotas">Cantidad de cuotas.</param>
        /// <param name="seleccion">Selección.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>Fila elegida o null si no hay.</returns>
        public DataRow? elegirCuota(DataTable cuotas, DataRowView? seleccion, out string mensaje)
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

        /// <summary>
        /// Salda cuota.
        /// </summary>
        /// <param name="cuotaId">Identificador de cuota.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool saldarCuota(int cuotaId, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_SaldarCuota");
            comando.Parameters.AddWithValue("@cuota_id", cuotaId);
            comando.Parameters.AddWithValue("@fecha_pago", DateTime.Today);
            return ejecutar(comando, out mensaje);
        }

        // Calcula el precio de cada actividad con la fórmula de costeo (la fórmula vive en la base de datos).
        // Las medidas vacías se ignoran; si alguna no es válida, mensaje dice cuál.
        /// <summary>
        /// Calcula el precio de actividades.
        /// </summary>
        /// <param name="ids">Identificadores.</param>
        /// <param name="medidas">Cantidades escritas por el usuario.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable cotizarActividades(List<int> ids, List<string> medidas, out string mensaje)
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
                string error = validarMedida(medidas[i], out medida);

                if (error != "")
                {
                    mensaje = error;
                    continue;
                }

                tabla.Rows.Add(ids[i], medida);
            }

            if (tabla.Rows.Count == 0)
                return new DataTable();

            SqlCommand comando = procedimiento("sp_CotizarLineas");
            agregarLista(comando, "@lineas", "dbo.tipo_lista_medidas", tabla);
            return consultar(comando);
        }

        // ---------- Cálculos ----------

        /// <summary>
        /// Calcula isv.
        /// </summary>
        /// <param name="subtotal">Subtotal en lempiras.</param>
        /// <returns>Valor decimal resultante.</returns>
        public decimal calcularIsv(decimal subtotal)
        {
            return Math.Round(subtotal * TASA_ISV, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Convierte el texto del descuento a porcentaje; si no es válido devuelve 0.
        /// </summary>
        /// <param name="texto">Porcentaje escrito por el usuario.</param>
        /// <returns>Porcentaje de descuento (0 si el texto no es válido).</returns>
        public decimal leerDescuento(string texto)
        {
            decimal porcentaje;

            if (validarDescuento(texto, out porcentaje) != "")
                return 0;

            return porcentaje;
        }

        /// <summary>
        /// Calcula el monto del descuento sobre un subtotal.
        /// </summary>
        /// <param name="subtotal">Subtotal en lempiras.</param>
        /// <param name="porcentaje">Porcentaje de descuento.</param>
        /// <returns>Monto del descuento en lempiras.</returns>
        public decimal calcularDescuento(decimal subtotal, decimal porcentaje)
        {
            return Math.Round(subtotal * porcentaje / 100, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calcula cuota.
        /// </summary>
        /// <param name="total">Total en lempiras.</param>
        /// <param name="cuotas">Cantidad de cuotas.</param>
        /// <returns>Valor decimal resultante.</returns>
        public decimal calcularCuota(decimal total, int cuotas)
        {
            if (cuotas <= 0)
                cuotas = 1;

            return Math.Round(total / cuotas, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Da formato de lempiras (L 1,234.50) a un monto.
        /// </summary>
        /// <param name="valor">Valor a usar.</param>
        /// <returns>Texto con el monto, por ejemplo L 1,234.50.</returns>
        public string dinero(decimal valor)
        {
            return "L " + valor.ToString("N2", CultureInfo.InvariantCulture);
        }

        // ---------- Validaciones ----------

        /// <summary>
        /// Valida el porcentaje de descuento: vacío cuenta como 0; debe ser menor que 100.
        /// </summary>
        /// <param name="texto">Porcentaje escrito por el usuario.</param>
        /// <param name="porcentaje">(salida) Porcentaje obtenido.</param>
        /// <returns>Mensaje de error, o texto vacío si es válido.</returns>
        public string validarDescuento(string texto, out decimal porcentaje)
        {
            porcentaje = 0;
            texto = texto.Trim().Replace(',', '.');

            if (texto == "")
                return "";
            if (!decimal.TryParse(texto, NumberStyles.Number, CultureInfo.InvariantCulture, out porcentaje))
                return "El descuento \"" + texto + "\" no es válido.";
            if (porcentaje < 0 || porcentaje >= 100)
                return "El descuento debe ser mayor o igual a 0 y menor que 100.";

            return "";
        }

        /// <summary>
        /// Valida medida.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="medida">(salida) Cantidad medida (m2 o unidades).</param>
        /// <returns>Texto resultante.</returns>
        public string validarMedida(string texto, out decimal medida)
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

        /// <summary>
        /// Valida actividades.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <returns>Texto resultante.</returns>
        private string validarActividades(DatosDocumento datos)
        {
            if (datos.servicioId <= 0)
                return "Seleccione el servicio.";
            if (datos.actividadesId.Count == 0)
                return "Agregue al menos una actividad.";

            for (int i = 0; i < datos.actividadesId.Count; i++)
            {
                decimal medida;
                string error = validarMedida(datos.actividadesMedida[i], out medida);

                if (error != "")
                    return datos.actividadesNombre[i] + ": " + error;
            }

            return "";
        }

        /// <summary>
        /// Valida cotización.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <returns>Texto resultante.</returns>
        private string validarCotizacion(DatosDocumento datos)
        {
            if (datos.contacto.Trim() == "")
                return "Escriba el nombre del contacto.";
            if (Cliente.limpiarNumero(datos.rtn) != "")
            {
                Cliente clienteBD = new Cliente();
                string rtn = Cliente.limpiarNumero(datos.rtn);
                string problema = clienteBD.validarIdentificacion(clienteBD.tipoPorLongitud(rtn), rtn);

                if (problema != "")
                    return problema;
            }
            if (datos.empresa.Trim().Length > 50 || datos.contacto.Trim().Length > 50
                || datos.apellido.Trim().Length > 50)
                return "La empresa y el nombre no pueden tener más de 50 caracteres.";
            if (datos.telefono.Trim() == "" && datos.correo.Trim() == "")
                return "Escriba un teléfono o un correo del cliente.";
            if (datos.telefono.Trim().Length > 50 || datos.correo.Trim().Length > 50)
                return "El teléfono y el correo no pueden tener más de 50 caracteres.";
            if (datos.direccion.Trim() == "")
                return "Escriba la dirección del servicio.";
            if (datos.direccion.Trim().Length > 80)
                return "La dirección no puede tener más de 80 caracteres.";

            if (!datos.fecha.HasValue)
                return "Seleccione la fecha.";
            if (!datos.validaHasta.HasValue)
                return "Seleccione hasta cuándo es válida la cotización.";
            if (datos.validaHasta.Value < datos.fecha.Value)
                return "La fecha de validez no puede ser anterior a la fecha de la cotización.";

            if (datos.observaciones.Trim().Length > 200)
                return "Las observaciones no pueden tener más de 200 caracteres.";

            decimal porcentaje;
            string errorDescuento = validarDescuento(datos.descuento, out porcentaje);
            if (errorDescuento != "")
                return errorDescuento;

            return validarActividades(datos);
        }

        /// <summary>
        /// Valida abono.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="saldo">Saldo pendiente en lempiras.</param>
        /// <param name="monto">(salida) Monto en lempiras.</param>
        /// <returns>Texto resultante.</returns>
        public string validarAbono(string texto, decimal saldo, out decimal monto)
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
                return "El abono no puede ser mayor que el saldo pendiente (" + dinero(saldo) + ").";

            return "";
        }

        /// <summary>
        /// Valida factura.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <returns>Texto resultante.</returns>
        private string validarFactura(DatosFactura datos)
        {
            if (datos.ordenId.Trim() == "")
                return "No se indicó la orden a facturar.";
            if (datos.tipoPago != CONTADO && datos.tipoPago != CREDITO)
                return "Seleccione si es al contado o a crédito.";
            if (datos.formaPagoId <= 0)
                return "Seleccione el método de pago.";
            if (datos.tipoPago == CREDITO && (datos.cuotas < 2 || datos.cuotas > 24))
                return "Seleccione el número de cuotas.";

            return "";
        }

        // ---------- Guardado ----------

        /// <summary>
        /// Guarda cotización.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <param name="cotizacionId">(salida) Identificador de cotización.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool guardarCotizacion(DatosDocumento datos, out string mensaje, out string cotizacionId)
        {
            cotizacionId = "";

            mensaje = validarCotizacion(datos);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento("sp_InsertarCotizacion");
            comando.Parameters.AddWithValue("@identificacion_cliente", Cliente.limpiarNumero(datos.rtn));
            comando.Parameters.AddWithValue("@nombre_empresa", datos.empresa.Trim());
            comando.Parameters.AddWithValue("@nombre_cliente", datos.contacto.Trim());
            comando.Parameters.AddWithValue("@apellido_cliente", datos.apellido.Trim());
            comando.Parameters.AddWithValue("@telefono_cliente", datos.telefono.Trim());
            comando.Parameters.AddWithValue("@correo_cliente", datos.correo.Trim());

            if (datos.locacionId > 0)
                comando.Parameters.AddWithValue("@locacion_id", datos.locacionId);
            else
                comando.Parameters.AddWithValue("@locacion_id", DBNull.Value);

            comando.Parameters.AddWithValue("@direccion", datos.direccion.Trim());
            comando.Parameters.AddWithValue("@fecha_cotizacion", datos.fecha.Value);
            comando.Parameters.AddWithValue("@valida_hasta", datos.validaHasta.Value);
            comando.Parameters.AddWithValue("@observacion", datos.observaciones.Trim());

            comando.Parameters.AddWithValue("@servicio_id", datos.servicioId);
            agregarLista(comando, "@lineas", "dbo.tipo_lista_medidas", tablaDeMedidas(datos));
            comando.Parameters.AddWithValue("@porcentaje_descuento", leerDescuento(datos.descuento));

            SqlParameter idParametro = new SqlParameter("@cotizacion_id", SqlDbType.VarChar, 50);
            idParametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(idParametro);

            bool guardada = ejecutar(comando, out mensaje);

            if (guardada)
            {
                cotizacionId = "" + idParametro.Value;
                mensaje = "Cotización " + cotizacionId + " registrada correctamente.";
            }

            return guardada;
        }

        /// <summary>
        /// Guarda factura.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <param name="facturaId">(salida) Identificador de factura.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool guardarFactura(DatosFactura datos, out string mensaje, out int facturaId)
        {
            facturaId = 0;

            mensaje = validarFactura(datos);
            if (mensaje != "")
                return false;

            int cuotas = 1;
            if (datos.tipoPago == CREDITO)
                cuotas = datos.cuotas;

            SqlCommand comando = procedimiento("sp_InsertarFactura");
            comando.Parameters.AddWithValue("@orden_id", datos.ordenId.Trim());
            comando.Parameters.AddWithValue("@fecha_factura", datos.fecha);
            comando.Parameters.AddWithValue("@forma_pago_id", datos.formaPagoId);
            comando.Parameters.AddWithValue("@numero_cuotas", cuotas);

            SqlParameter idParametro = new SqlParameter("@factura_id", SqlDbType.Int);
            idParametro.Direction = ParameterDirection.Output;
            comando.Parameters.Add(idParametro);

            bool guardada = ejecutar(comando, out mensaje);

            if (guardada)
            {
                facturaId = Convert.ToInt32(idParametro.Value);
                mensaje = "Factura " + numeroFactura(facturaId) + " registrada correctamente.";
            }

            return guardada;
        }

        // ---------- Auxiliares ----------

        /// <summary>
        /// Agrega lista.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <param name="nombre">Nombre.</param>
        /// <param name="tipo">Tipo de dato.</param>
        /// <param name="tabla">Tabla de datos.</param>
        private void agregarLista(SqlCommand comando, string nombre, string tipo, DataTable tabla)
        {
            SqlParameter parametro = comando.Parameters.AddWithValue(nombre, tabla);
            parametro.SqlDbType = SqlDbType.Structured;
            parametro.TypeName = tipo;
        }

        /// <summary>
        /// Arma la tabla de actividades y medidas que recibe el procedimiento almacenado.
        /// </summary>
        /// <param name="datos">Datos a usar.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        private DataTable tablaDeMedidas(DatosDocumento datos)
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add("actividad_id", typeof(int));
            tabla.Columns.Add("medida", typeof(decimal));

            for (int i = 0; i < datos.actividadesId.Count; i++)
            {
                decimal medida;
                validarMedida(datos.actividadesMedida[i], out medida);
                tabla.Rows.Add(datos.actividadesId[i], medida);
            }

            return tabla;
        }
    }
}