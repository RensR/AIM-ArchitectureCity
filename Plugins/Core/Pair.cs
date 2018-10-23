using System;

namespace AIM.Plugins.Core
{
    /// <summary>
    /// Mutable version of the <see cref="Tuple{T1, T2}"/> class
    /// </summary>
    /// <typeparam name="T1">
    /// Can be any type
    /// </typeparam>
    /// <typeparam name="T2">
    /// Can be any type, even the same as T1
    /// </typeparam>
    public class Pair<T1, T2>
    {
        /// <summary>
        /// Gets or sets the mutable object of class T1
        /// </summary>
        public T1 First { get; set; }

        /// <summary>
        /// Gets or sets the mutable object of class T2
        /// </summary>
        public T2 Second { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pair{T1,T2}"/> class. 
        /// </summary>
        /// <param name="f">
        /// The first object of the tuple of class T1
        /// </param>
        /// <param name="s">
        /// The second object of the tuple of class T2
        /// </param>
        public Pair(T1 f, T2 s)
        {
            this.First = f;
            this.Second = s;
        }
    }
}
