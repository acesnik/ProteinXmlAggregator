using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Proteomics;
using ProteinXmlAggregator;

namespace Test
{
    [TestFixture]
    public sealed class TestProteinMerge
    {
        [Test]
        public void merge_a_couple_proteins()
        {
            ModificationMotif.TryGetMotif("A", out ModificationMotif motif);
            Protein p = new Protein(
                "ASEQUENCE",
                "id",
                isContaminant: false,
                isDecoy: false,
                name: "name",
                full_name: "full_name",
                gene_names: new List<Tuple<string, string>> { new Tuple<string, string>("gene", "name") },
                databaseReferences: new List<DatabaseReference> { new DatabaseReference("ref", "id", new List<Tuple<string, string>> { new Tuple<string, string>("type", "property") })},
                sequenceVariations: new List<SequenceVariation> { new SequenceVariation(1, 2, "A", "B", "var") },
                proteolysisProducts: new List<ProteolysisProduct> { new ProteolysisProduct(1, 2, "prod") },
                oneBasedModifications: new Dictionary<int, List<Modification>> { { 1, new List<Modification> { new ModificationWithMass("mod", new Tuple<string, string>("acc", "acc"), motif, TerminusLocalization.Any, 1, null, null, null, "type") } } }
                );

            Protein p2 = new Protein(
                "ASEQUENCE",
                "id",
                isContaminant: false,
                isDecoy: false,
                name: "name",
                full_name: "full_name",
                gene_names: new List<Tuple<string, string>> { new Tuple<string, string>("gene", "name") },
                databaseReferences: new List<DatabaseReference> { new DatabaseReference("ref", "id", new List<Tuple<string, string>> { new Tuple<string, string>("type", "property") })},
                sequenceVariations: new List<SequenceVariation> { new SequenceVariation(1, 2, "A", "B", "var") },
                proteolysisProducts: new List<ProteolysisProduct> { new ProteolysisProduct(1, 2, "prod") },
                oneBasedModifications: new Dictionary<int, List<Modification>> { { 1, new List<Modification> { new ModificationWithMass("mod2", new Tuple<string, string>("acc2", "acc2"), motif, TerminusLocalization.Any, 10, null, null, null, "type") } } }
                );

            List<Protein> merged = Aggregator.merge_proteins(new List<Protein> { p, p2 }).ToList();
            Assert.AreEqual(1, merged.Count);
            Assert.AreEqual(1, merged.First().DatabaseReferences.Count());
            Assert.AreEqual(1, merged.First().GeneNames.Count());
            Assert.AreEqual(1, merged.First().SequenceVariations.Count());
            Assert.AreEqual(1, merged.First().ProteolysisProducts.Count());
            Assert.AreEqual(p.OneBasedPossibleLocalizedModifications.First().Value.First().GetHashCode(), p2.OneBasedPossibleLocalizedModifications.First().Value.First().GetHashCode());
            Assert.AreNotEqual(p.OneBasedPossibleLocalizedModifications.First().Value.First(), p2.OneBasedPossibleLocalizedModifications.First().Value.First());
            Assert.AreEqual(1, merged.First().OneBasedPossibleLocalizedModifications.Count());
            Assert.AreEqual(2, merged.First().OneBasedPossibleLocalizedModifications.First().Value.Count);
        }
    }
}
