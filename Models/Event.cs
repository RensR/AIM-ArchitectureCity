namespace Framework.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// The event class contains information about an event that can have multiple
    /// occurrences as <see cref="EventInstance"/>s. It contains all static information
    /// that would be the same over multiple <see cref="EventInstance"/>s.
    /// </summary>
    public class Event : IComparable
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        public string Thread { get; set; }

        public string Origin { get; set; }

        public bool IsStart { get; set; }

        public int Count { get; set; }

        public dynamic Attributes;

        public override string ToString()
        {
            return $"{this.ID}" +
                   $"\t{this.Name}" +
                   $"\t{this.Thread}" +
                   $"\t{this.Origin}" +
                   $"\t{this.IsStart}";
        }

        public int CompareTo(object obj)
        {
            Event toCompare = (Event)obj;
            return string.CompareOrdinal(this.Name, toCompare.Name);
        }
    }

    /// <summary>
    /// Contains all information of single instances of <see cref="Event"/>s. 
    /// </summary>
    public class EventInstance
    {
        public Event Event { get; set; }

        [Required]
        public string ProcessID { get; set; }

        public DateTime Datetime { get; set; }

        public EventInstance(Event @event, string processID, DateTime dateTime)
        {
            this.Event = @event;
            @event.Count += 1;
            this.ProcessID = processID;
            this.Datetime = dateTime;
        }
    }
}