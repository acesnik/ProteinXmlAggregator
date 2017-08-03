using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Proteomics;
using UsefulProteomicsDatabases;
using System.Text;
using System.Threading.Tasks;

namespace ProteinXmlAggregator.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> files = args.Where(f => File.Exists(f) & (Path.GetExtension(f) == ".xml" || Path.GetExtension(f) == ".xml.gz")).ToList();
            if (files.Count < 2)
            {
                Console.WriteLine("Please enter at least two protein .xml or .xml.gz databases.");
                return;
            }
            List<Protein> databases = files.SelectMany(f => ProteinDbLoader.LoadProteinXML(f, false, new List<Modification>(), false, new List<string>(), out Dictionary<string, Modification> un)).ToList();
            Dictionary<string, List<Protein>> proteinsByAcc = new Dictionary<string, List<Protein>>();
            foreach (Protein p in databases)
            {
                if (!proteinsByAcc.TryGetValue(p.Accession, out List<Protein> proteins)) proteinsByAcc.Add(p.Accession, new List<Protein> { p });
                else proteins.Add(p);
            }
            List<Protein>
        }

        public List<Protein> merge_proteins(KeyValuePair<string, List<Protein>> merge_these)
        {
            Dictionary<string, List<Protein>> proteinsBySequence = new Dictionary<string, List<Protein>>();
            foreach (Protein p in merge_these.Value)
            {
                if (!proteinsBySequence.TryGetValue(p.Accession, out List<Protein> asdf)) proteinsBySequence.Add(p.BaseSequence, new List<Protein> { p });
                else asdf.Add(p);
            }

            foreach (KeyValuePair<string, List<Protein>> proteins in proteinsBySequence)
            {
                HashSet<string> names = new HashSet<string>(proteins.Value.Select(p => p.Name));
                HashSet<string> fullnames = new HashSet<string>(proteins.Value.Select(p => p.FullName));
                HashSet<string> descriptions = new HashSet<string>(proteins.Value.Select(p => p.FullDescription));
                // all marked as target
                // all marked as not contaminants
                HashSet<Tuple<string, string>> genenames = new HashSet<Tuple<string, string>>(proteins.Value.SelectMany(p => p.GeneNames));
                HashSet<ProteolysisProduct> proteolysis = new HashSet<ProteolysisProduct>(proteins.Value.SelectMany(p => p.ProteolysisProducts));
                HashSet<SequenceVariation> variants = new HashSet<SequenceVariation>(proteins.Value.SelectMany(p => p.SequenceVariations));
                HashSet<DatabaseReference> references = new HashSet<DatabaseReference>(proteins.Value.SelectMany(p => p.DatabaseReferences));
                HashSet<DisulfideBond> bonds = new HashSet<DisulfideBond>(proteins.Value.SelectMany(p => p.DisulfideBonds));
                yield return new Protein(
                    proteins.Key, 
                    merge_these.Key, 
                    gene_names : genenames.ToList(), 
                    oneBasedModifications : GetModificationDict(proteins.Value.Select(p => p.OneBasedPossibleLocalizedModifications)),
                    proteolysisProducts : proteolysis.ToList(),
                    name : names.FirstOrDefault(),
                    full_name : fullnames.FirstOrDefault(),
                    databaseReferences : references.ToList(),
                    disulfideBonds : bonds.ToList(),
                    sequenceVariations : variants.ToList()
            }
        }

        private static IDictionary<int, List<Modification>> GetModificationDict(IEnumerable<IDictionary<int, List<Modification>>> mods)
        {
            var mod_dict = new Dictionary<int, HashSet<Modification>>();
            foreach (KeyValuePair<int, List<Modification>> nice in mods.SelectMany(x => x).ToList())
            {
                if (!mod_dict.TryGetValue(nice.Key, out HashSet<Modification> val))
                {
                    mod_dict.Add(nice.Key, new HashSet<Modification>(nice.Value));
                }
                else
                {
                    foreach (Modification mod in val)
                    {
                        val.Add(mod);
                    }
                }
            }
            return mod_dict.ToDictionary(kv => kv.Key, kv => kv.Value.ToList());
        }
    }
}
