using Codelex.AccesoDatos;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Codelex
{
    /// <summary>
    /// Ventana base de Codelex: reúne lo que repiten todas las pantallas (arrastrar, minimizar,
    /// cerrar, mensajes y teclas permitidas). Cada pantalla hereda de esta clase.
    /// </summary>
    public class VentanaCodelex : Window
    {
        /// <summary>
        /// Título que muestran los mensajes de esta pantalla; cada ventana lo pone al abrirse.
        /// </summary>
        protected string tituloMensajes = "Codelex";

        /// <summary>
        /// Permite mover la ventana arrastrando la barra superior con el mouse.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        protected void arrastrarVentana(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// Minimiza la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        protected void minimizarClic(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Cierra la ventana (las pantallas que necesitan otra cosa al salir lo reemplazan).
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        protected virtual void salirClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Cierra la ventana.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        protected void cerrarClic(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Muestra un mensaje de aviso (error) al usuario.
        /// </summary>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        protected void aviso(string mensaje)
        {
            MensajeCodelex.mostrar(this, tituloMensajes, mensaje, false);
        }

        /// <summary>
        /// Muestra un mensaje de éxito o de error según el resultado.
        /// </summary>
        /// <param name="correcto">true si es un mensaje de éxito; false si es un error.</param>
        /// <param name="mensaje">Mensaje con el resultado.</param>
        protected void mostrarMensaje(bool correcto, string mensaje)
        {
            MensajeCodelex.mostrar(this, tituloMensajes, mensaje, correcto);
        }

        /// <summary>
        /// Muestra un mensaje con el error ocurrido.
        /// </summary>
        /// <param name="ex">Excepción que se produjo.</param>
        protected void mostrarError(Exception ex)
        {
            MensajeCodelex.mostrar(this, tituloMensajes, ex.Message, false);
        }

        /// <summary>
        /// Deja escribir solo dígitos en una caja de texto.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        protected void soloNumeros(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Validacion.soloNumeros(e.Text);
        }

        /// <summary>
        /// Deja escribir solo dígitos y separador decimal en una caja de texto.
        /// </summary>
        /// <param name="sender">Control que originó el evento.</param>
        /// <param name="e">Datos del evento.</param>
        protected void soloDecimales(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Validacion.soloDecimales(e.Text);
        }

        /// <summary>
        /// Devuelve el identificador del elemento elegido en una lista desplegable.
        /// </summary>
        /// <param name="combo">Lista desplegable.</param>
        /// <returns>Identificador elegido; 0 si no hay nada elegido.</returns>
        protected int idSeleccionado(ComboBox combo)
        {
            ItemLista? item = combo.SelectedItem as ItemLista;
            if (item == null)
                return 0;

            return item.id;
        }

        /// <summary>
        /// Crea un pincel fijo a partir de un color escrito como texto (por ejemplo "#1E88E5").
        /// </summary>
        /// <param name="color">Color en texto.</param>
        /// <returns>Pincel listo para usar.</returns>
        public static Brush pincel(string color)
        {
            SolidColorBrush pincel = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            pincel.Freeze();
            return pincel;
        }

        /// <summary>
        /// Devuelve una raya si el texto está vacío, para no dejar campos en blanco.
        /// </summary>
        /// <param name="texto">Texto a revisar.</param>
        /// <returns>El mismo texto, o "—" si estaba vacío.</returns>
        protected static string vacio(string texto)
        {
            if (texto.Trim() == "")
                return "—";

            return texto;
        }
    }
}
