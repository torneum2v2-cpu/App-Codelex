using Codelex.AccesoDatos;
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

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public GestionVehiculos()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
            cargarVehiculos();
        }

        /// <summary>
        /// Carga vehículos.
        /// </summary>
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

        /// <summary>
        /// Carga en formulario.
        /// </summary>
        /// <param name="vehiculoId">Identificador de vehículo.</param>
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

        /// <summary>
        /// Limpia formulario.
        /// </summary>
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

        /// <summary>
        /// Actualiza botones.
        /// </summary>
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

        /// <summary>
        /// Impide escribir caracteres que no se permiten en una placa.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloPlaca(object sender, TextCompositionEventArgs e)
        {
            if (!vehiculoBD.caracteresDePlaca(e.Text))
                e.Handled = true;
        }

        /// <summary>
        /// Atiende el clic de registrar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void registrarClic(object sender, RoutedEventArgs e)
        {
            RegistrarVehiculo ventana = new RegistrarVehiculo();
            ventana.Owner = this;
            ventana.ShowDialog();

            cargarVehiculos();
        }

        /// <summary>
        /// Limpia los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void limpiarClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        /// <summary>
        /// Valida y guarda los cambios del registro seleccionado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

        /// <summary>
        /// Activa o desactiva el registro seleccionado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
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

            if (!MensajeCodelex.preguntar(this, "Vehículos", pregunta))
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

        /// <summary>
        /// Atiende el clic de vehículos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void vehiculosClic(object sender, MouseButtonEventArgs e)
        {
            if (dgVehiculos.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgVehiculos.SelectedItem;
            int vehiculoId = Convert.ToInt32(fila["vehiculo_id"]);

            cargarEnFormulario(vehiculoId);
        }

        /// <summary>
        /// Busca texto.
        /// </summary>
        private void buscarTexto()
        {
            textoBusqueda = txtBuscar.Text.Trim();
            cargarVehiculos();
        }

        /// <summary>
        /// Busca con el texto escrito.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarClic(object sender, RoutedEventArgs e)
        {
            buscarTexto();
        }

        /// <summary>
        /// Ejecuta la búsqueda al presionar Enter.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                buscarTexto();
        }

        /// <summary>
        /// Quita los filtros y muestra todos los registros.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void mostrarTodosClic(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            filtroEstado = "TODOS";
            cargarVehiculos();
        }

        /// <summary>
        /// Muestra solo los registros activos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void activosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            cargarVehiculos();
        }

        /// <summary>
        /// Muestra solo los registros inactivos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void inactivosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            cargarVehiculos();
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
        private void salirClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Vehículos", ex.Message, false);
        }
    }
}
