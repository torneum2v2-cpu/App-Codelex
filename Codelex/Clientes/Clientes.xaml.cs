using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class Clientes : Window
    {
        Cliente clienteBD = new Cliente();

        int clienteId = 0;
        string clienteActual = "";
        bool clienteActualActivo = true;

        string textoBusqueda = "";
        string buscarPor = "";
        string filtroEstado = "TODOS";

        public Clientes()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            foreach (string tipo in clienteBD.tiposIdentificacion())
            {
                cmbTipoId.Items.Add(tipo);
            }

            cmbTipoId.SelectionChanged += tipoIdSeleccionCambiada;
            cmbTipoId.SelectedIndex = 0;

            limpiarFormulario();
            cargarClientes();
        }

        private void tipoIdSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            txtRTN.MaxLength = clienteBD.largoIdentificacion("" + cmbTipoId.SelectedItem);
        }

        private void cargarClientes()
        {
            try
            {
                DataTable tabla = clienteBD.buscarClientes(textoBusqueda, buscarPor, filtroEstado);
                dgClientes.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cargarEnFormulario(string rtn)
        {
            try
            {
                DataTable tabla = clienteBD.obtenerCliente(rtn);

                if (tabla.Rows.Count == 0)
                {
                    MensajeCodelex.mostrar(this, "Clientes", "No se encontró el cliente.", false);
                    return;
                }

                DataRow fila = tabla.Rows[0];

                clienteId = Convert.ToInt32(fila["cliente_id"]);
                clienteActual = "" + fila["identificacion_cliente"];
                clienteActualActivo = Convert.ToBoolean(fila["estado_cliente"]);

                cmbTipoId.SelectedItem = "" + fila["tipo_identificacion"];
                txtRTN.Text = clienteActual;
                txtEmpresa.Text = fila["nombre_empresa"].ToString();
                txtNombre.Text = fila["nombre_cliente"].ToString();
                txtApellido.Text = fila["apellido_cliente"].ToString();
                txtTelefono.Text = fila["telefono_cliente"].ToString();
                txtCorreo.Text = fila["correo_cliente"].ToString();

                actualizarBotones();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void limpiarFormulario()
        {
            clienteId = 0;
            clienteActual = "";
            clienteActualActivo = true;

            cmbTipoId.SelectedIndex = 0;
            txtRTN.Text = "";
            txtEmpresa.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";

            actualizarBotones();
            txtRTN.Focus();
        }

        private void actualizarBotones()
        {
            if (clienteActual == "")
            {
                btnCrear.IsEnabled = true;
                btnActualizar.IsEnabled = false;
                btnCambiarEstado.IsEnabled = false;
                btnCambiarEstado.Content = "Desactivar";
                txtEstado.Text = "—";
                txtModo.Text = "Nuevo cliente: llene los datos y presione Crear.";
            }
            else
            {
                btnCrear.IsEnabled = false;
                btnActualizar.IsEnabled = true;
                btnCambiarEstado.IsEnabled = true;
                txtModo.Text = "Editando el cliente " + clienteActual + ". Presione Actualizar para guardar los cambios.";

                if (clienteActualActivo)
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

        private void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!clienteBD.soloNumeros(e.Text))
                e.Handled = true;
        }

        private void nuevoClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        private void crearClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool guardado = clienteBD.insertarCliente(
                    "" + cmbTipoId.SelectedItem, txtRTN.Text, txtEmpresa.Text, txtNombre.Text, txtApellido.Text,
                    txtTelefono.Text, txtCorreo.Text, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    cargarClientes();
                    cargarEnFormulario(Cliente.limpiarNumero(txtRTN.Text));
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void actualizarClic(object sender, RoutedEventArgs e)
        {
            if (clienteActual == "")
                return;

            try
            {
                string mensaje;

                bool guardado = clienteBD.actualizarCliente(
                    clienteId, "" + cmbTipoId.SelectedItem, txtRTN.Text, txtEmpresa.Text, txtNombre.Text, txtApellido.Text,
                    txtTelefono.Text, txtCorreo.Text, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    cargarClientes();
                    cargarEnFormulario(Cliente.limpiarNumero(txtRTN.Text));
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cambiarEstadoClic(object sender, RoutedEventArgs e)
        {
            if (clienteActual == "")
                return;

            bool nuevoEstado = !clienteActualActivo;

            string pregunta;
            if (nuevoEstado)
                pregunta = "¿Desea activar el cliente " + txtNombre.Text + "?";
            else
                pregunta = "¿Desea desactivar el cliente " + txtNombre.Text + "?";

            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Clientes", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                string mensaje;
                bool guardado = clienteBD.cambiarEstadoCliente(clienteActual, nuevoEstado, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    clienteActualActivo = nuevoEstado;
                    actualizarBotones();
                    cargarClientes();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void verDetalleClic(object sender, RoutedEventArgs e)
        {
            if (clienteActual == "")
            {
                MessageBox.Show("Seleccione un cliente de la tabla.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            abrirDetalle(clienteActual);
        }

        private void clientesClic(object sender, MouseButtonEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgClientes.SelectedItem;
            string rtn = "" + fila["identificacion_cliente"];

            abrirDetalle(rtn);

            dgClientes.SelectedItem = null;
        }

        private void abrirDetalle(string rtn)
        {
            ClienteDetalle detalle = new ClienteDetalle(rtn);
            detalle.Owner = this;
            detalle.ShowDialog();

            if (detalle.editarCliente)
                cargarEnFormulario(rtn);
        }

        private void porRTNClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "RTN";
            cargarClientes();
        }

        private void porNombreClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "NOMBRE";
            cargarClientes();
        }

        private void mostrarTodosClic(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            buscarPor = "";
            filtroEstado = "TODOS";
            cargarClientes();
        }

        private void activosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            cargarClientes();
        }

        private void inactivosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            cargarClientes();
        }

        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textoBusqueda = txtBuscar.Text.Trim();
                buscarPor = "";
                cargarClientes();
            }
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

        private void mostrarMensaje(bool correcto, string mensaje)
        {
            if (correcto)
               MensajeCodelex.mostrar(this,"Clientes", mensaje, true);
            else
                MensajeCodelex.mostrar(this, "Clientes", mensaje, false);
        }

        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Clientes", ex.Message, false);
        }
    }
}
