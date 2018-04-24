using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AIM.Models;
using Microsoft.Extensions.Logging;

namespace AIM.Plugins.Parsers
{
    /// <summary>
    /// DLParse implements the Parser class with specific components to parse logs from the company DL
    /// </summary>
    public class DLParse : Parser
    {
        private readonly Dictionary<string, Event> _events = new Dictionary<string, Event>();

        private readonly ILogger _logger;

        /// <summary>
        /// Returns the name of the parser for display purposes 
        /// </summary>
        public override string Name => "DLParse";

        /// <summary>
        /// Initializes a new instance of the <see cref="DLParse"/> class with a logger.
        /// </summary>
        /// <param name="logger">
        /// The logger that was injected in the controller is passed to the parser to be able
        /// to support logging.
        /// </param>
        public DLParse(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Runs the main parse loop to create a <see cref="RmrFileTypeModel"/> that contains all 
        /// relevant parsed information from the logs
        /// </summary>
        /// <param name="filepath">
        /// The filepath of the chosen log file
        /// </param>
        /// <returns>
        /// The <see cref="Task"/> Taskcompleted to indicate that the async task is complete
        /// </returns>
        public override Task Run(string filepath)
        {
            var eventList = RunPreParse(filepath);
            var traces = ParseTraces(eventList);
            var splitTraces = SplitTraces(traces);

            OutputModel = new RmrFileTypeModel
            {
                TraceList = splitTraces,
                EventList = this._events.Values.ToList()
            };

            _logger.LogDebug("Done parsing!");

            return Task.CompletedTask;
        }

        private IEnumerable<EventInstance> RunPreParse(string filepath)
        {
            var eventInstances = new List<EventInstance>();
            var reader = new StreamReader(File.OpenRead(filepath));
            var count = 0;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (count <= 0)
                {
                    count++;
                    continue;
                }

                if (line.Length < 1 || !char.IsNumber(line[0]))
                    continue;

                var brackLeft = line.IndexOf('(');
                var brackRight = line.IndexOf(')');
                var brackLeftSq = line.IndexOf('[');

                // Parse the name that is present after ')' and exclude the whitespace and bracket
                var name = line.Substring(brackRight + 2, line.Length - 2 - brackRight);

                // Remove variables
                if (name.StartsWith("- Added gebied"))
                    name = "Added gebied";
                else if (name.StartsWith("Databestand wordt gekopieerd"))
                    name = "Databestand wordt gekopieerd";
                else if (name.StartsWith("Exportbestand wordt verplaatst "))
                    name = "Exportbestand wordt verplaatst";
                else if (name.StartsWith("Exportbestand wordt verplaatst"))
                    name = "Exportbestand wordt verplaatst";
                else if (name.StartsWith("Exportbestand wordt gearchiveerd"))
                    name = "Exportbestand wordt gearchiveerd";
                else if (name.StartsWith("Detected file"))
                    name = "Detected file is ready";
                else if (name.StartsWith("Start: Herbereken"))
                    name = "Start: Herbereken";
                else if (name.StartsWith("Einde: Herbereken"))
                    name = "Einde: Herbereken bonnen";
                else if (name.StartsWith("[ab-stmoosb"))
                    continue;

                // Hard prune!
                else if (name.StartsWith("Toegang verleend"))
                    name = "Toegang verleend";

                // Next type of DL File
                else if (name.StartsWith("Toegang verleend op transport/inboeken_op_ssc"))
                    name = "Toegang verleend op transport/inboeken_op_ssc";
                else if (name.StartsWith("Toegang verleend op transport/folders_en_voorraad_overzicht"))
                    name = "Toegang verleend op transport/folders_en_voorraad_overzicht";
                else if (name.StartsWith("Toegang verleend op run/runZoek"))
                    name = "Toegang verleend op run/runZoek";
                else if (name.StartsWith("Toegang verleend op run/run aan"))
                    name = "Toegang verleend op run/run";
                else if (name.StartsWith("Toegang verleend op run/ weegbrieven"))
                    name = "Toegang verleend op run/ weegbrieven";
                else if (name.StartsWith("Toegang verleend op transport/folders_en_voorraad_overzicht"))
                    name = "Toegang verleend op transport/folders_en_voorraad_overzichtrunZoek";

                else if (name.StartsWith("Toegang verleend op Service2000"))
                    name = "Toegang verleend op Service2000";
                else if (name.StartsWith("REST url:"))
                    name = "REST url:";
                else if (name.StartsWith("Starting authentication:"))
                    name = "Starting authentication:";
                
                else if (name.StartsWith("Gebruiker"))
                    name = "Gebruiker is ingelogd";

                else if (name.StartsWith("Remote Username"))
                    name = "Remote Username";

                // Remove the error messages from the data
                else if (name.StartsWith("WFLYEJB"))
                    continue;

                name = Regex.Replace(name, @"[\d-]", string.Empty).Trim();

                // Parse the origin that is present in '[' and ']' and exclude the brackets
                var origin = line.Substring(brackLeftSq + 1, line.IndexOf(']') - 1 - brackLeftSq);

                // Parse the thread that is present in '(' and ')' and exclude the brackets 
                var processID = line.Substring(brackLeft   + 1, brackRight - 1 - brackLeft);

                // Parse the datetime that is always the first entry. Replace , with . for the datetime parse
                var dateTime = DateTime.Parse(line.Substring(0, 23).Replace(',', '.'));

                if (!_events.ContainsKey($"{origin}\t{name}"))
                    this._events.Add(
                        $"{origin}\t{name}",
                        new Event
                        {
                            ID = count,
                            Name = $"{origin}\t{name}",
                            Origin = origin,
                            IsStart = true,
                            Thread = processID
                        });
                eventInstances.Add(new EventInstance(this._events[$"{origin}\t{name}"], processID, dateTime));
                count++;
            }

            return eventInstances;
        }

        /// <summary>
        /// Splits the traces if there is a time longer than MaxSpan between two 
        /// consecutive events.
        /// </summary>
        /// <param name="traceDict">
        /// The trace dictionary with the non-split traces.
        /// </param>
        /// <returns>
        /// The <see>
        ///         <cref>Dictionary{string, Trace}</cref>
        ///     </see>
        ///     of traces that are split up.
        /// </returns>
        private static Dictionary<string, Trace> SplitTraces(Dictionary<string, Trace> traceDict)
        {
            var maxSpan = TimeSpan.FromMinutes(30);

            // DL log has no case-id property so we use thread-id
            // This however does not identify all traces and clusters some together
            // We use the MAX_SPAN to set a max timespan interval between two events
            // If it exceeds it we handle it as a new trace
            var splitTraces = new Dictionary<string, Trace>();
            foreach (var trace in traceDict)
            {
                var lastSplit = 0;
                for (int i = 1; i < trace.Value.EventList.Count; i++)
                {
                    var list = trace.Value.EventList;
                    if (list[i].Datetime - list[i - 1].Datetime > maxSpan)
                    {
                        var tmpList = list.GetRange(lastSplit, i - lastSplit);
                        lastSplit = i;
                        splitTraces.Add(trace.Key + $":{i}", new Trace(tmpList));
                    }
                }

                splitTraces.Add(
                    trace.Key,
                    new Trace(trace.Value.EventList.GetRange(lastSplit, trace.Value.EventList.Count - lastSplit)));
            }

            return splitTraces;
        }
    }
}
