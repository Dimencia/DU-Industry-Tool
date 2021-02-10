using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DU_Industry_Tool
{
    // Should handle parsing and storing market data.  
    // If it takes a while to parse the logfiles we'll store it ourselves
    // Hell, even if it doesn't we should store it and store which logfiles we've checked out so we don't check the same ones again?
    // But they might delete their logfiles, but we still want our data.
    public class MarketManager
    {

        // This is a terrible idea.
        private Regex MarketRegex = new Regex(@"MarketOrder:\[marketId = ([0-9]*), orderId = ([0-9]*), itemType = ([0-9]*), buyQuantity = ([\-0-9]*), expirationDate = @\([0-9]*\) ([^,]*), updateDate = @\([0-9]*\) ([^,]*), ownerId = EntityId:\[playerId = ([0-9]*), organizationId = ([0-9]*)\], ownerName = ([^,]*), unitPrice = Currency:\[amount = ([0-9]*)");

        public Dictionary<ulong,MarketData> MarketOrders = new Dictionary<ulong, MarketData>(); // Indexed by orderId for our purposes
        private List<string> CheckedLogFiles = new List<string>();
        public string _logFolderPath { get; set; }

        public MarketManager()
        {
            // First load a config file if there is one
            if (File.Exists("MarketOrders.json"))
            {
                var loadedInfo = JsonConvert.DeserializeObject<SaveableMarketData>(File.ReadAllText("MarketOrders.json"));
                MarketOrders = loadedInfo.Data;
                CheckedLogFiles = loadedInfo.CheckedLogFiles;
                _logFolderPath = loadedInfo.LogFolderPath;
            }
            if (_logFolderPath == null)
                _logFolderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"NQ\DualUniverse\log");

            // Do an initial scan?
            //UpdateMarketData();
            Console.WriteLine("Parsed " + MarketOrders.Count + " market orders from settings file");
        }


        public void UpdateMarketData(LoadingForm form = null)
        {

            // Before we read log files, discard any that are too old in our current collection
            var oldOrders = MarketOrders.Where(o => o.Value.ExpirationDate < DateTime.Now).ToList();
            foreach (var kvp in oldOrders)
                MarketOrders.Remove(kvp.Key);

            // Find the most recently updated one, and remove it from CheckedLogFiles if it's in there
            var directory = new DirectoryInfo(_logFolderPath);
            var mostRecent = directory.GetFiles().OrderByDescending(f => f.LastWriteTime).First();
            if (CheckedLogFiles.Contains(mostRecent.Name))
                CheckedLogFiles.Remove(mostRecent.Name);

            int numProcessed = 0;
            var files = System.IO.Directory.GetFiles(_logFolderPath, "*.xml").Where(f => !CheckedLogFiles.Contains(Path.GetFileName(f))).ToArray();


            DateTime lastDate = DateTime.MinValue;

            foreach (var file in files)
            {
                if (!CheckedLogFiles.Contains(Path.GetFileName(file)))
                {
                    var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Write);
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        while (!reader.EndOfStream)
                        {
                            string contents = reader.ReadLine();
                            // First see if we match a date
                            var dateMatch = Regex.Match(contents, @"<date>([^<]*)");
                            if (dateMatch.Success)
                            {
                                lastDate = DateTime.Parse(dateMatch.Groups[1].Value, null, DateTimeStyles.RoundtripKind);
                            }

                            var matches = MarketRegex.Matches(contents);
                            foreach (Match match in matches)
                            {
                                var data = new MarketData();
                                data.MarketId = ulong.Parse(match.Groups[1].Value);
                                data.OrderId = ulong.Parse(match.Groups[2].Value);
                                data.ItemType = ulong.Parse(match.Groups[3].Value);
                                data.BuyQuantity = long.Parse(match.Groups[4].Value);
                                data.ExpirationDate = DateTime.ParseExact(match.Groups[5].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                data.UpdateDate = DateTime.ParseExact(match.Groups[6].Value, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                data.PlayerId = ulong.Parse(match.Groups[7].Value);
                                data.OrganizationId = ulong.Parse(match.Groups[8].Value);
                                data.OwnerName = match.Groups[9].Value;
                                data.Price = ulong.Parse(match.Groups[10].Value)/100; // Weirdly, their prices are *100
                                data.LogDate = lastDate;


                                if (data.ExpirationDate > DateTime.Now)
                                {

                                    if (MarketOrders.ContainsKey(data.OrderId))
                                    {
                                        if (MarketOrders[data.OrderId].UpdateDate < data.UpdateDate)
                                            MarketOrders[data.OrderId] = data;
                                    }
                                    else
                                        MarketOrders[data.OrderId] = data;
                                }
                            }
                        }
                    }
                    CheckedLogFiles.Add(Path.GetFileName(file));
                    numProcessed++;
                    Console.WriteLine("Finished log file " + numProcessed + " of " + files.Length);
                    if (form != null)
                        form.UpdateProgressBar((int)(((float)numProcessed / files.Length) * 100));
                }
            }
            // Alright, here's the fun part.  Group all of them by ItemType, and then find the most recent LogTime for that ItemType.  Discard all who don't have that same LogTime
            foreach(var group in MarketOrders.GroupBy(o => o.Value.ItemType).ToList())
            {
                var MostRecentLogTime = group.OrderByDescending(o => o.Value.LogDate).First().Value.LogDate;
                foreach(var order in group)
                {
                    if (order.Value.LogDate != MostRecentLogTime)
                        MarketOrders.Remove(order.Key);
                }
            }
            Console.WriteLine("Parsed " + MarketOrders.Count + " market orders from log files");
            // And save it
            var saveable = new SaveableMarketData() { CheckedLogFiles = CheckedLogFiles, Data = MarketOrders, LogFolderPath = _logFolderPath };
            File.WriteAllText("MarketOrders.json", JsonConvert.SerializeObject(saveable));
        }

        private const char CR = '\r';
        private const char LF = '\n';
        private const char NULL = (char)0;
        public static long CountLinesSmarter(Stream stream)
        {

            var lineCount = 0L;

            var byteBuffer = new byte[1024 * 1024];
            var detectedEOL = NULL;
            var currentChar = NULL;

            int bytesRead;
            while ((bytesRead = stream.Read(byteBuffer, 0, byteBuffer.Length)) > 0)
            {
                for (var i = 0; i < bytesRead; i++)
                {
                    currentChar = (char)byteBuffer[i];

                    if (detectedEOL != NULL)
                    {
                        if (currentChar == detectedEOL)
                        {
                            lineCount++;
                        }
                    }
                    else if (currentChar == LF || currentChar == CR)
                    {
                        detectedEOL = currentChar;
                        lineCount++;
                    }
                }
            }

            // We had a NON-EOL character at the end without a new line
            if (currentChar != LF && currentChar != CR && currentChar != NULL)
            {
                lineCount++;
            }
            return lineCount;
        }
    }
}
