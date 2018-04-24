using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AIM.Models
{
    public class RmrFileTypeModel
    {
        public int ID { get; set; }

        public JObject Traces;

        public Dictionary<string, Trace> TraceList;

        public List<Event> EventList;


        public static RmrFileTypeModel ReadFromRmrFile(string path)
        {
            return new RmrFileTypeModel();
        }

        public static void WriteToFile(RmrFileTypeModel model)
        {
            using (var file = File.CreateText(@"log.json"))
            using (JsonWriter writer = new JsonTextWriter(file))
            {
                model.Traces.WriteTo(writer);
            }
        }
    }
}
