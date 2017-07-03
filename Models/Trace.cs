namespace Framework.Models
{
    using System.Collections.Generic;

    public class Trace
    {
        public int ID { get; set; }

        public List<EventInstance> EventList { get; set; }

        public Trace(List<EventInstance> eventList)
        {
            this.EventList = eventList;
        }
    }
}