using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text;
using System.Threading.Tasks;
using static Codelex.Conexion;

namespace Codelex
{
    public class RecuperarCuenta
    {
        private SmtpClient _smtpClient;

        protected string remitentecorreo { get; set; }

        protected string password { get; set; }

        protected string host { get; set; }

        protected int port { get; set; }

        protected bool ssl { get; set; }

        protected void iniciarclienteSmt()
        {
            _smtpClient = new SmtpClient();
            _smtpClient.Host = host;
            _smtpClient.Port = port;
            _smtpClient.EnableSsl = ssl;
            _smtpClient.UseDefaultCredentials = false;
            _smtpClient.Credentials = new NetworkCredential(remitentecorreo, password);

        }

        private static string GenerarCodigo()
        {
            return RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
        }

        public void sendMail(string subject, string body, List<string> destinatarios)
        {
            var mailMessage = new MailMessage();


            try
            {
                mailMessage.From = new MailAddress(remitentecorreo);

                foreach(string mail in destinatarios)
                {
                    mailMessage.To.Add(mail);
                }

                mailMessage.Subject = subject;

                mailMessage.Body = body;

                mailMessage.Priority = MailPriority.Normal;

                _smtpClient.Send(mailMessage);
            }
            catch (Exception ex) { }
            finally
            {
                mailMessage.Dispose();
                _smtpClient.Dispose();
            }
            

            

        }



        public string recoverPassword(string UsuarioSolicitado)
        {

            Conexion con = new Conexion();
            using(SqlConnection conexion = Abrir_conexion.ObtenerConexion())
            {
                

                using (var command = new SqlCommand())
                {
                    command.Connection = conexion;
                    command.CommandText = "select * from Empleados where correoEmpleado = @correo";
                    command.Parameters.AddWithValue("@login", UsuarioSolicitado);
                    command.Parameters.AddWithValue("@correo", UsuarioSolicitado);
                    command.CommandType = System.Data.CommandType.Text;

                    SqlDataReader reader = command.ExecuteReader();

                    if(reader.Read())
                    {
                        string nombreusuario = reader.GetString(2);
                        string correo = reader.GetString(reader.GetOrdinal("correoEmpleado"));
                        

                        var mailService = new CorreoSoporte();

                        mailService.sendMail(subject: "SISTEMA DE CAMBIO DE CLAVE: Solicitud de cambio de clave", body: "Hola " + nombreusuario + "\nUsted solicito recuperar su clave", destinatarios: new List<string> { correo });

                        return "Hola, " + nombreusuario + "\nUsted Solicito recuperar su clave";
                    }
                    else
                    {
                        return "lo ssentimos, no tiene un cuenta con ese correo";
                    }
                }
            }
        }

            

    }
}
