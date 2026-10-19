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
    private readonly ClienteDatos datos = new ClienteDatos();

    // 0 = formulario en modo "nuevo cliente"; mayor que 0 = id del cliente que se está editando
    private int clienteActualId = 0;
    private bool clienteActualActivo = true;

    // Última búsqueda, para refrescar la tabla después de guardar
    private string ultimoTexto = "";
    private string? ultimoBuscarPor = null;

    public Clientes()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        CargarLocaciones();
        LimpiarFormulario();
        CargarClientes("", null);
    }

    // ---------- Carga de datos ----------

    private void CargarLocaciones()
    {
        try
        {
            DataTable tabla = datos.ListarLocaciones();

            DataRow sinLocacion = tabla.NewRow();
            sinLocacion["locacion_id"] = DBNull.Value;
            sinLocacion["nombreLocacion"] = "(Sin locación)";
            tabla.Rows.InsertAt(sinLocacion, 0);

            cbLocacion.ItemsSource = tabla.DefaultView;
            cbLocacion.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private void CargarClientes(string texto, string? buscarPor)
    {
        try
        {
            ultimoTexto = texto;
            ultimoBuscarPor = buscarPor;
            dgClientes.ItemsSource = datos.Buscar(texto, buscarPor).DefaultView;
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private void CargarEnFormulario(int clienteId)
    {
        try
        {
            DataRow? fila = datos.Obtener(clienteId);
            if (fila == null)
            {
                MessageBox.Show("No se encontró el cliente.", "Clientes",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            clienteActualId = clienteId;
            clienteActualActivo = Convert.ToBoolean(fila["estadoCliente"]);

            txtRTN.Text = Texto(fila, "identificacionCliente");
            txtEmpresa.Text = Texto(fila, "nombreEmpresa");
            txtNombre.Text = Texto(fila, "nombreCliente");
            txtApellido.Text = Texto(fila, "apellidoCliente");
            txtTelefono.Text = Texto(fila, "telefonoCliente");
            txtCorreo.Text = Texto(fila, "correoCliente");
            cbLocacion.SelectedValue = fila["locacion_id"];
            if (cbLocacion.SelectedIndex < 0)
                cbLocacion.SelectedIndex = 0;

            ActualizarModo();
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    // ---------- Formulario ----------

    private void LimpiarFormulario()
    {
        clienteActualId = 0;
        clienteActualActivo = true;

        txtRTN.Clear();
        txtEmpresa.Clear();
        txtNombre.Clear();
        txtApellido.Clear();
        txtTelefono.Clear();
        txtCorreo.Clear();
        if (cbLocacion.Items.Count > 0)
            cbLocacion.SelectedIndex = 0;

        dgClientes.SelectedItem = null;
        ActualizarModo();
        txtRTN.Focus();
    }

    // Habilita los botones según si se está creando un cliente nuevo o editando uno existente
    private void ActualizarModo()
    {
        bool editando = clienteActualId > 0;

        btnCrear.IsEnabled = !editando;
        btnActualizar.IsEnabled = editando;
        btnCambiarEstado.IsEnabled = editando;
        btnCambiarEstado.Content = clienteActualActivo ? "Desactivar" : "Activar";

        txtEstado.Text = editando ? (clienteActualActivo ? "Activo" : "Inactivo") : "—";
        txtModo.Text = editando
            ? $"Editando el cliente N.° {clienteActualId}. Presione Actualizar para guardar los cambios."
            : "Nuevo cliente: llene los datos y presione Crear.";
    }

    private Cliente LeerFormulario()
    {
        return new Cliente
        {
            ClienteId = clienteActualId,
            Identificacion = txtRTN.Text.Trim(),
            Empresa = txtEmpresa.Text.Trim(),
            Nombre = txtNombre.Text.Trim(),
            Apellido = txtApellido.Text.Trim(),
            Telefono = txtTelefono.Text.Trim(),
            Correo = txtCorreo.Text.Trim(),
            LocacionId = cbLocacion.SelectedValue is int id ? id : null
        };
    }

    private bool ValidarFormulario()
    {
        if (string.IsNullOrWhiteSpace(txtRTN.Text))
        {
            MessageBox.Show("El RTN / identidad es obligatorio.", "Clientes",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            txtRTN.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("El nombre del contacto es obligatorio.", "Clientes",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNombre.Focus();
            return false;
        }

        return true;
    }

    // ---------- Botones del formulario ----------

    // Nuevo: solo limpia el formulario para empezar a registrar otro cliente
    private void Nuevo_Click(object sender, RoutedEventArgs e)
    {
        LimpiarFormulario();
    }

    // Crear: guarda en la base de datos lo que está en el formulario como cliente nuevo
    private void Crear_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarFormulario())
            return;

        try
        {
            var (ok, mensaje, nuevoId) = datos.Insertar(LeerFormulario());
            MostrarResultado(ok, mensaje);

            if (ok)
            {
                CargarClientes(ultimoTexto, ultimoBuscarPor);
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
        if (clienteActualId == 0 || !ValidarFormulario())
            return;

        try
        {
            var (ok, mensaje) = datos.Actualizar(LeerFormulario());
            MostrarResultado(ok, mensaje);

            if (ok)
                CargarClientes(ultimoTexto, ultimoBuscarPor);
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    // Reemplaza a "Eliminar": el cliente no se borra, solo se activa o desactiva
    private void CambiarEstado_Click(object sender, RoutedEventArgs e)
    {
        if (clienteActualId == 0)
            return;

        bool nuevoEstado = !clienteActualActivo;
        string accion = nuevoEstado ? "activar" : "desactivar";

        if (MessageBox.Show($"¿Desea {accion} el cliente {txtNombre.Text} ({txtRTN.Text})?", "Clientes",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            var (ok, mensaje) = datos.CambiarEstado(clienteActualId, nuevoEstado);
            MostrarResultado(ok, mensaje);

            if (ok)
            {
                clienteActualActivo = nuevoEstado;
                ActualizarModo();
                CargarClientes(ultimoTexto, ultimoBuscarPor);
            }
        }
        catch (Exception ex)
        {
            MostrarError(ex);
        }
    }

    private void VerDetalle_Click(object sender, RoutedEventArgs e)
    {
        int clienteId = clienteActualId;

        if (clienteId == 0 && dgClientes.SelectedItem is DataRowView seleccionado)
            clienteId = Convert.ToInt32(seleccionado["cliente_id"]);

        if (clienteId == 0)
        {
            MessageBox.Show("Seleccione un cliente de la tabla.", "Clientes",
                            MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        AbrirDetalle(clienteId);
    }

    // ---------- Tabla y búsqueda ----------

    private void FilaCliente_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRow fila && fila.Item is DataRowView cliente)
            AbrirDetalle(Convert.ToInt32(cliente["cliente_id"]));
    }

    private void AbrirDetalle(int clienteId)
    {
        ClienteDetalle detalle = new ClienteDetalle(clienteId) { Owner = this };
        detalle.ShowDialog();

        if (detalle.EditarSolicitado)
            CargarEnFormulario(clienteId);
    }

    private void PorRTN_Click(object sender, RoutedEventArgs e)
    {
        CargarClientes(txtBuscar.Text.Trim(), "RTN");
    }

    private void PorNombre_Click(object sender, RoutedEventArgs e)
    {
        CargarClientes(txtBuscar.Text.Trim(), "NOMBRE");
    }

    private void MostrarTodos_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Clear();
        CargarClientes("", null);
    }

    // Enter en el buscador: busca en todos los campos
    private void Buscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            CargarClientes(txtBuscar.Text.Trim(), null);
    }

    private void Salir_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // ---------- Utilidades ----------

    private static string Texto(DataRow fila, string columna)
    {
        return fila[columna] == DBNull.Value ? "" : fila[columna].ToString() ?? "";
    }

    private static void MostrarResultado(bool ok, string mensaje)
    {
        MessageBox.Show(mensaje, "Clientes", MessageBoxButton.OK,
                        ok ? MessageBoxImage.Information : MessageBoxImage.Warning);
    }

    private static void MostrarError(Exception ex)
    {
        MessageBox.Show(ex.Message, "Clientes", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
