using System;

namespace WpfScheduler
{
	public class ScheduleSubject
    {
        public Guid Id { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public DateTime Start {get; set; }
        public DateTime End { get; set; }
        public bool AllDay { get; set; }

        public ScheduleSubject()
        {
            Id = Guid.NewGuid();
        }
    }
}
