using Codelex.AccesoDatos;
using System.Globalization;
using System.Windows.Data;

namespace Codelex
{
    /// <summary>
    /// Convierte el teléfono guardado (8 dígitos) en el formato +504 XXXX-XXXX para mostrarlo en las tablas.
    /// </summary>
    public class FormatoTelefono : IValueConverter
    {
        /// <summary>
        /// Da formato al teléfono que se va a mostrar.
        /// </summary>
        /// <param name="value">Teléfono guardado.</param>
        /// <param name="targetType">Tipo de destino.</param>
        /// <param name="parameter">Parámetro opcional.</param>
        /// <param name="culture">Cultura en uso.</param>
        /// <returns>Teléfono con formato +504 XXXX-XXXX.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            return Validacion.formatoTelefono("" + value);
        }

        /// <summary>
        /// No se usa: el teléfono solo se muestra, no se escribe desde la tabla.
        /// </summary>
        /// <param name="value">Valor mostrado.</param>
        /// <param name="targetType">Tipo de destino.</param>
        /// <param name="parameter">Parámetro opcional.</param>
        /// <param name="culture">Cultura en uso.</param>
        /// <returns>Siempre el mismo valor recibido.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
