namespace Framework.Plugins.Core
{
    using System;

    /// <summary>
    /// Different types of connections between nodes 
    /// </summary>
    public enum Type
    {
        /// <summary>
        /// You pick one or the other, not both
        /// </summary>
        ExclusiveChoice,

        /// <summary>
        /// Simple sequence with one possible path
        /// </summary>
        Sequence,

        /// <summary>
        /// Both nodes will be run but the order is unknown
        /// </summary>
        Parallel
    }

    /// <summary>
    /// Helper class that houses all Operator types, or transitions between nodes 
    /// </summary>
    public class Operator
    {
        /// <summary>
        /// Gets or sets the starting node of the connection        
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the end node of the connection        
        /// </summary>
        public int End { get; set; }

        /// <summary>
        /// Gets or sets the weight of the connection        
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// Gets the symbol associated with the type of connection. This symbol is only
        /// used in the matrix display of the <see cref="KSuccessor"/> Draw method.
        /// </summary>
        public string Symbol
        {
            get
            {
                switch (this.Type)
                {
                    case Type.ExclusiveChoice:
                        return "?";
                    case Type.Sequence:
                        return ">";
                    case Type.Parallel:
                        return "|";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the connection        
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Operator"/> class.     
        /// </summary>
        /// <param name="start">
        /// The start node id
        /// </param>
        /// <param name="end">
        /// The end node id
        /// </param>
        /// <param name="type">
        /// The type of connection
        /// </param>
        /// <param name="weight">
        /// The weight of the connection
        /// </param>
        public Operator(int start, int end, Type type, int weight = 0)
        {
            Start = start;
            End = end;
            Type = type;
            Weight = weight;
        }

        /// <summary>
        /// Returns the string representation of the Operator class.      
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation including type and weight of the connection.
        /// </returns>
        public override string ToString()
        {
            return $"Type: {Type}, Weight: {Weight}";
        }
    }
}