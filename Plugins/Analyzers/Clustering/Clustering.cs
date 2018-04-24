using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AIM.Models;
using AIM.Plugins.Analyzers.Clustering.DataTypes;
using AIM.Plugins.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;

namespace AIM.Plugins.Analyzers.Clustering
{
    /// <summary>
    /// Superclass that includes the shared logic for every clustering algorithm. 
    /// Implementations should derive this class and add to it to create a working
    /// clustering algorithm.
    /// </summary>
    public class Clustering
    {
        /// <summary>
        /// The history of all the merge actions taken
        /// </summary>
        public readonly List<Node> MergeHistory = new List<Node>();

        /// <summary>
        /// Keeps track of the number of nodes to give them an unique ID
        /// </summary>
        public int NodeCounter { get; set; }

        /// <summary>
        /// The highest call count found in all actions
        /// </summary>
        private int _maxCallCount;

        /// <summary>
        /// All nodes currently in the graph. This includes clusters and excludes merged nodes
        /// </summary>
        public Dictionary<int, Node> Nodes { get; set; } = new Dictionary<int, Node>();

        /// <summary>
        /// Keeps the original fan in and out numbers for later use
        /// </summary>
        public Dictionary<int, Tuple<int, int>> OriginalFanInOut = new Dictionary<int, Tuple<int, int>>();

        /// <summary>
        /// Ensures that DOT files are created in the proper location even when different HDDs 
        /// are used for the OS and this program
        /// </summary>
        private readonly IHostingEnvironment _hostingEnvironment;

        /// <summary>
        /// Ensures that DOT files are created in the proper location even when different HDDs 
        /// are used for the OS and this program
        /// </summary>
        /// <param name="hostingEnvironment">Sets the IHostingEnvironment obtained in the Controller
        /// </param>
        /// <param name="eventList">List of events that will be clustered.
        /// </param>
        public Clustering(IHostingEnvironment hostingEnvironment, IEnumerable<Event> eventList)
        {
            _hostingEnvironment = hostingEnvironment;
            NodeCounter = eventList.Max(e => e.ID) + 1;
        }

        /// <summary>
        /// Adds all the edges to the nodelist from the KSuccessor object
        /// </summary>
        /// <param name="ks">K-successor object that contains all edge information
        /// </param>
        public void AddEdges(KSuccessor ks)
        {
            foreach (KeyValuePair<int, Dictionary<int, Operator>> source in ks.OperatorList)
            {
                foreach (KeyValuePair<int, Operator> sink in source.Value)
                {
                    // Add the fan out numbers to the source node
                    if (this.Nodes[source.Key].Out.ContainsKey(sink.Key)) this.Nodes[source.Key].Out[sink.Key].Second += sink.Value.Weight;
                    else this.Nodes[source.Key].Out.Add(sink.Key, new Pair<int, int>(sink.Key, sink.Value.Weight));

                    // Add the fan in numbers to the sink node
                    if (this.Nodes[sink.Key].In.ContainsKey(source.Key)) this.Nodes[sink.Key].In[source.Key].Second += sink.Value.Weight;
                    else this.Nodes[sink.Key].In.Add(source.Key, new Pair<int, int>(source.Key, sink.Value.Weight));
                }
            }

            foreach (var node in Nodes.Values)
            {
                this.OriginalFanInOut.Add(node.ID, new Tuple<int, int>(node.In.Count, node.Out.Count));
            }
        }

        /// <summary>
        /// Draws the nodes and edges in DOT with the predetermined properties from InitGraph
        /// </summary>
        /// <returns>String tuple containing the image location as first string and 
        /// the parsed dot output with locations as the second.</returns>
        public (string, string) Draw()
        {
            AdjacencyGraph<Node, Edge> graph = new AdjacencyGraph<Node, Edge>();
            graph.AddVertexRange(this.Nodes.Values);

            // Set the highest found callCount (back) to zero
            this._maxCallCount = 0;

            // Add all the edges by adding all out-edges only. If we would add both in and out we would
            // have all duplicated edges
            foreach (Node graphVertex in graph.Vertices)
            {
                graph.AddEdgeRange(
                    graphVertex.Out.Values.Select(x => new Edge(graphVertex, this.Nodes[x.First], x.Second)));

                // If the highest callcount is smaller than the found, update it
                if (graphVertex.CallCount > this._maxCallCount) this._maxCallCount = graphVertex.CallCount;
            }

            return ProcessDot(graph.ToGraphviz(InitGraph));
        }

