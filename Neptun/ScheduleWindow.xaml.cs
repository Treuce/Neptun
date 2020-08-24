using MindFusion.Scheduling;
using MindFusion.Scheduling.Wpf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfScheduler;

namespace Neptun
{
	/// <summary>
	/// Interaction logic for ScheduleWindow.xaml
	/// </summary>
	public partial class ScheduleWindow : Window
	{

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public ScheduleWindow()
		{
			InitializeComponent();
			DataContext = new ScheduleWindowViewModel(this);
			MainContent.DataContext = DI.ScheduleVM;
			scheduler.Loaded += Scheduler_Loaded;
			plan.Loaded += Plan_Loaded;
		}

		#endregion

		#region Private Members

		#region Events for schedulers loaded
		/// <summary>
		/// Method for adding events to the actual <see cref="Scheduler"/>
		/// that contains the selected courses
		/// </summary>
		/// <param name="sender"> The scheduler itself</param>
		/// <param name="e"></param>
		private void Plan_Loaded(object sender, RoutedEventArgs e)
		{
			var currentscheduler = sender as Scheduler;
			if (currentscheduler.DataContext is Neptun.ScheduleViewModel vm)
			{
				vm.Clear += () =>
				{
					OnClearInvoke(currentscheduler);
				};
				//vm.DeleteItem += async (SubjectViewModel a) =>
				//{
				//	OnDeleteItem(currentscheduler, a);
				//};
			}
		}

		/// <summary>
		/// Method for adding events to the actual <see cref="Scheduler"/>
		/// that contains all the courses
		/// </summary>
		/// <param name="sender"> The scheduler itself</param>
		/// <param name="e"></param>
		private void Scheduler_Loaded(object sender, RoutedEventArgs e)
		{
			var currentscheduler = sender as Scheduler;
			if (currentscheduler.DataContext is Neptun.ScheduleViewModel vm)
			{
				vm.Changed += (ScheduleSubject a, bool showdisabled) =>
				{
					OnChangedInvoke(currentscheduler, a, showdisabled);
				};
				vm.Clear += () =>
				{
					OnClearInvoke(currentscheduler);
				};
				vm.DeleteItem += async (SubjectViewModel a) =>
				{
					OnDeleteItem(currentscheduler, a);
				};
			}
		}
		#endregion

		#region Methods for scheduler events
		private void OnChangedInvoke(Scheduler currentscheduler, ScheduleSubject a, bool showdisabled)
		{
			if (showdisabled)
				currentscheduler.AddEvent(a);
			else if (a.IsEnabled)
				currentscheduler.AddEvent(a);
		}
		private void OnClearInvoke(Scheduler currentscheduler)
		{
			currentscheduler.Events.Clear();
			currentscheduler.WeekScheduler.PaintAllEvents(null);
		}

		/// <summary>
		/// Used when a <see cref="ScheduleViewModel.RemoveThis"/> is invoked
		/// Removes all the events from the scheduler that have the same subject code as the received <see cref="SubjectViewModel"/>
		/// </summary>
		/// <param name="currentscheduler"></param>
		/// <param name="subject"><see cref="SubjectViewModel"/>The actual subject whose courses are to be removed from the scheduler</param>
		private void OnDeleteItem(Scheduler currentscheduler, SubjectViewModel subject)
		{
			for (int i = 0; i < currentscheduler.Events.Count; ++i)
			{
				var b = currentscheduler.Events[i];
				if (b.Code == subject.Code)
				{
					currentscheduler.Events.Remove(b);
					--i;
				}
			}
			currentscheduler.WeekScheduler.PaintAllEvents(null);
		}
		#endregion

		#region Actual Event Hooks of this Window

		private void ContentControl_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			e.Handled = true;
			((sender as ContentControl).DataContext as SubjectViewModel).InfoExpanded ^= true;
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			DI.ScheduleVM.ClearList.Execute(false);
		}

