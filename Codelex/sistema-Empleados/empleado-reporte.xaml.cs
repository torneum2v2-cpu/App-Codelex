using Codelex.AccesoDatos;
using Microsoft.Win32;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex
{
    public partial class EmpleadoReporte : Window
    {
        Empleado empleadoBD = new Empleado();
        FotoPerfilBD fotoBD = new FotoPerfilBD();

        int empleadoInicial;
        bool cargando = true;

        DataTable asistencias = new DataTable();
        DataTable ordenes = new DataTable();

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="empleadoId">Identificador de empleado.</param>
        public EmpleadoReporte(int empleadoId)
        {
            InitializeComponent();
            Height = Math.Min(Height, SystemParameters.WorkArea.Height - 10);


            if (Sesion.Nivel == NivelAcceso.Basico)
                empleadoId = Sesion.EmpleadoId;

            empleadoInicial = empleadoId;
        }

        /// <summary>
        /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            DateTime hoy = DateTime.Today;
            dpDesde.SelectedDate = new DateTime(hoy.Year, hoy.Month, 1);
            dpHasta.SelectedDate = hoy;

            try
            {
                DataTable empleados = empleadoBD.buscarEmpleados("", "", "TODOS");

                if (Sesion.Nivel == NivelAcceso.Basico)
                {
                    empleados.DefaultView.RowFilter = "empleado_id = " + empleadoInicial;
                    cmbEmpleado.IsEnabled = false;
                }

                cmbEmpleado.ItemsSource = empleados.DefaultView;
                cmbEmpleado.SelectedValue = empleadoInicial;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }

            cargando = false;
            cargarDatosEmpleado();
            generarReporte();
        }

        /// <summary>
        /// Obtiene el identificador del empleado elegido.
        /// </summary>
        /// <returns>Identificador del empleado elegido, o 0 si no hay.</returns>
        private int empleadoSeleccionado()
        {
            if (cmbEmpleado.SelectedValue == null)
                return 0;

            return Convert.ToInt32(cmbEmpleado.SelectedValue);
        }

        /// <summary>
        /// Atiende el cambio de selección de empleado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void empleadoSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            if (cargando)
                return;

            cargarDatosEmpleado();
            generarReporte();
        }

        /// <summary>
        /// Carga datos empleado.
        /// </summary>
        private void cargarDatosEmpleado()
        {
            int id = empleadoSeleccionado();
            if (id == 0)
                return;

            try
            {
                DataTable tabla = empleadoBD.obtenerEmpleado(id);
                if (tabla.Rows.Count == 0)
                    return;

                DataRow fila = tabla.Rows[0];
                txtNombre.Text = fila["nombre_empleado"].ToString();
                txtApellido.Text = fila["apellido_empleado"].ToString();
                txtCargo.Text = fila["nombre_cargo"].ToString();
                txtIdentidad.Text = fila["identidad"].ToString();
                txtTelefono.Text = Validacion.formatoTelefono("" + fila["telefono_empleado"]);
                txtCorreo.Text = fila["correo_empleado"].ToString();
                txtUsuario.Text = fila["usuario"].ToString();
                txtEstado.Text = fila["estado"].ToString();

                cargarFoto(fila);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Muestra la foto de perfil del usuario del empleado, o sus iniciales si no tiene.
        /// </summary>
        /// <param name="fila">Fila con los datos del empleado.</param>
        private void cargarFoto(DataRow fila)
        {
            string iniciales = "";
            if (txtNombre.Text.Length > 0)
                iniciales += char.ToUpper(txtNombre.Text[0]);
            if (txtApellido.Text.Length > 0)
                iniciales += char.ToUpper(txtApellido.Text[0]);

            ImageSource? foto = null;

            try
            {
                if (fila.Table.Columns.Contains("usuario_id") && fila["usuario_id"] != DBNull.Value)
                {
                    byte[]? datos = fotoBD.obtener(Convert.ToInt32(fila["usuario_id"]));
                    if (datos != null)
                        foto = FotoPerfil.cargar(datos);
                }
            }
            catch
            {
                // Si no se puede leer la foto, se quedan las iniciales.
            }

            if (foto == null)
            {
                avatarEmpleado.Background = (Brush)FindResource("ButtonGradient");
                if (iniciales.Length > 0)
                    txtInicialesEmpleado.Text = iniciales;
                else
                    txtInicialesEmpleado.Text = "?";
                txtInicialesEmpleado.Visibility = Visibility.Visible;
            }
            else
            {
                avatarEmpleado.Background = new ImageBrush(foto) { Stretch = Stretch.UniformToFill };
                txtInicialesEmpleado.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Atiende el clic de generar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void generarClic(object sender, RoutedEventArgs e)
        {
            generarReporte();
        }

        /// <summary>
        /// Genera reporte.
        /// </summary>
        private void generarReporte()
        {
            int id = empleadoSeleccionado();
            if (id == 0)
                return;

            if (dpDesde.SelectedDate == null || dpHasta.SelectedDate == null)
            {
                MensajeCodelex.mostrar(this, "Reporte del empleado", "Seleccione las fechas Desde y Hasta.", false);
                return;
            }

            DateTime desde = dpDesde.SelectedDate.Value;
            DateTime hasta = dpHasta.SelectedDate.Value;

            if (desde > hasta)
            {
                MensajeCodelex.mostrar(this, "Reporte del empleado", "La fecha Desde no puede ser mayor que la fecha Hasta.", false);
                return;
            }

            try
            {
                asistencias = empleadoBD.asistenciasPorEmpleado(id, desde, hasta);
                ordenes = empleadoBD.ordenesPorEmpleado(id, desde, hasta);

                dgAsistencia.ItemsSource = asistencias.DefaultView;
                dgOrdenes.ItemsSource = ordenes.DefaultView;

                decimal totalHoras = 0;
                foreach (DataRow fila in asistencias.Rows)
                    totalHoras += Convert.ToDecimal(fila["horas"]);

                txtTotalHoras.Text = "Total de horas trabajadas: " + totalHoras.ToString("N2");
                txtTotalOrdenes.Text = "Órdenes asignadas: " + ordenes.Rows.Count;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el clic de exportar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void exportarClic(object sender, RoutedEventArgs e)
        {
            if (empleadoSeleccionado() == 0)
                return;

            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Filter = "Archivo CSV (*.csv)|*.csv";
            dialogo.FileName = "Reporte_" + txtNombre.Text + "_" + txtApellido.Text + "_" + DateTime.Today.ToString("yyyyMMdd") + ".csv";

            if (dialogo.ShowDialog() != true)
                return;

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("REPORTE DEL EMPLEADO");
                sb.AppendLine("Empleado;" + csv(txtNombre.Text + " " + txtApellido.Text));
                sb.AppendLine("Cargo;" + csv(txtCargo.Text));
                sb.AppendLine("Identidad;" + csv(txtIdentidad.Text));
                sb.AppendLine("Periodo;" + dpDesde.SelectedDate?.ToString("dd/MM/yyyy") + " al " + dpHasta.SelectedDate?.ToString("dd/MM/yyyy"));
                sb.AppendLine();

                sb.AppendLine("ASISTENCIA");
                sb.AppendLine("Fecha;Entrada;Salida;Horas");
                foreach (DataRow f in asistencias.Rows)
                    sb.AppendLine(csv(f["fecha_asistencia"]) + ";" + csv(f["hora_entrada"]) + ";" + csv(f["hora_salida"]) + ";" +
                                  Convert.ToDecimal(f["horas"]).ToString("N2", CultureInfo.InvariantCulture));
                sb.AppendLine(txtTotalHoras.Text.Replace(":", ";"));
                sb.AppendLine();

                sb.AppendLine("ORDENES ASIGNADAS");
                sb.AppendLine("Orden;Cliente;Servicio;Fecha;Turno;Piso / area");
                foreach (DataRow f in ordenes.Rows)
                    sb.AppendLine(csv(f["orden_id"]) + ";" + csv(f["cliente"]) + ";" + csv(f["nombre_servicio"]) + ";" +
                                  csv(f["fecha_servicio"]) + ";" + csv(f["turno"]) + ";" + csv(f["piso_area"]));
                sb.AppendLine(txtTotalOrdenes.Text.Replace(":", ";"));

                System.IO.File.WriteAllText(dialogo.FileName, sb.ToString(), new UTF8Encoding(true));
                MensajeCodelex.mostrar(this, "Reporte del empleado", "Reporte exportado correctamente.", true);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Prepara un valor para escribirlo en un archivo CSV (comillas y separadores).
        /// </summary>
        /// <param name="valor">Valor a usar.</param>
        /// <returns>Texto resultante.</returns>
        private string csv(object valor)
        {
            string texto = "" + valor;
            if (texto.Contains(";") || texto.Contains("\""))
                texto = "\"" + texto.Replace("\"", "\"\"") + "\"";
            return texto;
        }

        /// <summary>
        /// Atiende el clic de imprimir.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void imprimirClic(object sender, RoutedEventArgs e)
        {
            PrintDialog dialogo = new PrintDialog();
            if (dialogo.ShowDialog() == true)
                dialogo.PrintVisual(areaReporte, "Reporte del empleado");
        }

        /// <summary>
        /// Permite mover la ventana arrastrando la barra superior con el mouse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// Minimiza la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void minimizarClic(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Cierra la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void cerrarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Reporte del empleado", ex.Message, false);
        }
    }
}