using MindFusion.Scheduling;
using MindFusion.Scheduling.Wpf;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
			
		}

		#endregion

		private void ContentControl_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			((sender as ContentControl).DataContext as SubjectViewModel).InfoExpanded ^= true;
		}
	}
}
