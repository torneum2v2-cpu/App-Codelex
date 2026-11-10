using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Codelex.AccesoDatos;
using Codelex.AccesoDatos;
using Microsoft.Win32;
using Codelex.Ordenes;

namespace Codelex
{
    public partial class Menu : Window
    {
        private const string ICONO_LOCACIONES = "M21,10 C21,17 12,23 12,23 C12,23 3,17 3,10 A9,9 0 0 1 21,10 Z M9,10 A3,3 0 1 0 15,10 A3,3 0 1 0 9,10";
        private const string ICONO_INGRESAR_ORDEN = "M14,2 H6 A2,2 0 0 0 4,4 V20 A2,2 0 0 0 6,22 H18 A2,2 0 0 0 20,20 V8 Z M14,2 V8 H20 M12,18 V12 M9,15 H15";
        private const string ICONO_VEHICULOS = "M1,3 H16 V16 H1 Z M16,8 H20 L23,11 V16 H16 Z M3,18.5 A2.5,2.5 0 1 0 8,18.5 A2.5,2.5 0 1 0 3,18.5 M16,18.5 A2.5,2.5 0 1 0 21,18.5 A2.5,2.5 0 1 0 16,18.5";
        private const string ICONO_GESTION_ORDENES = "M9,4 H7 A2,2 0 0 0 5,6 V20 A2,2 0 0 0 7,22 H17 A2,2 0 0 0 19,20 V6 A2,2 0 0 0 17,4 H15 M9,2 H15 V6 H9 Z M9,14 L11,16 L15,12";
        private const string ICONO_REPORTES = "M18,20 V10 M12,20 V4 M6,20 V14";
        private const string ICONO_ORDENES = "M9,4 H7 A2,2 0 0 0 5,6 V20 A2,2 0 0 0 7,22 H17 A2,2 0 0 0 19,20 V6 A2,2 0 0 0 17,4 H15 M9,2 H15 V6 H9 Z M9,12 H15 M9,16 H13";
        private const string ICONO_CLIENTES = "M5,7 A4,4 0 1 0 13,7 A4,4 0 1 0 5,7 M2,21 V19 A4,4 0 0 1 6,15 H12 A4,4 0 0 1 16,19 V21 M16,3.1 A4,4 0 0 1 16,10.9 M22,21 V19 A4,4 0 0 0 19,15.1";
        private const string ICONO_EMPLEADOS = "M4,4 H20 A2,2 0 0 1 22,6 V18 A2,2 0 0 1 20,20 H4 A2,2 0 0 1 2,18 V6 A2,2 0 0 1 4,4 Z M7,9 A2,2 0 1 0 11,9 A2,2 0 1 0 7,9 M5,16 A4,4 0 0 1 13,16 M15,9 H19 M15,13 H19";
        private const string ICONO_INVENTARIO = "M3,7 L12,3 L21,7 L21,17 L12,21 L3,17 Z M3,7 L12,11 L21,7 M12,11 L12,21";
        private const string ICONO_USUARIOS = "M20,21 V19 A4,4 0 0 0 16,15 H8 A4,4 0 0 0 4,19 V21 M8,7 A4,4 0 1 0 16,7 A4,4 0 1 0 8,7";

        private const string ICONO_GESTION_VEHICULOS = "M8,6 H21 M8,12 H21 M8,18 H21 M3,6 H3.01 M3,12 H3.01 M3,18 H3.01";

        // Nivel mínimo de rol para ver cada sección.
        private static readonly Dictionary<string, NivelAcceso> NIVEL_POR_SECCION = new Dictionary<string, NivelAcceso>
        {
            { "miperfil", NivelAcceso.Basico },

            { "inicio", NivelAcceso.Medio },
            { "ordenes", NivelAcceso.Medio },
            { "vehiculos", NivelAcceso.Medio },
            { "clientes", NivelAcceso.Medio },
            { "empleados", NivelAcceso.Medio },
            { "inventario", NivelAcceso.Medio },
            { "usuarios", NivelAcceso.Medio },

            { "reportes", NivelAcceso.Avanzado }
        };

        // Nivel mínimo de rol para abrir cada módulo.
        private static readonly Dictionary<string, NivelAcceso> NIVEL_POR_DESTINO = new Dictionary<string, NivelAcceso>
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

            { "Reportes", NivelAcceso.Avanzado }
        };

        private readonly FotoPerfilBD fotoBD = new FotoPerfilBD();
        private DateTime ultimoCierrePerfil = DateTime.MinValue;
        private string seccionActual = "inicio";

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

            aplicarRol();
            mostrarSeccion(inicioDelRol());

