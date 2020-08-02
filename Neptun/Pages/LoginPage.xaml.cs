using Dna;
using System.Diagnostics;
using System.Security;
using System.Windows;
using static Neptun.DI;
namespace Neptun
{
	/// <summary>
	/// Interaction logic for LoginPage.xaml
	/// </summary>
	public partial class LoginPage : BasePage<LoginViewModel>, IHavePassword
	{
		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public LoginPage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Constructor with specific view model
		/// </summary>
		public LoginPage(LoginViewModel specificViewModel) : base(specificViewModel)
		{
			InitializeComponent();
		}

		#endregion

		/// <summary>
		/// The secure password for this login page
		/// </summary>
		public SecureString SecurePassword => PasswordText.SecurePassword;

		private void SaveLoginDetails_Handle(object sender, RoutedEventArgs e)
		{
			base.ViewModel.SaveLoginDetails = (bool)SaveLoginDetailsCheckBox.IsChecked;
		}
	}
}
