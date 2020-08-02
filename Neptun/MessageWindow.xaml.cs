using System.Threading.Tasks;
using System.Windows;

namespace Neptun
{
	/// <summary>
	/// Interaction logic for DialogWindow.xaml
	/// </summary>
	public partial class MessageWindow : Window
	{

		#region Private Members

		/// <summary>
		/// The view model for this window
		/// </summary>
		private MessageWindowViewModel mViewModel;

		#endregion

		#region Public Properties

		/// <summary>
		/// The view model for this window
		/// </summary>
		public MessageWindowViewModel ViewModel
		{
			get => mViewModel;
			set
			{
				// Set new value
				mViewModel = value;

				// Update data context
				DataContext = mViewModel;
			}
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public MessageWindow()
		{
			InitializeComponent();
		}

		#endregion

	}
}
