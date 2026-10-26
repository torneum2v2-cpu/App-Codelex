using Codelex.Clases_BDD;
using System.Windows;
using System.Windows.Input;

namespace Codelex
{
    public partial class NuevoProductoEquipo : Window
    {
        InventarioBD inventarioBD = new InventarioBD();

        public bool Registrado = false;
        public bool EsEquipo = false;
        public int ProductoId = 0;
        public string Nombre = "";
        public string Tipo = "";
        public string Marca = "";
        public string Modelo = "";

        public NuevoProductoEquipo()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MostrarCampos();
            LimpiarFormulario();
        }

        private void Tipo_Checked(object sender, RoutedEventArgs e)
        {
            if (spMarca == null || spModelo == null)
                return;

            MostrarCampos();
        }

        private void MostrarCampos()
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

        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            txtTipo.Text = "";
            txtMarca.Text = "";
            txtModelo.Text = "";
            txtNombre.Focus();
        }

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string mensaje;

                if (rbEquipo.IsChecked == true)
                {
                    mensaje = inventarioBD.ValidarEquipo(txtNombre.Text, txtTipo.Text, txtMarca.Text, txtModelo.Text);

                    if (mensaje != "")
                    {
                        MensajeCodelex.Mostrar(this, "Inventario", mensaje, false);
                        return;
                    }

                    EsEquipo = true;
                    Marca = txtMarca.Text.Trim();
                    Modelo = txtModelo.Text.Trim();
                }
                else
                {
                    int productoId;
                    bool guardado = inventarioBD.InsertarProducto(txtNombre.Text, txtTipo.Text, out mensaje, out productoId);

                    MensajeCodelex.Mostrar(this, "Inventario", mensaje, guardado);

                    if (!guardado)
                        return;

                    ProductoId = productoId;
                }

                Nombre = txtNombre.Text.Trim();
                Tipo = txtTipo.Text.Trim();
                Registrado = true;
                Close();
            }
            catch (Exception ex)
            {
                MensajeCodelex.Mostrar(this, "Inventario", ex.Message, false);
            }
        }

        private void Limpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarFormulario();
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void Salir_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
