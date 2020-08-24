using Dna;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfScheduler
{
	/// <summary>
	/// Interaction logic for UserControl1.xaml
	/// </summary>
	public partial class WeekScheduler : UserControl
	{
		private Scheduler _scheduler;

		internal event EventHandler<ScheduleSubject> OnEventDoubleClick;

		internal event EventHandler<DateTime> OnScheduleDoubleClick;

		#region FirstDay
		private DateTime _firstDay;
		internal DateTime FirstDay
		{
			get { return _firstDay; }
			set
			{
				while (value.DayOfWeek != DayOfWeek.Monday)
					value = value.AddDays(-1);
				_firstDay = value;
				AdjustFirstDay(value);
			}
		}

		private void AdjustFirstDay(DateTime firstDay)
		{
			dayLabel1.Content = firstDay.ToString("dddd");
			dayLabel2.Content = firstDay.AddDays(1).ToString("dddd");
			dayLabel3.Content = firstDay.AddDays(2).ToString("dddd");
			dayLabel4.Content = firstDay.AddDays(3).ToString("dddd");
			dayLabel5.Content = firstDay.AddDays(4).ToString("dddd");

			PaintAllEvents(null);
			PaintAllDayEvents();
		}
		#endregion

		public WeekScheduler()
		{
			InitializeComponent();

			column1.MouseDown += Canvas_MouseDown;
			column1.Background = Brushes.Transparent;
			column2.MouseDown += Canvas_MouseDown;
			column2.Background = Brushes.Transparent;
			column3.MouseDown += Canvas_MouseDown;
			column3.Background = Brushes.Transparent;
			column4.MouseDown += Canvas_MouseDown;
			column4.Background = Brushes.Transparent;
			column5.MouseDown += Canvas_MouseDown;
			column5.Background = Brushes.Transparent;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs ea)
		{
			try
			{
				DependencyObject ucParent = (sender as WeekScheduler).Parent;
				while (!(ucParent is Scheduler))
				{
					ucParent = LogicalTreeHelper.GetParent(ucParent);
				}
				_scheduler = ucParent as Scheduler;

				_scheduler.OnEventAdded += ((object s, ScheduleSubject e) =>
				{
					if (e.Start.Date == e.End.Date)
						PaintAllEvents(e.Start);
					else
						PaintAllDayEvents();
				});

				_scheduler.OnEventDeleted += ((object s, ScheduleSubject e) =>
				{
					if (e.Start.Date == e.End.Date)
						PaintAllEvents(e.Start);
					else
						PaintAllDayEvents();
				});

				_scheduler.OnEventsModified += ((object s, EventArgs e) =>
				{
					PaintAllEvents(null);
					PaintAllDayEvents();
				});


				this.SizeChanged += WeekScheduler_SizeChanged;

				ResizeGrids(new Size(this.ActualWidth, this.ActualHeight));
				PaintAllEvents(null);
				PaintAllDayEvents();
			}
			catch (Exception ex)
			{
				FrameworkDI.Logger.LogDebugSource("Idk", exception: ex);
				throw;
			}
		}

		private void WeekScheduler_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			try
			{
				ResizeGrids(e.NewSize);
				PaintAllEvents(null);
				PaintAllDayEvents();
			}
			catch (Exception ex)
			{
				FrameworkDI.Logger.LogDebugSource("no idea", exception: ex);
				throw;
			}
		}

		private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount >= 2)
			{
				int dayoffset = Convert.ToInt32(((Canvas)sender).Name.Replace("column", "")) - 1;
				OnScheduleDoubleClick(sender, new DateTime(FirstDay.Year, FirstDay.Month, FirstDay.Day).AddDays(dayoffset));
			}
		}

		public void PaintAllEvents(DateTime? date)
		{
			try
			{
				if (_scheduler == null || _scheduler.Events == null) return;

				IEnumerable<ScheduleSubject> eventList = _scheduler.Events.Where(ev => ev.Start.Date == ev.End.Date && !ev.AllDay).OrderBy(ev => ev.Start);

				if (date == null)
				{
					column1.Children.Clear();
					column2.Children.Clear();
					column3.Children.Clear();
					column4.Children.Clear();
					column5.Children.Clear();
				}
				else
				{
					int numColumn = (int)date.Value.Date.Subtract(FirstDay.Date).TotalDays + 1;
					((Canvas)this.FindName("column" + numColumn)).Children.Clear();

					eventList = eventList.Where(ev => ev.Start.Date == date.Value.Date).OrderBy(ev => ev.Start);
				}

				double columnWidth = EventsGrid.ColumnDefinitions[1].Width.Value;

				foreach (ScheduleSubject e in eventList)
				{
					int numColumn = (int)e.Start.Date.Subtract(FirstDay.Date).TotalDays + 1;
					if (numColumn >= 0 && numColumn < 6)
					{
						var sp = (Canvas)this.FindName("column" + numColumn);
						if (numColumn == 3 && (e.courseID == "15" || e.courseID == "7"))
							;
							//Debugger.Break();
						sp.Width = columnWidth;

						double oneHourHeight = sp.ActualHeight / 14;

						var concurrentEvents = _scheduler.Events.Where(e1 => ((e1.Start <= e.Start && e1.End > e.Start) ||
																		(e1.Start >= e.Start && e1.Start < e.End)) &&
																	   e1.End.Date == e1.Start.Date).OrderBy(ev => ev.Start).ToList();



						double marginTop = oneHourHeight * (e.Start.Hour + (e.Start.Minute / 60.0) - 8);
						double width = columnWidth / concurrentEvents.Count;
						double marginLeft = width * getIndex(e, concurrentEvents);

						var wEvent = new EventUserControl(e, true);
						wEvent.Width = width;
						var asd = e.End.Subtract(e.Start).TotalHours;
						wEvent.Height = e.End.Subtract(e.Start).TotalHours * oneHourHeight;
						wEvent.Margin = new Thickness(marginLeft, marginTop, 0, 0);
						wEvent.MouseDoubleClick += ((object sender, MouseButtonEventArgs ea) =>
						{
							ea.Handled = true;
							OnEventDoubleClick(sender, wEvent.Event);
						});

						sp.Children.Add(wEvent);
					}
				}
			}
			catch (Exception ex)
			{
				FrameworkDI.Logger.LogDebugSource("idk", exception: ex);
				throw;
			}
		}

		private int getIndex(ScheduleSubject e, List<ScheduleSubject> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (e.Id == list[i].Id) return i;
			}
			return -1;
		}

		private void PaintAllDayEvents()
		{
			try
			{
				if (_scheduler == null || _scheduler.Events == null) return;

				allDayEvents.Children.Clear();

				double columnWidth = EventsGrid.ColumnDefinitions[1].Width.Value;

				foreach (ScheduleSubject e in _scheduler.Events.Where(ev => ev.End.Date > ev.Start.Date || ev.AllDay))
				{
					int numColumn = (int)e.Start.Date.Subtract(FirstDay.Date).TotalDays;
					int numEndColumn = (int)e.End.Date.Subtract(FirstDay.Date).TotalDays + 1;

					if (numColumn >= 7 || numEndColumn <= 0) continue;

					if (numColumn < 0) numColumn = 0;
					if (numEndColumn > 7) numEndColumn = 7;

					if ((numColumn >= 0 && numColumn < 7) || (numEndColumn >= 0 && numEndColumn < 7))
					{
						double marginLeft = numColumn * columnWidth;

						var wEvent = new EventUserControl(e, false);
						wEvent.Width = columnWidth * (numEndColumn - numColumn);
						wEvent.Margin = new Thickness(marginLeft, 0, 0, 0);
						wEvent.MouseDoubleClick += ((object sender, MouseButtonEventArgs ea) =>
						{
							ea.Handled = true;
							OnEventDoubleClick(sender, wEvent.Event);
						});
						allDayEvents.Children.Add(wEvent);
					}
				}
			}
			catch (Exception ex)
			{
				FrameworkDI.Logger.LogDebugSource("idk", exception: ex);
				throw;
			}
		}

		private void ResizeGrids(Size newSize)
		{
			EventsGrid.Width = newSize.Width;
			EventsHeaderGrid.Width = newSize.Width;

			double columnWidth = (this.ActualWidth - EventsGrid.ColumnDefinitions[0].ActualWidth) / 5;
			for (int i = 1; i < EventsGrid.ColumnDefinitions.Count; i++)
			{
				EventsGrid.ColumnDefinitions[i].Width = new GridLength(columnWidth);
				EventsHeaderGrid.ColumnDefinitions[i].Width = new GridLength(columnWidth);
			}
		}
		/// <summary>
		/// Used so the content isn't just jumping around when clicked
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void column1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}
	}
}
