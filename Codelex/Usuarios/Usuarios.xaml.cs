using Codelex.BDD;
using Codelex.Clases_BDD;
using System.Data;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class Usuarios : Window
    {
        UsuarioBD usuarioBD = new UsuarioBD();

        int usuarioActual = 0;
        int empleadoActual = 0;
        bool usuarioActivo = true;

        string textoBusqueda = "";
        string buscarPor = "";
        string filtroEstado = "TODOS";

        public Usuarios()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            cargarRoles();
            limpiarFormulario();
            cargarUsuarios();
        }

        private void cargarRoles()
        {
            try
            {
                DataTable roles = usuarioBD.listarRoles();

                if (Sesion.Nivel != NivelAcceso.Avanzado)
                    roles.DefaultView.RowFilter = "nombreRol NOT LIKE '%Avanzado%'";

                cmbRol.ItemsSource = roles.DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cargarUsuarios()
        {
            try
            {
                DataTable tabla = usuarioBD.buscarUsuarios(textoBusqueda, buscarPor, filtroEstado);
                dgUsuarios.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void usuariosClic(object sender, MouseButtonEventArgs e)
        {
            if (dgUsuarios.SelectedItem == null)
                return;

            cargarUsuario((DataRowView)dgUsuarios.SelectedItem);
        }

        private void limpiarFormulario()
        {
            usuarioActual = 0;
            empleadoActual = 0;
            usuarioActivo = true;

            txtNombre.Text = "";
            txtApellido.Text = "";
            txtCorreo.Text = "";
            txtTelefono.Text = "";
            txtUsuario.Text = "";
            cmbRol.SelectedIndex = -1;
            pbContrasena.Clear();
            pbConfirmar.Clear();

            actualizarBotones();
        }

        private void actualizarBotones()
        {
            bool editando = usuarioActual > 0;

            btnCrear.IsEnabled = !editando;
            btnActualizar.IsEnabled = editando;
            btnBuscarEmpleado.IsEnabled = !editando;
            btnCambiarEstado.IsEnabled = editando;

            btnCambiarEstado.Content = usuarioActivo ? "Desactivar" : "Activar";

            if (editando)
                txtEstado.Text = usuarioActivo ? "Activo" : "Inactivo";
            else if (empleadoActual > 0)
                txtEstado.Text = "Activo";
            else
                txtEstado.Text = "—";
        }

        private void cargarEmpleado(DataRowView fila)
        {
            usuarioActual = 0;
            usuarioActivo = true;
            empleadoActual = Convert.ToInt32(fila["empleado_id"]);

            txtNombre.Text = "" + fila["nombre_empleado"];
            txtApellido.Text = "" + fila["apellido_empleado"];
            txtCorreo.Text = "" + fila["correo_empleado"];
            txtTelefono.Text = "" + fila["telefono_empleado"];

            string sugerido = generarUsuario(txtNombre.Text, txtApellido.Text);
            try
            {
                sugerido = usuarioBD.usuarioDisponible(sugerido);
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
            txtUsuario.Text = sugerido;

            cmbRol.SelectedIndex = -1;
            pbContrasena.Clear();
            pbConfirmar.Clear();

            actualizarBotones();
        }

        private void cargarUsuario(DataRowView fila)
        {
            usuarioActual = Convert.ToInt32(fila["usuario_id"]);
            empleadoActual = Convert.ToInt32(fila["empleado_id"]);
            usuarioActivo = Convert.ToBoolean(fila["estado_usuario"]);

            txtNombre.Text = "" + fila["nombre_empleado"];
            txtApellido.Text = "" + fila["apellido_empleado"];
            txtCorreo.Text = "" + fila["correo_empleado"];
            txtTelefono.Text = "" + fila["telefono_empleado"];

            txtUsuario.Text = "" + fila["usuario"];

            if (fila["rol_id"] == DBNull.Value)
                cmbRol.SelectedIndex = -1;
            else
                cmbRol.SelectedValue = Convert.ToInt32(fila["rol_id"]);

            pbContrasena.Clear();
            pbConfirmar.Clear();

            actualizarBotones();
        }

        private string generarUsuario(string nombre, string apellido)
        {
            string primerApellido = apellido.Trim();
            int espacio = primerApellido.IndexOf(' ');
            if (espacio > 0)
                primerApellido = primerApellido.Substring(0, espacio);

            string inicio = soloLetras(nombre);
            if (inicio.Length > 2)
                inicio = inicio.Substring(0, 2);

            return inicio + soloLetras(primerApellido);
        }

        private string soloLetras(string texto)
        {
            string descompuesto = texto.Normalize(NormalizationForm.FormD);
            StringBuilder resultado = new StringBuilder();

            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                    continue;

                if (char.IsLetter(c))
                    resultado.Append(char.ToLower(c));
            }

            return resultado.ToString();
        }

        private string validarFormulario(bool esNuevo)
        {
            if (empleadoActual == 0)
                return "Elija un empleado con el botón Buscar empleado.";

            if (esNuevo && !correoValido(txtCorreo.Text))
                return "El correo del empleado está vacío o no tiene un formato válido. " +
                       "Corríjalo en el módulo de Empleados antes de continuar.";

            if (txtUsuario.Text.Trim() == "")
                return "El usuario es obligatorio.";

            if (txtUsuario.Text.Trim().Contains(' '))
                return "El usuario no puede llevar espacios.";

            if (cmbRol.SelectedValue == null)
                return "Seleccione un rol.";

            if (esNuevo && pbContrasena.Password == "")
                return "La contraseña es obligatoria.";

            if (pbContrasena.Password != pbConfirmar.Password)
                return "Las contraseñas no coinciden.";

            return "";
        }

        private bool correoValido(string correo)
        {
            correo = correo.Trim();

            if (correo == "")
                return false;

            if (!MailAddress.TryCreate(correo, out MailAddress? direccion))
                return false;

            return direccion.Address == correo && direccion.Host.Contains('.');
        }

        private void nuevoClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
            dgUsuarios.SelectedItem = null;
        }

        private void buscarEmpleadoClic(object sender, RoutedEventArgs e)
        {
            ElegirEmpleado ventana = new ElegirEmpleado();
            ventana.Owner = this;

            if (ventana.ShowDialog() == true && ventana.empleadoElegido != null)
                cargarEmpleado(ventana.empleadoElegido);
        }

        private void crearClic(object sender, RoutedEventArgs e)
        {
            string error = validarFormulario(true);
            if (error != "")
            {
                MessageBox.Show(error, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string mensaje;

                bool guardado = usuarioBD.crearUsuario(
                    empleadoActual, txtUsuario.Text, pbContrasena.Password,
                    Convert.ToInt32(cmbRol.SelectedValue), out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    cargarUsuarios();
                    limpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void actualizarClic(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == 0)
                return;

            string error = validarFormulario(false);
            if (error != "")
            {
                MessageBox.Show(error, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string mensaje;

                bool guardado = usuarioBD.actualizarUsuario(
                    usuarioActual, txtUsuario.Text, pbContrasena.Password,
                    Convert.ToInt32(cmbRol.SelectedValue), out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    pbContrasena.Clear();
                    pbConfirmar.Clear();
                    cargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void cambiarEstadoClic(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == 0)
                return;

            bool nuevoEstado = !usuarioActivo;

            string pregunta;
            if (nuevoEstado)
                pregunta = "¿Desea activar el usuario " + txtUsuario.Text + "?";
            else
                pregunta = "¿Desea desactivar el usuario " + txtUsuario.Text + "?";

            MessageBoxResult respuesta = MessageBox.Show(pregunta, "Usuarios", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (respuesta != MessageBoxResult.Yes)
                return;

            try
            {
                string mensaje;
                bool guardado = usuarioBD.cambiarEstadoUsuario(usuarioActual, nuevoEstado, out mensaje);

                mostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    usuarioActivo = nuevoEstado;
                    actualizarBotones();
                    cargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                mostrarError(ex);
            }
        }

        private void porNombreClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "NOMBRE";
            cargarUsuarios();
        }

        private void porUsuarioClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "USUARIO";
            cargarUsuarios();
        }

        private void porCorreoClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "CORREO";
            cargarUsuarios();
        }

        private void porRolClic(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "ROL";
            cargarUsuarios();
        }

        private void activosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            cargarUsuarios();
        }

        private void inactivosClic(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            cargarUsuarios();
        }

        private void mostrarTodosClic(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            buscarPor = "";
            filtroEstado = "TODOS";
            cargarUsuarios();
        }

        private void buscarTeclaPresionada(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textoBusqueda = txtBuscar.Text.Trim();
                buscarPor = "";
                cargarUsuarios();
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
                MessageBox.Show(mensaje, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(mensaje, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void mostrarError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}