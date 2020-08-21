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
				if (b.Description == subject.Code)
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
			var eventcontrol = sender as WpfScheduler.EventUserControl;
			plan.AddEvent(eventcontrol.Event);
		}

		private void plan_OnEventDoubleClick(object sender, ScheduleSubject e)
		{
			var eventcontrol = sender as WpfScheduler.EventUserControl;
			plan.DeleteEvent(eventcontrol.Event.Id);
		}

		#endregion

		#endregion


	}
}
