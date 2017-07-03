﻿namespace Framework.Plugins.Visualizers
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Exports a list of <see cref="Building"/>s and <see cref="Road"/>s to the OpenSCAD format
    /// to be openen in the OpenSCAD viewer as a 3D representation of a city.
    /// </summary>
    public static class OpenSCADExport
    {
        /// <summary>
        /// Exports buildings and roads to the OpenSCAD format
        /// </summary>
        /// <param name="buildings">
        /// The buildings to be exported
        /// </param>
        /// <param name="roads">
        /// The roads to be exported
        /// </param>
        public static void Export(IEnumerable<Building> buildings, IEnumerable<Road> roads)
        {
            string output = "include<Building.scad>\n  " + 
                "$vpr = [47.3, 0, 32.7];\n" +
                "$vpd = 8524.52;\n\n" +
                "// Insert Buildings\n";

            foreach (var building in buildings)
            {
                float r, g, b;
                float a = 1f;

                if (building.Color[0] == '#')
                {
                    r = byte.Parse(building.Color.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier)
                        / 255f;
                    g = byte.Parse(building.Color.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier)
                        / 255f;
                    b = byte.Parse(building.Color.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier)
                        / 255f;
                }
                else
                {
                    var splits = building.Color.Split(',');
                    r = float.Parse(splits[0]);
                    g = float.Parse(splits[1]);
                    b = float.Parse(splits[2]);
                    a = float.Parse(splits[3]);
                }

                int height = (int)Math.Ceiling(building.Height);
                int width = (int)(building.OutLine.Width * 96);

                const int MaxChar = 16;
                building.Label = building.Label.Replace('\t', ' ');
                var lbl1 = building.Label.Substring(0, Math.Min(MaxChar, building.Label.Length)).Trim();
                var lbl2 = building.Label.Length > MaxChar
                               ? building.Label.Substring(MaxChar, Math.Min(MaxChar, building.Label.Length - MaxChar)).Trim()
                               : string.Empty;


                if (building.Label.Length > 10) building.Label = building.Label.Substring(0, 10);

                output += $"Building( {building.OutLine.X - width / 2}, " + $"{building.OutLine.Y - width / 2}, "
                          + $"{height}, " + $"{width}, " + $"[{r},{g},{b},{a}]," + $"\"{building.CallCount}\","
                          + $"\"{lbl1}\"," + $"\"{lbl2}\");\n";
            }

            output += $"\n// Insert Roads\n";

            foreach (var road in roads)
            {
                var roadColor = Math.Sqrt(road.Width);

                var r = Math.Min(0.93, Math.Max(0.25, (roadColor * 0.5) - 0.4));
                var g = Math.Min(0.9, Math.Max(0.15, 1.4 - (roadColor * 0.5)));

                // b and a are static as this creates a transition from green to red
                var color = $"{r}, {g}, 0.22, 1";


                for (var index = 2; index < road.Points.Count; index++)
                {
                    output += $"Road([{road.Points[index].X},{road.Points[index].Y},0],"
                              + $"[{road.Points[index - 1].X},{road.Points[index - 1].Y},0]," + $"{road.Width},"
                              + $"[{color}]);\n";
                }

                var lastPoint = road.Points[road.Points.Count - 1];

                float xDiff = lastPoint.X - road.Points[road.Points.Count - 2].X;
                float yDiff = lastPoint.Y - road.Points[road.Points.Count - 2].Y;
                var angle = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;

                output += $"Arrow([{lastPoint.X}, {lastPoint.Y}, 0]," + $"{angle}," + $"[{color}],"
                          + $"{3 * road.Width});\n";
            }

            File.WriteAllText($"OpenSCAD\\Input.scad", output);
        }
    }
}