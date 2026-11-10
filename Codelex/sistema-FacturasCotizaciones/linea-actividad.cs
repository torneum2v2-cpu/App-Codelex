using System.ComponentModel;

namespace Codelex.FacturasCotizaciones
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
            set { medida = value; avisar("Medida"); }
        }

        public int Personas
        {
            get { return personas; }
            set { personas = value; avisar("Personas"); }
        }

        public decimal Precio
        {
            get { return precio; }
            set { precio = value; avisar("Precio"); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void avisar(string propiedad)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propiedad));
        }
    }
}
