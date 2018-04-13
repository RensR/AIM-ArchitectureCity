namespace Framework.Plugins.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Models;

    /// <summary>
    /// K-successor class implements 
    /// </summary>
    public class KSuccessor
    {
        /// <summary>
        /// Gets or sets the max lookahead for the traces
        /// </summary>
        public int K { get; set; }

        /// <summary>
        /// Gets the list of operators
        /// </summary>
        public Dictionary<int, Dictionary<int, Operator>> OperatorList { get; } =
            new Dictionary<int, Dictionary<int, Operator>>();

        /// <summary>
        /// Gets or sets the list of events
        /// </summary>
        public List<Event> Events { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KSuccessor"/> class. 
        /// </summary>
        /// <param name="k">
        /// The max lookahead for the traces
        /// </param>
        /// <param name="events">
        /// The list of events
        /// </param>
        public KSuccessor(int k, List<Event> events)
        {
            K = k;
            Events = events;
        }

        /// <summary>
        /// Adds a <see cref="Trace"/> to the k-successor list and updates operators
        /// </summary>
        /// <param name="trace">
        /// The trace that is to be inserted in the k-successor.
        /// </param>
        public void Add(Trace trace)
        {
            var events = trace.EventList;
            for (var i = 0; i < events.Count; i++)
            {
                // Start looking at the next item in the list (j = i + 1)
                // Until the list is empty (j < events.Count)
                // Or until we looked at K followers (j <= i + K)
                for (var j = i + 1; j < events.Count && j <= i + K; j++)
                {
                    // Updates the existing Flow Graph with a new node
                    var start = events[i].Event.ID;
                    var end = events[j].Event.ID;
                    if (!this.OperatorList.ContainsKey(start)) this.OperatorList[start] = new Dictionary<int, Operator>();

                    var startDict = this.OperatorList[start];

                    // If the end key does not exist we check if the reverse relation exists
                    // If it does we update both to be concurrent
                    if (!startDict.ContainsKey(end))
                    {
                        if (this.OperatorList.ContainsKey(end) && this.OperatorList[end].ContainsKey(start))
                        {
                            this.OperatorList[end][start] = new Operator(end, start, Type.Parallel, this.OperatorList[end][start].Weight);
                            startDict[end] = new Operator(start, end, Type.Parallel, 1);
                        }
                        else

                            // If the relation does not exist in the other direction we insert a sequence
                            startDict[end] = new Operator(start, end, Type.Sequence, 1);
                    }
                    else startDict[end].Weight += 1;

                    if (start == end) break;
                }
            }
        }

        /// <summary>
        /// Draws the k-successor list to a matrix and writes it to file.
        /// </summary>
        public void Draw()
        {
            Events.Sort();

            // To keep it memory efficient write every line instead of creating a big string
            // and writing that to a file
            Directory.CreateDirectory("Tables");
            using (
                var sw =
                    new StreamWriter(
                        File.Open($"Tables\\{DateTime.Now.DayOfYear}.{DateTime.Now.Hour}.csv", FileMode.Create)))
            {
                // Empty space + every event + newline
                var header = "," + Events.Aggregate(string.Empty, (current, @event) => current + @event.Name + ',')
                             + '\n';
                sw.Write(header);

                foreach (var row in Events)
                {
                    var body = row.Name + ",";

                    // If it is not an end node it will be contained in the List
                    if (this.OperatorList.ContainsKey(row.ID))
                    {
                        foreach (var column in Events)
                        {
                            if (this.OperatorList[row.ID].TryGetValue(column.ID, out Operator value))
                            {
                                if (row == column) body += "|,";
                                else body += value.Symbol + ",";
                                continue;
                            }

                            if (this.OperatorList.TryGetValue(column.ID, out var dict))
                                if (dict.TryGetValue(row.ID, out value))
                                    if (value.Symbol == ">")
                                    {
                                        body += "<,";
                                        continue;
                                    }

                            body += "+,";
                        }
                    }
                    else
                    {
                        // Draw the end node data
                        foreach (var @event in Events)
                        {
                            if (this.OperatorList.ContainsKey(@event.ID) && this.OperatorList[@event.ID].ContainsKey(row.ID)) body += "<,";
                            else body += "+,";
                        }
                    }

                    sw.Write(body + '\n');
                }
            }
        }
    }
}