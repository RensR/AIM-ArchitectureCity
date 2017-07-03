namespace Framework.Plugins.Analyzers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Core;

    using Framework.Models;
    using Framework.Plugins.Analyzers.Clustering;
    using Framework.Plugins.Analyzers.Clustering.DataTypes;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    public class ClusteringAnalyzer : Analyzer
    {
        public override string Name => "PetriAna";

        public string ImagePath { get; set; }

        private KSuccessor K1 { get; }

        private List<Event> EventList { get; }

        public Dictionary<int, Node> NodeDict { get; set; }

        /// <summary>
        /// Keeps the original fan in and out numbers for later use
        /// </summary>
        public Dictionary<int, Tuple<int, int>> OriginalFanInOut = new Dictionary<int, Tuple<int, int>>();

        private readonly ILogger logger;

        private readonly IHostingEnvironment hostingEnvironment;

        public ClusteringAnalyzer(IHostingEnvironment hostingEnvironment, RmrFileTypeModel input, ILogger logger)
            : base(input)
        {
            this.logger = logger;
            this.hostingEnvironment = hostingEnvironment;
            this.EventList = input.EventList;

            var watch = new Stopwatch();
            watch.Start();

            K1 = new KSuccessor(1, EventList);
            foreach (var trace in input.TraceList) K1.Add(trace.Value);

            logger.LogDebug($"k1 computed in\t{watch.ElapsedMilliseconds / 1000f:N}seconds");
        }

        public (string, string) CalculateClusters(int depth, string type = "package")
        {
            var watch = new Stopwatch();
            watch.Start();

            Clustering.Clustering active = new Clustering.Clustering(this.hostingEnvironment, EventList);

            switch (type)
            {
                case "Clustering - package":
                    active = new PropertyClustering(
                        hostingEnvironment,
                        K1,
                        EventList,
                        x => x.Origin,
                        delegate(Node node, string s)
                            {
                                node.IdentifierSplit = s.Split('.');
                                return node;
                            });

                    // Calculates the code city for a given level
                    ((PropertyClustering)active).Compute(depth);
                    break;

                case "Clustering - caller":
                    active = new PropertyClustering(
                        hostingEnvironment,
                        K1,
                        EventList,
                        x => x.Thread,
                        delegate(Node node, string s)
                            {
                                node.IdentifierSplit = s.Split('.');
                                return node;
                            });

                    // Calculates the code city for a given level
                    ((PropertyClustering)active).Compute(depth);
                    break;

                case "Clustering - fan":
                    active = new FanClustering(hostingEnvironment, K1, EventList, depth);
                    break;

                default:
                    // BROKEN
                    throw new ArgumentOutOfRangeException();
            }

            this.OriginalFanInOut = active.OriginalFanInOut;
            this.NodeDict = active.Nodes;
            logger.LogDebug($"Clustering computed in\t{watch.ElapsedMilliseconds / 1000f:N}seconds");

            return active.Draw();
        }
    }
}
