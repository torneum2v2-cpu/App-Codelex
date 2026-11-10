using System.ComponentModel;

namespace Codelex.Facturas_y_Cotizaciones
{
    // Una actividad de la cotización. La medida (m² o unidades) la escribe el usuario;
    // las personas y el precio los calcula la base de datos.
    class LineaActividad : INotifyPropertyChanged
    {
        string medida = "";
        int personas = 0;
        decimal precio = 0;

        public int ActividadId { get; set; } = 0;
        public string Nombre { get; set; } = "";
        public string Unidad { get; set; } = "";

        public string Medida
        {
            get { return medida; }
            set { medida = value; Avisar("Medida"); }
        }

        public int Personas
        {
            get { return personas; }
            set { personas = value; Avisar("Personas"); }
        }

        public decimal Precio
        {
            get { return precio; }
            set { precio = value; Avisar("Precio"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void Avisar(string propiedad)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propiedad));
        }
    }
}
