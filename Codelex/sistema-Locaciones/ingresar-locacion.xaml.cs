using Codelex.AccesoDatos;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class IngresarLocacion : Window
    {
        Cliente clienteBD = new Cliente();
        Locacion locacionBD = new Locacion();

        string clienteRtn = "";
        string rtnInicial = "";

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public IngresarLocacion()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        public IngresarLocacion(string rtn) : this()
        {
            rtnInicial = rtn;
        }

        /// <summary>
        /// Prepara la ventana y carga los datos iniciales cuando termina de abrirse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (string tipo in locacionBD.tipos)
                {
                    cmbTipo.Items.Add(tipo);
                }

                if (rtnInicial != "")
                {
                    cardClientes.Visibility = Visibility.Collapsed;
                    cardLocaciones.Margin = new Thickness(0, 16, 0, 0);
                    Grid.SetColumnSpan(cardLocaciones, 2);
                    txtNotaCliente.Visibility = Visibility.Collapsed;

                    mostrarCliente(rtnInicial);
                    txtNombre.Focus();
                }
                else
                {
                    cargarClientes();
                    txtBuscarCliente.Focus();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Carga clientes.
        /// </summary>
        private void cargarClientes()
        {
            DataTable tabla = clienteBD.buscarClientes(txtBuscarCliente.Text.Trim(), "", "ACTIVOS");
            dgClientes.ItemsSource = tabla.DefaultView;
        }

        /// <summary>
        /// Muestra cliente.
        /// </summary>
        /// <param name="rtn">RTN o identidad del cliente (sin guiones).</param>
        private void mostrarCliente(string rtn)
        {
            DataTable tabla = clienteBD.obtenerCliente(rtn);

            if (tabla.Rows.Count == 0)
            {
                aviso("No se encontró el cliente.");
                return;
            }

            DataRow fila = tabla.Rows[0];

            clienteRtn = "" + fila["identificacion_cliente"];

            txtRTN.Text = clienteRtn;
            txtEmpresa.Text = "" + fila["nombre_empresa"];
            txtContacto.Text = fila["nombre_cliente"] + " " + fila["apellido_cliente"];
            txtTelefono.Text = "" + fila["telefono_cliente"];

            cargarLocaciones();
        }

        /// <summary>
        /// Carga locaciones.
        /// </summary>
        private void cargarLocaciones()
        {
            if (clienteRtn == "")
            {
                dgLocaciones.ItemsSource = null;
                return;
            }

            DataTable tabla = clienteBD.locacionesPorCliente(clienteRtn);
            dgLocaciones.ItemsSource = tabla.DefaultView;
        }

        /// <summary>
        /// Limpia formulario.
        /// </summary>
        private void limpiarFormulario()
        {
            txtNombre.Text = "";
            txtDireccion.Text = "";
            cmbTipo.SelectedIndex = -1;
            txtNombre.Focus();
        }

        /// <summary>
        /// Atiende el cambio de texto de buscar.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTextoCambiado(object sender, TextChangedEventArgs e)
        {
            try
            {
                cargarClientes();
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el cambio de selección de clientes.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void clientesSeleccionCambiada(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgClientes.SelectedItem;

            try
            {
                mostrarCliente("" + fila["identificacion_cliente"]);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
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
        /// Valida y guarda los datos del formulario.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool guardada = locacionBD.insertarLocacion(
                    clienteRtn, txtNombre.Text, "" + cmbTipo.SelectedItem, txtDireccion.Text, out mensaje);

                MensajeCodelex.mostrar(this, "Locaciones", mensaje, guardada);

                if (guardada)
                {
                    limpiarFormulario();
                    cargarLocaciones();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
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
        /// Muestra un mensaje de aviso al usuario.
        /// </summary>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        private void aviso(string mensaje)
        {
            MensajeCodelex.mostrar(this, "Locaciones", mensaje, false);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Locaciones", ex.Message, false);
        }
    }
}