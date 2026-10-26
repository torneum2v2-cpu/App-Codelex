using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class Vehiculo : BaseDatos
    {
        public DataTable BuscarVehiculos(string texto, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = Procedimiento("sp_BuscarVehiculos");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@estado", estado);
            return Consultar(comando);
        }

        public DataTable ObtenerVehiculo(int vehiculoId)
        {
            SqlCommand comando = Procedimiento("sp_ObtenerVehiculo");
            comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);
            return Consultar(comando);
        }

        public bool InsertarVehiculo(string placa, string marca, string modelo, out string mensaje)
        {
            return GuardarVehiculo("sp_InsertarVehiculo", 0, placa, marca, modelo, out mensaje);
        }

        public bool ActualizarVehiculo(int vehiculoId, string placa, string marca, string modelo, out string mensaje)
        {
            return GuardarVehiculo("sp_ActualizarVehiculo", vehiculoId, placa, marca, modelo, out mensaje);
        }

        public bool CambiarEstadoVehiculo(int vehiculoId, bool activo, out string mensaje)
        {
            SqlCommand comando = Procedimiento("sp_CambiarEstadoVehiculo");
            comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);
            comando.Parameters.AddWithValue("@estadoVehiculo", activo);

            return Ejecutar(comando, out mensaje);
        }

        public bool CaracteresDePlaca(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsLetterOrDigit(c) && c != '-')
                    return false;
            }

            return true;
        }

        private string ValidarVehiculo(string placa, string marca, string modelo)
        {
            placa = placa.Trim();
            marca = marca.Trim();
            modelo = modelo.Trim();

            if (placa == "")
                return "La placa es obligatoria.";
            if (!CaracteresDePlaca(placa))
                return "La placa solo puede tener letras, números y guion, sin espacios.";
            if (placa.Length < 5)
                return "La placa debe tener al menos 5 caracteres. Tiene " + placa.Length + ".";
            if (placa.Length > 15)
                return "La placa no puede tener más de 15 caracteres.";

            if (marca == "")
                return "La marca es obligatoria.";
            if (marca.Length > 20)
                return "La marca no puede tener más de 20 caracteres.";

            if (modelo == "")
                return "El modelo es obligatorio.";
            if (modelo.Length > 25)
                return "El modelo no puede tener más de 25 caracteres.";

            return "";
        }

        private bool GuardarVehiculo(string procedimiento, int vehiculoId, string placa, string marca,
                                     string modelo, out string mensaje)
        {
            mensaje = ValidarVehiculo(placa, marca, modelo);
            if (mensaje != "")
                return false;

            SqlCommand comando = Procedimiento(procedimiento);

            if (vehiculoId > 0)
                comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);

            comando.Parameters.AddWithValue("@placa", placa.Trim().ToUpper());
            comando.Parameters.AddWithValue("@marcaVehiculo", marca.Trim());
            comando.Parameters.AddWithValue("@modeloVehiculo", modelo.Trim());

            return Ejecutar(comando, out mensaje);
        }
    }
}
