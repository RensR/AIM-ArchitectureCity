using System.Collections.Generic;

namespace AIM.Plugins.Analyzers.Clustering.DataTypes
{
    /// <summary>
    /// Simple helper class to determine whether a node is a cluster or a single node
    /// </summary>
    public class Cluster : Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class. 
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="parents">
        /// The parents.
        /// </param>
        /// <param name="mergeLevel">
        /// The merge Level.
        /// </param>
        /// <param name="size">
        /// The size.
        /// </param>
        /// <param name="callCount">
        /// The call Count.
        /// </param>
        public Cluster(int id, List<Node> parents, int mergeLevel, int size, int callCount)
            : base(id, parents, mergeLevel, size, callCount)
        {
            this.ID = id;
            this.Parents = parents;
            this.MergeLevel = mergeLevel;
            this.Size = size;
            this.CallCount = callCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cluster"/> class. 
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="package">
        /// The package.
        /// </param>
        /// <param name="callCount">
        /// The call Count.
        /// </param>
        public Cluster(int id, string label, string package, int callCount)
            : base(id, label, package, callCount)
        {
        }

        /// <summary>
        /// Returns a string representation of the cluster.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the cluster.
        /// </returns>
        public override string ToString()
        {
            return $"cluster{this.ID}";
        }
    }
}