            Loaded += async (s, e) => await cargarFoto();
        }

        // ---------- Roles ----------

        private static string inicioDelRol()
        {
            return Sesion.Nivel == NivelAcceso.Basico ? "miperfil" : "inicio";
        }

        private static bool puedeSeccion(string clave)
        {
            return NIVEL_POR_SECCION.TryGetValue(clave, out NivelAcceso minimo) && Sesion.permite(minimo);
        }

        private static bool puedeDestino(string destino)
        {
            return NIVEL_POR_DESTINO.TryGetValue(destino, out NivelAcceso minimo) && Sesion.permite(minimo);
        }

        private static bool puedeAbrir(string seccionActual, string destino)
        {
            return seccionActual == "inicio" ? puedeSeccion(destino) : puedeDestino(destino);
        }

        // Oculta del menú lateral las secciones que el rol no puede usar.
        private void aplicarRol()
        {
            foreach (RadioButton boton in panelNav.Children)
                boton.Visibility = puedeSeccion("" + boton.Tag) ? Visibility.Visible : Visibility.Collapsed;
        }

        // ---------- Ventana ----------

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void minimizarClic(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void cerrarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // ---------- Perfil y foto ----------

        private void perfilClic(object sender, RoutedEventArgs e)
        {
            if ((DateTime.Now - ultimoCierrePerfil).TotalMilliseconds < 250)
                return;

            popupPerfil.IsOpen = true;
        }

        private void miPerfilClic(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;
            mostrarSeccion("miperfil");
        }

        private void cerrarSesionClic(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;
            Sesion.cerrar();

            if (CuentasRecordadas.listar().Count > 0)
                new SeleccionarCuenta().Show();
            else
                new VentanaPrincipal().Show();

            Close();
        }

        private async Task cargarFoto()
        {
            if (Sesion.UsuarioId == 0)
                return;

            try
            {
                int usuarioId = Sesion.UsuarioId;
                byte[]? datos = await Task.Run(() => fotoBD.obtener(usuarioId));
                pintarAvatar(datos == null ? null : FotoPerfil.cargar(datos));
            }
            catch
            {
                // Si no se puede leer la foto, se quedan las iniciales.
            }
        }

        private async void cambiarFotoClic(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;

            OpenFileDialog dialogo = new OpenFileDialog();
            dialogo.Title = "Elige tu foto de perfil";
            dialogo.Filter = "Imágenes (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";

            if (dialogo.ShowDialog(this) != true)
                return;

            try
            {
                byte[] foto = FotoPerfil.preparar(dialogo.FileName);
                int usuarioId = Sesion.UsuarioId;

                await Task.Run(() => fotoBD.guardar(usuarioId, foto));
                pintarAvatar(FotoPerfil.cargar(foto));
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Foto de perfil", "No se pudo cambiar la foto. " + ex.Message, false);
            }
        }

        private async void quitarFotoClic(object sender, RoutedEventArgs e)
        {
            popupPerfil.IsOpen = false;

            try
            {
                int usuarioId = Sesion.UsuarioId;
                await Task.Run(() => fotoBD.guardar(usuarioId, null));
                pintarAvatar(null);
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Foto de perfil", "No se pudo quitar la foto. " + ex.Message, false);
            }
        }

        // foto = null deja las iniciales con el degradado.
        private void pintarAvatar(ImageSource? foto)
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

        private void navClic(object sender, RoutedEventArgs e)
        {
            RadioButton boton = (RadioButton)sender;
            mostrarSeccion("" + boton.Tag);
        }

        private void opcionClic(object sender, RoutedEventArgs e)
        {
            Button boton = (Button)sender;
            string destino = "" + boton.Tag;

            if (seccionActual == "inicio")
                mostrarSeccion(destino);
            else
                abrirDestino(destino);
        }

        // Marca el botón de la barra lateral; si la sección no está en la barra (Mi perfil), no marca ninguno.
        private void marcarBoton(string clave)
        {
            foreach (RadioButton boton in panelNav.Children)
                boton.IsChecked = "" + boton.Tag == clave;
        }

        private void encabezado(string titulo, string subtitulo)
        {
            txtTitulo.Text = titulo;
            txtSubtitulo.Text = subtitulo;
        }

        private void mostrarSeccion(string clave)
        {
            if (Sesion.Nivel == NivelAcceso.SinRol)
            {
                encabezado("Sin acceso", "Tu usuario no tiene un rol asignado. Contacta al administrador.");
                listaOpciones.ItemsSource = new List<OpcionMenu>();
                return;
            }

            if (!puedeSeccion(clave))
                clave = inicioDelRol();

            seccionActual = clave;
            marcarBoton(clave);

            List<OpcionMenu> opciones = new List<OpcionMenu>();

            if (clave == "inicio")
            {
                encabezado("Panel principal", "Selecciona un módulo para comenzar.");
                opciones.Add(new OpcionMenu("Reportes", "Consulta reportes de órdenes, asistencia y empleados.", ICONO_REPORTES, "reportes"));
                opciones.Add(new OpcionMenu("Órdenes", "Ingresa y gestiona las órdenes de tus clientes.", ICONO_ORDENES, "ordenes"));
                opciones.Add(new OpcionMenu("Vehículos", "Registra y gestiona los vehículos de la empresa.", ICONO_VEHICULOS, "vehiculos"));
                opciones.Add(new OpcionMenu("Clientes", "Administra tus clientes y sus locaciones.", ICONO_CLIENTES, "clientes"));
                opciones.Add(new OpcionMenu("Empleados", "Gestiona el personal y sus accesos al sistema.", ICONO_EMPLEADOS, "empleados"));
                opciones.Add(new OpcionMenu("Inventario", "Controla productos, existencias y movimientos.", ICONO_INVENTARIO, "inventario"));
                opciones.Add(new OpcionMenu("Usuarios", "Administra las cuentas de acceso al sistema.", ICONO_USUARIOS, "usuarios"));
            }
            else if (clave == "miperfil")
            {
                encabezado("Mi perfil", "Consulta tu información y tus órdenes asignadas.");
                opciones.Add(new OpcionMenu("Mis datos", "Consulta tu perfil de empleado, tu asistencia y tus órdenes.", ICONO_EMPLEADOS, "MisDatos"));
                opciones.Add(new OpcionMenu("Mis órdenes", "Revisa el detalle de las órdenes en las que estás asignado.", ICONO_ORDENES, "MisOrdenes"));
            }
            else if (clave == "reportes")
            {
                encabezado("Reportes", "Consulta la información del negocio.");
                opciones.Add(new OpcionMenu("Consulta de reportes", "Reportes de órdenes, asistencia y empleados.", ICONO_REPORTES, "Reportes"));
            }
            else if (clave == "ordenes")
            {
                encabezado("Órdenes", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Ingresar orden", "Registra una nueva orden de servicio para un cliente.", ICONO_INGRESAR_ORDEN, "IngresarOrden"));
                opciones.Add(new OpcionMenu("Gestionar órdenes", "Consulta, edita y da seguimiento al estado de las órdenes.", ICONO_GESTION_ORDENES, "GestionOrdenes"));
            }
            else if (clave == "vehiculos")
            {
                encabezado("Vehículos", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Registrar vehículos", "Agrega nuevos vehículos a la flota de la empresa.", ICONO_VEHICULOS, "Vehiculos"));
                opciones.Add(new OpcionMenu("Gestionar vehículos", "Consulta, edita y controla los vehículos registrados.", ICONO_GESTION_VEHICULOS, "GestionVehiculos"));
            }
            else if (clave == "clientes")
            {
                encabezado("Clientes", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Clientes", "Consulta y administra los clientes y su detalle.", ICONO_CLIENTES, "Clientes"));
                opciones.Add(new OpcionMenu("Locaciones", "Registra las locaciones donde se presta el servicio.", ICONO_LOCACIONES, "Locaciones"));
            }
            else if (clave == "empleados")
            {
                encabezado("Empleados", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Registro de empleados", "Registra empleados y consulta su asistencia.", ICONO_EMPLEADOS, "Empleados"));
            }
            else if (clave == "inventario")
            {
                encabezado("Inventario", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Consulta de inventario", "Consulta existencias y agrega productos y equipos.", ICONO_INVENTARIO, "Inventario"));
            }
            else if (clave == "usuarios")
            {
                encabezado("Usuarios", "Elige qué deseas gestionar.");
                opciones.Add(new OpcionMenu("Gestión de usuarios", "Administra las cuentas de acceso al sistema.", ICONO_USUARIOS, "Usuarios"));
            }

            // Solo se muestran las opciones que el rol puede usar.
            opciones.RemoveAll(o => !puedeAbrir(clave, o.Destino));

            listaOpciones.ItemsSource = opciones;
        }

        private void moduloCerrado(object sender, EventArgs e)
        {
            Show();
        }

        private void abrirModulo(Window ventana)
        {
            ventana.Closed += moduloCerrado;
            Hide();
            ventana.Show();
        }

        private void abrirOrden()
        {
            DataRow? cotizacion = SeleccionarCotizacion.elegir(this, "");

            if (cotizacion != null)
                abrirModulo(new IngresarOrden(cotizacion));
        }

        private void abrirDestino(string destino)
        {
            // Segunda barrera: aunque alguien llegue aquí, se vuelve a comprobar el rol.
            if (!puedeDestino(destino))
            {
                MensajeCodelex.mostrar(this, "Acceso denegado", "Tu rol no tiene permiso para abrir este módulo.", false);
                return;
            }

            if (destino == "MisDatos")
                abrirModulo(new EmpleadoReporte(Sesion.EmpleadoId));
            else if (destino == "MisOrdenes")
                abrirModulo(new MisOrdenes());
            else if (destino == "Clientes")
                abrirModulo(new Clientes());
            else if (destino == "Vehiculos")
                abrirModulo(new RegistrarVehiculo());
            else if (destino == "GestionVehiculos")
                abrirModulo(new GestionVehiculos());
            else if (destino == "IngresarOrden")
                abrirOrden();
            else if (destino == "Usuarios")
                abrirModulo(new Usuarios());
            else if (destino == "Inventario")
                abrirModulo(new Inventario());
            else if (destino == "Empleados")
                abrirModulo(new Empleados());
            else if (destino == "Locaciones")
                abrirModulo(new IngresarLocacion());
            else if (destino == "Reportes")
                abrirModulo(new Reportes());
            else if (destino == "GestionOrdenes")
            abrirModulo(new GestionOrdenes());
            else
                MensajeCodelex.mostrar(this, "Próximamente", "Destino recibido: [" + destino + "]", true);
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