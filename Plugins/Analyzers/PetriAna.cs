using System.Diagnostics;
using System.Linq;
using AIM.Models;
using AIM.Plugins.Core;
using Microsoft.Extensions.Logging;

namespace AIM.Plugins.Analyzers
{
    public class PetriAna : Analyzer
    {
        public override string Name { get; } = "PetriAna";

        public PetriNet PetriNet { get; set; }

        public PetriAna(RmrFileTypeModel input, ILogger logger) : base(input)
        {
            var watch = new Stopwatch();
            watch.Start();
            var k1 = this.CreateKSuccessorList(1, input);

            logger.LogDebug($"k1-5 computed in\t{watch.ElapsedMilliseconds/1000f:N}seconds");
            watch.Restart();
            var kn = this.CreateKSuccessorList(int.MaxValue / 2, input);
            logger.LogDebug($"kn computed in\t{watch.ElapsedMilliseconds / 1000f:N}seconds");

            var firstNodes = input.TraceList.Values.Select(trace => trace.EventList[0].Event.ID).Distinct();

            watch.Restart();
            this.PetriNet = new PetriNet(k1, kn, firstNodes);

            logger.LogDebug($"Petrinet computed in\t{watch.ElapsedMilliseconds / 1000f:N}seconds");

            kn.Draw();
            logger.LogDebug($"kn drawn in\t{watch.ElapsedMilliseconds / 1000f:N}seconds");
            //k1.Draw();
        }

        private KSuccessor CreateKSuccessorList(int k, RmrFileTypeModel rmr)
        {
            // Do Reduce
            var ksuc = new KSuccessor(k, rmr.EventList);
            foreach (var trace in rmr.TraceList) ksuc.Add(trace.Value);

            return ksuc;
        }
    }
}
