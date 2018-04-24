namespace AIM.Plugins.Visualizers.DataTypes
{
    /// <summary>
    /// Helper class for combining two floating point numbers, mostly used as
    /// coordinates in a 2D space
    /// </summary>
    public class Point
    {
        /// <summary>
        /// Gets or sets the X portion of the Point
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the Y portion of the Point
        /// </summary>
        public float Y { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class. 
        /// </summary>
        /// <param name="x">
        /// The X coordinate of the point
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        public Point(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// Pretty prints the Point class
        /// </summary>
        /// <returns> Returns the pretty string version of the X and Y coordinate
        /// of the Point
        /// </returns>
        public override string ToString()
        {
            return $"x:{this.X}, y:{this.Y}";
        }
    }
}