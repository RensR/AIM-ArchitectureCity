namespace Framework.Controllers.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Framework.Data;
    using Framework.Models;
    using Framework.Plugins;
    using Framework.Plugins.Analyzers;
    using Framework.Plugins.Parsers.PGGMParse;
    using Framework.Plugins.Parsers.SpottaParse;
    using Framework.Plugins.Visualizers;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    public class ParserController : Controller
    {
        public List<PluginDescription> PluginList { get; set; }

        private readonly ILogger<PluginController> logger;

        private readonly DbContextOptions<PluginContext> options;

        private readonly IHostingEnvironment hostingEnvironment;

        /// <summary>
        /// The plugin types.
        /// </summary>
        public enum PluginType
        {
            Parser,

            Analyzer,

            Visualizer,

            Error
        }

        public ParserController(
            ILogger<PluginController> logger,
            DbContextOptions<PluginContext> options,
            IHostingEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.options = options;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Parser
        public ActionResult Index()
        {
            using (var db = new PluginContext(this.options))
            {
                PluginList = db.PluginDescription.Where(i => i.Type == PluginController.PluginType.Parser)
                    .ToList();
                PluginList.Sort(delegate(PluginDescription a, PluginDescription b)
                {
                    var xdiff = a.Type.CompareTo(b.Type);
                    return xdiff != 0 ? xdiff : string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                });
            }

            this.logger.LogInformation($"Plugins loaded: {PluginList.Count}");

            return View("~/Views/Plugin/Parser/Index.cshtml", PluginList);
        }

        // GET: Parser/Run/5
        public ActionResult Run(int id)
        {
            PluginDescription parser;
            using (var db = new PluginContext(this.options))
            {
                parser = db.PluginDescription.FirstOrDefault(i => i.ID == id);
            }

            var fileEntries = Directory.GetFiles("LogStorage").ToList();

            ViewBag.files = fileEntries;

            ViewBag.Analyzers = new List<string> { "Clustering - package", "Clustering - fan", "Clustering - caller", "Petri net" };

            if (parser != null)
                return View("~/Views/Plugin/Parser/Run.cshtml", parser);
            
            this.logger.LogError($"NOTFOUNDERROR\tplugin/parser/run/{id}\t{DateTime.Now}");
            ViewData.Add("Title", $"NotFoundError, the parser with id {id} was not found ");
            return View("~/Views/Shared/Error.cshtml");
        }

        // POST: Parser/Run/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Run(int id, IFormCollection collection)
        {
            // Determine the parser (hard coded)
            var parser = id == 1 ? (Parser)new PGGMParse(this.logger) : new SpottaParse(this.logger);

            collection.TryGetValue("input", out StringValues filePath);
            var watch = new Stopwatch();
            watch.Start();

            await parser.Run(filePath.ToString());

            this.logger.LogDebug($"{DateTime.Now}\tParser finished\ttime taken: {watch.ElapsedMilliseconds/1000f:N}s");
            watch.Restart();

            // Get analyzer type
            collection.TryGetValue("analyzer", out StringValues analyzerType);

            int maxDepth = 1;
            if (collection.TryGetValue("depth", out StringValues depth)) maxDepth = int.Parse(depth);

            switch (analyzerType.ToString())
            {
                case "Petri net":
                    var petriNetAnalyzer = new PetriAna(parser.OutputModel, this.logger);
                    this.logger.LogDebug(
                        $"{DateTime.Now}\tAnalyser finished\ttime taken: {watch.ElapsedMilliseconds / 1000f:N}s");
                    watch.Restart();

                    var visualizer = new PetriNetVis(petriNetAnalyzer.PetriNet, this.logger);

                    ViewBag.htmlString = await visualizer.RunAsync();
                    this.logger.LogDebug(
                        $"{DateTime.Now}\tVisualiser finished\ttime taken: {watch.ElapsedMilliseconds / 1000f:N}s");

                    return View("~/Views/Plugin/Visualiser/DependencyGraph.cshtml");

                case "Clustering - package":
                case "Clustering - fan":
                case "Clustering - caller":
                    var clusteringAnalyzer = new ClusteringAnalyzer(hostingEnvironment, parser.OutputModel, this.logger);
                    var pathAndData = clusteringAnalyzer.CalculateClusters(maxDepth, analyzerType.ToString());
                    var clusteringVisualizer = new ClusteringVisualizer(
                        pathAndData.Item2,
                        clusteringAnalyzer.NodeDict,
                        clusteringAnalyzer.OriginalFanInOut);

                    OpenSCADExport.Export(clusteringVisualizer.Buildings, clusteringVisualizer.Roads);

                    ViewBag.htmlString = await clusteringVisualizer.RunAsync();
                    ViewBag.ImagePath = "/images/" + pathAndData.Item1 + ".png";

                    return View("~/Views/Plugin/Visualiser/Clustering.cshtml");

                default:
                    return View("~/Views/Shared/Error.cshtml");
            }
        }
    }
}