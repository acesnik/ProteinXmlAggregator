using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UsefulProteomicsDatabases;

namespace ProteinXmlAggregator
{
    public class Aggregator
    {
        static void Main(string[] args)
        {
            List<string> files = args.Where(f => File.Exists(f) & (Path.GetExtension(f) == ".xml" || Path.GetExtension(f) == ".xml.gz")).ToList();
            if (files.Count < 2)
            {
                Console.WriteLine("Please enter at least two protein .xml or .xml.gz databases.");
                return;
            }

            // check that file path is valid
            string timestamp = DateTime.Now.Year.ToString("0000") + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + "-" + DateTime.Now.Hour.ToString("00") + "-" + DateTime.Now.Minute.ToString("00") + "-" + DateTime.Now.Second.ToString("00");
            string outpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "merged_database_" + timestamp + ".xml");

            // merge databases
            Loaders.LoadElements(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "elements.dat"));
            List<Protein> merged = ProteinDbLoader.merge_proteins(files.SelectMany(f => ProteinDbLoader.LoadProteinXML(f, false, new List<Modification>(), false, new List<string>(), out Dictionary<string, Modification> un))).ToList();
            ProteinDbWriter.WriteXmlDatabase(new Dictionary<string, HashSet<Tuple<int, Modification>>>(), merged, outpath);
        }
    }
}
