using System.Net.Mail;

namespace TekrarApp.Helpers
{
    public class EmailHelper
    {
        public bool SendEmail(string email, string message)
        {
            #region MailMessage
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("bticaret01@gmail.com");
            mailMessage.To.Add(email);
            mailMessage.Subject = "Üyelik Onaylama";
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;
            #endregion

            #region SmtpSettings
            SmtpClient client = new SmtpClient();
            client.Credentials = new System.Net.NetworkCredential("bticaret01@gmail.com", "Proje3434.**");
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            #endregion

            try
            {
                client.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
