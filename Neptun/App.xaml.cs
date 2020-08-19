using Dna;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neptun.Core;
using Neptun.Relational;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.WebSockets;
using System.Windows;
using System.Xml.Linq;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using static Neptun.DI;

namespace Neptun
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		/// <summary>
		/// Custom startup so we load our IoC immediately before anything else
		/// </summary>
		/// <param name="e"></param>
		protected override async void OnStartup(StartupEventArgs e)
		{
			// Let the base application do what it needs
			base.OnStartup(e);

			// Setup the main application 
			await ApplicationSetupAsync();

			// Log it
			Logger.LogDebugSource("Application starting...");

			if (await ClientDataStore.HasCredentialsAsync())
			{
				var cred = await ClientDataStore.GetLoginCredentialsAsync();
				// Call the server and attempt to login with credentials
				Uri tmp = RestWebClient.BaseUrl; //TODO: Use RestClient.Host and relative paths?
				var request = new RestRequest(WebRoutes.Login, Method.POST);

				request.Body = new RequestBody("ContentType = \"application/json\"", "application/json", $"{{\"user\":\"{cred.NeptunCode}\",\"pwd\":\"{cred.Password}\",\"UserLogin\":null,\"GUID\":null,\"captcha\":\"\"}}");
				var result = RestWebClient.Execute(request);

				var sessionCookie = result.Cookies.SingleOrDefault(x => x.Name == "ASP.NET_SessionId");
				if (sessionCookie != null)
					RestWebClient.CookieContainer.Add(new Cookie(sessionCookie.Name, sessionCookie.Value, sessionCookie.Path, sessionCookie.Domain));
				//Debugger.Break();
				//foreach (var a in result.Cookies)
				//	RestWebClient.CookieContainer.Add(new Cookie(a.Name, a.Value) { Domain = a.Domain });
				// If the response has an error...
				if (await result.HandleErrorIfFailedAsync("Sikertelen Bejelentkezés"))
					await ViewModelApplication.HandleSuccessfulLoginAsync(new UserProfileDetailsApiModel(cred.NeptunCode, cred.Password));
				else ViewModelApplication.GoToPage(ApplicationPage.Login);
				//ViewModelApplication.GoToPage(ApplicationPage.Messages);
				//TaskManager.RunAndForget(KeepConnectionAlive);
			}
			Current.MainWindow = new MainWindow();
			Current.MainWindow.Show();
			// Show the main window

			Logger.LogDebugSource("Application started..");

			//var asdasddas = asdasd.CreateNavigator();
			//Debugger.Break();
		}

		/// <summary>
		/// Configures our application ready for use
		/// </summary>
		private async Task ApplicationSetupAsync()
		{
			// Setup the Dna Framework
			Framework.Construct<DefaultFrameworkConstruction>()
				.AddFileLogger(logPath: "Neptun.log")
				.AddClientDataStore()
				.AddNeptunViewModels()
				.AddNeptunClientServices()
				.Build();

			// Ensure the client data store 
			await ClientDataStore.EnsureDataStoreAsync();

			RestWebClient.BaseUrl = new Uri(Configuration["NeptunServer:HostUrl"]);
			RestWebClient.UserAgent = "Mozilla/5.0";
			RestWebClient.UseJson();
			RestWebClient.Encoding = Encoding.UTF8;
			RestWebClient.CookieContainer = new CookieContainer();
			//Debugger.Break();
			// Monitor for server connection status
		}

		/// <summary>
		/// Keeps the session active by sending requests.
		/// </summary>
		public static async void KeepConnectionAlive()
		{
			try
			{
				var request = new RestRequest("main.aspx", Method.GET);
				while (true)
				{
					if (ViewModelApplication.isLoggedIn)
					{
						string tmphtml = String.Empty;
						lock (RestWebClient)
							tmphtml = RestWebClient.Execute(request).Content;
						var html = new HtmlDocument();
						html.LoadHtml(tmphtml);
						var unreadmessages = html.GetElementbyId("_lnkInbox").ChildNodes[0].InnerText.Split(' ');
						if (unreadmessages.Length == 3)
						{
							var str = unreadmessages[2].Trim('(', ')');
							Int32.TryParse(str, out int tmp);
							ViewModelApplication.UnreadMessageCount = tmp;
							//ViewModelApplication.OnPropertyChanged("");
							//ViewModelApplication.MainMenuVM.OnPropertyChanged("messagesText");
						}
					}
					await Task.Delay(TimeSpan.FromMinutes(2));
				}
			}
			catch (Exception e)
			{
				Logger.LogError(e.Message);
				Debugger.Break();
			}
		}

		private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			var request = new RestRequest("main.aspx/LogOutFromJS", Method.POST);
			request.Body = new RequestBody("ContentType = \"application/json\"", "application/json", "{\"link\": \"Login.aspx?timeout=\"}");
			request.Timeout = 100;
			CoreDI.RestWebClient.Execute(request);
			var sb = new StringBuilder();

			AppendExceptionMessages(sb, e.Exception);
			AppendExceptionStacktraces(sb, e.Exception);
			if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Neptun\MyAppCrashes.log")))
				Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Neptun\MyAppCrashes.log"));
			File.AppendAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Neptun\MyAppCrashes.log"), sb.ToString());
		}
		private void AppendExceptionMessages(StringBuilder sb, Exception e)
		{
			while (e != null)
			{
				sb.AppendLine("============== Exception ===============").AppendLine(e.Message);
				e = e.InnerException;
			}
		}
		private void AppendExceptionStacktraces(StringBuilder sb, Exception e)
		{
			while (e != null)
			{
				sb.AppendLine("======== Exception Stacktrace ==========").AppendLine(e.StackTrace);
				e = e.InnerException;
			}
		}

		private void Application_Exit(object sender, ExitEventArgs e)
		{
			var request = new RestRequest("main.aspx/LogOutFromJS", Method.POST);
			request.Body = new RequestBody("ContentType = \"application/json\"", "application/json", "{\"link\": \"Login.aspx?timeout=\"}");
			request.Timeout = 100;
			CoreDI.RestWebClient.Execute(request);
			Application.Current.Shutdown();
		}
	}
}
