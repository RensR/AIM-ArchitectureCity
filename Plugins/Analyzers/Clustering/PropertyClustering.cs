namespace Framework.Plugins.Analyzers.Clustering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Models;
    using DataTypes;
    using Core;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore.Internal;

    /// <summary>
    /// Implementation of <see cref="Clustering"/> as clustering based on properties
    /// </summary>
    public class PropertyClustering : Clustering
    {
        /// <summary>
        /// Gets or sets the maximum depth that is possible due to the maximum propty depth in the nodes
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Gets the ist of all groupings, or clusters, grouped by string
        /// </summary>
        public List<IGrouping<string, KeyValuePair<int, Node>>>[] Levels { get; }

        private readonly Func<Node, string, Node> setPropertyFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyClustering"/> class.  
        /// Fanclustering looks at the fan in and out of nodes to determine what nodes
        /// to merge into clusters
        /// </summary>
        /// <param name="hostingEnvironment">
        /// The hosting Environment.
        /// </param>
        /// <param name="ks">
        /// The k-successor object that contains all relations between nodes
        /// </param>
        /// <param name="eventList">
        /// The event List contains a list of all the events.
        /// </param>
        /// <param name="getProperty">
        /// The get Property.
        /// </param>
        /// <param name="setProperty">
        /// The set Property.
        /// </param>
        public PropertyClustering(
            IHostingEnvironment hostingEnvironment,
            KSuccessor ks,
            IEnumerable<Event> eventList,
            Func<Event, string> getProperty,
            Func<Node, string, Node> setProperty)
            : base(hostingEnvironment, eventList)
        {
            this.setPropertyFunc = setProperty;
            this.MaxDepth = eventList.Max(x => getProperty(x).Split('.').Length);

            // Initialize the allNodes graph with the nodes
            foreach (var @event in eventList)
            {
                var newNode = new Node(@event.ID, @event.Name, getProperty(@event), @event.Count);
                setProperty(newNode, getProperty(@event));
                this.Nodes.Add(@event.ID, newNode);
            }

            this.AddEdges(ks);

            this.Levels = new List<IGrouping<string, KeyValuePair<int, Node>>>[this.MaxDepth];
            this.Levels[0] = this.Nodes.GroupBy(x => x.Value.Identifier()).ToList();

            for (int i = 1; i < this.MaxDepth; i++)
            {
                this.Levels[i] = new List<IGrouping<string, KeyValuePair<int, Node>>>();
                foreach (IGrouping<string, KeyValuePair<int, Node>> grouping in this.Levels[i - 1])
                {
                    if (grouping.Key == string.Empty) this.Levels[i].Add(grouping);
                    else this.Levels[i].AddRange(grouping.GroupBy(x => x.Value.Identifier(i)));
                }
            }
        }

        /// <summary>
        /// Computes the clusters for the given level.
        /// </summary>
        /// <param name="level">
        /// The level is the depth, or cluster level, that should be merged
        /// </param>
        public void Compute(int level)
        {
            if (level > this.MaxDepth) level = this.MaxDepth;
            if (level <= 0) level = 1;

            foreach (var nodeGroup in this.Levels[level - 1])
            {
                var parentNodes = nodeGroup.Select(x => x.Value).ToList();
                int size = parentNodes.Select(n => n.Size).Sum();
                int callCount = parentNodes.Select(n => n.CallCount).Sum();

                Cluster node = new Cluster(this.NodeCounter++, parentNodes, level, size, callCount);

                this.setPropertyFunc(node, node.Parents[0].IdentifierSplit.Take(level).Join("."));
                this.MergeHistory.Add(node);

                this.UpdateFanInOut(node);
                
                // Add the new node to the collection
                this.Nodes.Add(node.ID, node);

                // Remove the old nodes that are merged into the new one
                foreach (var nodeParent in node.Parents)
                {
                    this.Nodes.Remove(nodeParent.ID);
                }
            }
        }
    }
}
