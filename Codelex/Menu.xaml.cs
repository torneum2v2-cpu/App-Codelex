using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Codelex
{
    public partial class Menu : Window
    {
        private const string IconoIngresarOrden = "M14,2 H6 A2,2 0 0 0 4,4 V20 A2,2 0 0 0 6,22 H18 A2,2 0 0 0 20,20 V8 Z M14,2 V8 H20 M12,18 V12 M9,15 H15";
        private const string IconoVehiculos = "M1,3 H16 V16 H1 Z M16,8 H20 L23,11 V16 H16 Z M3,18.5 A2.5,2.5 0 1 0 8,18.5 A2.5,2.5 0 1 0 3,18.5 M16,18.5 A2.5,2.5 0 1 0 21,18.5 A2.5,2.5 0 1 0 16,18.5";
        private const string IconoGestionOrdenes = "M9,4 H7 A2,2 0 0 0 5,6 V20 A2,2 0 0 0 7,22 H17 A2,2 0 0 0 19,20 V6 A2,2 0 0 0 17,4 H15 M9,2 H15 V6 H9 Z M9,14 L11,16 L15,12";
        private const string IconoFacturacion = "M14,2 H6 A2,2 0 0 0 4,4 V20 A2,2 0 0 0 6,22 H18 A2,2 0 0 0 20,20 V8 Z M14,2 V8 H20 M16,13 H8 M16,17 H8 M10,9 H8";
        private const string IconoCotizacion = "M12,1 V23 M17,5 H9.5 A3.5,3.5 0 0 0 9.5,12 H14.5 A3.5,3.5 0 0 1 14.5,19 H6";
        private const string IconoReportes = "M18,20 V10 M12,20 V4 M6,20 V14";
        private const string IconoOrdenes = "M9,4 H7 A2,2 0 0 0 5,6 V20 A2,2 0 0 0 7,22 H17 A2,2 0 0 0 19,20 V6 A2,2 0 0 0 17,4 H15 M9,2 H15 V6 H9 Z M9,12 H15 M9,16 H13";
        private const string IconoClientes = "M5,7 A4,4 0 1 0 13,7 A4,4 0 1 0 5,7 M2,21 V19 A4,4 0 0 1 6,15 H12 A4,4 0 0 1 16,19 V21 M16,3.1 A4,4 0 0 1 16,10.9 M22,21 V19 A4,4 0 0 0 19,15.1";
        private const string IconoEmpleados = "M4,4 H20 A2,2 0 0 1 22,6 V18 A2,2 0 0 1 20,20 H4 A2,2 0 0 1 2,18 V6 A2,2 0 0 1 4,4 Z M7,9 A2,2 0 1 0 11,9 A2,2 0 1 0 7,9 M5,16 A4,4 0 0 1 13,16 M15,9 H19 M15,13 H19";
        private const string IconoInventario = "M3,7 L12,3 L21,7 L21,17 L12,21 L3,17 Z M3,7 L12,11 L21,7 M12,11 L12,21";
        private const string IconoUsuarios = "M20,21 V19 A4,4 0 0 0 16,15 H8 A4,4 0 0 0 4,19 V21 M8,7 A4,4 0 1 0 16,7 A4,4 0 1 0 8,7";

        private const string IconoGestionVehiculos = "M8,6 H21 M8,12 H21 M8,18 H21 M3,6 H3.01 M3,12 H3.01 M3,18 H3.01";
        private string _seccionActual = "inicio";

        public Menu()
        {
            InitializeComponent();

            string fecha = DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es"));
            txtFecha.Text = char.ToUpper(fecha[0]) + fecha.Substring(1);

            MostrarSeccion("inicio");
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            RadioButton boton = (RadioButton)sender;
            MostrarSeccion("" + boton.Tag);
        }

        private void Opcion_Click(object sender, RoutedEventArgs e)
        {
            Button boton = (Button)sender;
            string destino = "" + boton.Tag;

            if (_seccionActual == "inicio")
                MostrarSeccion(destino);
            else
                AbrirDestino(destino);
        }

        private void MarcarBoton(string clave)
        {
            foreach (RadioButton boton in panelNav.Children)
            {
                if ("" + boton.Tag == clave)
                    boton.IsChecked = true;
            }
        }

        private void Encabezado(string titulo, string subtitulo)
        {
            txtTitulo.Text = titulo;
            txtSubtitulo.Text = subtitulo;
        }

        private void MostrarSeccion(string clave)
        {
            _seccionActual = clave;
            MarcarBoton(clave);

            List<OpcionMenu> opciones = new List<OpcionMenu>();

            if (clave == "inicio")
            {
                Encabezado("Panel principal", "Selecciona un módulo para comenzar.");
                opciones.Add(new OpcionMenu("Cotizaciones y facturación", "Genera cotizaciones y facturas para tus clientes.", IconoFacturacion, "facturacion"));
                opciones.Add(new OpcionMenu("Reportes", "Consulta reportes de órdenes, asistencia y empleados.", IconoReportes, "reportes"));
                opciones.Add(new OpcionMenu("Órdenes", "Ingresa y gestiona las órdenes de tus clientes.", IconoOrdenes, "ordenes"));
                opciones.Add(new OpcionMenu("Vehículos", "Registra y gestiona los vehículos de la empresa.", IconoVehiculos, "vehiculos"));
                opciones.Add(new OpcionMenu("Clientes", "Administra tu cartera de clientes y sus datos.", IconoClientes, "clientes"));
                opciones.Add(new OpcionMenu("Empleados", "Gestiona el personal y sus accesos al sistema.", IconoEmpleados, "empleados"));
                opciones.Add(new OpcionMenu("Inventario", "Controla productos, existencias y movimientos.", IconoInventario, "inventario"));
                opciones.Add(new OpcionMenu("Usuarios", "Administra las cuentas de acceso al sistema.", IconoUsuarios, "usuarios"));
            }
            else if (clave == "facturacion")
            {
                Encabezado("Cotizaciones y facturación", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Cotizaciones", "Crea y consulta las cotizaciones de servicios.", IconoCotizacion, "Cotizaciones"));
                opciones.Add(new OpcionMenu("Facturas", "Emite y consulta las facturas de las órdenes.", IconoFacturacion, "Facturas"));
            }
            else if (clave == "reportes")
            {
                Encabezado("Reportes", "Consulta la información del negocio.");
                opciones.Add(new OpcionMenu("Consulta de reportes", "Reportes de órdenes, asistencia y empleados.", IconoReportes, "Reportes"));
            }
            else if (clave == "ordenes")
            {
                Encabezado("Órdenes", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Ingresar orden", "Registra una nueva orden de servicio para un cliente.", IconoIngresarOrden, "IngresarOrden"));
                opciones.Add(new OpcionMenu("Gestionar órdenes", "Consulta, edita y da seguimiento al estado de las órdenes.", IconoGestionOrdenes, "GestionOrdenes"));
            }
            else if (clave == "vehiculos")
            {
                Encabezado("Vehículos", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Registrar vehículos", "Agrega nuevos vehículos a la flota de la empresa.", IconoVehiculos, "Vehiculos"));
                opciones.Add(new OpcionMenu("Gestionar vehículos", "Consulta, edita y controla los vehículos registrados.", IconoGestionVehiculos, "GestionVehiculos"));
            }
            else if (clave == "clientes")
            {
                Encabezado("Clientes", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Clientes", "Consulta y administra los clientes y su detalle.", IconoClientes, "Clientes"));
            }
            else if (clave == "empleados")
            {
                Encabezado("Empleados", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Registro de empleados", "Registra empleados y consulta su asistencia.", IconoEmpleados, "Empleados"));
            }
            else if (clave == "inventario")
            {
                Encabezado("Inventario", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Consulta de inventario", "Consulta existencias y agrega productos y equipos.", IconoInventario, "Inventario"));
            }
            else if (clave == "usuarios")
            {
                Encabezado("Usuarios", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Gestión de usuarios", "Administra las cuentas de acceso al sistema.", IconoUsuarios, "Usuarios"));
            }

            listaOpciones.ItemsSource = opciones;
        }

        private void ModuloCerrado(object sender, EventArgs e)
        {
            Show();
        }

        private void AbrirModulo(Window ventana)
        {
            ventana.Closed += ModuloCerrado;
            Hide();
            ventana.Show();
        }

        private void AbrirDestino(string destino)
        {
            if (destino == "Clientes")
                AbrirModulo(new Clientes());
            else if (destino == "Vehiculos")
                AbrirModulo(new RegistrarVehiculo());
            else if (destino == "GestionVehiculos")
                AbrirModulo(new GestionVehiculos());
            else if (destino == "IngresarOrden")
                AbrirModulo(new IngresarOrden());
            else if (destino == "Usuarios")
                AbrirModulo(new Usuarios());
            else if (destino == "Inventario")
                AbrirModulo(new Inventario());
            else if (destino == "Empleados")
                AbrirModulo(new Empleados());
            else
                MensajeCodelex.Mostrar(this, "Próximamente", "Destino recibido: [" + destino + "]", true);
        }
    }

    public class OpcionMenu
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Icono { get; set; }
        public string Destino { get; set; }

        public OpcionMenu(string titulo, string descripcion, string icono, string destino)
        {
            Titulo = titulo;
            Descripcion = descripcion;
            Icono = icono;
            Destino = destino;
        }
    }
}
