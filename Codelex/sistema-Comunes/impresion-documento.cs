using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Codelex
{
    /// <summary>
    /// Arma un documento (factura o cotización) y lo manda a imprimir.
    /// En el cuadro de impresión de Windows se puede elegir "Microsoft Print to PDF" para guardarlo como PDF.
    /// </summary>
    public static class ImpresionDocumento
    {
        /// <summary>
        /// Crea la tabla donde se van agregando las líneas del documento.
        /// </summary>
        /// <returns>Tabla con las columnas Concepto, Cantidad, Precio y Subtotal.</returns>
        public static DataTable crearLineas()
        {
            DataTable tabla = new DataTable();
            tabla.Columns.Add("Concepto", typeof(string));
            tabla.Columns.Add("Cantidad", typeof(string));
            tabla.Columns.Add("Precio", typeof(string));
            tabla.Columns.Add("Subtotal", typeof(string));
            return tabla;
        }

        /// <summary>
        /// Agrega una línea a la tabla del documento.
        /// </summary>
        /// <param name="lineas">Tabla creada con crearLineas.</param>
        /// <param name="concepto">Descripción del servicio.</param>
        /// <param name="cantidad">Cantidad o medida.</param>
        /// <param name="precio">Precio unitario ya con formato.</param>
        /// <param name="subtotal">Subtotal ya con formato.</param>
        public static void agregarLinea(DataTable lineas, string concepto, string cantidad, string precio, string subtotal)
        {
            lineas.Rows.Add(concepto, cantidad, precio, subtotal);
        }

        /// <summary>
        /// Muestra el cuadro de impresión y manda el documento a la impresora elegida.
        /// </summary>
        /// <param name="dueno">Ventana dueña del cuadro.</param>
        /// <param name="titulo">Título del documento (Factura o Cotización).</param>
        /// <param name="numero">Número del documento.</param>
        /// <param name="encabezado">Líneas de texto con los datos del cliente y del documento.</param>
        /// <param name="lineas">Tabla creada con crearLineas.</param>
        /// <param name="totales">Líneas de texto con los totales.</param>
        public static void imprimir(Window dueno, string titulo, string numero, List<string> encabezado, DataTable lineas, List<string> totales)
        {
            PrintDialog dialogo = new PrintDialog();
            if (dialogo.ShowDialog() != true)
                return;

            FlowDocument documento = armarDocumento(titulo, numero, encabezado, lineas, totales);
            documento.PageWidth = dialogo.PrintableAreaWidth;
            documento.PageHeight = dialogo.PrintableAreaHeight;
            documento.ColumnWidth = dialogo.PrintableAreaWidth;

            IDocumentPaginatorSource fuente = documento;
            dialogo.PrintDocument(fuente.DocumentPaginator, titulo + " " + numero);

            MensajeCodelex.mostrar(dueno, titulo, "El documento se envió a la impresora.", true);
        }

        /// <summary>
        /// Arma el documento con el encabezado, la tabla de líneas y los totales.
        /// </summary>
        /// <param name="titulo">Título del documento.</param>
        /// <param name="numero">Número del documento.</param>
        /// <param name="encabezado">Líneas de texto con los datos del cliente y del documento.</param>
        /// <param name="lineas">Tabla creada con crearLineas.</param>
        /// <param name="totales">Líneas de texto con los totales.</param>
        /// <returns>Documento listo para imprimir.</returns>
        private static FlowDocument armarDocumento(string titulo, string numero, List<string> encabezado, DataTable lineas, List<string> totales)
        {
            FlowDocument documento = new FlowDocument();
            documento.FontFamily = new FontFamily("Segoe UI");
            documento.FontSize = 12;
            documento.PagePadding = new Thickness(50);

            Paragraph nombre = new Paragraph(new Run("Codelex"));
            nombre.FontSize = 22;
            nombre.FontWeight = FontWeights.Bold;
            nombre.Foreground = new SolidColorBrush(Color.FromRgb(0x14, 0x83, 0x7A));
            nombre.Margin = new Thickness(0, 0, 0, 2);
            documento.Blocks.Add(nombre);

            Paragraph encabezadoDoc = new Paragraph(new Run(titulo + " " + numero));
            encabezadoDoc.FontSize = 16;
            encabezadoDoc.FontWeight = FontWeights.SemiBold;
            encabezadoDoc.Margin = new Thickness(0, 0, 0, 14);
            documento.Blocks.Add(encabezadoDoc);

            foreach (string linea in encabezado)
            {
                Paragraph parrafo = new Paragraph(new Run(linea));
                parrafo.Margin = new Thickness(0, 0, 0, 2);
                documento.Blocks.Add(parrafo);
            }

            Table tabla = new Table();
            tabla.CellSpacing = 0;
            tabla.Margin = new Thickness(0, 16, 0, 0);
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(3, GridUnitType.Star) });
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(1.2, GridUnitType.Star) });
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(1.2, GridUnitType.Star) });
            tabla.Columns.Add(new TableColumn() { Width = new GridLength(1.2, GridUnitType.Star) });

            TableRowGroup grupo = new TableRowGroup();
            grupo.Rows.Add(crearFila("Concepto", "Cantidad", "Precio", "Subtotal", true));

            foreach (DataRow fila in lineas.Rows)
            {
                grupo.Rows.Add(crearFila("" + fila["Concepto"], "" + fila["Cantidad"], "" + fila["Precio"], "" + fila["Subtotal"], false));
            }

            tabla.RowGroups.Add(grupo);
            documento.Blocks.Add(tabla);

            foreach (string total in totales)
            {
                Paragraph parrafo = new Paragraph(new Run(total));
                parrafo.TextAlignment = TextAlignment.Right;
                parrafo.FontWeight = FontWeights.SemiBold;
                parrafo.Margin = new Thickness(0, 4, 0, 0);
                documento.Blocks.Add(parrafo);
            }

            return documento;
        }

        /// <summary>
        /// Crea una fila de la tabla del documento.
        /// </summary>
        /// <param name="concepto">Texto de la primera columna.</param>
        /// <param name="cantidad">Texto de la segunda columna.</param>
        /// <param name="precio">Texto de la tercera columna.</param>
        /// <param name="subtotal">Texto de la cuarta columna.</param>
        /// <param name="esTitulo">true si es la fila de títulos.</param>
        /// <returns>Fila lista para agregar a la tabla.</returns>
        private static TableRow crearFila(string concepto, string cantidad, string precio, string subtotal, bool esTitulo)
        {
            TableRow fila = new TableRow();
            if (esTitulo)
            {
                fila.FontWeight = FontWeights.Bold;
                fila.Background = new SolidColorBrush(Color.FromRgb(0xE3, 0xF7, 0xF4));
            }

            fila.Cells.Add(crearCelda(concepto, TextAlignment.Left));
            fila.Cells.Add(crearCelda(cantidad, TextAlignment.Right));
            fila.Cells.Add(crearCelda(precio, TextAlignment.Right));
            fila.Cells.Add(crearCelda(subtotal, TextAlignment.Right));
            return fila;
        }

        /// <summary>
        /// Crea una celda de la tabla del documento.
        /// </summary>
        /// <param name="texto">Texto de la celda.</param>
        /// <param name="alineacion">Alineación del texto.</param>
        /// <returns>Celda lista para agregar a la fila.</returns>
        private static TableCell crearCelda(string texto, TextAlignment alineacion)
        {
            Paragraph parrafo = new Paragraph(new Run(texto));
            parrafo.TextAlignment = alineacion;

            TableCell celda = new TableCell(parrafo);
            celda.Padding = new Thickness(6, 4, 6, 4);
            celda.BorderBrush = new SolidColorBrush(Color.FromRgb(0xDD, 0xEA, 0xE8));
            celda.BorderThickness = new Thickness(0, 0, 0, 1);
            return celda;
        }
    }
}
