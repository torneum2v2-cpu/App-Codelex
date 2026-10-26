using Codelex.Clases_BDD;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class Empleados : Window
    {
        Empleado empleadoBD = new Empleado();

        int empleadoActual = 0;
        bool empleadoActualActivo = true;

        string textoBusqueda = "";
        string buscarPor = "";
        string filtroEstado = "TODOS";

        public Empleados()
        {
            InitializeComponent();
            Height = Math.Min(Height, SystemParameters.WorkArea.Height - 10);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                cmbCargo.ItemsSource = empleadoBD.ListarCargos().DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }

            LimpiarFormulario();
            CargarEmpleados();
        }

        private void CargarEmpleados()
        {
            try
            {
                DataTable tabla = empleadoBD.BuscarEmpleados(textoBusqueda, buscarPor, filtroEstado);
                dgEmpleados.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarEnFormulario(int id)
        {
            try
            {
                DataTable tabla = empleadoBD.ObtenerEmpleado(id);

                if (tabla.Rows.Count == 0)
                {
                    MensajeCodelex.Mostrar(this, "Empleados", "No se encontró el empleado.", false);
                    return;
                }

                DataRow fila = tabla.Rows[0];

                empleadoActual = Convert.ToInt32(fila["empleado_id"]);
                empleadoActualActivo = Convert.ToBoolean(fila["estadoEmpleado"]);

                if (fila["cargo_id"] == DBNull.Value)
                    cmbCargo.SelectedValue = null;
                else
                    cmbCargo.SelectedValue = fila["cargo_id"];
                txtNombre.Text = fila["nombreEmpleado"].ToString();
                txtApellido.Text = fila["apellidoEmpleado"].ToString();
                txtIdentidad.Text = fila["identidad"].ToString();
                txtTelefono.Text = fila["telefonoEmpleado"].ToString();
                txtCorreo.Text = fila["correoEmpleado"].ToString();
                txtHuella.Text = fila["huella_id"].ToString();
                if (fila["usuario"] == DBNull.Value)
                    txtUsuario.Text = "—";
                else
                    txtUsuario.Text = fila["usuario"].ToString();

                ActualizarBotones();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void LimpiarFormulario()
        {
            empleadoActual = 0;
            empleadoActualActivo = true;

            cmbCargo.SelectedIndex = -1;
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtIdentidad.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtHuella.Text = "";
            txtUsuario.Text = "—";

            ActualizarBotones();
            cmbCargo.Focus();
        }

        private void ActualizarBotones()
        {
            if (empleadoActual == 0)
            {
                btnCrear.IsEnabled = true;
                btnActualizar.IsEnabled = false;
                btnCambiarEstado.IsEnabled = false;
                btnVerReporte.IsEnabled = false;
                btnCambiarEstado.Content = "Desactivar";
                txtEstado.Text = "—";
                txtModo.Text = "Nuevo empleado: llene los datos y presione Crear.";
            }
            else
            {
                btnCrear.IsEnabled = false;
                btnActualizar.IsEnabled = true;
                btnCambiarEstado.IsEnabled = true;
                btnVerReporte.IsEnabled = true;
                txtModo.Text = "Editando empleado. Presione Actualizar para guardar.";

                if (empleadoActualActivo)
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

        private int CargoSeleccionado()
        {
            if (cmbCargo.SelectedValue == null)
                return 0;

            return Convert.ToInt32(cmbCargo.SelectedValue);
        }

        private void SoloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!empleadoBD.SoloNumeros(e.Text))
                e.Handled = true;
        }

        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            dgEmpleados.SelectedItem = null;
            LimpiarFormulario();
        }

        private void Crear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                int nuevoId;

                bool guardado = empleadoBD.InsertarEmpleado(
                    CargoSeleccionado(), txtNombre.Text, txtApellido.Text, txtIdentidad.Text,
                    txtTelefono.Text, txtCorreo.Text, txtHuella.Text, out nuevoId, out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    CargarEmpleados();
                    CargarEnFormulario(nuevoId);
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            if (empleadoActual == 0)
                return;

            try
            {
                string mensaje;

                bool guardado = empleadoBD.ActualizarEmpleado(
                    empleadoActual, CargoSeleccionado(), txtNombre.Text, txtApellido.Text, txtIdentidad.Text,
                    txtTelefono.Text, txtCorreo.Text, txtHuella.Text, out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                    CargarEmpleados();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (empleadoActual == 0)
                return;

            bool nuevoEstado = !empleadoActualActivo;
            string nombre = txtNombre.Text + " " + txtApellido.Text;

            string pregunta;
            if (nuevoEstado)
                pregunta = "¿Desea activar al empleado " + nombre + "?";
            else
                pregunta = "¿Desea desactivar al empleado " + nombre + "?";

            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Empleados", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                string mensaje;
                bool guardado = empleadoBD.CambiarEstadoEmpleado(empleadoActual, nuevoEstado, out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    empleadoActualActivo = nuevoEstado;
                    ActualizarBotones();
                    CargarEmpleados();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void VerReporte_Click(object sender, RoutedEventArgs e)
        {
            if (empleadoActual == 0)
            {
                MensajeCodelex.Mostrar(this, "Empleados", "Seleccione un empleado de la tabla.", false);
                return;
            }

            AbrirReporte(empleadoActual);
        }

        private void dgEmpleados_Click(object sender, MouseButtonEventArgs e)
        {
            if (dgEmpleados.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgEmpleados.SelectedItem;
            CargarEnFormulario(Convert.ToInt32(fila["empleado_id"]));
        }

        private void dgEmpleados_DobleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgEmpleados.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgEmpleados.SelectedItem;
            AbrirReporte(Convert.ToInt32(fila["empleado_id"]));
        }

        private void AbrirReporte(int id)
        {
            EmpleadoReporte reporte = new EmpleadoReporte(id);
            reporte.Owner = this;
            reporte.ShowDialog();
        }

        private void PorNombre_Click(object sender, RoutedEventArgs e)
        {
            Buscar("NOMBRE");
        }

        private void PorIdentidad_Click(object sender, RoutedEventArgs e)
        {
            Buscar("IDENTIDAD");
        }

        private void PorCargo_Click(object sender, RoutedEventArgs e)
        {
            Buscar("CARGO");
        }

        private void Buscar(string criterio)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = criterio;
            CargarEmpleados();
        }

        private void MostrarTodos_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            buscarPor = "";
            filtroEstado = "TODOS";
            CargarEmpleados();
        }

        private void Activos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            CargarEmpleados();
        }

        private void Inactivos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            CargarEmpleados();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Buscar("");
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
            MensajeCodelex.Mostrar(this, "Empleados", mensaje, correcto);
        }

        private void MostrarError(Exception ex)
        {
            MensajeCodelex.Mostrar(this, "Empleados", ex.Message, false);
        }
    }
}
