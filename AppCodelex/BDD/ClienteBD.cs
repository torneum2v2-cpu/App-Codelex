using System.Data;
using Microsoft.Data.SqlClient;

namespace AppCodelex.BDD;

public class ClienteBD
{
    public DataTable BuscarClientes(string texto, string buscarPor, string filtroEstado)
    {
        object estado = DBNull.Value;
        if (filtroEstado == "ACTIVOS")
            estado = true;
        else if (filtroEstado == "INACTIVOS")
            estado = false;

        SqlCommand comando = Procedimiento("sp_BuscarClientes");
        comando.Parameters.AddWithValue("@texto", texto);
        comando.Parameters.AddWithValue("@buscarPor", buscarPor);
        comando.Parameters.AddWithValue("@estado", estado);
        return Consultar(comando);
    }

    public DataTable ObtenerCliente(int clienteId)
    {
        return ConsultarPorCliente("sp_ObtenerCliente", clienteId);
    }

    public DataTable ListarLocaciones()
    {
        return Consultar(Procedimiento("sp_ListarLocaciones"));
    }

    public DataTable LocacionesPorCliente(int clienteId)
    {
        return ConsultarPorCliente("sp_ListarLocacionesPorCliente", clienteId);
    }

    public DataTable OrdenesPorCliente(int clienteId)
    {
        return ConsultarPorCliente("sp_ListarOrdenesPorCliente", clienteId);
    }

    public DataTable FacturasPorCliente(int clienteId)
    {
        return ConsultarPorCliente("sp_ListarFacturasPorCliente", clienteId);
    }

    public bool InsertarCliente(string rtn, string empresa, string nombre, string apellido,
                                string telefono, string correo, object locacionId,
                                out string mensaje, out int nuevoId)
    {
        nuevoId = 0;

        mensaje = ValidarCliente(rtn, empresa, nombre, apellido, telefono, correo);
        if (mensaje != "")
            return false;

        SqlCommand comando = Procedimiento("sp_InsertarCliente");
        AgregarDatosCliente(comando, rtn, empresa, nombre, apellido, telefono, correo, locacionId);
        comando.Parameters.Add("@cliente_id", SqlDbType.Int).Direction = ParameterDirection.Output;

        bool guardado = Ejecutar(comando, out mensaje);
        if (guardado)
            nuevoId = Convert.ToInt32(comando.Parameters["@cliente_id"].Value);

        return guardado;
    }

    public bool ActualizarCliente(int clienteId, string rtn, string empresa, string nombre, string apellido,
                                  string telefono, string correo, object locacionId, out string mensaje)
    {
        mensaje = ValidarCliente(rtn, empresa, nombre, apellido, telefono, correo);
        if (mensaje != "")
            return false;

        SqlCommand comando = Procedimiento("sp_ActualizarCliente");
        comando.Parameters.AddWithValue("@cliente_id", clienteId);
        AgregarDatosCliente(comando, rtn, empresa, nombre, apellido, telefono, correo, locacionId);

        return Ejecutar(comando, out mensaje);
    }

    public bool CambiarEstadoCliente(int clienteId, bool activo, out string mensaje)
    {
        SqlCommand comando = Procedimiento("sp_CambiarEstadoCliente");
        comando.Parameters.AddWithValue("@cliente_id", clienteId);
        comando.Parameters.AddWithValue("@estadoCliente", activo);

        return Ejecutar(comando, out mensaje);
    }

    public bool SoloNumeros(string texto)
    {
        foreach (char c in texto)
        {
            if (!char.IsDigit(c))
                return false;
        }

        return true;
    }

    private string ValidarCliente(string rtn, string empresa, string nombre, string apellido,
                                  string telefono, string correo)
    {
        rtn = rtn.Trim();
        telefono = telefono.Trim();
        correo = correo.Trim();

        if (rtn == "")
            return "El RTN / identidad es obligatorio.";
        if (!SoloNumeros(rtn))
            return "El RTN / identidad solo puede tener números, sin guiones ni espacios.";
        if (rtn.Length < 13 || rtn.Length > 14)
            return "El RTN / identidad debe tener 13 o 14 dígitos. Tiene " + rtn.Length + ".";

        if (empresa.Trim() == "")
            return "La empresa es obligatoria.";
        if (nombre.Trim() == "")
            return "El nombre del contacto es obligatorio.";
        if (apellido.Trim() == "")
            return "El apellido del contacto es obligatorio.";

        if (telefono == "")
            return "El teléfono es obligatorio.";
        if (!SoloNumeros(telefono))
            return "El teléfono solo puede tener números, sin guiones ni espacios.";
        if (telefono.Length != 8)
            return "El teléfono debe tener 8 dígitos. Tiene " + telefono.Length + ".";

        if (correo == "")
            return "El correo es obligatorio.";
        if (!correo.Contains("@") || !correo.Contains("."))
            return "El correo no es válido. Ejemplo: nombre@empresa.com";

        return "";
    }

    private void AgregarDatosCliente(SqlCommand comando, string rtn, string empresa, string nombre, string apellido,
                                     string telefono, string correo, object locacionId)
    {
        comando.Parameters.AddWithValue("@identificacionCliente", rtn.Trim());
        comando.Parameters.AddWithValue("@nombreEmpresa", empresa.Trim());
        comando.Parameters.AddWithValue("@nombreCliente", nombre.Trim());
        comando.Parameters.AddWithValue("@apellidoCliente", apellido.Trim());
        comando.Parameters.AddWithValue("@telefonoCliente", telefono.Trim());
        comando.Parameters.AddWithValue("@correoCliente", correo.Trim());
        comando.Parameters.AddWithValue("@locacion_id", locacionId);
    }

    private SqlCommand Procedimiento(string nombre)
    {
        SqlCommand comando = new SqlCommand(nombre);
        comando.CommandType = CommandType.StoredProcedure;
        return comando;
    }

    private DataTable ConsultarPorCliente(string procedimiento, int clienteId)
    {
        SqlCommand comando = Procedimiento(procedimiento);
        comando.Parameters.AddWithValue("@cliente_id", clienteId);
        return Consultar(comando);
    }

    private DataTable Consultar(SqlCommand comando)
    {
        DataTable tabla = new DataTable();

        using (SqlConnection conexion = Conexion.ObtenerConexion())
        {
            comando.Connection = conexion;
            SqlDataAdapter adaptador = new SqlDataAdapter(comando);
            adaptador.Fill(tabla);
        }

        return tabla;
    }

    private bool Ejecutar(SqlCommand comando, out string mensaje)
    {
        comando.Parameters.Add("@resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
        comando.Parameters.Add("@mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;

        using (SqlConnection conexion = Conexion.ObtenerConexion())
        {
            comando.Connection = conexion;
            comando.ExecuteNonQuery();
        }

        mensaje = comando.Parameters["@mensaje"].Value.ToString() ?? "";
        return Convert.ToInt32(comando.Parameters["@resultado"].Value) == 1;
    }
}
