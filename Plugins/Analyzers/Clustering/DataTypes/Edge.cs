namespace Framework.Plugins.Analyzers.Clustering.DataTypes
{
    using QuickGraph;

    /// <summary>
    /// Implementation of the IEdge interface of QuickGraph.
    /// </summary>
    public class Edge : IEdge<Node>
    {
        private readonly Node source;

        private readonly Node target;

        Node IEdge<Node>.Source => this.source;

        Node IEdge<Node>.Target => this.target;

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
            this.source = source;
            this.target = target;
            this.Weight = weight;
        }
    }
}
