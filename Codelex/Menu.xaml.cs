using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Codelex.BDD;
using Codelex.Clases_BDD;
using Microsoft.Win32;
using Codelex.Facturas_y_Cotizaciones;
using Codelex.Órdenes;

namespace Codelex
{
    public partial class Menu : Window
    {
        private const string IconoLocaciones = "M21,10 C21,17 12,23 12,23 C12,23 3,17 3,10 A9,9 0 0 1 21,10 Z M9,10 A3,3 0 1 0 15,10 A3,3 0 1 0 9,10";
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

        // Nivel mínimo de rol para ver cada sección.
        private static readonly Dictionary<string, NivelAcceso> NivelPorSeccion = new Dictionary<string, NivelAcceso>
        {
            { "miperfil", NivelAcceso.Basico },

            { "inicio", NivelAcceso.Medio },
            { "ordenes", NivelAcceso.Medio },
            { "vehiculos", NivelAcceso.Medio },
            { "clientes", NivelAcceso.Medio },
            { "empleados", NivelAcceso.Medio },
            { "inventario", NivelAcceso.Medio },
            { "usuarios", NivelAcceso.Medio },

            { "facturacion", NivelAcceso.Avanzado },
            { "reportes", NivelAcceso.Avanzado }
        };

        // Nivel mínimo de rol para abrir cada módulo.
        private static readonly Dictionary<string, NivelAcceso> NivelPorDestino = new Dictionary<string, NivelAcceso>
        {
            { "MisDatos", NivelAcceso.Basico },
            { "MisOrdenes", NivelAcceso.Basico },

            { "IngresarOrden", NivelAcceso.Medio },
            { "GestionOrdenes", NivelAcceso.Medio },
            { "Vehiculos", NivelAcceso.Medio },
            { "GestionVehiculos", NivelAcceso.Medio },
            { "Clientes", NivelAcceso.Medio },
            { "Locaciones", NivelAcceso.Medio },
            { "Empleados", NivelAcceso.Medio },
            { "Inventario", NivelAcceso.Medio },
            { "Usuarios", NivelAcceso.Medio },

            { "Cotizaciones", NivelAcceso.Avanzado },
            { "Facturas", NivelAcceso.Avanzado },
            { "Reportes", NivelAcceso.Avanzado }
        };

        private readonly FotoPerfilBD fotoBD = new FotoPerfilBD();
        private DateTime ultimoCierrePerfil = DateTime.MinValue;
        private string _seccionActual = "inicio";

        public Menu()
        {
            InitializeComponent();

            string fecha = DateTime.Now.ToString("dddd, d 'de' MMMM 'de' yyyy", new CultureInfo("es"));
            txtFecha.Text = char.ToUpper(fecha[0]) + fecha.Substring(1);

            txtIniciales.Text = Sesion.Iniciales;
            txtInicialesGrande.Text = Sesion.Iniciales;
            txtPopupNombre.Text = Sesion.NombreCompleto;
            txtPopupRol.Text = Sesion.Rol == "" ? "Sin rol asignado" : Sesion.Rol;

            // Sirve para que un clic en el avatar con el panel abierto lo cierre en vez de reabrirlo.
            popupPerfil.Closed += (s, e) => ultimoCierrePerfil = DateTime.Now;

            AplicarRol();
            MostrarSeccion(InicioDelRol());

            Loaded += async (s, e) => await CargarFoto();
        }

        // ---------- Roles ----------

        private static string InicioDelRol()
        {
            return Sesion.Nivel == NivelAcceso.Basico ? "miperfil" : "inicio";
        }

        private static bool PuedeSeccion(string clave)
        {
            return NivelPorSeccion.TryGetValue(clave, out NivelAcceso minimo) && Sesion.Permite(minimo);
        }

        private static bool PuedeDestino(string destino)
        {
            return NivelPorDestino.TryGetValue(destino, out NivelAcceso minimo) && Sesion.Permite(minimo);
        }

        private static bool PuedeAbrir(string seccionActual, string destino)
        {
            return seccionActual == "inicio" ? PuedeSeccion(destino) : PuedeDestino(destino);
        }

        // Oculta del menú lateral las secciones que el rol no puede usar.
        private void AplicarRol()
        {
            foreach (RadioButton boton in panelNav.Children)
                boton.Visibility = PuedeSeccion("" + boton.Tag) ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---------- Ventana ----------

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

        // ---------- Perfil y foto ----------

        private void Perfil_Click(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - ultimoCierrePerfil).TotalMilliseconds < 250)
                return;

            popupPerfil.IsOpen = true;
        }

        private void MiPerfil_Click(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;
            MostrarSeccion("miperfil");
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;
            Sesion.Cerrar();

            if (CuentasRecordadas.Listar().Count > 0)
                new SeleccionarCuenta().Show();
            else
                new MainWindow().Show();

            Close();
        }

        private async Task CargarFoto()
        {
            if (Sesion.UsuarioId == 0)
                return;

            try
            {
                int usuarioId = Sesion.UsuarioId;
                byte[]? datos = await Task.Run(() => fotoBD.Obtener(usuarioId));
                PintarAvatar(datos == null ? null : FotoPerfil.Cargar(datos));
            }
            catch
            {
                // Si no se puede leer la foto, se quedan las iniciales.
            }
        }

