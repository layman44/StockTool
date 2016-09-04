using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace GetStockDailyTradeDetail
{
    public class GetDailyDataHelper
    {
        private static List<string> codelist = new List<string>();

        private const string qqurl = "http://stock.gtimg.cn/data/index.php?appn=detail&action=download&c={0}&d={1}";

        private const string sinaurl = "http://market.finance.sina.com.cn/downxls.php?date={0}&symbol={1}";

        private const string logfilepath = @"D:\log.txt";

        private const string stockpath = @"F:\yc\projects\C#\Stock\test\allstock.txt";

        private static List<string> useragent = new List<string>()
        {
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/14.0.835.163 Safari/535.1",
            "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:6.0) Gecko/20100101 Firefox/6.0",
            "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/534.50 (KHTML, like Gecko) Version/5.1 Safari/534.50",
            "Opera/9.80 (Windows NT 6.1; U; zh-cn) Presto/2.9.168 Version/11.50",
            "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Win64; x64; Trident/5.0; .NET CLR 2.0.50727; SLCC2; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; Tablet PC 2.0; .NET4.0E)",
            "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.1 (KHTML, like Gecko) Chrome/13.0.782.41 Safari/535.1 QQBrowser/6.9.11079.201"
        };
        private static Random random = new Random();

        private static object lockobj = new object();

        private static void GetCodeList(string filepath)
        {
            if (codelist.Count == 0)
            {
                var allcodes = File.ReadAllText(filepath);
                codelist = allcodes.Split(' ').ToList();
            }
        }
        private static void DownloadFile(object odata)
        {
            ParamData data = odata as ParamData;
            var code = data.code;
            var url = data.url;
            var date = data.date;
            var filename = string.Format("D:\\tradedata\\{0}\\{1}.xls",date, code);
            var fileinfo = new FileInfo(filename);
            if (!Directory.Exists(fileinfo.DirectoryName)) Directory.CreateDirectory(fileinfo.DirectoryName);
            try
            {
                System.Net.HttpWebRequest myrq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                myrq.UserAgent = useragent[random.Next(5)];
                myrq.Timeout = 2000;
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)myrq.GetResponse();
                if (myrp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    long totalBytes = myrp.ContentLength;
                    System.IO.Stream st = myrp.GetResponseStream();
                    System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                    long totalDownloadedByte = 0;
                    byte[] by = new byte[1024];
                    int osize = st.Read(by, 0, (int)by.Length);
                    while (osize > 0)
                    {
                        totalDownloadedByte = osize + totalDownloadedByte;
                        so.Write(by, 0, osize);
                        osize = st.Read(by, 0, (int)by.Length);
                    }
                    so.Close();
                    st.Close();
                }
                else
                {
                    lock (lockobj)
                    {
                        File.AppendAllText(logfilepath, "code:[" + code + "] 超时");
                    }
                }
            }
            catch (Exception exc)
            {
                lock (lockobj)
                {
                    File.AppendAllText(logfilepath, "code:[" + code + "] " + exc.ToString());
                }
            }
        }
        public static void GetDailyData(DateTime dt)
        {
            GetCodeList(stockpath);
            var formatdate = dt.ToString("yyyyMMdd");
            var formatcode = string.Empty;
            foreach (string code in codelist)
            {
                if (code.StartsWith("6"))
                {
                    formatcode = "sh" + code;
                }
                else
                {
                    formatcode = "sz" + code;
                }
                ParamData data = new ParamData(string.Format(qqurl, formatcode, formatdate), code, formatdate);
                ThreadPool.QueueUserWorkItem(DownloadFile, data);
            }
        }
    }

    public class ParamData
    {
        public string url;
        public string code;
        public string date;
        public ParamData(string url, string code, string date)
        {
            this.url = url;
            this.code = code;
            this.date = date;
        }
    }
}
