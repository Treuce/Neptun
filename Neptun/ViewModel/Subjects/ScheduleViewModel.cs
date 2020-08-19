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
using WpfScheduler;

namespace Neptun
{
	public interface EventsChanged
	{
		Action<ScheduleSubject> Changed { get; set; }
	}
	public class ScheduleViewModel : BaseViewModel, EventsChanged
	{

		#region Constructor
		public ScheduleViewModel()
		{
			Subjects = new ObservableCollection<SubjectViewModel>();
			Events = new ObservableCollection<WpfScheduler.ScheduleSubject>();
			CoreDI.TaskManager.RunAndForget(async () =>
			{
				//await Task.Delay(5000);
				Dna.FrameworkDI.Logger.LogErrorSource("Adding now");
				System.Windows.Application.Current.Dispatcher.Invoke(() =>
				{

					var a = new ScheduleSubject()
					{
						Start = DateTime.Parse("2020-08-19 09:00:00"),
						End = DateTime.Parse("2020-08-19 11:00:00"),
						AllDay = false,
						Subject = "Test"
					};
					Changed.Invoke(a);
					a = new ScheduleSubject()
					{
						Start = DateTime.Parse("2020-08-19 09:00:00"),
						End = DateTime.Parse("2020-08-19 11:00:00"),
						AllDay = false,
						Subject = "Test2"
					};
					Changed.Invoke(a);
					a = new ScheduleSubject()
					{
						Start = DateTime.Parse("2020-08-19 11:00:00"),
						End = DateTime.Parse("2020-08-19 12:00:00"),
						AllDay = false,
						Subject = "Test2"
					};
					Changed.Invoke(a);
				});
			});
			Changed = new Action<ScheduleSubject>((ScheduleSubject a) => { });
		}
		public ObservableCollection<WpfScheduler.ScheduleSubject> Events { get; set; }
		public ObservableCollection<SubjectViewModel> Subjects { get; set; }
		public Action<ScheduleSubject> Changed { get; set; }

		#endregion
	}
}
