using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Codelex.Clases_BDD
{
    class Cliente
    {

        public Cliente()
        {

        }
        public DataTable BuscarClientes(string texto, string buscarPor, string filtroEstado)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_BuscarClientes", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@texto", texto);
                    comando.Parameters.AddWithValue("@buscarPor", buscarPor);

                    if (filtroEstado == "ACTIVOS")
                        comando.Parameters.AddWithValue("@estado", true);
                    else if (filtroEstado == "INACTIVOS")
                        comando.Parameters.AddWithValue("@estado", false);
                    else
                        comando.Parameters.AddWithValue("@estado", DBNull.Value);

                    SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                    adaptador.Fill(tabla);
                }
            }

            return tabla;
        }

        // Devuelve una tabla con una sola fila (o vacía si no existe el cliente)
        public DataTable ObtenerCliente(int clienteId)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_ObtenerCliente", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@cliente_id", clienteId);

                    SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                    adaptador.Fill(tabla);
                }
            }

            return tabla;
        }

        // Devuelve true si se guardó. En "mensaje" queda lo que respondió el procedimiento
        // y en "nuevoId" el número del cliente creado.
        public bool InsertarCliente(string rtn, string empresa, string nombre, string apellido,
                                    string telefono, string correo, object locacionId,
                                    out string mensaje, out int nuevoId)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_InsertarCliente", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@identificacionCliente", rtn);
                    comando.Parameters.AddWithValue("@nombreEmpresa", ValorONulo(empresa));
                    comando.Parameters.AddWithValue("@nombreCliente", nombre);
                    comando.Parameters.AddWithValue("@apellidoCliente", ValorONulo(apellido));
                    comando.Parameters.AddWithValue("@telefonoCliente", ValorONulo(telefono));
                    comando.Parameters.AddWithValue("@correoCliente", ValorONulo(correo));
                    comando.Parameters.AddWithValue("@locacion_id", locacionId);

                    comando.Parameters.Add("@cliente_id", SqlDbType.Int).Direction = ParameterDirection.Output;
                    comando.Parameters.Add("@resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    comando.Parameters.Add("@mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                    comando.ExecuteNonQuery();

                    mensaje = comando.Parameters["@mensaje"].Value.ToString() ?? "";
                    int resultado = Convert.ToInt32(comando.Parameters["@resultado"].Value);

                    if (resultado == 1)
                    {
                        nuevoId = Convert.ToInt32(comando.Parameters["@cliente_id"].Value);
                        return true;
                    }

                    nuevoId = 0;
                    return false;
                }
            }
        }

        public bool ActualizarCliente(int clienteId, string rtn, string empresa, string nombre, string apellido,
                                      string telefono, string correo, object locacionId, out string mensaje)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_ActualizarCliente", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@cliente_id", clienteId);
                    comando.Parameters.AddWithValue("@identificacionCliente", rtn);
                    comando.Parameters.AddWithValue("@nombreEmpresa", ValorONulo(empresa));
                    comando.Parameters.AddWithValue("@nombreCliente", nombre);
                    comando.Parameters.AddWithValue("@apellidoCliente", ValorONulo(apellido));
                    comando.Parameters.AddWithValue("@telefonoCliente", ValorONulo(telefono));
                    comando.Parameters.AddWithValue("@correoCliente", ValorONulo(correo));
                    comando.Parameters.AddWithValue("@locacion_id", locacionId);

                    comando.Parameters.Add("@resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    comando.Parameters.Add("@mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                    comando.ExecuteNonQuery();

                    mensaje = comando.Parameters["@mensaje"].Value.ToString() ?? "";
                    int resultado = Convert.ToInt32(comando.Parameters["@resultado"].Value);
                    return resultado == 1;
                }
            }
        }

        // activo = true para activar, false para desactivar (no se borra nada)
        public bool CambiarEstadoCliente(int clienteId, bool activo, out string mensaje)
        {
            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_CambiarEstadoCliente", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@cliente_id", clienteId);
                    comando.Parameters.AddWithValue("@estadoCliente", activo);

                    comando.Parameters.Add("@resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    comando.Parameters.Add("@mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

                    comando.ExecuteNonQuery();

                    mensaje = comando.Parameters["@mensaje"].Value.ToString() ?? "";
                    int resultado = Convert.ToInt32(comando.Parameters["@resultado"].Value);
                    return resultado == 1;
                }
            }
        }

        public DataTable ListarLocaciones()
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand("sp_ListarLocaciones", conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;

                    SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                    adaptador.Fill(tabla);
                }
            }

            return tabla;
        }

        public DataTable LocacionesPorCliente(int clienteId)
        {
            return ListarPorCliente("sp_ListarLocacionesPorCliente", clienteId);
        }

        public DataTable OrdenesPorCliente(int clienteId)
        {
            return ListarPorCliente("sp_ListarOrdenesPorCliente", clienteId);
        }

        public DataTable FacturasPorCliente(int clienteId)
        {
            return ListarPorCliente("sp_ListarFacturasPorCliente", clienteId);
        }

        // Ejecuta un procedimiento que solo recibe @cliente_id y devuelve una tabla
        private DataTable ListarPorCliente(string procedimiento, int clienteId)
        {
            DataTable tabla = new DataTable();

            using (SqlConnection conexion = Conexion.ObtenerConexion())
            {
                using (SqlCommand comando = new SqlCommand(procedimiento, conexion))
                {
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.Parameters.AddWithValue("@cliente_id", clienteId);

                    SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                    adaptador.Fill(tabla);
                }
            }

            return tabla;
        }

        // Si el texto está vacío se guarda NULL en la base de datos
        private object ValorONulo(string texto)
        {
            if (texto == "")
                return DBNull.Value;

            return texto;
        }
    }
}
