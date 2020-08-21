using Dna;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Extensions.Logging;
using MindFusion.Scheduling;
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
using System.Windows.Input;
using WpfScheduler;

namespace Neptun
{
	public class ScheduleViewModel : BaseViewModel
	{
		#region Private stuff
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
				//subject.Collapsed += () =>
				//	{
				//		Clear.Invoke();
				//	};
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

								startday = startday.AddDays((int)dayOfWeek - 1);
								enddate = enddate.AddDays((int)dayOfWeek - 1);
								Application.Current.Dispatcher.Invoke(() =>
								{
									//ScheduleSubject a = new ScheduleSubject
									Changed.Invoke(new ScheduleSubject()
									{
										Start = startday,
										End = enddate,
										AllDay = false,
										IsEnabled = course.isEnabled,
										Subject = $"{subject.Name} [{course.Type}] ({subject.Code}) {Environment.NewLine} #{course.CourseCode}{Environment.NewLine}{course.Teacher}{Environment.NewLine}{course.Note}",
										Code = subject.Code,
										Description = $"{course.ToolTip}"
									}, ShowDisabledCourses);
								});
							}

						}
					};
			}
			SubjectCounterDisplay = $"Tárgyak száma: {Subjects.Count}";
		}

		#endregion

		#region Constructor
		public ScheduleViewModel()
		{
			Subjects = new ObservableCollection<SubjectViewModel>();
			Changed = new Action<ScheduleSubject, bool>((ScheduleSubject a, bool showdisabled) => { });
			Clear = new Action(() => { });
			DeleteItem = new Action<SubjectViewModel>((SubjectViewModel a) => { });
			Subjects.CollectionChanged += Subjects_CollectionChanged;
			ClearList = new RelayCommand<bool>((bool clearsubjects) =>
			{
				if (clearsubjects)
					Subjects.Clear();
				Clear.Invoke();
			});
			RemoveThis = new RelayCommand<SubjectViewModel>((SubjectViewModel vm) =>
			{
				vm.InfoExpanded = false;
				DeleteItem.Invoke(vm);
			});
		}

		#endregion

		#region Public Properties
		//public ObservableCollection<WpfScheduler.ScheduleSubject> Events { get; set; }
		public ObservableCollection<SubjectViewModel> Subjects { get; set; }

		public bool ShowDisabledCourses { get; set; }

		public string SubjectCounterDisplay { get; set; } = "Tárgyak száma: 0";

		public int PlanCounter { get; set; } = 0;

		#endregion

		#region Public Actions
		public Action<ScheduleSubject, bool> Changed { get; set; }
		public Action Clear { get; set; }

		public Action<SubjectViewModel> DeleteItem { get; set; }
		#endregion

		#region Public Commands

		public ICommand ClearList { get; set; }

		public ICommand RemoveThis { get; set; }

		#endregion
	}
}
