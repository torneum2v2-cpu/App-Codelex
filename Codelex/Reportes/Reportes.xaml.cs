using Codelex.Clases_BDD;
using Microsoft.Win32;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Codelex
{
    public partial class Reportes : Window
    {
        private const double ANCHO_MAXIMO_BARRA = 330;
        private const int MAXIMO_BARRAS = 6;

        private enum TipoReporte { Ordenes, Asistencia, Empleados }

        private readonly ReporteBD reporteBD = new ReporteBD();
        private readonly Empleado empleadoBD = new Empleado();

        private TipoReporte tipo = TipoReporte.Ordenes;
        private TipoReporte tipoMostrado = TipoReporte.Ordenes;
        private bool cargando = true;
        private bool generando = false;

        private DataTable datos = new DataTable();
        private DateTime desdeUsado = DateTime.Today;
        private DateTime hastaUsado = DateTime.Today;

        private List<Indicador> indicadores = new List<Indicador>();
        private List<PuntoGrafico> barras = new List<PuntoGrafico>();
        private List<PuntoGrafico> puntos = new List<PuntoGrafico>();
        private bool graficoColumnas = false;
        private string tituloGrafico1 = "";
        private string tituloGrafico2 = "";

        public Reportes()
        {
            InitializeComponent();
            Height = Math.Min(Height, SystemParameters.WorkArea.Height - 10);
        }

        private async void ventanaCargada(object sender, RoutedEventArgs e)
        {
            DateTime hoy = DateTime.Today;
            dpDesde.SelectedDate = new DateTime(hoy.Year, hoy.Month, 1);
            dpHasta.SelectedDate = hoy;

            try
            {
                cmbEstadoOrden.ItemsSource = conTodos(reporteBD.listarEstadosOrden(), "estado_orden_id", "nombre_estado_orden", "Todos los estados");
                cmbServicio.ItemsSource = conTodos(reporteBD.listarServicios(), "servicio_id", "nombre_servicio", "Todos los servicios");
                cmbEmpleado.ItemsSource = conTodos(empleadoBD.buscarEmpleados("", "", "TODOS"), "empleado_id", "nombre_completo", "Todos los empleados");
                cmbCargo.ItemsSource = conTodos(empleadoBD.listarCargos(), "cargo_id", "nombre_cargo", "Todos los cargos");
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }

            cmbEstadoEmpleado.ItemsSource = new List<OpcionFiltro>
            {
                new OpcionFiltro(0, "Todos"),
                new OpcionFiltro(1, "Activos"),
                new OpcionFiltro(2, "Inactivos")
            };

            cmbEstadoOrden.SelectedIndex = 0;
            cmbServicio.SelectedIndex = 0;
            cmbEmpleado.SelectedIndex = 0;
            cmbCargo.SelectedIndex = 0;
            cmbEstadoEmpleado.SelectedIndex = 0;

            cargando = false;
            await generarReporte();
        }

        private static List<OpcionFiltro> conTodos(DataTable tabla, string columnaId, string columnaNombre, string textoTodos)
        {
            List<OpcionFiltro> lista = new List<OpcionFiltro> { new OpcionFiltro(0, textoTodos) };

            foreach (DataRow fila in tabla.Rows)
                lista.Add(new OpcionFiltro(Convert.ToInt32(fila[columnaId]), "" + fila[columnaNombre]));

            return lista;
        }

        private static int seleccionado(ComboBox combo)
        {
            if (combo.SelectedValue == null)
                return 0;

            return Convert.ToInt32(combo.SelectedValue);
        }

        private async void tipoMarcado(object sender, RoutedEventArgs e)
        {
            if (cargando)
                return;

            if (rbAsistencia.IsChecked == true)
                tipo = TipoReporte.Asistencia;
            else if (rbEmpleados.IsChecked == true)
                tipo = TipoReporte.Empleados;
            else
                tipo = TipoReporte.Ordenes;

            panelOrdenes.Visibility = tipo == TipoReporte.Ordenes ? Visibility.Visible : Visibility.Collapsed;
            panelAsistencia.Visibility = tipo == TipoReporte.Asistencia ? Visibility.Visible : Visibility.Collapsed;
            panelEmpleados.Visibility = tipo == TipoReporte.Empleados ? Visibility.Visible : Visibility.Collapsed;

            await generarReporte();
        }

        private async void generarClic(object sender, RoutedEventArgs e)
        {
            await generarReporte();
        }

        private async void filtroTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await generarReporte();
        }

        private async Task generarReporte()
        {
            if (generando)
                return;

            if (dpDesde.SelectedDate == null || dpHasta.SelectedDate == null)
            {
                MensajeCodelex.mostrar(this, "Reportes", "Seleccione las fechas Desde y Hasta.", false);
                return;
            }

            DateTime desde = dpDesde.SelectedDate.Value.Date;
            DateTime hasta = dpHasta.SelectedDate.Value.Date;

            if (desde > hasta)
            {
                MensajeCodelex.mostrar(this, "Reportes", "La fecha Desde no puede ser mayor que la fecha Hasta.", false);
                return;
            }

            TipoReporte tipoConsulta = tipo;
            int estadoOrdenId = seleccionado(cmbEstadoOrden);
            int servicioId = seleccionado(cmbServicio);
            string cliente = txtCliente.Text.Trim();
            int empleadoId = seleccionado(cmbEmpleado);
            int cargoId = seleccionado(cmbCargo);
            int estadoEmpleado = seleccionado(cmbEstadoEmpleado);

            generando = true;
            IsEnabled = false;
            Cargando espera = Cargando.iniciar(this, "Generando reporte...");

            DataTable? tabla = null;
            string errorBD = "";

            try
            {
                tabla = await Task.Run(() =>
                {
                    if (tipoConsulta == TipoReporte.Ordenes)
                        return reporteBD.ordenes(desde, hasta, estadoOrdenId, servicioId, cliente);
                    if (tipoConsulta == TipoReporte.Asistencia)
                        return reporteBD.asistencia(desde, hasta, empleadoId);
                    return reporteBD.empleados(desde, hasta, cargoId, estadoEmpleado);
                });
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            espera.terminar();
            IsEnabled = true;
            generando = false;

            if (tabla == null)
            {
                MensajeCodelex.mostrar(this, "Reportes", errorBD, false);
                return;
            }

            datos = tabla;
            desdeUsado = desde;
            hastaUsado = hasta;
            mostrarResultado(tipoConsulta);
        }

        // ---------- Armado de resultados ----------

        private void mostrarResultado(TipoReporte t)
        {
            tipoMostrado = t;
            List<DataRow> filas = datos.AsEnumerable().ToList();

            dgOrdenes.Visibility = t == TipoReporte.Ordenes ? Visibility.Visible : Visibility.Collapsed;
            dgAsistencia.Visibility = t == TipoReporte.Asistencia ? Visibility.Visible : Visibility.Collapsed;
            dgEmpleados.Visibility = t == TipoReporte.Empleados ? Visibility.Visible : Visibility.Collapsed;

            graficoColumnas = false;

            if (t == TipoReporte.Ordenes)
            {
                dgOrdenes.ItemsSource = datos.DefaultView;
                armarOrdenes(filas);
            }
            else if (t == TipoReporte.Asistencia)
            {
                dgAsistencia.ItemsSource = datos.DefaultView;
                armarAsistencia(filas);
            }
            else
            {
                dgEmpleados.ItemsSource = datos.DefaultView;
                armarEmpleados(filas);
            }

            lstIndicadores.ItemsSource = indicadores;
            txtTituloGrafico1.Text = tituloGrafico1;
            txtTituloGrafico2.Text = tituloGrafico2;
            txtResumen.Text = filas.Count + (filas.Count == 1 ? " registro" : " registros");

            mostrarBarras();
            dibujarGrafico();
        }

        private void armarOrdenes(List<DataRow> filas)
        {
            List<PuntoGrafico> porEstado = agrupar(filas, f => texto(f, "nombre_estado_orden"), f => 1);
            List<PuntoGrafico> porServicio = agrupar(filas, f => texto(f, "nombre_servicio"), f => 1);

            int evaluadas = filas.Count(f => !f.IsNull("conformidad_cliente"));
            int conformes = filas.Count(f => !f.IsNull("conformidad_cliente") && Convert.ToBoolean(f["conformidad_cliente"]));

            indicadores = new List<Indicador>
            {
                new Indicador("Total de órdenes", filas.Count.ToString(), "En el período seleccionado"),
                new Indicador("Estado más frecuente", primero(porEstado), detalle(porEstado, "órdenes")),
                new Indicador("Servicio más solicitado", primero(porServicio), detalle(porServicio, "órdenes")),
                new Indicador("Conformidad del cliente",
                              evaluadas == 0 ? "—" : (conformes * 100.0 / evaluadas).ToString("0") + "%",
                              conformes + " de " + evaluadas + " órdenes evaluadas")
            };

            tituloGrafico1 = "Órdenes por estado";
            tituloGrafico2 = "Órdenes por fecha de servicio";
            barras = porEstado;

            List<(DateTime Fecha, double Valor)> serie = filas
                .Where(f => !f.IsNull("fecha"))
                .Select(f => (Convert.ToDateTime(f["fecha"]), 1.0))
                .ToList();
            puntos = serieTiempo(serie, desdeUsado, hastaUsado);
        }

        private void armarAsistencia(List<DataRow> filas)
        {
            double horas = filas.Sum(f => Convert.ToDouble(f["horas"]));
            int jornadas = filas.Count(f => Convert.ToDouble(f["horas"]) > 0);
            int personas = filas.Select(f => Convert.ToInt32(f["empleado_id"])).Distinct().Count();

            indicadores = new List<Indicador>
            {
                new Indicador("Horas trabajadas", horas.ToString("0.00"), "Suma de todas las jornadas"),
                new Indicador("Registros de asistencia", filas.Count.ToString(), jornadas + " con entrada y salida"),
                new Indicador("Promedio por jornada",
                              jornadas == 0 ? "—" : (horas / jornadas).ToString("0.00") + " h",
                              "Solo jornadas completas"),
                new Indicador("Empleados con asistencia", personas.ToString(), "En el período seleccionado")
            };

            tituloGrafico1 = "Horas trabajadas por empleado";
            tituloGrafico2 = "Horas trabajadas por fecha";
            barras = agrupar(filas, f => texto(f, "empleado"), f => Convert.ToDouble(f["horas"]));

            List<(DateTime Fecha, double Valor)> serie = filas
                .Where(f => !f.IsNull("fecha_asistencia"))
                .Select(f => (Convert.ToDateTime(f["fecha_asistencia"]), Convert.ToDouble(f["horas"])))
                .ToList();
            puntos = serieTiempo(serie, desdeUsado, hastaUsado);
        }

        private void armarEmpleados(List<DataRow> filas)
        {
            int activos = filas.Count(f => texto(f, "estado") == "Activo");
            int inactivos = filas.Count - activos;
            double ordenes = filas.Sum(f => Convert.ToDouble(f["ordenes"]));

            indicadores = new List<Indicador>
            {
                new Indicador("Total de empleados", filas.Count.ToString(), "Según los filtros aplicados"),
                new Indicador("Activos", activos.ToString(), "Con acceso al sistema"),
                new Indicador("Inactivos", inactivos.ToString(), "Dados de baja"),
                new Indicador("Órdenes atendidas", ordenes.ToString("0"), "En el período seleccionado")
            };

            tituloGrafico1 = "Órdenes atendidas por empleado";
            tituloGrafico2 = "Empleados por cargo";
            barras = filas
                .Select(f => new PuntoGrafico(texto(f, "nombre_completo"), Convert.ToDouble(f["ordenes"])))
                .OrderByDescending(p => p.Valor)
                .ToList();

            puntos = agrupar(filas, f => texto(f, "nombre_cargo"), f => 1);
            graficoColumnas = true;
        }

        private static string texto(DataRow fila, string columna)
        {
            return "" + fila[columna];
        }

        private static string primero(List<PuntoGrafico> lista)
        {
            return lista.Count > 0 ? lista[0].Etiqueta : "—";
        }

        private static string detalle(List<PuntoGrafico> lista, string unidad)
        {
            return lista.Count > 0 ? lista[0].Valor.ToString("0.##") + " " + unidad : "Sin datos";
        }

        private static List<PuntoGrafico> agrupar(List<DataRow> filas, Func<DataRow, string> clave, Func<DataRow, double> valor)
        {
            return filas.GroupBy(clave)
                        .Select(g => new PuntoGrafico(g.Key, g.Sum(valor)))
                        .OrderByDescending(p => p.Valor)
                        .ToList();
        }

        private static List<PuntoGrafico> serieTiempo(List<(DateTime Fecha, double Valor)> valores, DateTime desde, DateTime hasta)
        {
            List<PuntoGrafico> resultado = new List<PuntoGrafico>();
            CultureInfo es = new CultureInfo("es");

            if ((hasta - desde).TotalDays > 62)
            {
                DateTime mes = new DateTime(desde.Year, desde.Month, 1);
                DateTime fin = new DateTime(hasta.Year, hasta.Month, 1);

                while (mes <= fin)
                {
                    double suma = valores.Where(v => v.Fecha.Year == mes.Year && v.Fecha.Month == mes.Month).Sum(v => v.Valor);
                    resultado.Add(new PuntoGrafico(mes.ToString("MMM yy", es), suma));
                    mes = mes.AddMonths(1);
                }
            }
            else
            {
                for (DateTime dia = desde.Date; dia <= hasta.Date; dia = dia.AddDays(1))
                {
                    double suma = valores.Where(v => v.Fecha.Date == dia).Sum(v => v.Valor);
                    resultado.Add(new PuntoGrafico(dia.ToString("dd/MM"), suma));
                }
            }

            return resultado;
        }

        // ---------- Gráfico de barras ----------

        private void mostrarBarras()
        {
            List<PuntoGrafico> mejores = barras.Take(MAXIMO_BARRAS).ToList();
            double maximo = mejores.Count == 0 ? 0 : mejores.Max(p => p.Valor);

            lstBarras.ItemsSource = mejores
                .Select(p => new BarraGrafico(
                    p.Etiqueta,
                    p.Valor.ToString("0.##"),
                    maximo <= 0 || p.Valor <= 0 ? 0 : Math.Max(6, p.Valor / maximo * ANCHO_MAXIMO_BARRA)))
                .ToList();

            txtSinDatos1.Visibility = mejores.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---------- Gráfico de líneas / columnas ----------

        private void canvasGraficoTamanoCambiado(object sender, SizeChangedEventArgs e)
        {
            dibujarGrafico();
        }

        private void dibujarGrafico()
        {
            canvasGrafico.Children.Clear();
            txtSinDatos2.Visibility = puntos.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            double ancho = canvasGrafico.ActualWidth;
            double alto = canvasGrafico.ActualHeight;
            if (puntos.Count == 0 || ancho < 120 || alto < 80)
                return;

            const double IZQUIERDA = 42;
            const double DERECHA = 12;
            const double ARRIBA = 16;
            const double ABAJO = 28;

            double areaAncho = ancho - IZQUIERDA - DERECHA;
            double areaAlto = alto - ARRIBA - ABAJO;
            double baseY = ARRIBA + areaAlto;

            bool enteros = puntos.All(p => p.Valor == Math.Floor(p.Valor));
            double maximo = escalaMaxima(puntos.Max(p => p.Valor), enteros);

            Brush colorLinea = (Brush)FindResource("CardLine");
            Brush colorTexto = (Brush)FindResource("SoftText");
            Brush colorPrimario = (Brush)FindResource("Primary");

            for (int i = 0; i <= 4; i++)
            {
                double y = baseY - areaAlto * i / 4.0;
                canvasGrafico.Children.Add(new Line
                {
                    X1 = IZQUIERDA,
                    X2 = IZQUIERDA + areaAncho,
                    Y1 = y,
                    Y2 = y,
                    Stroke = colorLinea,
                    StrokeThickness = 1
                });
                agregarTexto((maximo * i / 4.0).ToString("0.##"), 0, y - 8, IZQUIERDA - 6, TextAlignment.Right, colorTexto);
            }

            int n = puntos.Count;
            double ranura = areaAncho / n;

            if (graficoColumnas)
            {
                double anchoBarra = Math.Min(56, ranura * 0.6);

                for (int i = 0; i < n; i++)
                {
                    double altoBarra = areaAlto * puntos[i].Valor / maximo;
                    double x = IZQUIERDA + ranura * i + (ranura - anchoBarra) / 2;
                    double y = baseY - altoBarra;

                    Rectangle barra = new Rectangle
                    {
                        Width = anchoBarra,
                        Height = altoBarra,
                        RadiusX = 5,
                        RadiusY = 5,
                        Fill = (Brush)FindResource("ButtonGradient"),
                        ToolTip = puntos[i].Etiqueta + ": " + puntos[i].Valor.ToString("0.##")
                    };
                    Canvas.SetLeft(barra, x);
                    Canvas.SetTop(barra, y);
                    canvasGrafico.Children.Add(barra);

                    agregarTexto(puntos[i].Valor.ToString("0.##"), IZQUIERDA + ranura * i, y - 17, ranura,
                                 TextAlignment.Center, (Brush)FindResource("DarkText"));
                }
            }
            else
            {
                PointCollection linea = new PointCollection();
                for (int i = 0; i < n; i++)
                {
                    double x = IZQUIERDA + ranura * (i + 0.5);
                    double y = baseY - areaAlto * puntos[i].Valor / maximo;
                    linea.Add(new Point(x, y));
                }

                PointCollection area = new PointCollection(linea);
                area.Insert(0, new Point(linea[0].X, baseY));
                area.Add(new Point(linea[n - 1].X, baseY));

                canvasGrafico.Children.Add(new Polygon
                {
                    Points = area,
                    Fill = new LinearGradientBrush(Color.FromArgb(90, 44, 197, 180), Color.FromArgb(5, 44, 197, 180), 90)
                });

                canvasGrafico.Children.Add(new Polyline
                {
                    Points = linea,
                    Stroke = colorPrimario,
                    StrokeThickness = 2.5,
                    StrokeLineJoin = PenLineJoin.Round
                });

                if (n <= 40)
                {
                    for (int i = 0; i < n; i++)
                    {
                        Ellipse punto = new Ellipse
                        {
                            Width = 9,
                            Height = 9,
                            Fill = Brushes.White,
                            Stroke = colorPrimario,
                            StrokeThickness = 2,
                            ToolTip = puntos[i].Etiqueta + ": " + puntos[i].Valor.ToString("0.##")
                        };
                        Canvas.SetLeft(punto, linea[i].X - 4.5);
                        Canvas.SetTop(punto, linea[i].Y - 4.5);
                        canvasGrafico.Children.Add(punto);
                    }
                }
            }

            // Etiquetas del eje X (se saltan algunas si no caben)
            int paso = Math.Max(1, (int)Math.Ceiling(n * 62.0 / areaAncho));
            double anchoEtiqueta = ranura * paso;
            for (int i = 0; i < n; i += paso)
            {
                double centro = IZQUIERDA + ranura * (i + 0.5);
                agregarTexto(puntos[i].Etiqueta, centro - anchoEtiqueta / 2, baseY + 7, anchoEtiqueta,
                             TextAlignment.Center, colorTexto);
            }
        }

        private static double escalaMaxima(double maximo, bool enteros)
        {
            if (maximo <= 0)
                return 4;

            double bruto = maximo / 4.0;
            double exponente = Math.Pow(10, Math.Floor(Math.Log10(bruto)));
            double fraccion = bruto / exponente;

            double paso;
            if (fraccion <= 1)
                paso = 1;
            else if (fraccion <= 2)
                paso = 2;
            else if (fraccion <= 5)
                paso = 5;
            else
                paso = 10;

            paso *= exponente;
            if (enteros && paso < 1)
                paso = 1;

            return paso * 4;
        }

        private void agregarTexto(string texto, double x, double y, double ancho, TextAlignment alineacion, Brush color)
        {
            if (ancho <= 0)
                return;

            TextBlock bloque = new TextBlock
            {
                Text = texto,
                Width = ancho,
                TextAlignment = alineacion,
                Foreground = color,
                FontSize = 11,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = texto
            };
            Canvas.SetLeft(bloque, x);
            Canvas.SetTop(bloque, y);
            canvasGrafico.Children.Add(bloque);
        }

        // ---------- Exportar ----------

        private void exportarClic(object sender, RoutedEventArgs e)
        {
            if (datos.Rows.Count == 0)
            {
                MensajeCodelex.mostrar(this, "Reportes", "No hay datos para exportar. Genera un reporte con resultados primero.", false);
                return;
            }

            string nombre = nombreReporte(tipoMostrado);

            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Filter = "Archivo CSV (*.csv)|*.csv";
            dialogo.FileName = "Reporte_" + nombre.Replace("Ó", "O") + "_" + DateTime.Today.ToString("yyyyMMdd") + ".csv";

            if (dialogo.ShowDialog() != true)
                return;

            try
            {
                List<(string Encabezado, string Columna)> columnasCsv = obtenerColumnasCsv(tipoMostrado);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("REPORTE DE " + nombre.ToUpper());
                sb.AppendLine("Periodo;" + desdeUsado.ToString("dd/MM/yyyy") + " al " + hastaUsado.ToString("dd/MM/yyyy"));
                sb.AppendLine();

                sb.AppendLine("RESUMEN");
                foreach (Indicador indicador in indicadores)
                    sb.AppendLine(csv(indicador.Titulo) + ";" + csv(indicador.Valor) + ";" + csv(indicador.Detalle));
                sb.AppendLine();

                sb.AppendLine("DETALLE");
                sb.AppendLine(string.Join(";", columnasCsv.Select(c => c.Encabezado)));
                foreach (DataRow fila in datos.Rows)
                    sb.AppendLine(string.Join(";", columnasCsv.Select(c => celda(fila[c.Columna]))));

                System.IO.File.WriteAllText(dialogo.FileName, sb.ToString(), new UTF8Encoding(true));
                MensajeCodelex.mostrar(this, "Reportes", "Reporte exportado correctamente.", true);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private static string nombreReporte(TipoReporte t)
        {
            if (t == TipoReporte.Asistencia)
                return "Asistencia";
            if (t == TipoReporte.Empleados)
                return "Empleados";
            return "Órdenes";
        }

        private static List<(string Encabezado, string Columna)> obtenerColumnasCsv(TipoReporte t)
        {
            if (t == TipoReporte.Asistencia)
                return new List<(string, string)>
                {
                    ("Empleado", "empleado"), ("Cargo", "nombre_cargo"), ("Fecha", "fecha_asistencia"),
                    ("Entrada", "hora_entrada"), ("Salida", "hora_salida"), ("Horas", "horas")
                };

            if (t == TipoReporte.Empleados)
                return new List<(string, string)>
                {
                    ("Empleado", "nombre_completo"), ("Cargo", "nombre_cargo"), ("Estado", "estado"),
                    ("Órdenes", "ordenes"), ("Días asistidos", "dias"), ("Horas", "horas")
                };

            return new List<(string, string)>
            {
                ("Orden", "orden_id"), ("Cliente", "cliente"), ("Servicio", "nombre_servicio"),
                ("Supervisor", "supervisor"), ("Fecha", "fecha"), ("Estado", "nombre_estado_orden"),
                ("Conformidad", "conformidad")
            };
        }

        private static string celda(object valor)
        {
            if (valor is DateTime fecha)
                return fecha.ToString("dd/MM/yyyy");
            if (valor is decimal numero)
                return numero.ToString("0.00", CultureInfo.InvariantCulture);

            return csv(valor);
        }

        private static string csv(object valor)
        {
            string texto = "" + valor;
            if (texto.Contains(";") || texto.Contains("\""))
                texto = "\"" + texto.Replace("\"", "\"\"") + "\"";
            return texto;
        }

        // ---------- Ventana ----------

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Reportes", ex.Message, false);
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void minimizarClic(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void cerrarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public record OpcionFiltro(int Id, string Nombre);

    public record Indicador(string Titulo, string Valor, string Detalle);

    public record PuntoGrafico(string Etiqueta, double Valor);

    public record BarraGrafico(string Etiqueta, string ValorTexto, double Ancho);
}