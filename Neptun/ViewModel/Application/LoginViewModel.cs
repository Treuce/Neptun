using Dna;
using Neptun.Core;
using RestSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using static Neptun.DI;

namespace Neptun
{
	/// <summary>
	/// The View Model for a login screen
	/// </summary>
	public class LoginViewModel : BaseViewModel
	{
		#region Public Properties

		/// <summary>
		/// The email of the user
		/// </summary>
		public string NeptunCode { get; set; }

		/// <summary>
		/// A flag indicating if the login command is running
		/// </summary>
		public bool LoginIsRunning { get; set; }

		public bool SaveLoginDetails { get; set; }

		#endregion

		#region Commands

		/// <summary>
		/// The command to login
		/// </summary>
		public ICommand LoginCommand { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public LoginViewModel()
		{
			// Create commands
			LoginCommand = new RelayParameterizedCommand(async (parameter) => await LoginAsync(parameter));

			Task.Run(async () =>
			//TaskManager.RunAndForget(async () =>
			{
				if (await ClientDataStore.HasCredentialsAsync())
				{
					var cred = await ClientDataStore.GetLoginCredentialsAsync();
					NeptunCode = cred.NeptunCode;
					try
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							if ((Application.Current.MainWindow as MainWindow).MainWindowPageHost.NewPage.Content is LoginPage)
							{
								var smt = (Application.Current.MainWindow as MainWindow).MainWindowPageHost.NewPage.Content as LoginPage;
								smt.PasswordText.Password = cred.Password;
							}
							else
								Debugger.Break();
						});
					}
					catch (Exception e)
					{
						Logger.LogCriticalSource(e.Message);
						Debugger.Break();
					}
				}
			});
			//);

		}

		#endregion

		/// <summary>
		/// Attempts to log the user in
		/// </summary>
		/// <param name="parameter">The <see cref="SecureString"/> passed in from the view for the users password</param>
		/// <returns></returns>
		public async Task LoginAsync(object parameter)
		{
			await RunCommandAsync(() => LoginIsRunning, async () =>
			{
				try
				{

					// Call the server and attempt to login with credentials
					var request = new RestRequest(WebRoutes.Login, Method.POST);

					request.Body = new RequestBody("ContentType = \"application/json\"", "application/json", $"{{\"user\":\"{NeptunCode}\",\"pwd\":\"{(parameter as IHavePassword).SecurePassword.Unsecure()}\",\"UserLogin\":null,\"GUID\":null,\"captcha\":\"\"}}");
					var result = RestWebClient.Execute(request);
					var sessionCookie = result.Cookies.SingleOrDefault(x => x.Name == "ASP.NET_SessionId");
					if (sessionCookie != null)
						RestWebClient.CookieContainer.Add(new Cookie(sessionCookie.Name, sessionCookie.Value, sessionCookie.Path, sessionCookie.Domain));
					//foreach (var a in result.Cookies)
					//	RestWebClient.CookieContainer.Add(new Cookie(a.Name, a.Value) { Domain = a.Domain });
					// If the response has an error...
					if (await result.HandleErrorIfFailedAsync("Sikertelen Bejelentkezés"))
						//                // We are done
						//                return;

						// OK successfully logged in... now get users data
						//var loginResult = result2.ServerResponse.Response;

						// Let the application view model handle what happens
						// with the successful login
						await ViewModelApplication.HandleSuccessfulLoginAsync(new UserProfileDetailsApiModel(NeptunCode, (parameter as IHavePassword).SecurePassword), SaveLoginDetails);

					//var requestmainpage = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx", Method.GET);
					//var MainHTMLPage = RestWebClient.Execute(requestmainpage);
				}
				catch (Exception e)
				{
					Logger.LogCriticalSource(e.Message);
					Debugger.Break();
				}
			});
		}
	}
}
