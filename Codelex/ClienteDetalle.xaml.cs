using Codelex.Clases_BDD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Codelex
{
    /// <summary>
    /// Lógica de interacción para ClienteDetalle.xaml
    /// </summary>
    public partial class ClienteDetalle : Window
    {
        Cliente clienteBD = new Cliente();
        int clienteId;

        // La pantalla de Clientes lo revisa al cerrar: si es true, carga el cliente en el formulario
        public bool EditarCliente = false;

        public ClienteDetalle(int clienteId)
        {
            InitializeComponent();
            this.clienteId = clienteId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarDatosCliente();
                CargarLocaciones();
                CargarOrdenes();
                CargarFacturas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Detalle del cliente", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDatosCliente()
        {
            DataTable tabla = clienteBD.ObtenerCliente(clienteId);

            if (tabla.Rows.Count == 0)
            {
                MessageBox.Show("No se encontró el cliente.", "Detalle del cliente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DataRow fila = tabla.Rows[0];

            txtNumero.Text = clienteId.ToString();
            txtRTN.Text = fila["identificacionCliente"].ToString();
            txtEmpresa.Text = fila["nombreEmpresa"].ToString();
            txtContacto.Text = fila["nombreCliente"].ToString();
            txtApellido.Text = fila["apellidoCliente"].ToString();
            txtTelefono.Text = fila["telefonoCliente"].ToString();
            txtCorreo.Text = fila["correoCliente"].ToString();
            txtEstado.Text = fila["estado"].ToString();
        }

        // Cada lista muestra los datos en el mismo orden que el texto de arriba de la lista

        private void CargarLocaciones()
        {
            DataTable tabla = clienteBD.LocacionesPorCliente(clienteId);
            lstLocaciones.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string linea = fila["nombreLocacion"] + " · " + fila["tipoLocacion"] + " · " + fila["direccion"];
                lstLocaciones.Items.Add(linea);
            }
        }

        private void CargarOrdenes()
        {
            DataTable tabla = clienteBD.OrdenesPorCliente(clienteId);
            lstOrdenes.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string linea = fila["orden_id"] + " · " + fila["servicio"] + " · " + Fecha(fila["fecha"]) + " · " + fila["estado"];
                lstOrdenes.Items.Add(linea);
            }
        }

        private void CargarFacturas()
        {
            DataTable tabla = clienteBD.FacturasPorCliente(clienteId);
            lstFacturas.Items.Clear();

            foreach (DataRow fila in tabla.Rows)
            {
                string monto = "";
                if (fila["monto"] != DBNull.Value)
                    monto = "L " + Convert.ToDecimal(fila["monto"]).ToString("N2");

                string linea = fila["factura_id"] + " · " + Fecha(fila["fecha"]) + " · " + fila["estado_pago"] + " · " + monto;
                lstFacturas.Items.Add(linea);
            }
        }

        // Muestra la fecha como día/mes/año (o vacío si no tiene)
        private string Fecha(object valor)
        {
            if (valor == DBNull.Value)
                return "";

            return Convert.ToDateTime(valor).ToString("dd/MM/yyyy");
        }

        // ===================== BOTONES =====================

        private void NuevaOrden_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("El módulo de Órdenes todavía no está disponible.", "Detalle del cliente",
                            MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Editar_Click(object sender, RoutedEventArgs e)
        {
            EditarCliente = true;
            Close();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

}

