namespace Framework.Models.Nodes
{
    using System.ComponentModel.DataAnnotations;

    public class SimpleNodeModel
    {
        public static string Header => "label,source,name";

        [Key]
        [Required]
        public long ID { get; set; }

        [Required]
        [Display(Name = "Label")]
        public string Label { get; set; }

        [Display(Name = "Source")]
        public string Source { get; set; }

        public NodeType Type { get; set; }

        public override string ToString()
        {
            return $"label: {this.Label}, source: {this.Source}, id: {this.ID}, type: {this.Type}";
        }

        public SimpleNodeModel(int id, string label, NodeType type = NodeType.Normal, string source = "c")
        {
            this.ID = id;
            this.Label = label;
            this.Source = source;
            this.Type = type;
        }

        public string ToCsvWithoutId()
        {
            return $"{this.ID},{this.Source},{this.Label}";
        }
    }

    public enum NodeType
    {
        Start,
        End,
        Helper,
        Normal
    }
}
