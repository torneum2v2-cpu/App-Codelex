using Microsoft.Data.SqlClient;
using System.Data;

namespace Codelex.Clases_BDD
{
    class Locacion : BaseDatos
    {
        public string[] Tipos = { "Oficina", "Edificio", "Casa", "Local comercial", "Bodega",
                                  "Clínica / Hospital", "Centro educativo", "Industria", "Otro" };

        public bool InsertarLocacion(string rtn, string nombre, string tipo, string direccion, out string mensaje)
        {
            mensaje = ValidarLocacion(rtn, nombre, tipo, direccion);
            if (mensaje != "")
                return false;

            SqlParameter locacionId = new SqlParameter("@locacion_id", SqlDbType.Int);
            locacionId.Direction = ParameterDirection.Output;

            SqlCommand comando = Procedimiento("sp_InsertarLocacion");
            comando.Parameters.AddWithValue("@identificacionCliente", rtn.Trim());
            comando.Parameters.AddWithValue("@nombreLocacion", nombre.Trim());
            comando.Parameters.AddWithValue("@direccion", direccion.Trim());
            comando.Parameters.AddWithValue("@tipoLocacion", tipo.Trim());
            comando.Parameters.Add(locacionId);

            return Ejecutar(comando, out mensaje);
        }

        private string ValidarLocacion(string rtn, string nombre, string tipo, string direccion)
        {
            if (rtn.Trim() == "")
                return "Seleccione un cliente.";

            if (nombre.Trim() == "")
                return "El nombre de la locación es obligatorio.";
            if (nombre.Trim().Length > 60)
                return "El nombre de la locación no puede tener más de 60 caracteres.";

            if (tipo.Trim() == "")
                return "Seleccione el tipo de locación.";

            if (direccion.Trim() == "")
                return "La dirección es obligatoria.";
            if (direccion.Trim().Length > 80)
                return "La dirección no puede tener más de 80 caracteres.";

            Cliente clienteBD = new Cliente();
            DataTable existentes = clienteBD.LocacionesPorCliente(rtn);

            foreach (DataRow fila in existentes.Rows)
            {
                if (("" + fila["nombreLocacion"]).Trim().ToLower() == nombre.Trim().ToLower())
                    return "Este cliente ya tiene una locación con ese nombre.";
            }

            return "";
        }
    }
}