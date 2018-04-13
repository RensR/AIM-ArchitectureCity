using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Framework.Models;
using Microsoft.Extensions.Logging;

namespace Framework.Plugins.Parsers
{
    /// <summary>
    /// FIParse implements the Parser class with specific components to parse logs from the company FI
    /// </summary>
    public class FIParse : Parser
    {
        private readonly Dictionary<string, Event> _events = new Dictionary<string, Event>();

        private readonly ILogger _logger;

        /// <summary>
        /// Returns the name of the parser for display purposes 
        /// </summary>
        public override string Name => "FIParse";

        /// <summary>
        /// Initializes a new instance of the <see cref="FIParse"/> class with a logger.
        /// </summary>
        /// <param name="logger">
        /// The logger that was injected in the controller is passed to the parser to be able
        /// to support logging.
        /// </param>
        public FIParse(ILogger logger)
        {
            this._logger = logger;
        }

        public override Task Run(string filepath)
        {
            var eventList = RunPreParse(filepath);
            var traces = ParseTraces(eventList);

            OutputModel = new RmrFileTypeModel { TraceList = traces, EventList = _events.Values.ToList() };
            return Task.CompletedTask;
        }

        private IEnumerable<EventInstance> RunPreParse(string filepath)
        {
            var eventInstances = new List<EventInstance>();
            var reader = new StreamReader(File.OpenRead(filepath));
            var count = -1;
            while (!reader.EndOfStream)
            {
                if (count < 0)
                {
                    reader.ReadLine();
                    count++;
                    continue;
                }

                var values = reader.ReadLine().Split('\t');
                var startTime = DateTime.MaxValue;

                try
                {
                    startTime = DateTime.Parse(values[4]);
                }
                catch (Exception)
                {
                    this._logger.LogError(
                        $"Datetime could not be parsed. Value '{values[4]}' or '{values[5]}' is not a datetime");
                }

                var processID = values[8];
                var name = values[0];
                if (name.StartsWith("Load dbo_HUB")) name = "Load dbo_HUB";
                if (name.StartsWith("Load usp_UPD_SAL")) name = "Load usp_UPD_SAL";
                if (name.StartsWith("DFT")) continue;
                if (name.StartsWith("Dummy")) continue;
                if (name.StartsWith("Sequence Container")) continue;
                if (name.StartsWith("Transform_Fact")) name = "Transform_Fact";

                if (name.StartsWith("Set PackageName")) continue;
                if (name.StartsWith("EXEC")) continue;
                if (name.StartsWith("Transform DIM")) name = "Transform DIM";
                if (name.StartsWith("EPT Load_AVA")) name = "EPT Load_AVA";
                if (name.StartsWith("SQL")) continue;
                if (name.StartsWith("Start metadata")) continue;
                if (name.StartsWith("Ophalen batchID")) continue;
                if (name.StartsWith("Ophalen actieID")) continue;
                if (name.StartsWith("Status Batch in uitvoering")) continue;

                if (name.StartsWith("Nieuwe actie")) continue;
                if (name.StartsWith("Afsluiten actie")) continue;
                if (name.StartsWith("Actie juist verlopen")) continue;
                if (name.StartsWith("SEQC Start Log")) continue;
                if (name.StartsWith("SEQC Eind log")) continue;
                if (name.StartsWith("Update all statistics")) name = "Update all statistics";

 
                var origin = values[1].Replace('\\', '.');
                if (origin[0] == '.') origin = origin.Substring(1, origin.Length - 1);

                if (!this._events.ContainsKey(name))
                    this._events.Add(
                        name,
                        new Event
                            {
                                ID = count,
                                Name = name,
                                Origin = origin,
                                IsStart = true,
                                Attributes = new ExpandoObject()
                            });
                var startEvent = new EventInstance(this._events[name], processID, startTime);
                startEvent.Event.Attributes.ThirteenKCount = int.Parse(values[9]);
                startEvent.Event.Attributes.SLValue = values[7];

                // var endEvent = new Event
                // {
                //    ID = count,
                //    Name = name + 'e',
                //    Origin = values[1],
                //    Datetime = dTime,
                //    IsStart = false,
                //    ProcessID = values[8],
                //    Attributes = new ExpandoObject()
                // };
                // endEvent.Attributes.ThirteenKCount = int.Parse(values[9]);
                // endEvent.Attributes.SLValue = values[7];
                eventInstances.Add(startEvent);

                count++;
            }

            return eventInstances;
        }
    }
}