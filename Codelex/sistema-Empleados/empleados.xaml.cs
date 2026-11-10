using Codelex.AccesoDatos;
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

        /// <summary>
        /// Inicializa la ventana y prepara sus controles.
        /// </summary>
        public Empleados()
        {
            InitializeComponent();
            Height = Math.Min(Height, SystemParameters.WorkArea.Height - 10);
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
                cmbCargo.ItemsSource = empleadoBD.listarCargos().DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }

            limpiarFormulario();
            cargarEmpleados();
        }

        /// <summary>
        /// Carga empleados.
        /// </summary>
        private void cargarEmpleados()
        {
            try
            {
                DataTable tabla = empleadoBD.buscarEmpleados(textoBusqueda, buscarPor, filtroEstado);
                dgEmpleados.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Carga en formulario.
        /// </summary>
        /// <param name="id">Identificador.</param>
        private void cargarEnFormulario(int id)
        {
            try
            {
                DataTable tabla = empleadoBD.obtenerEmpleado(id);

                if (tabla.Rows.Count == 0)
                {
                    MensajeCodelex.mostrar(this, "Empleados", "No se encontró el empleado.", false);
                    return;
                }

                DataRow fila = tabla.Rows[0];

                empleadoActual = Convert.ToInt32(fila["empleado_id"]);
                empleadoActualActivo = Convert.ToBoolean(fila["estado_empleado"]);

                if (fila["cargo_id"] == DBNull.Value)
                    cmbCargo.SelectedValue = null;
                else
                    cmbCargo.SelectedValue = fila["cargo_id"];
                txtNombre.Text = fila["nombre_empleado"].ToString();
                txtApellido.Text = fila["apellido_empleado"].ToString();
                txtIdentidad.Text = fila["identidad"].ToString();
                txtTelefono.Text = fila["telefono_empleado"].ToString();
                txtCorreo.Text = fila["correo_empleado"].ToString();
                if (fila["usuario"] == DBNull.Value)
                    txtUsuario.Text = "—";
                else
                    txtUsuario.Text = fila["usuario"].ToString();

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
            empleadoActual = 0;
            empleadoActualActivo = true;

            cmbCargo.SelectedIndex = -1;
            txtNombre.Text = "";
            txtApellido.Text = "";
            txtIdentidad.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtUsuario.Text = "—";

            actualizarBotones();
            cmbCargo.Focus();
        }

        /// <summary>
        /// Actualiza botones.
        /// </summary>
        private void actualizarBotones()
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

        /// <summary>
        /// Obtiene el identificador del cargo elegido.
        /// </summary>
        /// <returns>Identificador del cargo elegido, o 0 si no hay.</returns>
        private int cargoSeleccionado()
        {
            if (cmbCargo.SelectedValue == null)
                return 0;

            return Convert.ToInt32(cmbCargo.SelectedValue);
        }

        /// <summary>
        /// Impide escribir caracteres que no sean números.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            if (!empleadoBD.soloNumeros(e.Text))
                e.Handled = true;
        }

        /// <summary>
        /// Limpia el formulario para registrar uno nuevo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void nuevoClic(object sender, RoutedEventArgs e)
        {
            dgEmpleados.SelectedItem = null;
            limpiarFormulario();
        }

        /// <summary>
        /// Valida y guarda el registro nuevo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void crearClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;
                int nuevoId;

                bool guardado = empleadoBD.insertarEmpleado(
                    cargoSeleccionado(), txtNombre.Text, txtApellido.Text, txtIdentidad.Text,
                    txtTelefono.Text, txtCorreo.Text, out nuevoId, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    cargarEmpleados();
                    cargarEnFormulario(nuevoId);
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Valida y guarda los cambios del registro seleccionado.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void actualizarClic(object sender, RoutedEventArgs e)
        {
            if (empleadoActual == 0)
                return;

            try
            {
                string mensaje;

                bool guardado = empleadoBD.actualizarEmpleado(
                    empleadoActual, cargoSeleccionado(), txtNombre.Text, txtApellido.Text, txtIdentidad.Text,
                    txtTelefono.Text, txtCorreo.Text, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                    cargarEmpleados();
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
                bool guardado = empleadoBD.cambiarEstadoEmpleado(empleadoActual, nuevoEstado, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    empleadoActualActivo = nuevoEstado;
                    actualizarBotones();
                    cargarEmpleados();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        /// <summary>
        /// Atiende el clic de ver reporte.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void verReporteClic(object sender, RoutedEventArgs e)
        {
            if (empleadoActual == 0)
            {
                MensajeCodelex.mostrar(this, "Empleados", "Seleccione un empleado de la tabla.", false);
                return;
            }

            abrirReporte(empleadoActual);
        }

        /// <summary>
        /// Atiende el clic de empleados.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void empleadosClic(object sender, MouseButtonEventArgs e)
        {
            if (dgEmpleados.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgEmpleados.SelectedItem;
            cargarEnFormulario(Convert.ToInt32(fila["empleado_id"]));
        }

        /// <summary>
        /// Atiende el doble clic en empleados.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void empleadosDobleClic(object sender, MouseButtonEventArgs e)
        {
            if (dgEmpleados.SelectedItem == null)
                return;

            DataRowView fila = (DataRowView)dgEmpleados.SelectedItem;
            abrirReporte(Convert.ToInt32(fila["empleado_id"]));
        }

        /// <summary>
        /// Abre reporte.
        /// </summary>
        /// <param name="id">Identificador.</param>
        private void abrirReporte(int id)
        {
            EmpleadoReporte reporte = new EmpleadoReporte(id);
            reporte.Owner = this;
            reporte.ShowDialog();
        }

        /// <summary>
        /// Atiende el clic de por nombre.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void porNombreClic(object sender, RoutedEventArgs e)
        {
            buscar("NOMBRE");
        }

        /// <summary>
        /// Atiende el clic de por identidad.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void porIdentidadClic(object sender, RoutedEventArgs e)
        {
            buscar("IDENTIDAD");
        }

        /// <summary>
        /// Atiende el clic de por cargo.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void porCargoClic(object sender, RoutedEventArgs e)
        {
            buscar("CARGO");
        }

        /// <summary>
        /// Busca empleados según el criterio indicado.
        /// </summary>
        /// <param name="criterio">Criterio de búsqueda.</param>
        private void buscar(string criterio)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = criterio;
            cargarEmpleados();
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
            buscarPor = "";
            filtroEstado = "TODOS";
            cargarEmpleados();
        }

        /// <summary>
        /// Muestra solo los registros activos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void activosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            cargarEmpleados();
        }

        /// <summary>
        /// Muestra solo los registros inactivos.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void inactivosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            cargarEmpleados();
        }

        /// <summary>
        /// Ejecuta la búsqueda al presionar Enter.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                buscar("");
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
        /// Muestra mensaje.
        /// </summary>
        /// <param name="correcto">Correcto.</param>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        private void mostrarMensaje(bool correcto, string mensaje)
        {
            MensajeCodelex.mostrar(this, "Empleados", mensaje, correcto);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        private void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, "Empleados", ex.Message, false);
        }
    }
}
