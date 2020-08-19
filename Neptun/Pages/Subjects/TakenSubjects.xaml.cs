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
    /// Interaction logic for TakenSubjects.xaml
    /// </summary>
    public partial class TakenSubjectsPage : BasePage<TakenSubjectsPageViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public TakenSubjectsPage() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with specific view model
        /// </summary>
        /// <param name="specificViewModel">The specific view model to use for this page</param>
        public TakenSubjectsPage(TakenSubjectsPageViewModel specificViewModel) : base(specificViewModel)
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
	}
}
