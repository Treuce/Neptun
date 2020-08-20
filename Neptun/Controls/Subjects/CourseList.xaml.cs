using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Neptun
{
	/// <summary>
	/// Interaction logic for TFControl.xaml
	/// </summary>
	public partial class CourseList : UserControl
	{
		public CourseList()
		{
			InitializeComponent();
		}

		private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue)
			{
				if ((sender as Grid).DataContext == null && Neptun.DI.ViewModelApplication.isLoggedIn)
				{
					var owner = UIHelper.FindVisualParent<ListViewItem>(sender as Grid);
					var asd = ((Application.Current.MainWindow as MainWindow).MainWindowPageHost.NewPage.Content as TFPage).DataContext as TFViewModel;
					var data = owner.DataContext as SubjectViewModel;
					data.Expanded.Invoke();
					(sender as Grid).DataContext = new TakeSubjectViewModel(data.id, data, asd);
					
				}
			}
			else
			{
				var owner = UIHelper.FindVisualParent<ListViewItem>(sender as Grid);
				var data = owner.DataContext as SubjectViewModel;
				data.Collapsed.Invoke();
				(sender as Grid).DataContext = null;
			}
		}

		private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
		{
			((sender as RadioButton).DataContext as TakeSubjectViewModel.Course).SelectionChanged = true;
			((sender as RadioButton).DataContext as TakeSubjectViewModel.Course).isSelected = false;
			//Debugger.Break();
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			((sender as RadioButton).DataContext as TakeSubjectViewModel.Course).SelectionChanged = true;
		}
	}
}
