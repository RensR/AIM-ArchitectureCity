namespace Framework.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;

    using Framework.Data;

    using Microsoft.EntityFrameworkCore;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class RmrFileTypeModel
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public JObject Traces;

        public Dictionary<string, Trace> TraceList;

        public List<Event> EventList;


        public static RmrFileTypeModel ReadFromRmrFile(string path)
        {
            return new RmrFileTypeModel();
        }

        public static RmrFileTypeModel ReadFromDB(int id, DbContextOptions<PluginContext> options)
        {
            RmrFileTypeModel result;
            using (var db = new PluginContext(options))
            {
                result = db.FindAsync<RmrFileTypeModel>(id).Result;
            }

            return result;
        }

        public static void WriteToDb(RmrFileTypeModel model, DbContextOptions<PluginContext> options)
        {
            using (var db = new PluginContext(options))
            {
                db.Add(model);
            }
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
