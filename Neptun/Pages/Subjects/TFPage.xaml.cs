using Dna;
using Neptun.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Neptun
{
	/// <summary>
	/// Interaction logic for MainPage.xaml
	/// </summary>
	public partial class TFPage : BasePage<TFViewModel>
	{
		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public TFPage() : base()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor with specific view model
		/// </summary>
		/// <param name="specificViewModel">The specific view model to use for this page</param>
		public TFPage(TFViewModel specificViewModel) : base(specificViewModel)
		{
			InitializeComponent();
		}

		#endregion

		#region Override Methods

		/// <summary>
		/// Fired when the view model changes
		/// </summary>

		#endregion

		private void ExpandTFControl(object sender, MouseButtonEventArgs e)
		{
			((sender as ContentControl).DataContext as SubjectViewModel).InfoExpanded ^= true;
			//Debugger.Break();
		}

		private void ContentControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			((sender as ContentControl).DataContext as SubjectViewModel).isPopUpOpen ^= true;
		}

		private async void ContentControl_LostMouseCapture(object sender, MouseEventArgs e)
		{
			try
			{
				if ((sender as ContentControl)?.DataContext is SubjectViewModel vm)
				{
					await Task.Delay(2000);
					vm.isPopUpOpen = false;
				}
			}
			catch (Exception ex)
			{
				FrameworkDI.Logger.LogDebugSource("Control gone probs", exception: ex);
			}
		}
	}
}
