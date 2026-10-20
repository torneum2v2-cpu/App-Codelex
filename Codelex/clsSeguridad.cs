using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;

namespace Codelex
{
    internal class clsSeguridad
    {

        public clsSeguridad()
        {

        }

        public static string CrearHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }


        public static bool Verificar(string password, string hashGuardado)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashGuardado);
        }
    }
}
