using Neptun.Core;
using System;
using System.Collections.Generic;
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
    public partial class NoUIPage : BasePage<TFViewModel>
    {
        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public NoUIPage() : base()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with specific view model
        /// </summary>
        /// <param name="specificViewModel">The specific view model to use for this page</param>
        public NoUIPage(TFViewModel specificViewModel) : base(specificViewModel)
        {
            InitializeComponent();
        }

		#endregion

		#region Override Methods

		/// <summary>
		/// Fired when the view model changes
		/// </summary>

		#endregion

	}
}
