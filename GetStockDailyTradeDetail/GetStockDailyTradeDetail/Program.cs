using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetStockDailyTradeDetail
{
    class Program
    {
        public const int MAXTHREAD = 100;
        static void Main(string[] args)
        {           
            GetDailyDataHelper.GetDailyData(new DateTime(2016,9,1));
            Console.ReadKey();
        }
    }
}
