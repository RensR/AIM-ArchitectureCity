namespace Framework.Plugins.Analyzers.Clustering
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Models;
    using DataTypes;
    using Core;

    using Microsoft.AspNetCore.Hosting;

    using MoreLinq;

    /// <summary>
    /// Implementation of <see cref="Clustering"/> as clustering based on fan in and out
    /// </summary>
    public class FanClustering : Clustering
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FanClustering"/> class. 
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
        /// <param name="depth">
        /// The maximum depth determines how many nodes should be left in the final
        /// clustering. When this depth is reached the algorithm is done.
        /// </param>
        public FanClustering(
            IHostingEnvironment hostingEnvironment,
            KSuccessor ks,
            IEnumerable<Event> eventList,
            int depth)
            : base(hostingEnvironment, eventList)
        {
            var maxDepth = eventList.Count() - depth;

            // Initialize the nodes graph with the nodes
            foreach (var @event in eventList) this.Nodes.Add(@event.ID, new Node(@event.ID, @event.Name, @event.Origin, @event.Count));

            this.AddEdges(ks);

            // We're going to merge the nodes according to some criterion until there is
            // just one left. If we have n nodes, we can merge n-1 times. We start looping
            // at 1, meaning we loop n-1 times. i is also our mergeLevel.
            var initialGraphSize = this.Nodes.Count;

            for (int i = 1; i < initialGraphSize && i <= maxDepth; i++)
            {
                try
                {
                    var candidate = this.GetMergeCandidate();
                    int size = candidate.Item1.Size + candidate.Item2.Size;
                    int callCount = candidate.Item1.CallCount + candidate.Item2.CallCount;
                    var node = new Cluster(
                        this.NodeCounter++,
                        new List<Node> { candidate.Item1, candidate.Item2 },
                        i,
                        size,
                        callCount);
                    this.UpdateFanInOut(node);

                    this.MergeHistory.Add(node);

                    // Add the new node to the collection
                    this.Nodes.Add(node.ID, node);

                    // Remove the old nodes that are merged into the new one
                    this.Nodes.Remove(candidate.Item1.ID);
                    this.Nodes.Remove(candidate.Item2.ID);

                    // Save a snapshot to keep the whole history complete
                }
                catch (SequenceException)
                {
                    // We found no nodes that can be merged because the highest fan in/out is 0
                    // Yes this is a case of exception based programming as a SequenceException 
                    // will only be thrown from within the 
                    break;
                }
            }
        }

        /// <summary>
        /// Finds the two nodes that should be merged based on their fan in and fan out
        /// </summary>
        /// <returns>
        /// The <see cref="Tuple"/> containing the two nodes that should be merged
        /// </returns>
        private Tuple<Node, Node> GetMergeCandidate()
        {
            // gets the node with the highest fan in or out while ignoring self edges
            var candidateOne = this.Nodes.Values.MaxBy(x => Math.Max(x.In.Count(inkey => inkey.Key != x.ID), x.Out.Count(outkey => outkey.Key != x.ID)));

            if (candidateOne.In.Count == 0 && candidateOne.Out.Count == 0) throw new SequenceException();

            var candidateTwo = candidateOne.In.Count > candidateOne.Out.Count
                                   ? candidateOne.In.Where(node => node.Key != candidateOne.ID).MaxBy(x => x.Value.Second).Key
                                   : candidateOne.Out.Where(node => node.Key != candidateOne.ID).MaxBy(x => x.Value.Second).Key;

            return new Tuple<Node, Node>(candidateOne, this.Nodes[candidateTwo]);
        }
    }
}
