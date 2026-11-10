using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevoProductoEquipo : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        public bool registrado = false;
        public bool esEquipo = false;
        public int productoId = 0;
        public string nombre = "";
        public string tipo = "";
        public string marca = "";
        public string modelo = "";

        public NuevoProductoEquipo()
        {
            InitializeComponent();
        }

        private void ventanaCargada(object sender, RoutedEventArgs e)
        {
            mostrarCampos();
            limpiarFormulario();
        }

        private void tipoMarcado(object sender, RoutedEventArgs e)
        {
            if (spMarca == null || spModelo == null)
                return;

            mostrarCampos();
        }

        private void mostrarCampos()
        {
            if (rbEquipo.IsChecked == true)
            {
                lblTipo.Text = "Tipo de equipo *";
                txtTipo.MaxLength = 50;
                spMarca.Visibility = Visibility.Visible;
                spModelo.Visibility = Visibility.Visible;
                txtNota.Text = "Los equipos se guardan por unidad: el equipo nuevo aparece en la pestaña Equipos y sus unidades se crean al guardar el ingreso.";
            }
            else
            {
                lblTipo.Text = "Tipo de producto *";
                txtTipo.MaxLength = 20;
                spMarca.Visibility = Visibility.Collapsed;
                spModelo.Visibility = Visibility.Collapsed;
                txtNota.Text = "El producto se registra con existencia 0. Luego agréguelo a la lista para ingresar lo que se compró.";
            }
        }

        private void limpiarFormulario()
        {
            txtNombre.Text = "";
            txtTipo.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            txtNombre.Focus();
        }

        private void guardarClic(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                if (rbEquipo.IsChecked == true)
                {
                    mensaje = inventarioBD.validarEquipo(txtNombre.Text, txtTipo.Text, txtMarca.Text, txtModelo.Text);

                    if (mensaje != "")
                    {
                        MensajeCodelex.mostrar(this, "Inventario", mensaje, false);
                        return;
                    }

                    esEquipo = true;
                    marca = txtMarca.Text.Trim();
                    modelo = txtModelo.Text.Trim();
                }
                else
                {
                    int productoId;
                    bool guardado = inventarioBD.insertarProducto(txtNombre.Text, txtTipo.Text, out mensaje, out productoId);

                    MensajeCodelex.mostrar(this, "Inventario", mensaje, guardado);

                    if (!guardado)
                        return;

                    productoId = productoId;
                }

                nombre = txtNombre.Text.Trim();
                tipo = txtTipo.Text.Trim();
                registrado = true;
                Close();
            }
            catch (Exception ex)
            {
                MensajeCodelex.mostrar(this, "Inventario", ex.Message, false);
            }
        }

        private void limpiarClic(object sender, RoutedEventArgs e)
        {
            limpiarFormulario();
        }

        private void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void salirClic(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
