using System.Net.Mail;
using System.Net;

namespace Shopping_Tutorial.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("anhhung8hot@gmail.com", "ytzmcznmpgigwquf")
            };

            return client.SendMailAsync(
                new MailMessage(from: "anhhung8hot@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
