using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UsefulProteomicsDatabases;

namespace ProteinXmlAggregator
{
    public class ProteinXmlAggregator
    {
        static void Main(string[] args)
        {
            List<string> files = args.Where(f => File.Exists(f) & (Path.GetExtension(f) == ".xml" || Path.GetExtension(f) == ".xml.gz")).ToList();
            if (files.Count < 2)
            {
                Console.WriteLine("Please enter at least two protein .xml or .xml.gz databases.");
                return;
            }
            List<Protein> merged = merge_proteins(files.SelectMany(f => ProteinDbLoader.LoadProteinXML(f, false, new List<Modification>(), false, new List<string>(), out Dictionary<string, Modification> un))).ToList();
            ProteinDbWriter.WriteXmlDatabase(null, merged, Path.Combine(Path.GetDirectoryName(files.First()), "new_database_" + DateTime.Now.Month + DateTime.Now.Date + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".xml"));
        }

        /// <summary>
        /// Merges proteins of the same accession and sequence. Considers contaminants separately.
        /// </summary>
        /// <param name="merge_these"></param>
        /// <returns></returns>
        public static IEnumerable<Protein> merge_proteins(IEnumerable<Protein> merge_these)
        {
            Dictionary<Tuple<string, string, bool, bool>, List<Protein>> proteins_by_accession_seq_cont_isdecoy = new Dictionary<Tuple<string, string, bool, bool>, List<Protein>>();
            foreach (Protein p in merge_these)
            {
                Tuple<string, string, bool, bool> key = new Tuple<string, string, bool, bool>(p.Accession, p.BaseSequence, p.IsContaminant, p.IsDecoy);
                if (!proteins_by_accession_seq_cont_isdecoy.TryGetValue(key, out List<Protein> bundled))
                    proteins_by_accession_seq_cont_isdecoy.Add(key, new List<Protein> { p });
                else
                    bundled.Add(p);
            }

            foreach (KeyValuePair<Tuple<string, string, bool, bool>, List<Protein>> proteins in proteins_by_accession_seq_cont_isdecoy)
            {
                HashSet<string> names = new HashSet<string>(proteins.Value.Select(p => p.Name));
                HashSet<string> fullnames = new HashSet<string>(proteins.Value.Select(p => p.FullName));
                HashSet<string> descriptions = new HashSet<string>(proteins.Value.Select(p => p.FullDescription));
                HashSet<Tuple<string, string>> genenames = new HashSet<Tuple<string, string>>(proteins.Value.SelectMany(p => p.GeneNames));
                HashSet<ProteolysisProduct> proteolysis = new HashSet<ProteolysisProduct>(proteins.Value.SelectMany(p => p.ProteolysisProducts));
                HashSet<SequenceVariation> variants = new HashSet<SequenceVariation>(proteins.Value.SelectMany(p => p.SequenceVariations));
                HashSet<DatabaseReference> references = new HashSet<DatabaseReference>(proteins.Value.SelectMany(p => p.DatabaseReferences));
                HashSet<DisulfideBond> bonds = new HashSet<DisulfideBond>(proteins.Value.SelectMany(p => p.DisulfideBonds));

                Dictionary<int, List<Modification>> mod_dict = new Dictionary<int, List<Modification>>();
                foreach (KeyValuePair<int, List<Modification>> nice in proteins.Value.SelectMany(p => p.OneBasedPossibleLocalizedModifications).ToList())
                {
                    if (!mod_dict.TryGetValue(nice.Key, out List<Modification> val))
                        mod_dict.Add(nice.Key, new List<Modification>(nice.Value));
                    else
                        foreach (Modification mod in nice.Value)
                        {
                            if (!val.Any(m => m.Equals(mod)))
                                val.Add(mod); // consider modification mass, which isn't hashed
                        }
                }

                yield return new Protein(
                    proteins.Key.Item2,
                    proteins.Key.Item1,
                    isContaminant : proteins.Key.Item3,
                    isDecoy : proteins.Key.Item4,
                    gene_names: genenames.ToList(),
                    oneBasedModifications: mod_dict,
                    proteolysisProducts: proteolysis.ToList(),
                    name: names.FirstOrDefault(),
                    full_name: fullnames.FirstOrDefault(),
                    databaseReferences: references.ToList(),
                    disulfideBonds: bonds.ToList(),
                    sequenceVariations: variants.ToList()
                    );
            }
        }
    }
}
