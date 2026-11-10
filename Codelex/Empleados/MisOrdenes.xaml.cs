using Codelex.BDD;
using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class MisOrdenes : Window
    {
        private readonly MiTrabajoBD trabajoBD = new MiTrabajoBD();

        // El empleado sale siempre de la sesión, nunca de un campo que el usuario pueda cambiar.
        private readonly int empleadoId = Sesion.EmpleadoId;

        public MisOrdenes()
        {
            InitializeComponent();
            Height = Math.Min(Height, SystemParameters.WorkArea.Height - 10);
        }

        private async void ventanaCargada(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            Cargando espera = Cargando.iniciar(this, "Cargando tus órdenes...");

            DataTable? tabla = null;
            string errorBD = "";

            try
            {
                tabla = await Task.Run(() => trabajoBD.ordenes(empleadoId));
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            espera.terminar();
            IsEnabled = true;

            if (tabla == null)
            {
                MensajeCodelex.mostrar(this, "Mis órdenes", errorBD, false);
                return;
            }

            dgOrdenes.ItemsSource = tabla.DefaultView;
            txtResumen.Text = tabla.Rows.Count + (tabla.Rows.Count == 1 ? " orden" : " órdenes");

            if (tabla.Rows.Count == 0)
            {
                limpiarDetalle();
                return;
            }

            dgOrdenes.SelectedIndex = 0;
        }

        private void ordenesSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrdenes.SelectedItem is not DataRowView fila)
                return;

            cargarDetalle("" + fila["orden_id"]);
        }

        private void cargarDetalle(string ordenId)
        {
            try
            {
                DataTable cabecera = trabajoBD.detalle(ordenId, empleadoId);
                if (cabecera.Rows.Count == 0)
                {
                    limpiarDetalle();
                    return;
                }

                DataRow f = cabecera.Rows[0];

                txtSinOrdenes.Visibility = Visibility.Collapsed;
                txtOrden.Text = "Orden " + ordenId;
                txtEstado.Text = "" + f["estado"];
                bordeEstado.Visibility = Visibility.Visible;

                lstCampos.ItemsSource = new List<CampoDetalle>
                {
                    new CampoDetalle("Cliente", texto(f, "cliente")),
                    new CampoDetalle("Servicio", texto(f, "servicio")),
                    new CampoDetalle("Fecha de solicitud", fecha(f, "fecha_solicitud")),
                    new CampoDetalle("Fecha del servicio", fecha(f, "fecha_servicio")),
                    new CampoDetalle("Locación", texto(f, "locacion")),
                    new CampoDetalle("Dirección", texto(f, "direccion")),
                    new CampoDetalle("Vehículo", texto(f, "vehiculo")),
                    new CampoDetalle("Supervisor", texto(f, "supervisor")),
                    new CampoDetalle("Mi turno", texto(f, "turno")),
                    new CampoDetalle("Mi piso / área", texto(f, "piso_area")),
                    new CampoDetalle("Observaciones", texto(f, "observacion"))
                };

                DataTable listas = trabajoBD.listas(ordenId, empleadoId);

                lstSecciones.ItemsSource = listas.AsEnumerable()
                    .GroupBy(r => new { Orden = Convert.ToInt32(r["orden"]), Nombre = "" + r["seccion"] })
                    .OrderBy(g => g.Key.Orden)
                    .Select(g => new SeccionDetalle(
                        g.Key.Nombre,
                        g.Select(r => new ItemDetalle("" + r["titulo"], "" + r["detalle"])).ToList()))
                    .ToList();
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Mis órdenes", ex.Message, false);
            }
        }

        private void limpiarDetalle()
        {
            txtOrden.Text = "Detalle de la orden";
            bordeEstado.Visibility = Visibility.Collapsed;
            lstCampos.ItemsSource = null;
            lstSecciones.ItemsSource = null;
            txtSinOrdenes.Visibility = dgOrdenes.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static string texto(DataRow fila, string columna)
        {
            string valor = ("" + fila[columna]).Trim();
            return valor == "" ? "—" : valor;
        }

        private static string fecha(DataRow fila, string columna)
        {
            if (fila.IsNull(columna))
                return "—";

            return Convert.ToDateTime(fila[columna]).ToString("dd/MM/yyyy");
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

    public record CampoDetalle(string Etiqueta, string Valor);

    public record ItemDetalle(string Titulo, string Detalle);

    public record SeccionDetalle(string Titulo, List<ItemDetalle> Items);
}