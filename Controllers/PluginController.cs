namespace Framework.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Framework.Data;
    using Framework.Models;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    using Newtonsoft.Json;

    /// <summary>
    /// Controller that handles all web requests the contain plugin interaction
    /// </summary>
    public class PluginController : Controller
    {
        /// <summary>
        /// Gets or sets the list that contains all available plugins
        /// </summary>
        public List<PluginDescription> PluginList { get; set; }

        private readonly ILogger<PluginController> logger;
        private readonly DbContextOptions<PluginContext> options;

        /// <summary>
        /// Enums for every plugin type and an error case
        /// </summary>
        public enum PluginType
        {
            Parser,

            Analyzer,

            Visualizer,

            Error
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginController"/> class. 
        /// </summary>
        /// <param name="logger">
        /// The logger that comes from dependency injection 
        /// </param>
        /// <param name="options">
        /// The database context options for the Postgres database
        /// </param>
        public PluginController(ILogger<PluginController> logger, DbContextOptions<PluginContext> options)
        {
            this.logger = logger;
            this.options = options;
        }

        [HttpGet]
        public IActionResult Index()
        {
            using (var db = new PluginContext(this.options))
            {
                PluginList = db.PluginDescription
                    .ToList();
                PluginList.Sort(delegate(PluginDescription a, PluginDescription b)
                {
                    var xdiff = a.Type.CompareTo(b.Type);
                    return xdiff != 0 ? xdiff : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                });
            }

            this.logger.LogInformation($"Plugins loaded: {PluginList.Count}");

            return View(PluginList);
        }

        [HttpPost]
        public ActionResult LookForPlugins()
        {
            this.logger.LogInformation("Looking for new .dll plugins");

            var plugins = new List<PluginDescription>();

            try
            {
                foreach (var file in Directory.GetFiles("plugins\\", "*.json*", SearchOption.AllDirectories))
                {
                    using (var fi = System.IO.File.OpenText(file))
                    {
                        var serializer = new JsonSerializer();
                        var plugin = (PluginDescription)serializer.Deserialize(fi, typeof(PluginDescription));
                        plugin.FilePath = file;
                        plugin.ID = 0;
                        plugins.Add(plugin);
                    }
                }
            }
            catch (DirectoryNotFoundException e)
            {
                this.logger.LogError($"Plugin directory not found, no plugins will be added: {e.InnerException}");
            }

            try
            {
                using (var db = new PluginContext(this.options))
                {
                    var newPlugins = 0;
                    var updatedPlugins = 0;
                    foreach (var p in plugins)
                    {
                        var oldVersion =
                            db.PluginDescription.FirstOrDefault(description => p.Name == description.Name);
                        if (oldVersion == null)
                        {
                            db.Add(p);
                            newPlugins++;
                            this.logger.LogInformation($"New plugin {p.Name} added.");
                        }
                        else if (oldVersion.Version != p.Version)
                        {
                            this.logger.LogInformation($"Plugin {p.Name} updated from version {oldVersion.Version} to version {p.Version}.");
                            oldVersion.Description = p.Description;
                            oldVersion.FilePath = p.FilePath;
                            oldVersion.Type = p.Type;
                            oldVersion.Author = p.Author;
                            oldVersion.Version = p.Version;
                            updatedPlugins++;
                        }
                    }

                    if (updatedPlugins + newPlugins == 0)
                        this.logger.LogInformation("No new plugins found");
                    else
                    {
                        this.logger.LogInformation($"New plugins found: {newPlugins} \n" +
                                             $"      Updated plugins  : {updatedPlugins}");
                        db.SaveChanges();
                    }
                }
            }
            catch (DbUpdateException e)
            {
                this.logger.LogError($"Database could not update while getting new plugins: {e.InnerException}");
            }

            return RedirectToAction("Index", "Plugin");
        }
    }
}