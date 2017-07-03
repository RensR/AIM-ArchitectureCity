namespace Framework.Models
{
    using System.ComponentModel.DataAnnotations;

    using Framework.Controllers;

    public class PluginDescription
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public PluginController.PluginType Type { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        public string FilePath { get; set; }
    }
}