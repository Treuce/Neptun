using Neptun.Core;
using System.Threading.Tasks;
using static Neptun.DI;
using static Neptun.Core.CoreDI;
using System.Windows.Input;
using System.Diagnostics;
using System;
using static Dna.FrameworkDI;
using Dna;
using System.Windows.Navigation;

namespace Neptun
{
	/// <summary>
	/// The application state as a view model
	/// </summary>
	public class ApplicationViewModel : BaseViewModel
	{
		#region Private Members

		#endregion

		#region Public Properties

		/// <summary>
		/// The current page of the application
		/// The starter page loaded when login is successful
		/// </summary>
		public ApplicationPage CurrentPage { get; private set; } = ApplicationPage.Login;

		/// <summary>
		/// The view model to use for the current page when the CurrentPage changes
		/// NOTE: This is not a live up-to-date view model of the current page
		///       it is simply used to set the view model of the current page 
		///       at the time it changes
		/// </summary>
		public BaseViewModel CurrentPageViewModel { get; set; }



		/// <summary>
		/// The name and the neptun code of the logged in person
		/// </summary>
		public string NameAndNeptun { get; set; } = null;

		/// <summary>
		/// Checks or hides the user information part
		/// </summary>
		public bool HasNameAndNeptun { get => NameAndNeptun != null; }

		public bool isLoggedIn { get; set; }

		public bool IsMessagesVisible { get => CurrentPage != ApplicationPage.Messages; }

		//public string EventValidateStr { get; set; }
		//public string ViewStateStr { get; set; }
		/// <summary>
		/// Used for the main menu, loads in its ctor
		/// </summary>
		public MainNavMenuViewModel MainMenuVM { get; set; } = null;

		public MessagesListViewModel MessageList { get; set; }

		public int UnreadMessageCount { get; set; } = 0;

		#endregion

		#region Public Commands

		#endregion

		#region Constructor

		/// <summary>
		/// The default constructor
		/// </summary>
		public ApplicationViewModel()
		{
		}

		#endregion

		#region Command Methods


		#endregion

		#region Public Helper Methods

		/// <summary>
		/// Navigates to the specified page
		/// </summary>
		/// <param name="page">The page to go to</param>
		/// <param name="viewModel">The view model, if any, to set explicitly to the new page</param>
		public void GoToPage(ApplicationPage page, BaseViewModel viewModel = null)
		{

			//var a = CurrentPageViewModel.GetType();
			//if (page == CurrentPage)
			//{
			//	Debugger.Break();
			//	return;
			//}
			// Set the view model
			CurrentPageViewModel = viewModel;

			// See if page has changed
			var different = CurrentPage != page;

			// Set the current page
			CurrentPage = page;

			// If the page hasn't changed, fire off notification
			// So pages still update if just the view model has changed
			if (!different)
				OnPropertyChanged(nameof(CurrentPage));

		}

		/// <summary>
		/// Handles what happens when we have successfully logged in
		/// </summary>
		/// <param name="loginResult">The results from the successful login</param>
		public async Task HandleSuccessfulLoginAsync(UserProfileDetailsApiModel loginResult, bool SaveLoginDetails = true)
		{
			try
			{
				isLoggedIn = true;


				if (MainMenuVM == null)
					MainMenuVM = new MainNavMenuViewModel();
				// Store this in the client data store
				if (SaveLoginDetails)
					await ClientDataStore.SaveLoginCredentialsAsync(loginResult.ToLoginCredentialsDataModel());

				//Start keeping alive the session
				TaskManager.RunAndForget(App.KeepConnectionAlive);

				// Go to chat page
				ViewModelApplication.GoToPage(ApplicationPage.Tárgyfelvétel);
			}
			catch (Exception e)
			{
				Logger.LogCriticalSource(e.Message);
				Debugger.Break();
			}
		}

		#endregion
	}
}
