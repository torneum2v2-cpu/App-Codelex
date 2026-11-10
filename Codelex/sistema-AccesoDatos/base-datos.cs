using Codelex.AccesoDatos;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.AccesoDatos
{
    class BaseDatos
    {
        /// <summary>
        /// Crea un comando listo para llamar a un procedimiento almacenado.
        /// </summary>
        /// <param name="nombre">Nombre.</param>
        /// <returns>Comando listo para ejecutar.</returns>
        protected SqlCommand procedimiento(string nombre)
        {
            SqlCommand comando = new SqlCommand(nombre);
            comando.CommandType = CommandType.StoredProcedure;
            return comando;
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado y devuelve el resultado en una tabla.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <returns>Tabla con los datos consultados.</returns>
        protected DataTable consultar(SqlCommand comando)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.obtenerConexion())
            {
                comando.Connection = conexion;
                SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                adaptador.Fill(tabla);
            }

            return tabla;
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado que responde con un resultado y un mensaje.
        /// </summary>
        /// <param name="comando">Comando de SQL Server.</param>
        /// <param name="mensaje">(salida) Mensaje con el resultado.</param>
        /// <returns>true si el procedimiento respondió resultado 1; false en caso contrario.</returns>
        protected bool ejecutar(SqlCommand comando, out string mensaje)
        {
            SqlParameter resultado = new SqlParameter("@resultado", SqlDbType.Int);
            resultado.Direction = ParameterDirection.Output;
            comando.Parameters.Add(resultado);

            SqlParameter textoMensaje = new SqlParameter("@mensaje", SqlDbType.VarChar, 100);
            textoMensaje.Direction = ParameterDirection.Output;
            comando.Parameters.Add(textoMensaje);

            using (SqlConnection conexion = Conexion.obtenerConexion())
            {
                comando.Connection = conexion;
                comando.ExecuteNonQuery();
            }

            mensaje = "" + textoMensaje.Value;
            return Convert.ToInt32(resultado.Value) == 1;
        }
    }
}
