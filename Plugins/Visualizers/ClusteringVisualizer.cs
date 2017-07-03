namespace Framework.Plugins.Visualizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Framework.Plugins.Analyzers.Clustering.DataTypes;
    using Framework.Plugins.Visualizers.DataTypes;

    /// <summary>
    /// Parses a dot file to a more general building/road combination that is then passed to another
    /// specific visualizer that renders it to a 3D model.
    /// </summary>
    public class ClusteringVisualizer : Visualizer
    {

        /// <summary>
        /// Feature toggle: fancolors
        /// </summary>
        private const bool FanColors = true;

        /// <summary>
        /// Return the name of the visualizer
        /// </summary>
        public override string Name => "Clustering Visualizer";

        /// <summary>
        /// Gets the list of buildings
        /// </summary>
        public List<Building> Buildings { get; } = new List<Building>();

        /// <summary>
        /// Gets the list of roads
        /// </summary>
        public List<Road> Roads { get; } = new List<Road>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusteringVisualizer"/> class that parses the dot
        /// file into buildings and roads.
        /// </summary>
        /// <param name="filePath"> The path for the dot file
        /// </param>
        /// <param name="nodeDict"> is a dictionary of nodes to be able trace the node ids from the dot
        ///     file back to actual nodes
        /// </param>
        /// <param name="originalFanInOut">The original fan in and out values of every node
        /// </param>
        public ClusteringVisualizer(
            string filePath,
            Dictionary<int, Node> nodeDict,
            Dictionary<int, Tuple<int, int>> originalFanInOut)
        {
            var maxInOut = originalFanInOut.Max(x => x.Value.Item1 + x.Value.Item2);

            filePath = $"DOTGraph\\{filePath}-Co.dot";
            var reader = new StringReader(filePath);

            // Parse the constants
            // Not used line e.g. digraph G {
            reader.ReadLine();

            // Total size of the area of the graph
            // e.g. graph [bb="0,0,1818,2101"];
            string line;
            reader.ReadLine();
            reader.ReadLine();

            while ((line = reader.ReadLine()) != null)
            {
                var split = Regex.Match(line, "\t[^\t]*\t").Value;

                // Read the lines and remove any newline chars within properties
                // Split the line on properties. These will be parsed by the Road/Building parsers
                while (!Regex.Match(line, "\\];").Success)
                {
                    var nextLine = reader.ReadLine();
                    if (nextLine == null) return;
                    line += "\n" + nextLine;
                }

                line = Regex.Replace(line, "[^(,|;)]\n", string.Empty);
                var properties = line.Split('\n');

                // Check if it's an edge or node
                // If it contains -> it is an edge
                if (split.Contains("->")) this.Roads.Add(ParseRoad(properties));
                else
                {
                    var newBuilding = ParseBuilding(properties);
                    this.Buildings.Add(newBuilding);

                    var parentNodes = GetParents(nodeDict[newBuilding.ID]);
                    var max = (int)Math.Ceiling(Math.Sqrt(parentNodes.Count));

                    for (int i = 0; i < max; i++)
                    {
                        for (int j = 0; j < max; j++)
                        {
                            if ((i * max) + j >= parentNodes.Count) break;
                            Node current = parentNodes[(i * max) + j];
                            Building b = new Building(current.ID)
                                             {
                                                 Height =
                                                     ((float)Math.Log10(current.CallCount) * 60 + 3),
                                                 OutLine =
                                                     new Rectangle(
                                                         newBuilding.OutLine.X + (i * 96)
                                                         - (((newBuilding.OutLine.Height / 2) - 0.5f) * 96),
                                                         newBuilding.OutLine.Y + (j * 96)
                                                         - (((newBuilding.OutLine.Width / 2) - 0.5f) * 96),
                                                         0.8f,
                                                         0.8f),

                                                 // Green 0x419636ff
                                                 Color = "#419636ff",
                                                 CallCount = current.CallCount,
                                                 Label = current.Label
                                             };
                            if (FanColors)
                            {
                                var @in = originalFanInOut[current.ID].Item1;
                                var @out = originalFanInOut[current.ID].Item2;

                                if (@in == 0)
                                {
                                    b.Color = "0.9,0.9,0.9";
                                }
                                else if (@out == 0)
                                {
                                    b.Color = "0.25,0.25,0.25";
                                }
                                else
                                {
                                    var inOut = (@in / (float)@out) - 1;
                                    if (@in > @out) inOut = -(@out / (float)@in) + 1;

                                    var r = Math.Min(0.95, Math.Max(0.25, 0.6 + (inOut * 0.4)));
                                    var g = Math.Min(0.9, Math.Max(0.15, 0.55 + (-inOut * 0.4)));

                                    b.Color = $"{r}, {g}, 0.22";
                                }

                                b.Color += $",{((@in + @out) / (float)maxInOut) / 3 + 0.66}";
                            }
                            else
                            {
                                // Orange 0xf48942ff
                                if (current.CallCount > 100) b.Color = "#f48942ff";

                                // Red 0xb50707ff
                                if (current.CallCount > 10000) b.Color = "#b50707ff";
                            }

                            Buildings.Add(b);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves all the parents nodes recursively
        /// </summary>
        /// <param name="node"> The node of which the parents need to be returned
        /// </param>
        /// <returns> A list containing all the parent nodes if there are any. If there are none, it returns itself in a list
        /// </returns>
        private static List<Node> GetParents(Node node)
        {
            if (node.Parents.Count <= 0)
                return new List<Node> { node };
            var nodelist = new List<Node>();
            foreach (var nodeParent in node.Parents)
            {
                nodelist.AddRange(GetParents(nodeParent));
            }

            return nodelist;
        }

        /// <summary>
        /// A run method that return a task while all of the work is done on creation of this class.
        /// </summary>
        /// <returns> A status message that is displayed in the view
        /// </returns>
        public override async Task<string> RunAsync()
        {
            return "The image below is the 2D blueprint of the ArchitectureCity. To see the 3D image please load the proper file into OpenSCAD";
        }

        /// <summary>
        /// Parses a road from the corresponding lines of dot
        /// </summary>
        /// <param name="properties"> The lines of dot that correspond to the road
        /// </param>
        /// <returns> A road containing all the properties from the lines of dot the method received
        /// </returns>
        private static Road ParseRoad(IList<string> properties)
        {
            // 0-> 5[label = 2372,
            // lp = "497.5,3175.5",
            //// pos = "e,466.97,3150.3 520.28,3224.5 506.62,3211.8 493.89,3197.8 483.5,3183 478.51,3175.9 474.29,3167.9 470.72,3159.7"];

            string ids = Regex.Match(properties[0], "(?<=\t)\\d+ -> \\d+(?=\t)").Value;
            properties[0] = Regex.Replace(properties[0], "\t\\d+ -> \\d+\t \\[", "\t\t");

            double width = 0;
            string color = string.Empty;
            List<Point> trace = new List<Point>();

            foreach (string property in properties)
            {
                // The second character is [3] because it starts with \t\t and is 0 based
                switch (property[3])
                {
                    // label
                    case 'a':
                        width = 2 + Math.Log10(double.Parse(Regex.Match(property, "(?<==)\\d+(?=,)").Value));
                        break;

                    // ?? 
                    case 'p':
                        break;

                    // Coordinates
                    case 'o':

                        var splits = Regex.Matches(property, "(?<=(,| ))-?\\d+(\\.\\d+)?(?=( |\"|,))");
                        for (int i = 0; i < splits.Count; i += 2)
                        {
                            trace.Add(
                                new Point(
                                    float.Parse(splits[i].Value) * 1.34f,
                                    float.Parse(splits[i + 1].Value) * 1.34f));
                        }

                        break;
                }
            }

            return new Road(trace, width, color);
        }

        /// <summary>
        /// Parses a building from the corresponding lines of dot
        /// </summary>
        /// <param name="properties"> The lines of dot that correspond to the building
        /// </param>
        /// <returns> A road containing all the properties from the lines of dot the method received
        /// </returns>
        private static Building ParseBuilding(IList<string> properties)
        {
            // A node looks like e.g.
            // 	0	 [fillcolor="#17171717",
            // fixedsize=true,
            // height=2.25,
            // tlabel=151762,
            // pos="247,2020",
            // shape=box,
            // style=filled,
            // width=4.5];
            // The first line has the node ID after one '\t'
            string id = Regex.Match(properties[0], "(?<=\t)\\d+(?=\t)").Value;
            Building building = new Building(int.Parse(id));

            properties[0] = Regex.Replace(properties[0], "\t\\d+\t \\[", "\t\t");

            Rectangle rectangle = new Rectangle();

            foreach (string property in properties)
            {
                var equalIndex = property.IndexOf('=') + 1;
                var length = property.Contains("]")
                                 ? property.IndexOf(']') - equalIndex
                                 : property.IndexOf(',') - equalIndex;

                // We can identify the property by the third character of the property name

                // The third character is [4] because it starts with \t\t and is 0 based
                switch (property[4])
                {
                    // fillcolor
                    case 'l':
                        // + 1 and - 2 to remove the "" around the string
                        building.Color = FanColors ? "#3d3d3d" : property.Substring(equalIndex + 1, length - 2);
                            
                        break;

                    // height
                    case 'i':
                        building.Height = 1;
                        break;

                    // label
                    case 'b':
                        building.Label = property.Substring(equalIndex, length);
                        building.ID = int.Parse(building.Label);
                        break;

                    // pos
                    case 's':
                        rectangle.TopLeft.X = float.Parse(property.Substring(equalIndex + 1, length - 1)) * 1.34f;
                        var match = Regex.Match(property, "(\\.?\\d)+(?=\")").Value;
                        rectangle.TopLeft.Y = float.Parse(match) * 1.34f;
                        break;

                    // width
                    case 'd':
                        rectangle.Width = float.Parse(property.Substring(equalIndex, length));
                        rectangle.Height = rectangle.Width;
                        break;

                    // fixedsize
                    // shape
                    // style
                    case 'x':
                    case 'a':
                    case 'y':
                        break;
                }
            }

            building.OutLine = rectangle;
            return building;
        }
    }

    public class Building
    {
        public Rectangle OutLine { get; set; }

        public float Height { get; set; } = 1;

        public string Color { get; set; }

        public string Label { get; set; } = string.Empty;

        public int ID { get; set; }

        public int CallCount { get; set; }

        public Building(int id)
        {
            this.ID = id;
        }
    }

    public class Road
    {
        public List<Point> Points { get; set; }

        public double Width { get; set; }

        public string Color { get; set; }

        public Building From { get; set; }

        public Building To { get; set; }

        public Road(List<Point> points, double width, string color)
        {
            Points = points;
            Width = width;
            Color = color;
        }
    }
}
