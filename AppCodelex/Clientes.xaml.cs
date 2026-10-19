using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppCodelex.BDD;

namespace AppCodelex;

/// <summary>
/// Interaction logic for Clientes.xaml
/// </summary>
public partial class Clientes : Window
{
    ClienteBD clienteBD = new ClienteBD();

    // Cliente que se está editando. Si es 0, el formulario está en modo "nuevo cliente".
    int clienteActualId = 0;
    bool clienteActualActivo = true;

    // Búsqueda actual, para volver a cargar la tabla después de guardar
    string textoBusqueda = "";
    string buscarPor = "";
    string filtroEstado = "TODOS";

    public Clientes()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CargarLocaciones();
        LimpiarFormulario();
        CargarClientes();
    }

    // ===================== CARGAR DATOS =====================

    private void CargarLocaciones()
    {
        try
        {
            DataTable tabla = clienteBD.ListarLocaciones();

            // Primera opción del combo: sin locación
            DataRow filaVacia = tabla.NewRow();
            filaVacia["locacion_id"] = DBNull.Value;
            filaVacia["nombreLocacion"] = "(Sin locación)";
            tabla.Rows.InsertAt(filaVacia, 0);

            cbLocacion.ItemsSource = tabla.DefaultView;
            cbLocacion.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Pone los datos de un cliente en el formulario para poder editarlo
    private void CargarEnFormulario(int clienteId)
    {
        try
        {
            DataTable tabla = clienteBD.ObtenerCliente(clienteId);

            if (tabla.Rows.Count == 0)
            {
                MessageBox.Show("No se encontró el cliente.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRow fila = tabla.Rows[0];

            clienteActualId = clienteId;
            clienteActualActivo = Convert.ToBoolean(fila["estadoCliente"]);

            txtRTN.Text = fila["identificacionCliente"].ToString();
            txtEmpresa.Text = fila["nombreEmpresa"].ToString();
            txtNombre.Text = fila["nombreCliente"].ToString();
            txtApellido.Text = fila["apellidoCliente"].ToString();
            txtTelefono.Text = fila["telefonoCliente"].ToString();
            txtCorreo.Text = fila["correoCliente"].ToString();

            if (fila["locacion_id"] == DBNull.Value)
                cbLocacion.SelectedIndex = 0;
            else
                cbLocacion.SelectedValue = fila["locacion_id"];

            ActualizarBotones();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ===================== FORMULARIO =====================

    private void LimpiarFormulario()
    {
        clienteActualId = 0;
        clienteActualActivo = true;

        txtRTN.Text = "";
        txtEmpresa.Text = "";
        txtNombre.Text = "";
        txtApellido.Text = "";
        txtTelefono.Text = "";
        txtCorreo.Text = "";
        cbLocacion.SelectedIndex = 0;

        ActualizarBotones();
        txtRTN.Focus();
    }

    // Activa o desactiva los botones según si se está creando o editando un cliente
    private void ActualizarBotones()
    {
        if (clienteActualId == 0)
        {
            // Modo nuevo cliente
            btnCrear.IsEnabled = true;
            btnActualizar.IsEnabled = false;
            btnCambiarEstado.IsEnabled = false;
            btnCambiarEstado.Content = "Desactivar";
            txtEstado.Text = "—";
            txtModo.Text = "Nuevo cliente: llene los datos y presione Crear.";
        }
        else
        {
            // Modo editar cliente
            btnCrear.IsEnabled = false;
            btnActualizar.IsEnabled = true;
            btnCambiarEstado.IsEnabled = true;
            txtModo.Text = "Editando el cliente N.° " + clienteActualId + ". Presione Actualizar para guardar los cambios.";

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

    private bool DatosValidos()
    {
        if (txtRTN.Text.Trim() == "")
        {
            MessageBox.Show("El RTN / identidad es obligatorio.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtRTN.Focus();
            return false;
        }

        if (txtNombre.Text.Trim() == "")
        {
            MessageBox.Show("El nombre del contacto es obligatorio.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNombre.Focus();
            return false;
        }

        return true;
    }

    // Devuelve el id de la locación elegida, o NULL si eligió "(Sin locación)"
    private object LocacionSeleccionada()
    {
        if (cbLocacion.SelectedValue == null)
            return DBNull.Value;

        return cbLocacion.SelectedValue;
    }

    // ===================== BOTONES DEL FORMULARIO =====================

    // Nuevo: solo limpia el formulario para registrar otro cliente
    private void Nuevo_Click(object sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
    }

    // Crear: guarda en la base de datos el cliente que está en el formulario
    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        if (!DatosValidos())
            return;

        try
        {
            string mensaje;
            int nuevoId;

            bool guardado = clienteBD.InsertarCliente(
                txtRTN.Text.Trim(), txtEmpresa.Text.Trim(), txtNombre.Text.Trim(), txtApellido.Text.Trim(),
                txtTelefono.Text.Trim(), txtCorreo.Text.Trim(), LocacionSeleccionada(),
                out mensaje, out nuevoId);

            if (guardado)
            {
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarClientes();
                CargarEnFormulario(nuevoId);
            }
            else
            {
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Actualizar: guarda los cambios del cliente que se está editando
    private void Actualizar_Click(object sender, RoutedEventArgs e)
    {
        if (clienteActualId == 0)
            return;

        if (!DatosValidos())
            return;

        try
        {
            string mensaje;

            bool guardado = clienteBD.ActualizarCliente(
                clienteActualId, txtRTN.Text.Trim(), txtEmpresa.Text.Trim(), txtNombre.Text.Trim(),
                txtApellido.Text.Trim(), txtTelefono.Text.Trim(), txtCorreo.Text.Trim(), LocacionSeleccionada(),
                out mensaje);

            if (guardado)
            {
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarClientes();
            }
            else
            {
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Activar / Desactivar: reemplaza a "Eliminar". El cliente no se borra.
    private void CambiarEstado_Click(object sender, RoutedEventArgs e)
    {
        if (clienteActualId == 0)
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
            bool guardado = clienteBD.CambiarEstadoCliente(clienteActualId, nuevoEstado, out mensaje);

            if (guardado)
            {
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Information);
                clienteActualActivo = nuevoEstado;
                ActualizarBotones();
                CargarClientes();
            }
            else
            {
                MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void VerDetalle_Click(object sender, RoutedEventArgs e)
    {
        if (clienteActualId == 0)
        {
            MessageBox.Show("Seleccione un cliente de la tabla.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        AbrirDetalle(clienteActualId);
    }

    // ===================== TABLA =====================

    // Al hacer clic en un cliente de la tabla se abre su detalle
    private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgClientes.SelectedItem == null)
            return;

        DataRowView fila = (DataRowView)dgClientes.SelectedItem;
        int clienteId = Convert.ToInt32(fila["cliente_id"]);

        AbrirDetalle(clienteId);

        // Se quita la selección para que se pueda volver a hacer clic en el mismo cliente
        dgClientes.SelectedItem = null;
    }

    private void AbrirDetalle(int clienteId)
    {
        ClienteDetalle detalle = new ClienteDetalle(clienteId);
        detalle.Owner = this;
        detalle.ShowDialog();

        // Si en el detalle presionaron "Editar cliente", se carga en el formulario
        if (detalle.EditarCliente)
            CargarEnFormulario(clienteId);
    }

    // ===================== BÚSQUEDA Y FILTROS =====================

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

    // Enter en el buscador: busca en todos los campos
    private void Buscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            textoBusqueda = txtBuscar.Text.Trim();
            buscarPor = "";
            CargarClientes();
        }
    }

    private void Salir_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
