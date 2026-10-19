using System.Data;
using Microsoft.Data.SqlClient;

namespace AppCodelex.BDD;

public class Cliente
{
    public int ClienteId { get; set; }
    public string Identificacion { get; set; } = "";
    public string Empresa { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Correo { get; set; } = "";
    public int? LocacionId { get; set; }
}

/// <summary>
/// Acceso a datos de clientes. Todo se hace con procedimientos almacenados.
/// </summary>
public class ClienteDatos
{
    // buscarPor: "RTN", "NOMBRE" o null (busca en todos los campos)
    public DataTable Buscar(string texto, string? buscarPor)
    {
        return Consultar("sp_BuscarClientes",
            Parametro("@texto", SqlDbType.VarChar, 50, texto),
            Parametro("@buscarPor", SqlDbType.VarChar, 10, buscarPor));
    }

    public DataRow? Obtener(int clienteId)
    {
        DataTable tabla = Consultar("sp_ObtenerCliente", IdCliente(clienteId));
        return tabla.Rows.Count > 0 ? tabla.Rows[0] : null;
    }

    public (bool ok, string mensaje, int clienteId) Insertar(Cliente c)
    {
        using SqlConnection cn = Conexion.ObtenerConexion();
        using SqlCommand cmd = Comando(cn, "sp_InsertarCliente");
        AgregarDatosCliente(cmd, c);

        SqlParameter id = cmd.Parameters.Add("@cliente_id", SqlDbType.Int);
        id.Direction = ParameterDirection.Output;
        var (resultado, mensaje) = AgregarSalidas(cmd);

        cmd.ExecuteNonQuery();

        int nuevoId = id.Value is int valor ? valor : 0;
        return (Convert.ToInt32(resultado.Value) == 1, mensaje.Value?.ToString() ?? "", nuevoId);
    }

    public (bool ok, string mensaje) Actualizar(Cliente c)
    {
        using SqlConnection cn = Conexion.ObtenerConexion();
        using SqlCommand cmd = Comando(cn, "sp_ActualizarCliente");
        cmd.Parameters.Add(IdCliente(c.ClienteId));
        AgregarDatosCliente(cmd, c);
        var (resultado, mensaje) = AgregarSalidas(cmd);

        cmd.ExecuteNonQuery();

        return (Convert.ToInt32(resultado.Value) == 1, mensaje.Value?.ToString() ?? "");
    }

    public (bool ok, string mensaje) CambiarEstado(int clienteId, bool activo)
    {
        using SqlConnection cn = Conexion.ObtenerConexion();
        using SqlCommand cmd = Comando(cn, "sp_CambiarEstadoCliente");
        cmd.Parameters.Add(IdCliente(clienteId));
        cmd.Parameters.Add("@estadoCliente", SqlDbType.Bit).Value = activo;
        var (resultado, mensaje) = AgregarSalidas(cmd);

        cmd.ExecuteNonQuery();

        return (Convert.ToInt32(resultado.Value) == 1, mensaje.Value?.ToString() ?? "");
    }

    public DataTable ListarLocaciones() => Consultar("sp_ListarLocaciones");

    public DataTable LocacionesPorCliente(int clienteId) =>
        Consultar("sp_ListarLocacionesPorCliente", IdCliente(clienteId));

    public DataTable OrdenesPorCliente(int clienteId) =>
        Consultar("sp_ListarOrdenesPorCliente", IdCliente(clienteId));

    public DataTable FacturasPorCliente(int clienteId) =>
        Consultar("sp_ListarFacturasPorCliente", IdCliente(clienteId));

    private static void AgregarDatosCliente(SqlCommand cmd, Cliente c)
    {
        cmd.Parameters.Add(Parametro("@nombreEmpresa", SqlDbType.VarChar, 50, c.Empresa));
        cmd.Parameters.Add(Parametro("@nombreCliente", SqlDbType.VarChar, 50, c.Nombre));
        cmd.Parameters.Add(Parametro("@apellidoCliente", SqlDbType.VarChar, 50, c.Apellido));
        cmd.Parameters.Add(Parametro("@telefonoCliente", SqlDbType.VarChar, 50, c.Telefono));
        cmd.Parameters.Add(Parametro("@identificacionCliente", SqlDbType.VarChar, 30, c.Identificacion));
        cmd.Parameters.Add(Parametro("@correoCliente", SqlDbType.VarChar, 50, c.Correo));
        cmd.Parameters.Add("@locacion_id", SqlDbType.Int).Value =
            c.LocacionId.HasValue ? c.LocacionId.Value : DBNull.Value;
    }

    private static (SqlParameter resultado, SqlParameter mensaje) AgregarSalidas(SqlCommand cmd)
    {
        SqlParameter resultado = cmd.Parameters.Add("@resultado", SqlDbType.Int);
        resultado.Direction = ParameterDirection.Output;

        SqlParameter mensaje = cmd.Parameters.Add("@mensaje", SqlDbType.VarChar, 100);
        mensaje.Direction = ParameterDirection.Output;

        return (resultado, mensaje);
    }

    private static DataTable Consultar(string procedimiento, params SqlParameter[] parametros)
    {
        using SqlConnection cn = Conexion.ObtenerConexion();
        using SqlCommand cmd = Comando(cn, procedimiento);
        cmd.Parameters.AddRange(parametros);

        DataTable tabla = new DataTable();
        using SqlDataAdapter adaptador = new SqlDataAdapter(cmd);
        adaptador.Fill(tabla);
        return tabla;
    }

    private static SqlCommand Comando(SqlConnection cn, string procedimiento)
    {
        return new SqlCommand(procedimiento, cn) { CommandType = CommandType.StoredProcedure };
    }

    private static SqlParameter IdCliente(int clienteId)
    {
        return new SqlParameter("@cliente_id", SqlDbType.Int) { Value = clienteId };
    }

    // Los textos vacíos se mandan como NULL
    private static SqlParameter Parametro(string nombre, SqlDbType tipo, int tamano, string? valor)
    {
        return new SqlParameter(nombre, tipo, tamano)
        {
            Value = string.IsNullOrWhiteSpace(valor) ? DBNull.Value : valor.Trim()
        };
    }
}