		private void scheduler_OnEventDoubleClick(object sender, WpfScheduler.ScheduleSubject e)
		{
			//++DI.ScheduleVM.PlanCounter;
			var currentevent = (sender as WpfScheduler.EventUserControl)?.Event;
			var newevent = new ScheduleSubject(currentevent);
			newevent.BackGroundColor_HEX = "#ffffff";
			foreach (var a in scheduler.Events.Where(s => s.Code == e.Code && s != e && s.Type == e.Type))
			{
				a.BackGroundColor_HEX = Brushes.Gray.Color.ToString();
			}
			if (plan.Events.Any(s => s.Code == e.Code && s.Type == e.Type))
			{
				var asd = plan.Events.First(s => s.Code == e.Code && s.Type == e.Type);
				plan.DeleteEvent(asd.Id);
				//--DI.ScheduleVM.PlanCounter;
			}
			//scheduler.WeekScheduler.PaintAllEvents();
			//(sender as WpfScheduler.EventUserControl).BorderElement.Background = Brushes.Green;
			currentevent.BackGroundColor_HEX = Brushes.Green.Color.ToString();
			plan.AddEvent(newevent);

			var concurrentEvents = scheduler.Events.Where(e1 => ((e1.Start <= newevent.Start && e1.End > newevent.End) ||
							(e1.Start >= newevent.Start && e1.Start < newevent.End) ||
							(e1.Start < newevent.Start && e1.End < newevent.End && !(e1.End <= newevent.Start))) &&
						   e1.End.Date == newevent.End.Date && e1.courseID != newevent.courseID);

			foreach (var a in concurrentEvents)
				a.BackGroundColor_HEX = "#ed0202";

			var concurrentEvents2 = plan.Events.Where(e1 => ((e1.Start <= newevent.Start && e1.End > newevent.End) ||
				(e1.Start >= newevent.Start && e1.Start < newevent.End) ||
				(e1.Start < newevent.Start && e1.End < newevent.End && !(e1.End <= newevent.Start))) &&
			   e1.End.Date == newevent.End.Date && e1.courseID != newevent.courseID).ToList();
			for (int i = 0; i < concurrentEvents2.Count; ++i)
			{
				var tmp = concurrentEvents2[i];
				plan.DeleteEvent(tmp.Id);
				var asdasd = scheduler.Events.Where(e1 => ((e1.Start <= tmp.Start && e1.End > tmp.End) ||
							(e1.Start >= tmp.Start && e1.Start < tmp.End) ||
							(e1.Start < tmp.Start && e1.End < tmp.End && !(e1.End <= tmp.Start))) &&
						   e1.End.Date == tmp.End.Date && e1.courseID != tmp.courseID && e1 != e);
				foreach (var ev in asdasd)
					ev.BackGroundColor_HEX = "#ffffff";

			}
		}

		private void plan_OnEventDoubleClick(object sender, ScheduleSubject e)
		{
			//--DI.ScheduleVM.PlanCounter;
			var currentevent = (sender as WpfScheduler.EventUserControl)?.Event;
			plan.DeleteEvent(currentevent.Id);

			foreach (var a in scheduler.Events.Where(s => s.Code == e.Code && s != e && s.Type == e.Type))
			{
				a.BackGroundColor_HEX = "#ffffff";
			}
			var concurrentEvents = scheduler.Events.Where(e1 => ((e1.Start <= e.Start && e1.End > e.End) ||
				(e1.Start >= e.Start && e1.Start < e.End) || (e1.Start < e.Start && e1.End < e.End)) &&
			   e1.End.Date == e.End.Date);
			foreach (var a in concurrentEvents)
			{
				if (!plan.Events.Any(e1 => ((e1.Start <= a.Start && e1.End > a.End) ||
						(e1.Start >= a.Start && e1.Start < a.End) || (e1.Start < a.Start && e1.End < a.End)) &&
						e1.End.Date == a.End.Date))
						a.BackGroundColor_HEX = "#ffffff";
			}
		}

		#endregion

		#endregion


	}
}
