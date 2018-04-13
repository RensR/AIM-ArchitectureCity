namespace Framework.Plugins
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Models;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Plugin abstract class to contain shared information
    /// </summary>
    public abstract class Plugin
    {
        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        public abstract string Name { get; }
    }

    /// <summary>
    /// Parser abstract class that implements the abstract <see cref="Plugin"/> class
    /// </summary>
    public abstract class Parser : Plugin
    {
        /// <summary>
        /// Gets or sets the output model <see cref="RmrFileTypeModel"/> that contains all
        /// parsed information from the logs in a standardized format.
        /// </summary>
        public RmrFileTypeModel OutputModel { get; set; }

        /// <summary>
        /// The abstract run class that should be implemented by every parser.
        /// </summary>
        /// <param name="filepath"> 
        /// The filepath of the file to be parsed
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> that indicated that the file has been parsed.
        /// </returns>
        public abstract Task Run(string filepath);

        public static RmrFileTypeModel TraceListToRmr(Dictionary<string, Trace> traceList)
        {
            var traceJson =
                JObject.FromObject(
                    new
                        {
                            Trace =
                            traceList.Select(
                                t =>
                                    new
                                        {
                                            t.Key,
                                            Event = t.Value.EventList.Select(e => new { e.Event.ID, e.Event.Name, e.Datetime })
                                        })
                        });
            return new RmrFileTypeModel { Traces = traceJson };
        }

        /// <summary>
        /// Parses all events intro <see cref="Trace"/>s by creating dictionaries and splitting on processID
        /// Traces are then sorted by date.
        /// </summary>
        /// <param name="eventList">
        /// The event List.
        /// </param>
        /// <returns>
        /// The <see cref="Dictionary{sting, Trace}"/> that contains all traces within a dictionary. The keys
        /// indicate the processID of the trace.
        /// </returns>
        public static Dictionary<string, Trace> ParseTraces(IEnumerable<EventInstance> eventList)
        {
            var traceDict = new Dictionary<string, Trace>();

            foreach (var instance in eventList)
            {
                if (!traceDict.TryGetValue(instance.ProcessID, out var trace))
                {
                    trace = new Trace(new List<EventInstance>());
                    traceDict.Add(instance.ProcessID, trace);
                }

                traceDict[instance.ProcessID].EventList.Add(instance);
            }

            foreach (var trace in traceDict)
                trace.Value.EventList = trace.Value.EventList.OrderBy(e => e.Datetime).ToList();

            return traceDict;
        }
    }

    /// <summary>
    /// Analyzer abstract class that implements the abstract <see cref="Plugin"/> class
    /// </summary>
    public abstract class Analyzer : Plugin
    {
        private RmrFileTypeModel input;

        protected Analyzer(RmrFileTypeModel input)
        {
            this.input = input;
        }
    }

    /// <summary>
    /// Visualizer abstract class that implements the abstract <see cref="Plugin"/> class
    /// </summary>
    public abstract class Visualizer : Plugin
    {
        public abstract Task<string> RunAsync();
    }
}
