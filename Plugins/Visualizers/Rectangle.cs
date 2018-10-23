using AIM.Plugins.Visualizers.DataTypes;

namespace AIM.Plugins.Visualizers
{
    /// <summary>
    /// Rectangle class that represents a equal sides rectangle with a variable height
    /// </summary>
    public class Rectangle
    {
        public Point TopLeft { get; set; } = new Point(0, 0);

        public float X => this.TopLeft.X;

        public float Y => this.TopLeft.Y;

        public float Width { get; set; }

        public float Height { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> class. 
        /// </summary>
        /// <param name="x"> the X coordinate of the top left corner of the rectangle
        /// </param>
        /// <param name="y"> the Y coordinate of the top left corner of the rectangle
        /// </param>
        /// <param name="width"> the width and depth of the rectangle
        /// </param>
        /// <param name="height"> the height of the rectangle
        /// </param>
        public Rectangle(float x, float y, float width, float height)
        {
            this.TopLeft = new Point(x, y);
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> class. 
        /// The empty initializer is needed to construct a rectangle while parsing
        /// a text file because the arguments are not present at the same time.
        /// </summary>
        public Rectangle()
        {  
        }

        /// <summary>
        /// Pretty prints the string version of the rectangle
        /// </summary>
        /// <returns> A pretty printed string of the rectangle
        /// </returns>
        public override string ToString()
        {
            return $"{this.TopLeft}, width:{this.Width}, height:{this.Height}";
        }
    }
}