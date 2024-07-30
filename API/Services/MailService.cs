using System.Net.Mail;
using API.Model;

namespace API.Services
{
    public class MailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ModifyMail(string message, Dictionary<string, string> parameterList)
        {
            foreach (var item in parameterList)
            {
                message = message.Contains($"<{item.Key}>") ? message.Replace($"<{item.Key}>", item.Value) : message;
            }

            return message;
        }

        public async Task SendEmail(string subject, string body, List<string> to, List<string>? copie = null)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:mail"]!, _configuration["Email:alias"])
                };

                foreach (string email in to)
                {
                    mailMessage.To.Add(email);
                }

                if (copie != null)
                {
                    foreach (string email in copie)
                    {
                        mailMessage.CC.Add(email);
                    }
                }

                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = subject;
                mailMessage.Body = body + @"
                    <br /><br />
                    <p style=""font-style: italic; font-size: 12px; "">Merci de ne pas répondre à ce message. Les réponses à ce message sont acheminées vers une boîte aux lettres non surveillée.</p>
                ";

                var smtp = new SmtpClient(_configuration["Email:smtp"])
                {
                    Port = int.Parse(_configuration["Email:port"]!),
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(_configuration["Email:mail"], _configuration["Email:password"]),
                    EnableSsl = true,
                    Host = _configuration["Email:smtp"]!
                };

                await smtp.SendMailAsync(mailMessage);
            }
            catch (SmtpFailedRecipientsException)
            {
            }
        }

        public async Task SendEmailWithAttachements(string subject, string body, List<string> to, List<string> attachementsPath)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:mail"]!, _configuration["Email:alias"])
                };

                foreach (string email in to)
                {
                    mailMessage.To.Add(email);
                }

                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = subject;
                mailMessage.Body = body + @"
                    <br /><br />
                    <p style=""font-style: italic; font-size: 12px; "">Merci de ne pas répondre à ce message. Les réponses à ce message sont acheminées vers une boîte aux lettres non surveillée.</p>
                ";

                for (int i = 0; i < attachementsPath.Count; i += 1)
                {
                    mailMessage.Attachments.Add(new Attachment(attachementsPath[i]));
                }

                var smtp = new SmtpClient(_configuration["Email:smtp"])
                {
                    Port = int.Parse(_configuration["Email:port"]!),
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(_configuration["Email:mail"], _configuration["Email:password"]),
                    EnableSsl = true,
                    Host = _configuration["Email:smtp"]!
                };

                await smtp.SendMailAsync(mailMessage);
            }
            catch (SmtpFailedRecipientsException)
            {
            }
        }

        public async Task SendAppropriateMail(MailType type, Dictionary<string, string>? parameterList, List<string> to, List<string>? copie = null)
        {
            string body = ModifyMail("", parameterList);

            // await SendEmail(mail.Subject, body, to, copie);
        }
    }
}
