using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class GestionVehiculos : Window
    {
        Vehiculo vehiculoBD = new Vehiculo();

        int vehiculoActual = 0;
        string placaActual = "";
        bool vehiculoActualActivo = true;

        string textoBusqueda = "";
        string filtroEstado = "TODOS";

        public GestionVehiculos()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            CargarVehiculos();
        }

        private void CargarVehiculos()
        {
            try
            {
                DataTable tabla = vehiculoBD.BuscarVehiculos(textoBusqueda, filtroEstado);
                dgVehiculos.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarEnFormulario(int vehiculoId)
        {
            try
            {
                DataTable tabla = vehiculoBD.ObtenerVehiculo(vehiculoId);

                if (tabla.Rows.Count == 0)
                {
                    MensajeCodelex.Mostrar(this, "Vehículos", "No se encontró el vehículo.", false);
                    return;
                }

                DataRow fila = tabla.Rows[0];

                vehiculoActual = Convert.ToInt32(fila["vehiculo_id"]);
                placaActual = fila["placa"].ToString();
                vehiculoActualActivo = Convert.ToBoolean(fila["estadoVehiculo"]);

                txtPlaca.Text = placaActual;
                txtMarca.Text = fila["marcaVehiculo"].ToString();
                txtModelo.Text = fila["modeloVehiculo"].ToString();

                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void LimpiarFormulario()
        {
            vehiculoActual = 0;
            placaActual = "";
            vehiculoActualActivo = true;

            txtPlaca.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            dgVehiculos.SelectedItem = null;

            ActualizarBotones();
            txtBuscar.Focus();
        }

        private void ActualizarBotones()
        {
            bool haySeleccion = vehiculoActual > 0;

            txtPlaca.IsReadOnly = !haySeleccion;
            txtMarca.IsReadOnly = !haySeleccion;
            txtModelo.IsReadOnly = !haySeleccion;
            btnActualizar.IsEnabled = haySeleccion;
            btnCambiarEstado.IsEnabled = haySeleccion;

            if (!haySeleccion)
            {
                btnCambiarEstado.Content = "Desactivar";
                txtEstado.Text = "—";
                txtModo.Text = "Seleccione un vehículo de la tabla para editarlo.";
            }
            else
            {
                txtModo.Text = "Editando el vehículo " + placaActual + ". Presione Actualizar para guardar los cambios.";

                if (vehiculoActualActivo)
                {
                    btnCambiarEstado.Content = "Desactivar";
                    txtEstado.Text = "Activo";
                }
                else
                {
                    btnCambiarEstado.Content = "Activar";
                    txtEstado.Text = "Inactivo";
                }
            }
        }

        private void SoloPlaca(object sender, TextCompositionEventArgs e)
        {
            if (!vehiculoBD.CaracteresDePlaca(e.Text))
                e.Handled = true;
        }

        private void Registrar_Click(object sender, RoutedEventArgs e)
        {
            RegistrarVehiculo ventana = new RegistrarVehiculo();
            ventana.Owner = this;
            ventana.ShowDialog();

            CargarVehiculos();
        }

        private void Limpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            if (vehiculoActual == 0)
                return;

            try
            {
                string mensaje;

                bool guardado = vehiculoBD.ActualizarVehiculo(
                    vehiculoActual, txtPlaca.Text, txtMarca.Text, txtModelo.Text, out mensaje);

                MensajeCodelex.Mostrar(this, "Vehículos", mensaje, guardado);

                if (guardado)
                {
                    placaActual = txtPlaca.Text.Trim().ToUpper();
                    ActualizarBotones();
                    CargarVehiculos();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (vehiculoActual == 0)
                return;

            bool nuevoEstado = !vehiculoActualActivo;

            string pregunta;
            if (nuevoEstado)
                pregunta = "¿Desea activar el vehículo " + placaActual + "?";
            else
                pregunta = "¿Desea desactivar el vehículo " + placaActual + "?";

            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Vehículos", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                string mensaje;
                bool guardado = vehiculoBD.CambiarEstadoVehiculo(vehiculoActual, nuevoEstado, out mensaje);

                MensajeCodelex.Mostrar(this, "Vehículos", mensaje, guardado);

                if (guardado)
                {
                    vehiculoActualActivo = nuevoEstado;
                    ActualizarBotones();
                    CargarVehiculos();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void dgVehiculos_Click(object sender, MouseButtonEventArgs e)
        {
            if (dgVehiculos.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgVehiculos.SelectedItem;
            int vehiculoId = Convert.ToInt32(fila["vehiculo_id"]);

            CargarEnFormulario(vehiculoId);
        }

        private void BuscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();
            CargarVehiculos();
        }

        private void Buscar_Click(object sender, RoutedEventArgs e)
        {
            BuscarTexto();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                BuscarTexto();
        }

        private void MostrarTodos_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            filtroEstado = "TODOS";
            CargarVehiculos();
        }

        private void Activos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            CargarVehiculos();
        }

        private void Inactivos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            CargarVehiculos();
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

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Vehículos", ex.Message, false);
        }
    }
}
