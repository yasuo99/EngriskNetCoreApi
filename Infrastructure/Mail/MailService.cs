using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Mail
{
    public static class MailService
    {
        static bool mailSent = false;
        public static void SendMail(string to, string fullname,string subject, string body)
        {
            using (var message = new MailMessage())
            {
                message.To.Add(new MailAddress(to, fullname));
                message.From = new MailAddress("from@email.com", "Engrisk");
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587;
                    client.Credentials = new NetworkCredential("yasuo120999@gmail.com", "Thanhpro1999@");
                    client.EnableSsl = true;
                    client.Send(message);
                }
            }
        }
    }
}