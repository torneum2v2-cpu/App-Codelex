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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CargarRoles();
            LimpiarFormulario();
            CargarUsuarios();
        }

        private void CargarRoles()
        {
            try
            {
                DataTable roles = usuarioBD.ListarRoles();
                cmbRol.ItemsSource = roles.DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CargarUsuarios()
        {
            try
            {
                DataTable tabla = usuarioBD.BuscarUsuarios(textoBusqueda, buscarPor, filtroEstado);
                dgUsuarios.ItemsSource = tabla.DefaultView;
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void dgUsuarios_Click(object sender, MouseButtonEventArgs e)
        {
            if (dgUsuarios.SelectedItem == null)
                return;

            CargarUsuario((DataRowView)dgUsuarios.SelectedItem);
        }

        private void LimpiarFormulario()
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

            ActualizarBotones();
        }

        private void ActualizarBotones()
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

        private void CargarEmpleado(DataRowView fila)
        {
            usuarioActual = 0;
            usuarioActivo = true;
            empleadoActual = Convert.ToInt32(fila["empleado_id"]);

            txtNombre.Text = "" + fila["nombreEmpleado"];
            txtApellido.Text = "" + fila["apellidoEmpleado"];
            txtCorreo.Text = "" + fila["correoEmpleado"];
            txtTelefono.Text = "" + fila["telefonoEmpleado"];

            string sugerido = GenerarUsuario(txtNombre.Text, txtApellido.Text);
            try
            {
                sugerido = usuarioBD.UsuarioDisponible(sugerido);
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
            txtUsuario.Text = sugerido;

            cmbRol.SelectedIndex = -1;
            pbContrasena.Clear();
            pbConfirmar.Clear();

            ActualizarBotones();
        }

        private void CargarUsuario(DataRowView fila)
        {
            usuarioActual = Convert.ToInt32(fila["usuario_id"]);
            empleadoActual = Convert.ToInt32(fila["empleado_id"]);
            usuarioActivo = Convert.ToBoolean(fila["estadoUsuario"]);

            txtNombre.Text = "" + fila["nombreEmpleado"];
            txtApellido.Text = "" + fila["apellidoEmpleado"];
            txtCorreo.Text = "" + fila["correoEmpleado"];
            txtTelefono.Text = "" + fila["telefonoEmpleado"];

            txtUsuario.Text = "" + fila["usuario"];

            if (fila["rol_id"] == DBNull.Value)
                cmbRol.SelectedIndex = -1;
            else
                cmbRol.SelectedValue = Convert.ToInt32(fila["rol_id"]);

            pbContrasena.Clear();
            pbConfirmar.Clear();

            ActualizarBotones();
        }

        private string GenerarUsuario(string nombre, string apellido)
        {
            string primerApellido = apellido.Trim();
            int espacio = primerApellido.IndexOf(' ');
            if (espacio > 0)
                primerApellido = primerApellido.Substring(0, espacio);

            string inicio = SoloLetras(nombre);
            if (inicio.Length > 2)
                inicio = inicio.Substring(0, 2);

            return inicio + SoloLetras(primerApellido);
        }

        private string SoloLetras(string texto)
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

        private string ValidarFormulario(bool esNuevo)
        {
            if (empleadoActual == 0)
                return "Elija un empleado con el botón Buscar empleado.";

            if (esNuevo && !CorreoValido(txtCorreo.Text))
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

        private bool CorreoValido(string correo)
        {
            correo = correo.Trim();

            if (correo == "")
                return false;

            if (!MailAddress.TryCreate(correo, out MailAddress? direccion))
                return false;

            return direccion.Address == correo && direccion.Host.Contains('.');
        }

        private void Nuevo_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
            dgUsuarios.SelectedItem = null;
        }

        private void BuscarEmpleado_Click(object sender, RoutedEventArgs e)
        {
            ElegirEmpleado ventana = new ElegirEmpleado();
            ventana.Owner = this;

            if (ventana.ShowDialog() == true && ventana.EmpleadoElegido != null)
                CargarEmpleado(ventana.EmpleadoElegido);
        }

        private void Crear_Click(object sender, RoutedEventArgs e)
        {
            string error = ValidarFormulario(true);
            if (error != "")
            {
                MessageBox.Show(error, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string mensaje;

                bool guardado = usuarioBD.CrearUsuario(
                    empleadoActual, txtUsuario.Text, pbContrasena.Password,
                    Convert.ToInt32(cmbRol.SelectedValue), out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    CargarUsuarios();
                    LimpiarFormulario();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void Actualizar_Click(object sender, RoutedEventArgs e)
        {
            if (usuarioActual == 0)
                return;

            string error = ValidarFormulario(false);
            if (error != "")
            {
                MessageBox.Show(error, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string mensaje;

                bool guardado = usuarioBD.ActualizarUsuario(
                    usuarioActual, txtUsuario.Text, pbContrasena.Password,
                    Convert.ToInt32(cmbRol.SelectedValue), out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    pbContrasena.Clear();
                    pbConfirmar.Clear();
                    CargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void CambiarEstado_Click(object sender, RoutedEventArgs e)
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
                bool guardado = usuarioBD.CambiarEstadoUsuario(usuarioActual, nuevoEstado, out mensaje);

                MostrarMensaje(guardado, mensaje);

                if (guardado)
                {
                    usuarioActivo = nuevoEstado;
                    ActualizarBotones();
                    CargarUsuarios();
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        private void PorNombre_Click(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "NOMBRE";
            CargarUsuarios();
        }

        private void PorUsuario_Click(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "USUARIO";
            CargarUsuarios();
        }

        private void PorCorreo_Click(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "CORREO";
            CargarUsuarios();
        }

        private void PorRol_Click(object sender, RoutedEventArgs e)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "ROL";
            CargarUsuarios();
        }

        private void Activos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "ACTIVOS";
            CargarUsuarios();
        }

        private void Inactivos_Click(object sender, RoutedEventArgs e)
        {
            filtroEstado = "INACTIVOS";
            CargarUsuarios();
        }

        private void MostrarTodos_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = "";
            textoBusqueda = "";
            buscarPor = "";
            filtroEstado = "TODOS";
            CargarUsuarios();
        }

        private void Buscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                textoBusqueda = txtBuscar.Text.Trim();
                buscarPor = "";
                CargarUsuarios();
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
                MessageBox.Show(mensaje, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Information);
            else
                MessageBox.Show(mensaje, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MostrarError(Exception ex)
        {
            MessageBox.Show(ex.Message, "Usuarios", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}