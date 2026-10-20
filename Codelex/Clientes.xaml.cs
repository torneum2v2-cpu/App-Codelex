using Codelex.Clases_BDD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Codelex
{
    /// <summary>
    /// Lógica de interacción para Clientes.xaml
    /// </summary>
    public partial class Clientes : Window
    {
        Cliente clienteBD = new Cliente();

        string clienteActual = "";
        bool clienteActualActivo = true;

        string textoBusqueda = "";
        string buscarPor = "";
        string filtroEstado = "TODOS";

        public Clientes()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            CargarClientes();
        }

        private void CargarClientes()
        {
            try
            {
                DataTable tabla = clienteBD.BuscarClientes(textoBusqueda, buscarPor, filtroEstado);
                dgClientes.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarEnFormulario(string rtn)
        {
            try
            {
                DataTable tabla = clienteBD.ObtenerCliente(rtn);

                if (tabla.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró el cliente.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DataRow fila = tabla.Rows[0];

                clienteActual = "" + fila["identificacionCliente"];
                clienteActualActivo = Convert.ToBoolean(fila["estadoCliente"]);

                txtRTN.Text = clienteActual;
                txtEmpresa.Text = fila["nombreEmpresa"].ToString();
                txtNombre.Text = fila["nombreCliente"].ToString();
                txtApellido.Text = fila["apellidoCliente"].ToString();
                txtTelefono.Text = fila["telefonoCliente"].ToString();
                txtCorreo.Text = fila["correoCliente"].ToString();

                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void LimpiarFormulario()
        {
            clienteActual = "";
            clienteActualActivo = true;

            txtRTN.Text = "";
            txtEmpresa.Text = "";
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";

            ActualizarBotones();
            txtRTN.Focus();
        }

        private void ActualizarBotones()
        {
            if (clienteActual == "")
            {
                txtRTN.IsReadOnly = false;
                btnCrear.IsEnabled = true;
                btnActualizar.IsEnabled = false;
                btnCambiarEstado.IsEnabled = false;
                btnCambiarEstado.Content = "Desactivar";
                txtEstado.Text = "—";
                txtModo.Text = "Nuevo cliente: llene los datos y presione Crear.";
            }
            else
            {
                txtRTN.IsReadOnly = true;
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

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!clienteBD.SoloNumeros(e.Text))
                e.Handled = true;
        }

        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void Crear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                bool guardado = clienteBD.InsertarCliente(
                    txtRTN.Text, txtEmpresa.Text, txtNombre.Text, txtApellido.Text,
                    txtTelefono.Text, txtCorreo.Text, out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    CargarClientes();
                    CargarEnFormulario(txtRTN.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            if (clienteActual == "")
                return;

            try
            {
                string mensaje;

                bool guardado = clienteBD.ActualizarCliente(
                    clienteActual, txtEmpresa.Text, txtNombre.Text, txtApellido.Text,
                    txtTelefono.Text, txtCorreo.Text, out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    CargarClientes();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CambiarEstado_Click(object sender, RoutedEventArgs e)
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
                bool guardado = clienteBD.CambiarEstadoCliente(clienteActual, nuevoEstado, out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    clienteActualActivo = nuevoEstado;
                    ActualizarBotones();
                    CargarClientes();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void VerDetalle_Click(object sender, RoutedEventArgs e)
        {
            if (clienteActual == "")
            {
                MessageBox.Show("Seleccione un cliente de la tabla.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AbrirDetalle(clienteActual);
        }

        private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgClientes.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgClientes.SelectedItem;
            string rtn = "" + fila["identificacionCliente"];

            AbrirDetalle(rtn);

            dgClientes.SelectedItem = null;
        }

        private void AbrirDetalle(string rtn)
        {
            ClienteDetalle detalle = new ClienteDetalle(rtn);
            detalle.Owner = this;
            detalle.ShowDialog();

            if (detalle.EditarCliente)
                CargarEnFormulario(rtn);
        }

        private void PorRTN_Click(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "RTN";
            CargarClientes();
        }

        private void PorNombre_Click(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "NOMBRE";
            CargarClientes();
        }

        private void MostrarTodos_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            buscarPor = "";
            filtroEstado = "TODOS";
            CargarClientes();
        }

        private void Activos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            CargarClientes();
        }

        private void Inactivos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            CargarClientes();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textoBusqueda = txtBuscar.Text.Trim();
                buscarPor = "";
                CargarClientes();
            }
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

        private void MostrarMensaje(bool correcto, string mensaje)
        {
            if (correcto)
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MostrarError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
