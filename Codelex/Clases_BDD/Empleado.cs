using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Codelex.Clases_BDD
{
    class Empleado
    {
        public Empleado() { }
        public bool ExisteCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return false;

            const string consulta =
                "SELECT COUNT(1) FROM EMPLEADOS WHERE correoEmpleado = @correo";

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                using (SqlCommand comando = new SqlCommand(consulta, conexion))
                {
                    comando.Parameters.AddWithValue("@correo", correo.Trim());

                    int cantidad = (int)comando.ExecuteScalar();
                    return cantidad > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar el correo del empleado: " + ex.Message);
            }
        }
    }
}
