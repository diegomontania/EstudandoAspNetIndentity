using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace ByteBank.Forum.App_Start.Identity
{
    public class EmailServico : IIdentityMessageService
    {
        //para enviar emails em grande volume, pode-se utilizar um serviço de terceiros como :
        //https://www.mailgun.com/ ou https://sendgrid.com/

        //acessando as configurações do 'Web.config'
        private readonly string EMAIL_ORIGEM = ConfigurationManager.AppSettings["emailServico:email_remetente"];
        private readonly string EMAIL_SENHA = ConfigurationManager.AppSettings["emailServico:email_senha"];

        //Neste exemplo irá ser utilizado a biblioteca do .net para enviar os emails
        public async Task SendAsync(IdentityMessage message)
        {
            //criando mensagem de email
            using (var mensagemDeEmail = new MailMessage())
            {
                //quem está enviando
                mensagemDeEmail.From = new MailAddress(EMAIL_ORIGEM);

                //assunto, destinatario e corpo
                mensagemDeEmail.Subject = message.Subject;
                mensagemDeEmail.To.Add(message.Destination);
                mensagemDeEmail.Body = message.Body;

                //protocolop SMTP
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(EMAIL_ORIGEM, EMAIL_SENHA);

                    //configurações do gmail
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.EnableSsl = true;
                    smtpClient.Timeout = 20_000;

                    //envia o email
                    await smtpClient.SendMailAsync(mensagemDeEmail);
                }
            }
        }
    }
}