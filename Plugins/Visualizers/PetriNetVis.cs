namespace Framework.Plugins.Visualizers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Framework.Data;
    using Framework.Models.Nodes;
    using Framework.Plugins.Analyzers;
    using Framework.Plugins.Core;

    using Microsoft.Extensions.Logging;

    public class PetriNetVis : Visualizer
    {
        public override string Name => "PetriVis";

        private readonly Neo4JContext context;

        private readonly ILogger logger;

        private readonly PetriNet petriNet;

        public PetriNetVis(PetriNet petriNet, ILogger logger)
        {
            this.petriNet = petriNet;
            this.context = new Neo4JContext(logger);
            this.logger = logger;
        }

        public override async Task<string> RunAsync()
        {
            // Empty the current database
            this.context.ClearDb();

            // Create a list of all the nodes
            var nodeTypes = this.petriNet.Nodes.GroupBy(x => x.Type);
            var watch = new Stopwatch();
            watch.Start();

            // Insert all the nodes
            foreach (IGrouping<NodeType, SimpleNodeModel> nodeList in nodeTypes)
            {
                await this.context.CsvInsertNodes(nodeList.ToList(), nodeList.Key.ToString());
            }

            this.logger.LogDebug($"{DateTime.Now}\tNodes inserted\ttime taken: {watch.ElapsedMilliseconds/1000f:N}s\t{this.petriNet.Nodes.Count} nodes inserted");
            watch.Restart();

            List<Operator> operators = (from start in this.petriNet.Edges from end in start.Value select end.Value).ToList();

            foreach (var type in operators.GroupBy(x => x.Type))
            {
                await this.context.InsertEdgesByID(
                    type.Select(x => x.Start).ToList(),
                    type.Select(x => x.End).ToList(),
                    type.Select(x => x.Weight).ToList(),
                    type.Key.ToString());
            }

            this.logger.LogDebug($"{DateTime.Now}\tEdges inserted\ttime taken: {watch.ElapsedMilliseconds/1000f:N}s\tedges inserted: {operators.Count}");

            return $"The run found {this.petriNet.Nodes.Count} objects! \n " +
                   $"Check the local Neo4j client for results (default http://localhost:7474/browser/).";
        }
    }
}
