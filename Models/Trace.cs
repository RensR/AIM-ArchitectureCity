using System.Collections.Generic;

namespace AIM.Models
{
    public class Trace
    {
        public int ID { get; set; }

        public List<EventInstance> EventList { get; set; }

        public Trace(List<EventInstance> eventList)
        {
            EventList = eventList;
        }
    }
}