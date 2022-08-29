using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DU_Industry_Tool
{
    public class MarketData
    {
        public ulong MarketId { get; set; }
        public ulong OrderId { get; set; }
        public ulong ItemType { get; set; }
        public long BuyQuantity { get; set; } // Negative when selling
        public DateTime ExpirationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public ulong PlayerId { get; set; }
        public ulong OrganizationId { get; set; }
        public string OwnerName { get; set; }
        public double Price { get; set; }
        public DateTime LogDate { get; set; }
    }

    public class SaveableMarketData
    {
        public List<string> CheckedLogFiles { get; set; } = new List<string>();
        public Dictionary<ulong, MarketData> Data = new Dictionary<ulong, MarketData>();
        public string LogFolderPath { get; set; }
    }
}
