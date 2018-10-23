using QuickGraph;

namespace AIM.Plugins.Analyzers.Clustering.DataTypes
{
    /// <summary>
    /// Implementation of the IEdge interface of QuickGraph.
    /// </summary>
    public class Edge : IEdge<Node>
    {
        private readonly Node _source;

        private readonly Node _target;

        Node IEdge<Node>.Source => _source;

        Node IEdge<Node>.Target => _target;

        /// <summary>
        /// Gets or sets the weight of an edge
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Edge"/> class. 
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="target">
        /// The target.
        /// </param>
        /// <param name="weight">
        /// The weight.
        /// </param>
        public Edge(Node source, Node target, int weight = 0)
        {
            _source = source;
            _target = target;
            this.Weight = weight;
        }
    }
}
