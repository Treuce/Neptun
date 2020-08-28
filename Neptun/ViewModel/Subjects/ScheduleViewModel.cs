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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
						var ResultList = new List<ScheduleSubject>();
						var courselist = subject.TakeViewModel.Courses.OrderBy(s => s.StartTime).ToList();
						foreach (var course in courselist)
						{
							try
							{
								var asd = course.Schedule;
								var damn = new Dictionary<string, string>();
								if ( course.NeptunHasSchedule)
								{
									var ind = asd.IndexOf('(');
									var day = asd.Substring(0, (ind == -1 ? asd.Length - 1 : ind)).TrimEnd().Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
									for (int i = 0; i < day.Length; ++i)
										damn.Add(day[i], day[++i]);
								}
								else
								{
									var day = asd.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
									for (int i = 0; i < day.Length; ++i)
										damn.Add(day[i], day[++i]);
								}

								var currentDate = DateTime.Today;
								foreach (var b in damn)
								{
									try
									{

										var start_time = DateTime.Today;
										var end_time = DateTime.Today;
										var timespans = b.Value.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
										start_time += TimeSpan.Parse(timespans[0]);
										end_time += TimeSpan.Parse(timespans[1]);
										DayOfWeek dayOfWeek;
										switch (b.Key)
										{
											case "H":
											case "h":
											case "Hétfő":
												dayOfWeek = DayOfWeek.Monday;
												break;
											case "Kedd":
											case "k":
											case "K":
												dayOfWeek = DayOfWeek.Tuesday;
												break;
											case "Szerda":
											case "sz":
											case "Sz":
											case "SZE":
											case "SZ":
												dayOfWeek = DayOfWeek.Wednesday;
												break;
											case "Cs":
											case "CS":
											case "cs":
											case "Csütörtök":
												dayOfWeek = DayOfWeek.Thursday;
												break;
											case "Péntek":
											case "P":
											case "p":
												dayOfWeek = DayOfWeek.Friday;
												break;
											default:
												throw new Exception("Fuck my life");
												break;
										}
										while (start_time.DayOfWeek != DayOfWeek.Monday)
										{
											start_time = start_time.AddDays(-1);
											end_time = end_time.AddDays(-1);
										}

										start_time = start_time.AddDays((int)dayOfWeek - 1);
										end_time = end_time.AddDays((int)dayOfWeek - 1);

										var concurrentEvents = ScheduledEvents.Where(e1 => (
														(e1.Start <= start_time && e1.End > end_time) ||
														(e1.Start >= start_time && e1.Start < end_time) || (e1.Start < start_time && e1.End < end_time && !(e1.End <= start_time))) &&
													   e1.End.Date == end_time.Date);
										string hexcolor = course.isEnabled ? "#ffffff" : "#ed0202";
										var asdasdasd = concurrentEvents.Count();
										if (concurrentEvents.Count() > 0)
										{
											try
											{
												var e1 = ScheduledEvents.First(ev => ((ev.Start <= start_time && ev.End > end_time) ||
															(ev.Start >= start_time && ev.Start < end_time) ||
															(ev.Start < start_time && ev.End < end_time)
															) &&
														   ev.End.Date == end_time.Date);
												if (e1.courseID != course.CourseCode)
													hexcolor = "#ed0202";
												else hexcolor = "#FF008000";
											}
											catch (Exception exception)
											{
												Dna.FrameworkDI.Logger.LogDebugSource("FUCKMYLIFE", exception: exception);
											}
										}
										foreach (var ev in ScheduledEvents.Where(s => s.Code == subject.Code && s.Type == course.Type && s.courseID != course.CourseCode))
										{
											hexcolor = "#FF808080";
										}
										//Application.Current.Dispatcher.Invoke(() =>
										//{
										//ScheduleSubject a = new ScheduleSubject
										//Changed.Invoke
										ResultList.Add(new ScheduleSubject()
										{
											Start = start_time,
											End = end_time,
											AllDay = false,
											IsEnabled = course.isEnabled,
											Type = course.Type,
											BackGroundColor_HEX = hexcolor,
											courseID = course.CourseCode,
											Subject = $"{subject.Name} [{course.Type}] ({subject.Code}) {Environment.NewLine} #{course.CourseCode}{Environment.NewLine}{course.Teacher}{Environment.NewLine}{course.Note}",
											Code = subject.Code,
											Description = $"{course.ToolTip}"
										});
									}
									catch (Exception ex)
									{
										Debugger.Break();
										FrameworkDI.Logger.LogDebugSource(ex.Message, exception: ex);
									}
									//}, ShowDisabledCourses);
									//});
								}
							}
							catch (Exception ex)
							{
								Dna.FrameworkDI.Logger.LogDebugSource("Loading course data..", exception: ex);
							}
						}
						ResultList.Sort(delegate (ScheduleSubject a, ScheduleSubject b)
						{
							return a.Start.CompareTo(b.Start);
						});
						Application.Current.Dispatcher.Invoke(() =>
						{
							RangeChanged.Invoke(ResultList, ShowDisabledCourses);
						});
					};
			}
			SubjectCounterDisplay = $"Tárgyak száma: {Subjects.Count}";
		}

		private void ScheduledEvents_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			OnPropertyChanged(nameof(PlanCounter));
		}

		#endregion

		#region Constructor
		public ScheduleViewModel()
		{
			Subjects = new ObservableCollection<SubjectViewModel>();
			AllCoursesEvents = new ObservableRangeCollection<ScheduleSubject>();
			///*ScheduledEvents*/ = new ObservableCollection<ScheduleSubject>();
			ScheduledEvents = new ObservableRangeCollection<ScheduleSubject>();
			//ScheduledEvents.CollectionChanged += ScheduledEvents_CollectionChanged;
			ScheduledEvents.CollectionChanged += ScheduledEvents_CollectionChanged;
			//Changed = new Action<ScheduleSubject, bool>((ScheduleSubject a, bool showdisabled) => { });
			//Clear = new Action(() => { PlanCounter = 0; });
			//DeleteItem = new Action<SubjectViewModel>((SubjectViewModel a) => { });
			Subjects.CollectionChanged += Subjects_CollectionChanged;
			//ScheduledEvents.CollectionChanged += ScheduledEvents_CollectionChanged;
			ClearList = new RelayCommand<bool>((bool clearsubjects) =>
			{
				if (clearsubjects)
					Subjects.Clear();
				else
					foreach (var s in Subjects)
						s.InfoExpanded = false;
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
		public ObservableRangeCollection<WpfScheduler.ScheduleSubject> AllCoursesEvents { get; set; }
		//public ObservableCollection<WpfScheduler.ScheduleSubject> ScheduledEvents { get; set; }
		public ObservableCollection<SubjectViewModel> Subjects { get; set; }

		public ObservableRangeCollection<ScheduleSubject> ScheduledEvents { get; set; }

		public bool ShowDisabledCourses { get; set; }

		public string SubjectCounterDisplay { get; set; } = "Tárgyak száma: 0";

		public int PlanCounter { get => ScheduledEvents.Count; }

		#endregion

		#region Public Actions
		public Action<ScheduleSubject, bool> Changed { get; set; }
		public Action<IEnumerable<ScheduleSubject>, bool> RangeChanged { get; set; }
		public Action Clear { get; set; }

		public Action<SubjectViewModel> DeleteItem { get; set; }
		#endregion

		#region Public Commands

		public ICommand ClearList { get; set; }

		public ICommand RemoveThis { get; set; }

		#endregion
	}
}
