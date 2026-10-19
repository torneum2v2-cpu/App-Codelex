using System.Data;
using System.Windows;
using AppCodelex.BDD;

namespace AppCodelex;

/// <summary>
/// Interaction logic for ClienteDetalle.xaml
/// </summary>
public partial class ClienteDetalle : Window
{
    private readonly ClienteDatos datos = new ClienteDatos();
    private readonly int clienteId;

    // La pantalla de Clientes lo revisa al cerrar para cargar el cliente en el formulario
    public bool EditarSolicitado { get; private set; }

    public ClienteDetalle(int clienteId)
    {
        InitializeComponent();
        this.clienteId = clienteId;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            DataRow? cliente = datos.Obtener(clienteId);
            if (cliente == null)
            {
                MessageBox.Show("No se encontró el cliente.", "Detalle del cliente",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            txtNumero.Text = clienteId.ToString();
            txtRTN.Text = Texto(cliente, "identificacionCliente");
            txtEmpresa.Text = Texto(cliente, "nombreEmpresa");
            txtContacto.Text = Texto(cliente, "nombreCliente");
            txtApellido.Text = Texto(cliente, "apellidoCliente");
            txtTelefono.Text = Texto(cliente, "telefonoCliente");
            txtCorreo.Text = Texto(cliente, "correoCliente");
            txtEstado.Text = Texto(cliente, "estado");

            Llenar(lstLocaciones, datos.LocacionesPorCliente(clienteId),
                   "nombreLocacion", "tipoLocacion", "direccion");
            Llenar(lstOrdenes, datos.OrdenesPorCliente(clienteId),
                   "orden_id", "servicio", "fecha", "estado");
            Llenar(lstFacturas, datos.FacturasPorCliente(clienteId),
                   "factura_id", "fecha", "estado_pago", "monto");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Detalle del cliente",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Cada fila se muestra como "valor · valor · valor", en el mismo orden del encabezado
    private static void Llenar(System.Windows.Controls.ListBox lista, DataTable tabla, params string[] columnas)
    {
        lista.Items.Clear();

        foreach (DataRow fila in tabla.Rows)
            lista.Items.Add(string.Join(" · ", columnas.Select(c => Texto(fila, c))));
    }

    private static string Texto(DataRow fila, string columna)
    {
        object valor = fila[columna];

        return valor switch
        {
            DBNull => "—",
            DateTime fecha => fecha.ToString("dd/MM/yyyy"),
            decimal monto => "L " + monto.ToString("N2"),
            _ => valor.ToString() ?? ""
        };
    }

    private void NuevaOrden_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("El módulo de Órdenes todavía no está disponible.", "Detalle del cliente",
                        MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Editar_Click(object sender, RoutedEventArgs e)
    {
        EditarSolicitado = true;
        Close();
    }

    private void Cerrar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