        private async void CambiarFoto_Click(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;

            OpenFileDialog dialogo = new OpenFileDialog();
            dialogo.Title = "Elige tu foto de perfil";
            dialogo.Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialogo.ShowDialog(this) != true)
                return;

            try
            {
                byte[] foto = FotoPerfil.Preparar(dialogo.FileName);
                int usuarioId = Sesion.UsuarioId;

                await Task.Run(() => fotoBD.Guardar(usuarioId, foto));
                PintarAvatar(FotoPerfil.Cargar(foto));
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Foto de perfil", "No se pudo cambiar la foto. " + ex.Message, false);
            }
        }

        private async void QuitarFoto_Click(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;

            try
            {
                int usuarioId = Sesion.UsuarioId;
                await Task.Run(() => fotoBD.Guardar(usuarioId, null));
                PintarAvatar(null);
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Foto de perfil", "No se pudo quitar la foto. " + ex.Message, false);
            }
        }

        // foto = null deja las iniciales con el degradado.
        private void PintarAvatar(ImageSource? foto)
        {
            if (foto == null)
            {
                Brush degradado = (Brush)FindResource("ButtonGradient");
                avatarFondo.Background = degradado;
                avatarGrande.Background = degradado;
                txtIniciales.Visibility = Visibility.Visible;
                txtInicialesGrande.Visibility = Visibility.Visible;
                btnQuitarFoto.Visibility = Visibility.Collapsed;
                return;
            }

            avatarFondo.Background = new ImageBrush(foto) { Stretch = Stretch.UniformToFill };
            avatarGrande.Background = new ImageBrush(foto) { Stretch = Stretch.UniformToFill };
            txtIniciales.Visibility = Visibility.Collapsed;
            txtInicialesGrande.Visibility = Visibility.Collapsed;
            btnQuitarFoto.Visibility = Visibility.Visible;
        }

        // ---------- Navegación ----------

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

        // Marca el botón de la barra lateral; si la sección no está en la barra (Mi perfil), no marca ninguno.
        private void MarcarBoton(string clave)
        {
            foreach (RadioButton boton in panelNav.Children)
                boton.IsChecked = "" + boton.Tag == clave;
        }

        private void Encabezado(string titulo, string subtitulo)
        {
            txtTitulo.Text = titulo;
            txtSubtitulo.Text = subtitulo;
        }

        private void MostrarSeccion(string clave)
        {
            if (Sesion.Nivel == NivelAcceso.SinRol)
            {
                Encabezado("Sin acceso", "Tu usuario no tiene un rol asignado. Contacta al administrador.");
                listaOpciones.ItemsSource = new List<OpcionMenu>();
                return;
            }

            if (!PuedeSeccion(clave))
                clave = InicioDelRol();

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
                opciones.Add(new OpcionMenu("Clientes", "Administra tus clientes y sus locaciones.", IconoClientes, "clientes"));
                opciones.Add(new OpcionMenu("Empleados", "Gestiona el personal y sus accesos al sistema.", IconoEmpleados, "empleados"));
                opciones.Add(new OpcionMenu("Inventario", "Controla productos, existencias y movimientos.", IconoInventario, "inventario"));
                opciones.Add(new OpcionMenu("Usuarios", "Administra las cuentas de acceso al sistema.", IconoUsuarios, "usuarios"));
            }
            else if (clave == "miperfil")
            {
                Encabezado("Mi perfil", "Consulta tu información y tus órdenes asignadas.");
                opciones.Add(new OpcionMenu("Mis datos", "Consulta tu perfil de empleado, tu asistencia y tus órdenes.", IconoEmpleados, "MisDatos"));
                opciones.Add(new OpcionMenu("Mis órdenes", "Revisa el detalle de las órdenes en las que estás asignado.", IconoOrdenes, "MisOrdenes"));
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
                opciones.Add(new OpcionMenu("Locaciones", "Registra las locaciones donde se presta el servicio.", IconoLocaciones, "Locaciones"));
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

            // Solo se muestran las opciones que el rol puede usar.
            opciones.RemoveAll(o => !PuedeAbrir(clave, o.Destino));

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
            // Segunda barrera: aunque alguien llegue aquí, se vuelve a comprobar el rol.
            if (!PuedeDestino(destino))
            {
                MensajeCodelex.Mostrar(this, "Acceso denegado", "Tu rol no tiene permiso para abrir este módulo.", false);
                return;
            }

            if (destino == "MisDatos")
                AbrirModulo(new EmpleadoReporte(Sesion.EmpleadoId));
            else if (destino == "MisOrdenes")
                AbrirModulo(new MisOrdenes());
            else if (destino == "Clientes")
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
            else if (destino == "Locaciones")
                AbrirModulo(new IngresarLocacion());
            else if (destino == "Reportes")
                AbrirModulo(new Reportes());
            else if (destino == "Cotizaciones")
                AbrirModulo(new Cotizacion());
            else if (destino == "Facturas")
                AbrirModulo(new Factura());
            else if (destino == "GestionOrdenes")
            AbrirModulo(new GestionOrdenes());
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