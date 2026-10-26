using Codelex.Clases_BDD;
using Microsoft.Win32;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class EmpleadoReporte : Window
    {
        Empleado empleadoBD = new Empleado();

        int empleadoInicial;
        bool cargando = true;

        DataTable asistencias = new DataTable();
        DataTable ordenes = new DataTable();

        public EmpleadoReporte(int empleadoId)
        {
            InitializeComponent();
            Height = Math.Min(Height, SystemParameters.WorkArea.Height - 10);
            empleadoInicial = empleadoId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime hoy = DateTime.Today;
            dpDesde.SelectedDate = new DateTime(hoy.Year, hoy.Month, 1);
            dpHasta.SelectedDate = hoy;

            try
            {
                cmbEmpleado.ItemsSource = empleadoBD.BuscarEmpleados("", "", "TODOS").DefaultView;
                cmbEmpleado.SelectedValue = empleadoInicial;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }

            cargando = false;
            CargarDatosEmpleado();
            GenerarReporte();
        }

        private int EmpleadoSeleccionado()
        {
            if (cmbEmpleado.SelectedValue == null)
                return 0;

            return Convert.ToInt32(cmbEmpleado.SelectedValue);
        }

        private void cmbEmpleado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cargando)
                return;

            CargarDatosEmpleado();
            GenerarReporte();
        }

        private void CargarDatosEmpleado()
        {
            int id = EmpleadoSeleccionado();
            if (id == 0)
                return;

            try
            {
                DataTable tabla = empleadoBD.ObtenerEmpleado(id);
                if (tabla.Rows.Count == 0)
                    return;

                DataRow fila = tabla.Rows[0];
                txtNombre.Text = fila["nombreEmpleado"].ToString();
                txtApellido.Text = fila["apellidoEmpleado"].ToString();
                txtCargo.Text = fila["nombreCargo"].ToString();
                txtIdentidad.Text = fila["identidad"].ToString();
                txtTelefono.Text = fila["telefonoEmpleado"].ToString();
                txtCorreo.Text = fila["correoEmpleado"].ToString();
                txtHuella.Text = fila["huella_id"].ToString();
                txtUsuario.Text = fila["usuario"].ToString();
                txtEstado.Text = fila["estado"].ToString();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void Generar_Click(object sender, RoutedEventArgs e)
        {
            GenerarReporte();
        }

        private void GenerarReporte()
        {
            int id = EmpleadoSeleccionado();
            if (id == 0)
                return;

            if (dpDesde.SelectedDate == null || dpHasta.SelectedDate == null)
            {
                MensajeCodelex.Mostrar(this, "Reporte del empleado", "Seleccione las fechas Desde y Hasta.", false);
                return;
            }

            DateTime desde = dpDesde.SelectedDate.Value;
            DateTime hasta = dpHasta.SelectedDate.Value;

            if (desde > hasta)
            {
                MensajeCodelex.Mostrar(this, "Reporte del empleado", "La fecha Desde no puede ser mayor que la fecha Hasta.", false);
                return;
            }

            try
            {
                asistencias = empleadoBD.AsistenciasPorEmpleado(id, desde, hasta);
                ordenes = empleadoBD.OrdenesPorEmpleado(id, desde, hasta);

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
                MostrarError(ex);
            }
        }

        private void Exportar_Click(object sender, RoutedEventArgs e)
        {
            if (EmpleadoSeleccionado() == 0)
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
                sb.AppendLine("Empleado;" + Csv(txtNombre.Text + " " + txtApellido.Text));
                sb.AppendLine("Cargo;" + Csv(txtCargo.Text));
                sb.AppendLine("Identidad;" + Csv(txtIdentidad.Text));
                sb.AppendLine("Periodo;" + dpDesde.SelectedDate?.ToString("dd/MM/yyyy") + " al " + dpHasta.SelectedDate?.ToString("dd/MM/yyyy"));
                sb.AppendLine();

                sb.AppendLine("ASISTENCIA");
                sb.AppendLine("Fecha;Entrada;Salida;Horas");
                foreach (DataRow f in asistencias.Rows)
                    sb.AppendLine(Csv(f["fechaAsistencia"]) + ";" + Csv(f["hora_entrada"]) + ";" + Csv(f["hora_salida"]) + ";" +
                                  Convert.ToDecimal(f["horas"]).ToString("N2", CultureInfo.InvariantCulture));
                sb.AppendLine(txtTotalHoras.Text.Replace(":", ";"));
                sb.AppendLine();

                sb.AppendLine("ORDENES ASIGNADAS");
                sb.AppendLine("Orden;Cliente;Servicio;Fecha;Turno;Piso / area");
                foreach (DataRow f in ordenes.Rows)
                    sb.AppendLine(Csv(f["orden_id"]) + ";" + Csv(f["cliente"]) + ";" + Csv(f["nombreServicio"]) + ";" +
                                  Csv(f["fecha_servicio"]) + ";" + Csv(f["turno"]) + ";" + Csv(f["piso_area"]));
                sb.AppendLine(txtTotalOrdenes.Text.Replace(":", ";"));

                System.IO.File.WriteAllText(dialogo.FileName, sb.ToString(), new UTF8Encoding(true));
                MensajeCodelex.Mostrar(this, "Reporte del empleado", "Reporte exportado correctamente.", true);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private string Csv(object valor)
        {
            string texto = "" + valor;
            if (texto.Contains(";") || texto.Contains("\""))
                texto = "\"" + texto.Replace("\"", "\"\"") + "\"";
            return texto;
        }

        private void Imprimir_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog dialogo = new PrintDialog();
            if (dialogo.ShowDialog() == true)
                dialogo.PrintVisual(areaReporte, "Reporte del empleado");
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimizar_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Reporte del empleado", ex.Message, false);
        }
    }
}
