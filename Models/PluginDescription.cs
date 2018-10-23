namespace AIM.Models
{
    public class PluginDescription
    {
        public enum PluginType
        {
            Parser,

            Analyzer,

            Visualizer,

            Error
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public PluginType Type { get; set; }

        public string Description { get; set; }

        public string Version { get; set; }

        public string Author { get; set; }

        public PluginDescription(
            int id,
            string name,
            PluginType type,
            string description,
            string version,
            string author)
        {

            ID = id;
            Name = name;
            Type = type;
            Description = description;
            Version = version;
            Author = author;
        }
    }
}