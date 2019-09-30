using System;
using System.Text;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace WebSearcher
{
    public class MailHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties
        private string _webUrl = ConfigurationManager.AppSettings["SiteURL"];
        private string _fromMail = ConfigurationManager.AppSettings["FromMail"];
        private string _fromMailPassword = ConfigurationManager.AppSettings["FromMailPassword"];
        private string _fromMailDisplayName = ConfigurationManager.AppSettings["FromMailDisplayName"];
        private string _fromSubject = ConfigurationManager.AppSettings["FromSubject"];
        private string _fromHost = ConfigurationManager.AppSettings["FromHost"];
        private int _fromPort = int.Parse(ConfigurationManager.AppSettings["FromPort"]);
        private bool _fromEnableSsl = bool.Parse(ConfigurationManager.AppSettings["FromEnableSsl"]);
        private string _toMail = ConfigurationManager.AppSettings["ToMail"];
        private string[] _errorMails = ConfigurationManager.AppSettings["ErrorMails"].Split(';');
        private string _errorMailSubject = ConfigurationManager.AppSettings["ErrorMailSubject"];
        private string _keyWords = ConfigurationManager.AppSettings["KeyWords"];
        #endregion

        public void SendNotification(List<TableData> filteredTableData)
        {
            log.Info("Sending notification mail...");

            MailAddress from = new MailAddress(_fromMail, _fromMailDisplayName);
            MailAddress to = new MailAddress(_toMail);

            SmtpClient client = ReturnSmtpClient();

            MailMessage mail = new MailMessage(from, to);
            mail.Priority = MailPriority.High;
            mail.IsBodyHtml = true;
            mail.Subject = _fromSubject;
            StringBuilder mailBody = new StringBuilder();
            mailBody.Append(
                "<p>Sehr geehrte Damen und Herren,</p>" +
                "<p>zu den hinterlegten Stichworten \"<span style='background:yellow'>" + _keyWords + "</span>\" für die Ausschreibungen der Pensionsversicherungsanstalt wurde(n) " + filteredTableData.Count + " Ergebnis(se) gefunden.</p>" +
                "<p><table><tbody><tr><th>Titel</th><th>Beginn</th><th>Ende</th></tr>"
            );

            foreach (TableData data in filteredTableData)
            {
                mailBody.Append(
                    "<tr><td>" + data.Title + "</td><td>" + data.Start + "</td><td>" + data.End + "</td></tr>"
                );
            }

            mailBody.Append(
                "</tbody></table></p>" + 
                "<p><strong>Link zu den Ausschreibungen: <a href='" + _webUrl + "'>Link</a></strong></p>" +
                "<p>mfg</p>" +
                "<p>Ausschreibung-Melder</p>"
            );

            mail.Body = mailBody.ToString();

            client.Send(mail);
            log.Info("Notification mail was sent successfully!");
        }

        public void SendErrorNotification(string exMsg)
        {
            try
            {
                MailAddress from = new MailAddress(_fromMail, _fromMailDisplayName);
                SmtpClient client = ReturnSmtpClient();

                foreach (string toMail in _errorMails)
                {
                    log.Info("Sending error notification mail...");

                    MailAddress to = new MailAddress(toMail);
                    MailMessage mail = new MailMessage(from, to);
                    mail.Priority = MailPriority.High;
                    mail.IsBodyHtml = true;
                    mail.Subject = _errorMailSubject;
                    mail.Body = "<p>Beim letzten Durchlauf des Programms trat ein Fehler auf.</p>" +
                                "<p>Fehlermeldung:</p>" +
                                "<p style='color:red'>" + exMsg + "</p>";

                    client.Send(mail);

                    log.Info("Error notification mail was sent successfully!");
                }
            }
            catch (Exception ex)
            {
                log.Info(ex.Message);
            }
        }

        private SmtpClient ReturnSmtpClient()
        {
            return new SmtpClient
            {
                Host = _fromHost,
                Port = _fromPort,
                EnableSsl = _fromEnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_fromMail, _fromMailPassword)
            };
        }
    }
}
