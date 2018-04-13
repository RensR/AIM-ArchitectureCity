namespace Framework.Plugins.Visualizers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using Data;
    using Models.Nodes;
    using Analyzers;
    using Core;

    using Microsoft.Extensions.Logging;

    public class PetriNetVis : Visualizer
    {
        public override string Name => "PetriVis";

        private readonly Neo4JContext _context;

        private readonly ILogger _logger;

        private readonly PetriNet _petriNet;

        public PetriNetVis(PetriNet petriNet, ILogger logger)
        {
            _petriNet = petriNet;
            _context = new Neo4JContext(logger);
            _logger = logger;
        }

        public override async Task<string> RunAsync()
        {
            // Empty the current database
            _context.ClearDb();

            // Create a list of all the nodes
            var nodeTypes = _petriNet.Nodes.GroupBy(x => x.Type);
            var watch = new Stopwatch();
            watch.Start();

            // Insert all the nodes
            foreach (IGrouping<NodeType, SimpleNodeModel> nodeList in nodeTypes)
            {
                await _context.CsvInsertNodes(nodeList.ToList(), nodeList.Key.ToString());
            }

            _logger.LogDebug($"{DateTime.Now}\tNodes inserted\ttime taken: {watch.ElapsedMilliseconds/1000f:N}s\t{_petriNet.Nodes.Count} nodes inserted");
            watch.Restart();

            List<Operator> operators = (from start in _petriNet.Edges from end in start.Value select end.Value).ToList();

            foreach (var type in operators.GroupBy(x => x.Type))
            {
                await _context.InsertEdgesByID(
                    type.Select(x => x.Start).ToList(),
                    type.Select(x => x.End).ToList(),
                    type.Select(x => x.Weight).ToList(),
                    type.Key.ToString());
            }

            _logger.LogDebug($"{DateTime.Now}\tEdges inserted\ttime taken: {watch.ElapsedMilliseconds/1000f:N}s\tedges inserted: {operators.Count}");

            return $"The run found {this._petriNet.Nodes.Count} objects! \n " +
                   $"Check the local Neo4j client for results (default http://localhost:7474/browser/).";
        }
    }
}
