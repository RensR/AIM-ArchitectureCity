using System.Collections.Generic;
using System.Linq;
using Framework.Models.Nodes;
using Framework.Plugins.Core;

namespace Framework.Plugins.Analyzers
{
    public class PetriNet
    {
        public List<SimpleNodeModel> Nodes { get; set; }

        public Dictionary<int, Dictionary<int, Operator>> Edges { get; set; }

        public int NodeCounter { get; set; }

        public PetriNet(KSuccessor k1, KSuccessor kn, IEnumerable<int> startList)
        {
            // Insert the normal nodes and edges first
            this.Nodes = kn.Events.Select(@event => new SimpleNodeModel(@event.ID, @event.Name)).ToList();
            this.Edges = k1.OperatorList;

            NodeCounter = (int)Nodes.Max(n => n.ID);

            //this.CreateHelperNodes();

            // Create an extra "END" node and edges to the end node from all 
            // events that are last in a trace
            this.Nodes.Add(new SimpleNodeModel(++NodeCounter, "END", NodeType.End));

            foreach (var knEvent in kn.Events)
            {
                if (!kn.OperatorList.ContainsKey(knEvent.ID))
                {
                    this.Edges[knEvent.ID] = new Dictionary<int, Operator>
                    {
                        [NodeCounter] = new Operator(knEvent.ID, NodeCounter, Type.Sequence)
                    };
                }
            }

            // Create an extra "START" node and edges from the start node to all 
            // events that can start a trace
            this.Nodes.Add(new SimpleNodeModel(++NodeCounter, "START", NodeType.Start));
            this.Edges[NodeCounter] = new Dictionary<int, Operator>();

            foreach (var node in startList)
                this.Edges[NodeCounter][node] = new Operator(NodeCounter, node, Type.Sequence);
        }

        private void CreateHelperNodes()
        {
            // Create the helper nodes between the actual events
            var newEdges = new Dictionary<int, Dictionary<int, Operator>>();
            foreach (var edge in this.Edges)
            {
                List<KeyValuePair<int, Operator>> sortedEdges = edge.Value.ToList();
                sortedEdges.Sort((x, y) => x.Key.CompareTo(y.Key));

                int name = NodeCounter++;

                // TODO Weight is wrong if there are more than 1 edges incoming for a helper node
                var weight = edge.Value.Sum(value => value.Value.Weight);

                this.Nodes.Add(new SimpleNodeModel(name, " ", NodeType.Helper));
                newEdges[edge.Key] = new Dictionary<int, Operator>
                {
                    [name] = new Operator(edge.Key, name, Type.Sequence, weight)
                };

                // Add the edge from the old node to the new helper node with the combined weight
                newEdges[name] = new Dictionary<int, Operator>();

                foreach (var @operator in edge.Value)
                {
                    @operator.Value.Start = name;
                    newEdges[name][@operator.Key] = @operator.Value;
                }
            }

            this.Edges = newEdges;
        }
    }
}