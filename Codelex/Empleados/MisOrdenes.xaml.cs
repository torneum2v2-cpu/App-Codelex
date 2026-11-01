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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            Cargando espera = Cargando.Iniciar(this, "Cargando tus órdenes...");

            DataTable? tabla = null;
            string errorBD = "";

            try
            {
                tabla = await Task.Run(() => trabajoBD.Ordenes(empleadoId));
            }
            catch (Exception ex)
            {
                errorBD = ex.Message;
            }

            espera.Terminar();
            IsEnabled = true;

            if (tabla == null)
            {
                MensajeCodelex.Mostrar(this, "Mis órdenes", errorBD, false);
                return;
            }

            dgOrdenes.ItemsSource = tabla.DefaultView;
            txtResumen.Text = tabla.Rows.Count + (tabla.Rows.Count == 1 ? " orden" : " órdenes");

            if (tabla.Rows.Count == 0)
            {
                LimpiarDetalle();
                return;
            }

            dgOrdenes.SelectedIndex = 0;
        }

        private void Ordenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrdenes.SelectedItem is not DataRowView fila)
                return;

            CargarDetalle("" + fila["orden_id"]);
        }

        private void CargarDetalle(string ordenId)
        {
            try
            {
                DataTable cabecera = trabajoBD.Detalle(ordenId, empleadoId);
                if (cabecera.Rows.Count == 0)
                {
                    LimpiarDetalle();
                    return;
                }

                DataRow f = cabecera.Rows[0];

                txtSinOrdenes.Visibility = Visibility.Collapsed;
                txtOrden.Text = "Orden " + ordenId;
                txtEstado.Text = "" + f["estado"];
                bordeEstado.Visibility = Visibility.Visible;

                lstCampos.ItemsSource = new List<CampoDetalle>
                {
                    new CampoDetalle("Cliente", Texto(f, "cliente")),
                    new CampoDetalle("Servicio", Texto(f, "servicio")),
                    new CampoDetalle("Fecha de solicitud", Fecha(f, "fecha_solicitud")),
                    new CampoDetalle("Fecha del servicio", Fecha(f, "fecha_servicio")),
                    new CampoDetalle("Locación", Texto(f, "locacion")),
                    new CampoDetalle("Dirección", Texto(f, "direccion")),
                    new CampoDetalle("Vehículo", Texto(f, "vehiculo")),
                    new CampoDetalle("Supervisor", Texto(f, "supervisor")),
                    new CampoDetalle("Mi turno", Texto(f, "turno")),
                    new CampoDetalle("Mi piso / área", Texto(f, "piso_area")),
                    new CampoDetalle("Observaciones", Texto(f, "observaciones"))
                };

                DataTable listas = trabajoBD.Listas(ordenId, empleadoId);

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
                MensajeCodelex.Mostrar(this, "Mis órdenes", ex.Message, false);
            }
        }

        private void LimpiarDetalle()
        {
            txtOrden.Text = "Detalle de la orden";
            bordeEstado.Visibility = Visibility.Collapsed;
            lstCampos.ItemsSource = null;
            lstSecciones.ItemsSource = null;
            txtSinOrdenes.Visibility = dgOrdenes.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static string Texto(DataRow fila, string columna)
        {
            string valor = ("" + fila[columna]).Trim();
            return valor == "" ? "—" : valor;
        }

        private static string Fecha(DataRow fila, string columna)
        {
            if (fila.IsNull(columna))
                return "—";

            return Convert.ToDateTime(fila[columna]).ToString("dd/MM/yyyy");
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
    }

    public record CampoDetalle(string Etiqueta, string Valor);

    public record ItemDetalle(string Titulo, string Detalle);

    public record SeccionDetalle(string Titulo, List<ItemDetalle> Items);
}