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

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
            cargarVehiculos();
        }

        private void cargarVehiculos()
        {
            try
            {
                DataTable tabla = vehiculoBD.buscarVehiculos(textoBusqueda, filtroEstado);
                dgVehiculos.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cargarEnFormulario(int vehiculoId)
        {
            try
            {
                DataTable tabla = vehiculoBD.obtenerVehiculo(vehiculoId);

                if (tabla.Rows.Count == 0)
                {
                    MensajeCodelex.mostrar(this, "Vehículos", "No se encontró el vehículo.", false);
                    return;
                }

                DataRow fila = tabla.Rows[0];

                vehiculoActual = Convert.ToInt32(fila["vehiculo_id"]);
                placaActual = fila["placa"].ToString();
                vehiculoActualActivo = Convert.ToBoolean(fila["estado_vehiculo"]);

                txtPlaca.Text = placaActual;
                txtMarca.Text = fila["marca_vehiculo"].ToString();
                txtModelo.Text = fila["modelo_vehiculo"].ToString();

                actualizarBotones();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void limpiarFormulario()
        {
            vehiculoActual = 0;
            placaActual = "";
            vehiculoActualActivo = true;

            txtPlaca.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            dgVehiculos.SelectedItem = null;

            actualizarBotones();
            txtBuscar.Focus();
        }

        private void actualizarBotones()
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

        private void soloPlaca(object sender, TextCompositionEventArgs e)
        {
            if (!vehiculoBD.caracteresDePlaca(e.Text))
                e.Handled = true;
        }

        private void registrarClic(object sender, RoutedEventArgs e)
        {
            RegistrarVehiculo ventana = new RegistrarVehiculo();
            ventana.Owner = this;
            ventana.ShowDialog();

            cargarVehiculos();
        }

        private void limpiarClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        private void actualizarClic(object sender, RoutedEventArgs e)
        {
            if (vehiculoActual == 0)
                return;

            try
            {
                string mensaje;

                bool guardado = vehiculoBD.actualizarVehiculo(
                    vehiculoActual, txtPlaca.Text, txtMarca.Text, txtModelo.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Vehículos", mensaje, guardado);

                if (guardado)
                {
                    placaActual = txtPlaca.Text.Trim().ToUpper();
                    actualizarBotones();
                    cargarVehiculos();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cambiarEstadoClic(object sender, RoutedEventArgs e)
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
                bool guardado = vehiculoBD.cambiarEstadoVehiculo(vehiculoActual, nuevoEstado, out mensaje);

                MensajeCodelex.mostrar(this, "Vehículos", mensaje, guardado);

                if (guardado)
                {
                    vehiculoActualActivo = nuevoEstado;
                    actualizarBotones();
                    cargarVehiculos();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void vehiculosClic(object sender, MouseButtonEventArgs e)
        {
            if (dgVehiculos.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgVehiculos.SelectedItem;
            int vehiculoId = Convert.ToInt32(fila["vehiculo_id"]);

            cargarEnFormulario(vehiculoId);
        }

        private void buscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();
            cargarVehiculos();
        }

        private void buscarClic(object sender, RoutedEventArgs e)
        {
            buscarTexto();
        }

        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                buscarTexto();
        }

        private void mostrarTodosClic(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            filtroEstado = "TODOS";
            cargarVehiculos();
        }

        private void activosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            cargarVehiculos();
        }

        private void inactivosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            cargarVehiculos();
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

        private void salirClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Vehículos", ex.Message, false);
        }
    }
}
