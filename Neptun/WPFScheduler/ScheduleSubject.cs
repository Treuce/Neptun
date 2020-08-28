using Neptun;
using Neptun.Core;
using System;
using System.Windows.Media;

namespace WpfScheduler
{
	public class ScheduleSubject : Neptun.BaseViewModel
	{
		private DateRange mDateRange;
		public Guid Id { get; set; }
		public string Subject { get; set; }
		public string Description { get; set; }
		public string Code { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }

		public DateRange DateRange { get => mDateRange ??= new DateRange(Start, End); set => mDateRange = value; }
		public bool AllDay { get; set; }

		public bool IsEnabled { get; set; }
		public string Type { get; set; }
		public string BackGroundColor_HEX { get; set; }
		public string courseID { get; set; }
		public ScheduleSubject()
		{
			Id = Guid.NewGuid();
		}
		public ScheduleSubject(ScheduleSubject other)
		{
			Id = Guid.NewGuid();
			DateRange = other.DateRange;
			Subject = other.Subject;
			Description = other.Description;
			Code = other.Code;
			Start = other.Start;
			Type = other.Type;
			End = other.End;
			courseID = other.courseID;
			AllDay = other.AllDay;
			IsEnabled = other.IsEnabled;
			BackGroundColor_HEX = other.BackGroundColor_HEX;
		}
	}
}
