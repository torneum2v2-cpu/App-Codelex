using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class Vehiculo : BaseDatos
    {
        public DataTable buscarVehiculos(string texto, string filtroEstado)
        {
            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = true;
            else if (filtroEstado == "INACTIVOS")
                estado = false;

            SqlCommand comando = procedimiento("sp_BuscarVehiculos");
            comando.Parameters.AddWithValue("@texto", texto);
            comando.Parameters.AddWithValue("@estado", estado);
            return consultar(comando);
        }

        public DataTable obtenerVehiculo(int vehiculoId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerVehiculo");
            comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);
            return consultar(comando);
        }

        public bool insertarVehiculo(string placa, string marca, string modelo, out string mensaje)
        {
            return guardarVehiculo("sp_InsertarVehiculo", 0, placa, marca, modelo, out mensaje);
        }

        public bool actualizarVehiculo(int vehiculoId, string placa, string marca, string modelo, out string mensaje)
        {
            return guardarVehiculo("sp_ActualizarVehiculo", vehiculoId, placa, marca, modelo, out mensaje);
        }

        public bool cambiarEstadoVehiculo(int vehiculoId, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoVehiculo");
            comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);
            comando.Parameters.AddWithValue("@estado_vehiculo", activo);

            return ejecutar(comando, out mensaje);
        }

        public bool caracteresDePlaca(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsLetterOrDigit(c) && c != '-')
                    return false;
            }

            return true;
        }

        private string validarVehiculo(string placa, string marca, string modelo)
        {
            placa = placa.Trim();
            marca = marca.Trim();
            modelo = modelo.Trim();

            if (placa == "")
                return "La placa es obligatoria.";
            if (!caracteresDePlaca(placa))
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

        private bool guardarVehiculo(string nombreProcedimiento, int vehiculoId, string placa, string marca,
                                     string modelo, out string mensaje)
        {
            mensaje = validarVehiculo(placa, marca, modelo);
            if (mensaje != "")
                return false;

            SqlCommand comando = procedimiento(nombreProcedimiento);

            if (vehiculoId > 0)
                comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);

            comando.Parameters.AddWithValue("@placa", placa.Trim().ToUpper());
            comando.Parameters.AddWithValue("@marca_vehiculo", marca.Trim());
            comando.Parameters.AddWithValue("@modelo_vehiculo", modelo.Trim());

            return ejecutar(comando, out mensaje);
        }
    }
}
