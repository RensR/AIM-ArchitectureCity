using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AIM.Models;
using AIM.Plugins;
using AIM.Plugins.Analyzers;
using AIM.Plugins.Parsers;
using AIM.Plugins.Visualizers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace AIM.Controllers.Plugin
{
    public class ParserController : Controller
    {
        public List<PluginDescription> PluginList = new List<PluginDescription>
                             {
                                 new PluginDescription(
                                     0,
                                     "DLParse",
                                     PluginDescription.PluginType.Parser, 
                                     "Parser for DL logs",
                                     "v1.0.0",
                                     "R.M. Rooimans"),
                                 new PluginDescription(
                                     1,
                                     "FIParse",
                                     PluginDescription.PluginType.Parser,
                                     "Parser for FI logs",
                                     "v1.0.0",
                                     "R.M. Rooimans")
                             };

        private readonly ILogger<ParserController> _logger;

        private readonly IHostingEnvironment _hostingEnvironment;

        public ParserController(
            ILogger<ParserController> logger,
            IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: Parser
        public ActionResult Index()
        {
            _logger.LogInformation($"Plugins loaded: {PluginList.Count}");

            return View("~/Views/Plugin/Parser/Index.cshtml", PluginList);
        }

        // GET: Parser/Run/5
        public ActionResult Run(int id)
        {
            PluginDescription parser = PluginList.Find(p => p.ID == id);
            
            // If the LogStorage folder does not exist, create it
            Directory.CreateDirectory("LogStorage");

            ViewBag.files = Directory.GetFiles("LogStorage").ToList();

            ViewBag.Analyzers = new List<string> { "Clustering - package", "Clustering - fan", "Petri net" };

            if (parser != null)
                return View("~/Views/Plugin/Parser/Run.cshtml", parser);
            
            _logger.LogError($"NOTFOUNDERROR\tplugin/parser/run/{id}\t{DateTime.Now}");
            ViewData.Add("Title", $"NotFoundError, the parser with id {id} was not found ");
            return View("~/Views/Shared/Error.cshtml");
        }

        // POST: Parser/Run/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Run(int id, IFormCollection collection)
        {
            // Determine the parser (hard coded)
            var parser = id == 1 ? (Parser)new FIParse(_logger) : new DLParse(_logger);

            collection.TryGetValue("input", out StringValues filePath);


            if (filePath.ToString() == string.Empty)
                return View("~/Views/Shared/Error.cshtml");

            var watch = new Stopwatch();
            watch.Start();

            await parser.Run(filePath.ToString());

            _logger.LogDebug($"{DateTime.Now}\tParser finished\ttime taken: {watch.ElapsedMilliseconds/1000f:N}s");
            watch.Restart();

            // Get analyzer type
            collection.TryGetValue("analyzer", out StringValues analyzerType);

            int maxDepth = 1;
            if (collection.TryGetValue("depth", out StringValues depth)) maxDepth = int.Parse(depth);

            switch (analyzerType.ToString())
            {
                case "Petri net":
                    var petriNetAnalyzer = new PetriAna(parser.OutputModel, _logger);
                    _logger.LogDebug(
                        $"{DateTime.Now}\tAnalyser finished\ttime taken: {watch.ElapsedMilliseconds / 1000f:N}s");
                    watch.Restart();

                    var visualizer = new PetriNetVis(petriNetAnalyzer.PetriNet, _logger);

                    ViewBag.htmlString = await visualizer.RunAsync();
                    _logger.LogDebug(
                        $"{DateTime.Now}\tVisualiser finished\ttime taken: {watch.ElapsedMilliseconds / 1000f:N}s");

                    return View("~/Views/Plugin/Visualiser/DependencyGraph.cshtml");

                case "Clustering - package":
                case "Clustering - fan":
                case "Clustering - caller":
                    var clusteringAnalyzer = new ClusteringAnalyzer(_hostingEnvironment, parser.OutputModel, _logger);
                    (string, string) pathAndData;
                    try
                    {
                        pathAndData = clusteringAnalyzer.CalculateClusters(maxDepth, analyzerType.ToString());
                    }
                    catch (EntryPointNotFoundException e)
                    {
                        ViewBag.error =
                            "Please install Graphviz (https://www.graphviz.org/download/) and add it to your path in order to run the visualizations.";
                        return View("~/Views/Shared/Error.cshtml");
                    }
                    
                    var clusteringVisualizer = new ClusteringVisualizer(
                        pathAndData.Item2,
                        clusteringAnalyzer.NodeDict,
                        clusteringAnalyzer.OriginalFanInOut);

                    var architectureCity = OpenJSCADExport.Export(clusteringVisualizer.Buildings, clusteringVisualizer.Roads);
                    OpenSCADExport.Export(clusteringVisualizer.Buildings, clusteringVisualizer.Roads);

                    ViewBag.htmlString = await clusteringVisualizer.RunAsync();
                    ViewBag.architectureCity = architectureCity;

                    return View("~/Views/Plugin/Visualiser/ArchitectureCity.cshtml");

                default:
                    return View("~/Views/Shared/Error.cshtml");
            }
        }
    }
}