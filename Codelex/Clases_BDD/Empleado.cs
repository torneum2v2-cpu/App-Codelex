using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class Empleado
    {
        public bool ExisteCorreo(string correo)
        {
            if (correo.Trim() == "")
                return false;

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand("sp_ExisteCorreoEmpleado", conexion))
                    {
                        comando.CommandType = CommandType.StoredProcedure;
                        comando.Parameters.AddWithValue("@correo", correo.Trim());

                        int cantidad = Convert.ToInt32(comando.ExecuteScalar());
                        return cantidad > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar el correo del empleado: " + ex.Message);
            }
        }
    }
}
