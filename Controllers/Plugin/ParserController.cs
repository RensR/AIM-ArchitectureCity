namespace Framework.Controllers.Plugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Framework.Models;
    using Framework.Plugins;
    using Framework.Plugins.Analyzers;
    using Framework.Plugins.Parsers.PGGMParse;
    using Framework.Plugins.Parsers.SpottaParse;
    using Framework.Plugins.Visualizers;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    public class ParserController : Controller
    {
        public List<PluginDescription> PluginList = new List<PluginDescription>
                             {
                                 new PluginDescription(
                                     0,
                                     "SpottaParse",
                                     PluginDescription.PluginType.Parser, 
                                     "Parser for Spotta logs",
                                     "v1.0.0",
                                     "R.M. Rooimans"),
                                 new PluginDescription(
                                     1,
                                     "PGGMParse",
                                     PluginDescription.PluginType.Parser,
                                     "Parser for PGGM logs",
                                     "v1.0.0",
                                     "R.M. Rooimans")
                             };

        private readonly ILogger<ParserController> logger;

        private readonly IHostingEnvironment hostingEnvironment;

        public ParserController(
            ILogger<ParserController> logger,
            IHostingEnvironment hostingEnvironment)
        {
            this.logger = logger;
            this.hostingEnvironment = hostingEnvironment;
        }

        // GET: Parser
        public ActionResult Index()
        {
            this.logger.LogInformation($"Plugins loaded: {PluginList.Count}");

            return View("~/Views/Plugin/Parser/Index.cshtml", PluginList);
        }

        // GET: Parser/Run/5
        public ActionResult Run(int id)
        {
            PluginDescription parser = this.PluginList.Find(p => p.ID == id);

            ViewBag.files = Directory.GetFiles("LogStorage").ToList(); ;

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