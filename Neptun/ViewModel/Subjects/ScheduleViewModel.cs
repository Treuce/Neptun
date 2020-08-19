using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfScheduler;

namespace Neptun
{
	public class ScheduleViewModel : BaseViewModel
	{
		#region Constructor
		public ScheduleViewModel()
		{
			Subjects = new ObservableCollection<SubjectViewModel>();
			Events = new ObservableCollection<WpfScheduler.ScheduleSubject>();
			Events.Add(new ScheduleSubject()
			{
				Start = DateTime.Now,
				End = DateTime.Now.AddHours(1),
				AllDay = false,
				Subject = "Test"
			});
		}
		public ObservableCollection<WpfScheduler.ScheduleSubject> Events { get; set; }
		public ObservableCollection<SubjectViewModel> Subjects { get; set; }
		#endregion
	}
}
