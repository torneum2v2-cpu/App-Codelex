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

    
    int clienteActualId = 0;
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
        CargarLocaciones();
        LimpiarFormulario();
        CargarClientes();
    }

    

    private void CargarLocaciones()
    {
        try
        {
            DataTable tabla = clienteBD.ListarLocaciones();

            
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

    
    private void ActualizarBotones()
    {
        if (clienteActualId == 0)
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
        string rtn = txtRTN.Text.Trim();

        if (rtn == "")
        {
            MessageBox.Show("El RTN / identidad es obligatorio.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtRTN.Focus();
            return false;
        }

        foreach (char c in rtn)
        {
            if (!char.IsDigit(c))
            {
                MessageBox.Show("El RTN / identidad solo puede tener números, sin guiones ni espacios.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtRTN.Focus();
                return false;
            }
        }

        if (rtn.Length < 13 || rtn.Length > 14)
        {
            MessageBox.Show("El RTN / identidad debe tener 13 o 14 dígitos. Tiene " + rtn.Length + ".", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtRTN.Focus();
            return false;
        }

        if (txtEmpresa.Text.Trim() == "")
        {
            MessageBox.Show("La empresa es obligatoria.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEmpresa.Focus();
            return false;
        }

        if (txtNombre.Text.Trim() == "")
        {
            MessageBox.Show("El nombre del contacto es obligatorio.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNombre.Focus();
            return false;
        }

        if (txtApellido.Text.Trim() == "")
        {
            MessageBox.Show("El apellido del contacto es obligatorio.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtApellido.Focus();
            return false;
        }

        if (txtTelefono.Text.Trim() == "")
        {
            MessageBox.Show("El teléfono es obligatorio.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtTelefono.Focus();
            return false;
        }

        string correo = txtCorreo.Text.Trim();

        if (correo == "")
        {
            MessageBox.Show("El correo es obligatorio.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCorreo.Focus();
            return false;
        }

        if (!correo.Contains("@") || !correo.Contains("."))
        {
            MessageBox.Show("El correo no es válido. Ejemplo: nombre@empresa.com", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCorreo.Focus();
            return false;
        }

        return true;
    }

    private void SoloNumeros(object sender, TextCompositionEventArgs e)
    {
        foreach (char c in e.Text)
        {
            if (!char.IsDigit(c))
                e.Handled = true;
        }
    }

    
    private object LocacionSeleccionada()
    {
        if (cbLocacion.SelectedValue == null)
            return DBNull.Value;

        return cbLocacion.SelectedValue;
    }

    
    private void Nuevo_Click(object sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
    }

    
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

    
    private void dgClientes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgClientes.SelectedItem == null)
            return;

        DataRowView fila = (DataRowView)dgClientes.SelectedItem;
        int clienteId = Convert.ToInt32(fila["cliente_id"]);

        AbrirDetalle(clienteId);

        
        dgClientes.SelectedItem = null;
    }

    private void AbrirDetalle(int clienteId)
    {
        ClienteDetalle detalle = new ClienteDetalle(clienteId);
        detalle.Owner = this;
        detalle.ShowDialog();

        
        if (detalle.EditarCliente)
            CargarEnFormulario(clienteId);
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

    private void Salir_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
