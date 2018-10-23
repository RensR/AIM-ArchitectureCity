using System.Collections.Generic;
using System.Linq;
using AIM.Plugins.Core;

namespace AIM.Plugins.Analyzers.Clustering.DataTypes
{
    /// <summary>
    /// Node class to represent a single element that can be clustered together.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Gets or sets the ID of the node.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the label of the node.
        /// </summary>
        public string Label { get; set; } = "mergedNode";

        /// <summary>
        /// Gets or sets split up identifier information as an array of strings
        /// </summary>
        public string[] IdentifierSplit { get; set; }

        /// <summary>
        /// Returns the identifier of a given level
        /// </summary>
        /// <param name="level">
        /// The level of depth of which the identifier should be returned
        /// </param>
        /// <returns>
        /// The identifier of a given level as a <see cref="string"/> 
        /// </returns>
        public string Identifier(int level = 0)
        {
            return level < IdentifierSplit.Length ? IdentifierSplit[level] : string.Empty;
        }

        /// <summary>
        /// Gets or sets the size of the node
        /// </summary>
        public int Size { get; set; } = 1;

        /// <summary>
        /// Gets or sets parent nodes
        /// </summary>
        public List<Node> Parents { get; set; } = new List<Node>();

        /// <summary>
        /// Returns the total number of parent nodes
        /// </summary>
        /// <returns>
        /// The number of parent nodes as an <see cref="int"/>.
        /// </returns>
        public int ParentCount()
        {
            return this.Parents.Count <= 0 ? 1 : this.Parents.Sum(p => p.ParentCount());
        }

        /// <summary>
        /// Gets or sets the callcount of the nodes.
        /// </summary>
        public int CallCount { get; set; }

        /// <summary>
        /// Gets or sets the mergelevel of the node
        /// </summary>
        public int MergeLevel { get; set; }

        public Dictionary<int, Pair<int, int>> In { get; set; }

        public Dictionary<int, Pair<int, int>> Out { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class from the Cluster class. This should
        /// only be used to create clusters
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
        public Node(int id, List<Node> parents, int mergeLevel, int size, int callCount)
        {
            this.ID = id;
            this.Parents = parents;
            this.MergeLevel = mergeLevel;
            this.Size = size;
            this.CallCount = callCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class. 
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="label">
        /// The label.
        /// </param>
        /// <param name="identifier">
        /// The identifier.
        /// </param>
        /// <param name="callCount">
        /// The call Count.
        /// </param>
        public Node(int id, string label, string identifier, int callCount)
        {
            this.ID = id;
            this.Label = label;
            this.MergeLevel = 0;
            this.CallCount = callCount;

            this.In = new Dictionary<int, Pair<int, int>>();
            this.Out = new Dictionary<int, Pair<int, int>>();

            this.IdentifierSplit = identifier.Split('.');
        }

        /// <summary>
        /// Returns a string representation of the node.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the node.
        /// </returns>
        public override string ToString()
        {
            return this.ID.ToString();
        }
    }
}