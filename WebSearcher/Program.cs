using System;

namespace WebSearcher
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            try
            {
                log.Info("<-----START----->");
                WebSearcher ws = new WebSearcher();
                ws.RunMain();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                MailHandler mh = new MailHandler();
                mh.SendErrorNotification(ex.Message);
            }
            finally
            {
                log.Info("<------END-s----->");
            }
        }
    }
}
