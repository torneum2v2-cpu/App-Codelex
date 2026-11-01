using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Codelex
{
    public static class FotoPerfil
    {
        private const int Lado = 256;

        // Lee una imagen del disco, la recorta cuadrada al centro, la reduce a 256x256 y devuelve un PNG.
        public static byte[] Preparar(string ruta)
        {
            BitmapImage origen = new BitmapImage();
            origen.BeginInit();
            origen.CacheOption = BitmapCacheOption.OnLoad;
            origen.UriSource = new Uri(ruta);
            origen.EndInit();
            origen.Freeze();

            int lado = Math.Min(origen.PixelWidth, origen.PixelHeight);
            int x = (origen.PixelWidth - lado) / 2;
            int y = (origen.PixelHeight - lado) / 2;

            BitmapSource resultado = new CroppedBitmap(origen, new Int32Rect(x, y, lado, lado));

            if (lado > Lado)
            {
                double escala = (double)Lado / lado;
                resultado = new TransformedBitmap(resultado, new ScaleTransform(escala, escala));
            }

            PngBitmapEncoder codificador = new PngBitmapEncoder();
            codificador.Frames.Add(BitmapFrame.Create(resultado));

            using (MemoryStream flujo = new MemoryStream())
            {
                codificador.Save(flujo);
                return flujo.ToArray();
            }
        }

        // Convierte los bytes guardados en una imagen que WPF puede mostrar.
        public static ImageSource? Cargar(byte[] datos)
        {
            try
            {
                BitmapImage imagen = new BitmapImage();

                using (MemoryStream flujo = new MemoryStream(datos))
                {
                    imagen.BeginInit();
                    imagen.CacheOption = BitmapCacheOption.OnLoad;
                    imagen.StreamSource = flujo;
                    imagen.EndInit();
                }

                imagen.Freeze();
                return imagen;
            }
            catch
            {
                return null;
            }
        }
    }
}