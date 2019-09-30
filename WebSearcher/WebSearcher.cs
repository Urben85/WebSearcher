using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Text;
using HtmlAgilityPack;

namespace WebSearcher
{
    public class WebSearcher
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Properties
        private string _webUrl = ConfigurationManager.AppSettings["SiteURL"];
        private string _tableClass = ConfigurationManager.AppSettings["TableClass"];
        private List<TableData> _downloadedTableData = new List<TableData>();
        private string[] _keyWords = ConfigurationManager.AppSettings["KeyWords"].Split(';');
        private List<TableData> _filteredTableData = new List<TableData>();
        #endregion

        public void RunMain ()
        {
            DownloadTableData();
            FilterTableData();

            if (_filteredTableData.Count != 0)
            {
                log.Info("Found matches: " + _filteredTableData.Count);
                MailHandler mh = new MailHandler();
                mh.SendNotification(_filteredTableData);
            }
            else
                log.Info("Nothing was found.");
        }

        #region Private Methods
        private void DownloadTableData ()
        {
            log.Info("Try downloading table...");
            WebClient client = new WebClient();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(client.DownloadString(_webUrl));

            HtmlNodeCollection tableNodes = htmlDoc.DocumentNode.SelectNodes("//table[@class='" + _tableClass + "']");
            if (tableNodes != null)
            {
                foreach (HtmlNode table in tableNodes)
                {
                    foreach (HtmlNode row in table.SelectNodes("tr").Skip(1))
                    {
                        HtmlNodeCollection cells = row.SelectNodes("th|td");
                        TableData data = new TableData();
                        for (int i = 0; i < 3; i++)
                        {
                            HtmlNode cell = cells[i];
                            if (i == 0)
                            {
                                data.Title = ReturnCellValue(cell.InnerText);
                                data.LinkHtml = ReturnCellValue(cell.InnerHtml);
                            }
                            else
                                data.End = ReturnCellValue(cell.InnerText);
                        }
                        _downloadedTableData.Add(data);
                    }
                }
            }
            else
                throw new ArgumentException("Table \"" + _tableClass + "\" not found!");
        }

        private void FilterTableData()
        {
            log.Info("Filtering table by keywords...");
            foreach (TableData downloadedTableData in _downloadedTableData)
            {
                bool foundSomething = false;
                foreach (string keyWord in _keyWords)
                {
                    if (downloadedTableData.Title.ToLower().Contains(keyWord.ToLower()))
                        foundSomething = true;
                }
                if (foundSomething)
                    _filteredTableData.Add(downloadedTableData);
            }
        }

        private string ReturnCellValue (string cellValue)
        {
            cellValue = Encoding.UTF8.GetString(Encoding.Default.GetBytes(cellValue));
            StringBuilder value = new StringBuilder(cellValue);
            value.Replace("&nbsp;", "");
            return value.ToString();
        }
        #endregion
    }
}