        /// <summary>
        /// Sets the Vertex and Edge properties within the GraphvizAlgorithm
        /// </summary>
        /// <param name="graphviz"> The GraphvizAlgorithm of which the properties have to be set
        /// </param>
        private void InitGraph(GraphvizAlgorithm<Node, Edge> graphviz)
        {
            graphviz.FormatVertex += (sender, args) =>
                {
                    // The size should hold every elemnt in a square so the size should be at least
                    // the sqrt of the number of child elements
                    int size = (int)Math.Ceiling(Math.Sqrt(args.Vertex.ParentCount()));
                    args.VertexFormatter.Label = $"{args.Vertex.ID}";

                    int b = args.Vertex.CallCount * 255 / this._maxCallCount * 2;
                    byte c = (byte)b;

                    args.VertexFormatter.Shape = GraphvizVertexShape.Box;
                    args.VertexFormatter.Size = new GraphvizSizeF(size, size);
                    args.VertexFormatter.FixedSize = true;
                    args.VertexFormatter.Style = GraphvizVertexStyle.Filled;
                    args.VertexFormatter.FillColor = new GraphvizColor(c, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                };

            graphviz.FormatEdge += (sender, args) => args.EdgeFormatter.Label.Value = args.Edge.Weight.ToString();
        }

        /// <summary>
        /// Update the In and Out properties of a newly created cluster node. This is important
        /// to keep the in and out edges up to date for further calculations.
        /// </summary>
        /// <param name="node"> The newly created cluster 
        /// </param>
        public void UpdateFanInOut(Cluster node)
        {
            node.In = this.MergeDict(node.Parents.Select(x => x.In).ToList());
            node.Out = this.MergeDict(node.Parents.Select(x => x.Out).ToList());

            // Remove edges to self
            foreach (Node nodeParent in node.Parents)
            {
                node.In.Remove(nodeParent.ID);
                node.Out.Remove(nodeParent.ID);
            }

            foreach (KeyValuePair<int, Pair<int, int>> link in node.In)
            {
                foreach (var nodeParent in node.Parents)
                {
                    if (this.Nodes[link.Key].Out.ContainsKey(nodeParent.ID))
                    {
                        if (this.Nodes[link.Key].Out.ContainsKey(node.ID)) this.Nodes[link.Key].Out[node.ID].Second += link.Value.Second;
                        else this.Nodes[link.Key].Out.Add(node.ID, new Pair<int, int>(node.ID, link.Value.Second));
                        this.Nodes[link.Key].Out.Remove(nodeParent.ID);
                    }
                }
            }

            foreach (KeyValuePair<int, Pair<int, int>> link in node.Out)
            {
                foreach (var nodeParent in node.Parents)
                {
                    if (this.Nodes[link.Key].In.ContainsKey(nodeParent.ID))
                    {
                        if (this.Nodes[link.Key].In.ContainsKey(node.ID)) this.Nodes[link.Key].In[node.ID].Second += link.Value.Second;
                        else this.Nodes[link.Key].In.Add(node.ID, new Pair<int, int>(node.ID, link.Value.Second));
                        this.Nodes[link.Key].In.Remove(nodeParent.ID);
                    }
                }
            }
        }

        /// <summary>
        /// Merges two dictionaries into one
        /// </summary>
        /// <param name="a"> Dictionary a to merge into b
        /// </param>
        /// <param name="b"> Dictionary b to merge a in to
        /// </param>
        private Dictionary<int, Pair<int, int>> MergeDict(
            Dictionary<int, Pair<int, int>> a,
            Dictionary<int, Pair<int, int>> b)
        {
            foreach (var pair in a)
            {
                if (b.ContainsKey(pair.Key)) b[pair.Key].Second += pair.Value.Second;
                else b.Add(pair.Key, pair.Value);
            }

            return b;
        }

        /// <summary>
        /// Merges a list of dictionaries into a single one.
        /// </summary>
        /// <param name="dictList"> List of dictionaries to be merged into each other.
        /// </param>
        private Dictionary<int, Pair<int, int>> MergeDict(IReadOnlyList<Dictionary<int, Pair<int, int>>> dictList)
        {
            if (dictList.Count == 1) return dictList[0];
            if (dictList.Count == 2) return this.MergeDict(dictList[0], dictList[1]);

            var totalDict = dictList[0];

            for (int i = 1; i < dictList.Count; i++)
            {
                totalDict = this.MergeDict(totalDict, dictList[i]);
            }

            return totalDict;
        }

        /// <summary>
        /// Processes and writes DOT input and optional arguments to file and returns the file
        /// location and the parsed DOT string.
        /// </summary>
        /// <param name="dot">DOT code to be parsed by the graphviz command line tool
        /// </param>
        /// <param name="args">arguments to be passed on to graphviz command line
        /// with a default case of sting.empty for no arguments</param>
        public (string, string) ProcessDot(string dot, string args = "")
        {
            const string Folder = "DOTGraph";
            string file = $"{DateTime.Now.DayOfYear}_{DateTime.Now.Hour}{".dot"}";

            Directory.CreateDirectory(Folder);
            File.WriteAllText($"{Folder}\\{file}", dot);

            return ("", this.RunDotProcess($"{args} \"{Folder}\\{file}\""));
        }

        /// <summary>
        /// Create a process to run the DOT.EXE command to draw a graph using graphviz
        /// </summary>
        /// <param name="args">arguments to be passed on to graphviz command line</param>
        private string RunDotProcess(string args)
        {
            var process = new Process
                              {
                                  StartInfo =
                                      new ProcessStartInfo
                                          {
                                              WorkingDirectory =
                                                  this._hostingEnvironment.ContentRootPath,
                                              FileName = "dot.exe",
                                              Arguments = args,
                                              UseShellExecute = false,
                                              RedirectStandardOutput = true,
                                              CreateNoWindow = false
                                          }
                              };
            try
            {
                process.Start();
                return process.StandardOutput.ReadToEnd();
            }
            catch(Exception e)
            {
                if(e.Message == "The system cannot find the file specified")
                    throw new EntryPointNotFoundException();
                throw;
            }
            finally
            {
                process.Dispose();
            }
        }
    }
}
