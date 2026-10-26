using Codelex.BDD;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class UsuarioBD
    {
        private string PrepararContrasena(string contrasena)
        {
            return contrasena;
        }

        public DataTable ListarRoles()
        {
            DataTable tabla = new DataTable();

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand(
                        "SELECT rol_id, nombreRol FROM ROLES ORDER BY rol_id", conexion))
                    {
                        SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                        adaptador.Fill(tabla);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar los roles: " + ex.Message);
            }

            return tabla;
        }

        public DataTable BuscarUsuarios(string texto, string buscarPor, string filtroEstado)
        {
            DataTable tabla = new DataTable();

            object estado = DBNull.Value;
            if (filtroEstado == "ACTIVOS")
                estado = 1;
            else if (filtroEstado == "INACTIVOS")
                estado = 0;

            string sql = @"
                SELECT u.usuario_id, u.empleado_id, u.usuario, u.rol_id, u.estadoUsuario,
                       e.nombreEmpleado, e.apellidoEmpleado, e.correoEmpleado, e.telefonoEmpleado,
                       r.nombreRol AS rol,
                       CASE WHEN u.estadoUsuario = 1 THEN 'Activo' ELSE 'Inactivo' END AS estado
                FROM USUARIOS u
                INNER JOIN EMPLEADOS e ON e.empleado_id = u.empleado_id
                LEFT JOIN ROLES r ON r.rol_id = u.rol_id
                WHERE (@estado IS NULL OR u.estadoUsuario = @estado)
                  AND (@texto = ''
                   OR (@buscarPor = 'NOMBRE'
                       AND (e.nombreEmpleado LIKE '%' + @texto + '%'
                            OR e.apellidoEmpleado LIKE '%' + @texto + '%'
                            OR ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '') LIKE '%' + @texto + '%'))
                   OR (@buscarPor = 'USUARIO' AND u.usuario LIKE '%' + @texto + '%')
                   OR (@buscarPor = 'CORREO'  AND e.correoEmpleado LIKE '%' + @texto + '%')
                   OR (@buscarPor = 'ROL'     AND r.nombreRol LIKE '%' + @texto + '%')
                   OR (@buscarPor = ''
                       AND (u.usuario LIKE '%' + @texto + '%'
                            OR e.nombreEmpleado LIKE '%' + @texto + '%'
                            OR e.apellidoEmpleado LIKE '%' + @texto + '%'
                            OR e.correoEmpleado LIKE '%' + @texto + '%'
                            OR r.nombreRol LIKE '%' + @texto + '%')))
                ORDER BY e.nombreEmpleado, e.apellidoEmpleado";

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@texto", texto.Trim());
                        comando.Parameters.AddWithValue("@buscarPor", buscarPor);
                        comando.Parameters.AddWithValue("@estado", estado);

                        SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                        adaptador.Fill(tabla);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al buscar usuarios: " + ex.Message);
            }

            return tabla;
        }

        public DataTable BuscarEmpleadosSinUsuario(string texto)
        {
            DataTable tabla = new DataTable();

            string sql = @"
                SELECT e.empleado_id, e.nombreEmpleado, e.apellidoEmpleado, e.identidad,
                       e.telefonoEmpleado, e.correoEmpleado, c.nombreCargo
                FROM EMPLEADOS e
                LEFT JOIN CARGOS c ON c.cargo_id = e.cargo_id
                WHERE NOT EXISTS (SELECT 1 FROM USUARIOS u WHERE u.empleado_id = e.empleado_id)
                  AND (@texto = ''
                       OR e.identidad LIKE '%' + @texto + '%'
                       OR e.nombreEmpleado LIKE '%' + @texto + '%'
                       OR e.apellidoEmpleado LIKE '%' + @texto + '%'
                       OR ISNULL(e.nombreEmpleado, '') + ' ' + ISNULL(e.apellidoEmpleado, '') LIKE '%' + @texto + '%')
                ORDER BY e.nombreEmpleado, e.apellidoEmpleado";

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@texto", texto.Trim());

                        SqlDataAdapter adaptador = new SqlDataAdapter(comando);
                        adaptador.Fill(tabla);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al buscar empleados: " + ex.Message);
            }

            return tabla;
        }

        public bool ExisteUsuario(string usuario, int excluirUsuarioId)
        {
            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    using (SqlCommand comando = new SqlCommand(
                        "SELECT COUNT(1) FROM USUARIOS WHERE usuario = @usuario AND usuario_id <> @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuario.Trim());
                        comando.Parameters.AddWithValue("@id", excluirUsuarioId);

                        return Convert.ToInt32(comando.ExecuteScalar()) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al validar el usuario: " + ex.Message);
            }
        }

        public string UsuarioDisponible(string usuarioBase)
        {
            string candidato = usuarioBase;
            int numero = 2;

            while (candidato != "" && ExisteUsuario(candidato, 0))
            {
                candidato = usuarioBase + numero;
                numero++;
            }

            return candidato;
        }

        public bool CrearUsuario(int empleadoId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            mensaje = "";
            usuario = usuario.Trim();

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    if (!Existe(conexion, "SELECT COUNT(1) FROM EMPLEADOS WHERE empleado_id = @valor", empleadoId))
                    {
                        mensaje = "El empleado no existe.";
                        return false;
                    }

                    if (Existe(conexion, "SELECT COUNT(1) FROM USUARIOS WHERE empleado_id = @valor", empleadoId))
                    {
                        mensaje = "Ese empleado ya tiene un usuario.";
                        return false;
                    }

                    if (Existe(conexion, "SELECT COUNT(1) FROM USUARIOS WHERE usuario = @valor", usuario))
                    {
                        mensaje = "Ya existe un usuario con ese nombre.";
                        return false;
                    }

                    if (!Existe(conexion, "SELECT COUNT(1) FROM ROLES WHERE rol_id = @valor", rolId))
                    {
                        mensaje = "El rol seleccionado no existe.";
                        return false;
                    }

                    using (SqlCommand comando = new SqlCommand(
                        "INSERT INTO USUARIOS (empleado_id, usuario, contrasena_hash, rol_id) " +
                        "VALUES (@empleado_id, @usuario, @contrasena, @rol_id)", conexion))
                    {
                        comando.Parameters.AddWithValue("@empleado_id", empleadoId);
                        comando.Parameters.AddWithValue("@usuario", usuario);
                        comando.Parameters.AddWithValue("@contrasena", PrepararContrasena(contrasena));
                        comando.Parameters.AddWithValue("@rol_id", rolId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                mensaje = "Ya existe un usuario con ese nombre, o ese empleado ya tiene usuario.";
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el usuario: " + ex.Message);
            }

            mensaje = "Usuario creado correctamente.";
            return true;
        }

        public bool ActualizarUsuario(int usuarioId, string usuario, string contrasena, int rolId, out string mensaje)
        {
            mensaje = "";
            usuario = usuario.Trim();

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    if (!Existe(conexion, "SELECT COUNT(1) FROM USUARIOS WHERE usuario_id = @valor", usuarioId))
                    {
                        mensaje = "El usuario no existe.";
                        return false;
                    }

                    if (!Existe(conexion, "SELECT COUNT(1) FROM ROLES WHERE rol_id = @valor", rolId))
                    {
                        mensaje = "El rol seleccionado no existe.";
                        return false;
                    }

                    using (SqlCommand repetido = new SqlCommand(
                        "SELECT COUNT(1) FROM USUARIOS WHERE usuario = @usuario AND usuario_id <> @id", conexion))
                    {
                        repetido.Parameters.AddWithValue("@usuario", usuario);
                        repetido.Parameters.AddWithValue("@id", usuarioId);

                        if (Convert.ToInt32(repetido.ExecuteScalar()) > 0)
                        {
                            mensaje = "Ya existe otro usuario con ese nombre.";
                            return false;
                        }
                    }

                    bool cambiaClave = contrasena != "";

                    string sql = cambiaClave
                        ? "UPDATE USUARIOS SET usuario = @usuario, rol_id = @rol_id, contrasena_hash = @contrasena WHERE usuario_id = @id"
                        : "UPDATE USUARIOS SET usuario = @usuario, rol_id = @rol_id WHERE usuario_id = @id";

                    using (SqlCommand comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@usuario", usuario);
                        comando.Parameters.AddWithValue("@rol_id", rolId);
                        comando.Parameters.AddWithValue("@id", usuarioId);

                        if (cambiaClave)
                            comando.Parameters.AddWithValue("@contrasena", PrepararContrasena(contrasena));

                        comando.ExecuteNonQuery();
                    }

                    mensaje = cambiaClave
                        ? "Usuario y contraseña actualizados correctamente."
                        : "Usuario actualizado correctamente.";
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
            {
                mensaje = "Ya existe otro usuario con ese nombre.";
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el usuario: " + ex.Message);
            }

            return true;
        }

        public bool CambiarEstadoUsuario(int usuarioId, bool activo, out string mensaje)
        {
            mensaje = "";

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    if (!Existe(conexion, "SELECT COUNT(1) FROM USUARIOS WHERE usuario_id = @valor", usuarioId))
                    {
                        mensaje = "El usuario no existe.";
                        return false;
                    }

                    using (SqlCommand comando = new SqlCommand(
                        "UPDATE USUARIOS SET estadoUsuario = @estado WHERE usuario_id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@estado", activo);
                        comando.Parameters.AddWithValue("@id", usuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar el estado del usuario: " + ex.Message);
            }

            mensaje = activo ? "Usuario activado correctamente." : "Usuario desactivado correctamente.";
            return true;
        }

        public bool EliminarUsuario(int usuarioId, out string mensaje)
        {
            mensaje = "";

            try
            {
                using (SqlConnection conexion = Conexion.ObtenerConexion())
                {
                    if (!Existe(conexion, "SELECT COUNT(1) FROM USUARIOS WHERE usuario_id = @valor", usuarioId))
                    {
                        mensaje = "El usuario no existe.";
                        return false;
                    }

                    using (SqlCommand comando = new SqlCommand(
                        "DELETE FROM USUARIOS WHERE usuario_id = @id", conexion))
                    {
                        comando.Parameters.AddWithValue("@id", usuarioId);
                        comando.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                mensaje = "No se puede eliminar: este usuario tiene registros en la bitácora. Desactívelo en su lugar.";
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el usuario: " + ex.Message);
            }

            mensaje = "Usuario eliminado correctamente.";
            return true;
        }

        private bool Existe(SqlConnection conexion, string sql, object valor)
        {
            using (SqlCommand comando = new SqlCommand(sql, conexion))
            {
                comando.Parameters.AddWithValue("@valor", valor);
                return Convert.ToInt32(comando.ExecuteScalar()) > 0;
            }
        }
    }
}
