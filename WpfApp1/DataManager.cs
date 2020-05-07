using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VDS.RDF;


namespace WpfApp1
{
    public class DataManager
    {
        public List<Tuple<string, string, string>> Triples { get; set; }

        public int MainClassesCount { get; set; }
        public int AllClassesCount { get; set; }
        public int TypesCount { get; set; }
        public int PredicatesCount { get; set; }
        public int PrefixesCount { get; set; }
        public int WhyCount { get; set; }
        public int WhatCount { get; set; }
        public int WhatForCount { get; set; }
        public int Count { get; set; }

        public Dictionary<string, int> counts;
        public Dictionary<string, string> prefNamespaces;
        public List<string> properties;
        public List<string> objects;
        public List<string> classes;
        


        public Graph graph;
        public string sourceFile;

        public DataManager()
        {
            properties = new List<string>();
            objects  = new List<string>();
            classes  = new List<string>();

            sourceFile = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Source.n3";
            graph = new Graph();
            graph.LoadFromFile(sourceFile);
            prefNamespaces = new Dictionary<string, string>();
            InitDataSources();

            GetNamespaceMap();
        }

        public List<Tuple<string, string, string>> GetTriplesStringList(BaseTripleCollection triples)
        {
            List<Tuple<string, string, string>> temp = new List<Tuple<string, string, string>>();
            foreach (var triple in triples)
            {
                Tuple<string, string, string> tempTuple;
                string item1 = "";
                string item2 = "";
                string item3 = "";
                if (triple.Subject.ToString().StartsWith("urn:myClasses:"))
                    item1 = "Class:" + triple.Subject.ToString().Remove(0, 14);
                if (triple.Subject.ToString().StartsWith("urn:myObjects:"))
                    item1 = "Object:" + triple.Subject.ToString().Remove(0, 14);
                if (triple.Object.ToString().StartsWith("urn:myObjects:"))
                    item3 = "Object:" + triple.Object.ToString().Remove(0, 14);
                if (triple.Object.ToString().StartsWith("urn:myClasses:"))
                    item3 = "Class:" + triple.Object.ToString().Remove(0, 14);
                if (triple.Object.ToString().StartsWith("http://www.w3.org/2000/01/rdf-schema#Class"))
                    item3 = "rdfs:Class" + triple.Object.ToString().Remove(0, 42);
                if (triple.Predicate.ToString().StartsWith(@"http://www.w3.org/1999/02/22-rdf-syntax-ns#type"))
                    item2 = "type";
                if (triple.Predicate.ToString().StartsWith(@"http://www.w3.org/2000/01/rdf-schema#subClassOf"))
                    item2 = "subClassOf";
                if (triple.Predicate.ToString().StartsWith(@"http://www.wikidata.org/wiki/Property:P1542"))
                    item2 = "whatFor";
                if (triple.Predicate.ToString().StartsWith(@"http://www.wikidata.org/wiki/Property:P2868"))
                    item2 = "causesWhy";
                if (triple.Subject.ToString().StartsWith("urn:myProperties:"))
                    item1 = "Property:" + triple.Subject.ToString().Remove(0, 17);
                if (triple.Object.ToString().StartsWith(@"http://www.w3.org/1999/02/22-rdf-syntax-ns#Property"))
                    item3 = "Property";


                temp.Add(new Tuple<string, string, string>(item1, item2, item3));

            }

            return temp;
        }

        public void AddTriple(string subject, string predicate, string obj)
        {
            INode sub = graph.CreateUriNode(subject);
            INode pred = graph.CreateUriNode(predicate);
            INode ob = graph.CreateUriNode(obj);

            Triple triple = new Triple(sub, pred, ob);

            graph.Assert(triple);

            SaveAndReloadGraph();
        }

        public void SaveAndReloadGraph()
        {
            graph.SaveToFile(sourceFile);
            graph = new Graph();
            graph.LoadFromFile(sourceFile);
            InitDataSources();
        }

        public void GetNamespaceMap()
        {
            foreach (var prefix in graph.NamespaceMap.Prefixes)
            {
                prefNamespaces.Add(prefix, graph.NamespaceMap.GetNamespaceUri(prefix).ToString());
            }
        }

        public void InitDataSources()
        {
            properties = new List<string>();
            objects = new List<string>();
            classes = new List<string>();

            Triples = GetTriplesStringList(graph.Triples);

            properties.AddRange(new[] {"whatFor", "causesWhy" });
            foreach (var triple in graph.GetTriplesWithObject(graph.CreateUriNode("rdf:Property")))
            {
                properties.Add(triple.Subject.ToString().Substring(17));
            }
            
            foreach (var triple in graph.GetTriplesWithPredicate(graph.CreateUriNode("rdf:type")))
            {
                if(!triple.Object.Equals(graph.CreateUriNode("rdfs:Class")) && !triple.Object.Equals(graph.CreateUriNode("rdf:Property"))) 
                    objects.Add(triple.Subject.ToString().Substring(14));
            } 

            foreach (var triple in graph.GetTriplesWithPredicate(graph.CreateUriNode("rdfs:subClassOf")))
            {
                classes.Add(triple.Subject.ToString().Substring(14));
            }

            foreach (var triple in graph.GetTriplesWithPredicateObject(graph.CreateUriNode("rdf:type"), graph.CreateUriNode("rdfs:Class")))
            {
                classes.Add(triple.Subject.ToString().Substring(14));
            }


            GetStatistics();

        }

        public void GetStatistics()
        {
            counts = new Dictionary<string, int>();

            PrefixesCount = graph.NamespaceMap.Prefixes.Count();
            PredicatesCount = graph.Triples.PredicateNodes.Distinct().Count();
            MainClassesCount = graph.GetTriplesWithPredicateObject(graph.CreateUriNode("rdf:type"), graph.CreateUriNode("rdfs:Class")).Count();
            WhatForCount = graph.GetTriplesWithPredicate(graph.CreateUriNode("wikiProperty:P1542")).Count();
            WhyCount = graph.GetTriplesWithPredicate(graph.CreateUriNode("wikiProperty:P2868")).Count();

            AllClassesCount = graph.GetTriplesWithPredicate(graph.CreateUriNode("rdfs:subClassOf")).Count() +
                              MainClassesCount;
            TypesCount = graph.GetTriplesWithPredicate(graph.CreateUriNode("rdf:type")).Count() - MainClassesCount;

            WhatCount = AllClassesCount + TypesCount + MainClassesCount;
            Count = graph.Triples.Count;

            counts.Add(nameof(PrefixesCount), PrefixesCount);
            counts.Add(nameof(PredicatesCount), PredicatesCount);
            counts.Add(nameof(MainClassesCount), MainClassesCount);
            counts.Add(nameof(AllClassesCount), AllClassesCount);
            counts.Add(nameof(TypesCount), TypesCount);
            counts.Add(nameof(WhatForCount), WhatForCount);
            counts.Add(nameof(WhatCount), WhatCount);
            counts.Add(nameof(WhyCount), WhyCount);
            counts.Add(nameof(Count), Count);

        }

    }
}
