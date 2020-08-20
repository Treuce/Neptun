using Dna;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Extensions.Logging;
using MindFusion.Scheduling.Wpf;
using Neptun.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfScheduler;

namespace Neptun
{
	public interface EventsChanged
	{
		Action<ScheduleSubject> Changed { get; set; }
		Action Clear { get; set; }
	}
	public class ScheduleViewModel : BaseViewModel, EventsChanged
	{

		#region Constructor
		public ScheduleViewModel()
		{
			Subjects = new ObservableCollection<SubjectViewModel>();
			Events = new ObservableCollection<WpfScheduler.ScheduleSubject>();
			Changed = new Action<ScheduleSubject>((ScheduleSubject a) => { });
			Clear = new Action(() => { });
			Subjects.CollectionChanged += Subjects_CollectionChanged;
		}

		private void Subjects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			foreach (var subject in (sender as ObservableCollection<SubjectViewModel>))
			{
				subject.Expanded += () =>
				{
					foreach (var b in Subjects)
					{
						if (b.Code != subject.Code)
							b.InfoExpanded = false;
					}
				};
				subject.Collapsed += () =>
				{
					Clear.Invoke();
				};
				subject.HasCourses += () =>
				{
					foreach (var course in subject.TakeViewModel.Courses)
					{
						var asd = course.Schedule;
						var day = asd.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						var damn = new Dictionary<string, string>();
						for (int i = 0; i < day.Length; ++i)
							damn.Add(day[i], day[++i]);
						var currentDate = DateTime.Today;
						foreach (var b in damn)
						{
							var startday = DateTime.Today;
							var enddate = DateTime.Today;
							var timespans = b.Value.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
							startday += TimeSpan.Parse(timespans[0]);
							enddate += TimeSpan.Parse(timespans[1]);
							DayOfWeek dayOfWeek;
							switch (b.Key)
							{
								case "Hétfő":
									dayOfWeek = DayOfWeek.Monday;
									break;
								case "Kedd":
									dayOfWeek = DayOfWeek.Tuesday;
									break;
								case "Szerda":
									dayOfWeek = DayOfWeek.Wednesday;
									break;
								case "Csütörtök":
									dayOfWeek = DayOfWeek.Thursday;
									break;
								default:
									dayOfWeek = DayOfWeek.Friday;
									break;
							}
							while (startday.DayOfWeek != DayOfWeek.Monday)
							{
								startday = startday.AddDays(-1);
								enddate = enddate.AddDays(-1);
							}
						
							startday = startday.AddDays((int)dayOfWeek-1);
							enddate = enddate.AddDays((int)dayOfWeek-1);
							Application.Current.Dispatcher.Invoke(() =>
							{
								//ScheduleSubject a = new ScheduleSubject
								Changed.Invoke(new ScheduleSubject()
								{
									Start = startday,
									End = enddate,
									AllDay = false,
									Subject = $"{subject.Name} ({subject.Code}) {Environment.NewLine} #{course.CourseCode}{Environment.NewLine}{course.Teacher}",
									Description = $"{course.Teacher}"
								});
							});
						}

					}
				};
			}	
		}

		public ObservableCollection<WpfScheduler.ScheduleSubject> Events { get; set; }
		public ObservableCollection<SubjectViewModel> Subjects { get; set; }
		public Action<ScheduleSubject> Changed { get; set; }
		public Action Clear { get; set; }

		#endregion
	}
}
