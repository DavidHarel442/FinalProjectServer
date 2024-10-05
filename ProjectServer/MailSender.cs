using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProjectServer
{
    internal class MailSender
    {// this class is incharge of managing the forgot password case

        /// <summary>
        /// this function sends the code to your mail that the client will put to change his password. using SMTP protocol
        /// </summary>
        /// <param name="mailTo"></param>
        /// <param name="code"></param>
        public void SendForgotPasswordMail(string mailTo, string code)
        {
            string Subject = "Forgot Password";
            string Content = "This is The Code u need to use to put a new password: " + code;
            SmtpClient smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("hathatulofficial@gmail.com", "jixd sxth iyow qhug\r\n")
            };
            string[] toMail2 = mailTo.Split('/');
            string mailFrom = "hathatulofficial@gmail.com";
            MailMessage msg = new System.Net.Mail.MailMessage(mailFrom, toMail2[0], Subject, Content);
            if (toMail2.Length > 1)
            {
                for (int i = 1; i < toMail2.Length; i++)
                {
                    msg.To.Add(toMail2[i]);
                }
            }
            smtpClient.Send(msg);
        }
        /// <summary>
        /// this function sends the code to your mail that the client will put to verify himself. using SMTP protocol
        /// </summary>
        /// <param name="mailTo"></param>
        /// <param name="code"></param>
        public void SendTwoStepAuthenticationMail(string mailTo, string code)
        {
            string Subject = "Two Step Authentication";
            string Content = "This is The Code u need to use to verify yourself: " + code;
            SmtpClient smtpClient = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("hathatulofficial@gmail.com", "jixd sxth iyow qhug\r\n")
            };
            string[] toMail2 = mailTo.Split('/');
            string mailFrom = "hathatulofficial@gmail.com";
            MailMessage msg = new System.Net.Mail.MailMessage(mailFrom, toMail2[0], Subject, Content);
            if (toMail2.Length > 1)
            {
                for (int i = 1; i < toMail2.Length; i++)
                {
                    msg.To.Add(toMail2[i]);
                }
            }
            smtpClient.Send(msg);
        }
    }
}
