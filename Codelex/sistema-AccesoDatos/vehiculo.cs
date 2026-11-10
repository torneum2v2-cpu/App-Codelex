using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class Vehiculo : BaseDatos
    {
        /// <summary>
        /// Busca vehículos.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <param name="filtroEstado">Filtro de estado.</param>
        /// <returns>Tabla con los datos consultados.</returns>
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

        /// <summary>
        /// Obtiene vehículo.
        /// </summary>
        /// <param name="vehiculoId">Identificador de vehículo.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        public DataTable obtenerVehiculo(int vehiculoId)
        {
            SqlCommand comando = procedimiento("sp_ObtenerVehiculo");
            comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);
            return consultar(comando);
        }

        /// <summary>
        /// Inserta vehículo.
        /// </summary>
        /// <param name="placa">Placa del vehículo.</param>
        /// <param name="marca">Marca.</param>
        /// <param name="modelo">Modelo.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool insertarVehiculo(string placa, string marca, string modelo, out string mensaje)
        {
            return guardarVehiculo("sp_InsertarVehiculo", 0, placa, marca, modelo, out mensaje);
        }

        /// <summary>
        /// Actualiza vehículo.
        /// </summary>
        /// <param name="vehiculoId">Identificador de vehículo.</param>
        /// <param name="placa">Placa del vehículo.</param>
        /// <param name="marca">Marca.</param>
        /// <param name="modelo">Modelo.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool actualizarVehiculo(int vehiculoId, string placa, string marca, string modelo, out string mensaje)
        {
            return guardarVehiculo("sp_ActualizarVehiculo", vehiculoId, placa, marca, modelo, out mensaje);
        }

        /// <summary>
        /// Cambia estado vehículo.
        /// </summary>
        /// <param name="vehiculoId">Identificador de vehículo.</param>
        /// <param name="activo">true para activar, false para desactivar.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
        public bool cambiarEstadoVehiculo(int vehiculoId, bool activo, out string mensaje)
        {
            SqlCommand comando = procedimiento("sp_CambiarEstadoVehiculo");
            comando.Parameters.AddWithValue("@vehiculo_id", vehiculoId);
            comando.Parameters.AddWithValue("@estado_vehiculo", activo);

            return ejecutar(comando, out mensaje);
        }

        /// <summary>
        /// Indica si el texto solo tiene letras, números o guion.
        /// </summary>
        /// <param name="texto">Texto a usar.</param>
        /// <returns>true si todos los caracteres son válidos; false en caso contrario.</returns>
        public bool caracteresDePlaca(string texto)
        {
            foreach (char c in texto)
            {
                if (!char.IsLetterOrDigit(c) && c != '-')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Valida vehículo.
        /// </summary>
        /// <param name="placa">Placa del vehículo.</param>
        /// <param name="marca">Marca.</param>
        /// <param name="modelo">Modelo.</param>
        /// <returns>Texto resultante.</returns>
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

        /// <summary>
        /// Guarda vehículo.
        /// </summary>
        /// <param name="nombreProcedimiento">Nombre del procedimiento almacenado.</param>
        /// <param name="vehiculoId">Identificador de vehículo.</param>
        /// <param name="placa">Placa del vehículo.</param>
        /// <param name="marca">Marca.</param>
        /// <param name="modelo">Modelo.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si la operación fue correcta; false en caso contrario.</returns>
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
