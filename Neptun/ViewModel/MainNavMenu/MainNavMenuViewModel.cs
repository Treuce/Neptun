using HtmlAgilityPack;
using RestSharp;
using System.Collections.Generic;
using System.Diagnostics;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using static Neptun.DI;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dna;
using System;

namespace Neptun
{
	public class MainNavMenuViewModel : BaseViewModel
	{
		public ICommand MessagesCommand { get; set; }

		public ObservableCollection<MainMenuItemViewModel> Items { get; set; }

		public string messagesText
		{
			get =>
"Üzenetek" + (ViewModelApplication.UnreadMessageCount == 0 ? "" : $" ({ViewModelApplication.UnreadMessageCount})");
		}

		public MainNavMenuViewModel()
		{
			//Logger.LogCriticalSource("Constructing MainMenuVM");
			Items = new ObservableCollection<MainMenuItemViewModel>();
			MessagesCommand = new RelayCommand(() =>
			{
				ViewModelApplication.GoToPage(Core.ApplicationPage.Messages);
			});
			//IsMessagesVisible = true;

			Task.Run(() =>
			{
				var TempItems = new ObservableCollection<MainMenuItemViewModel>();
				var requestmainpage = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx", Method.GET);
				IRestResponse MainHTMLPage;
				lock (RestWebClient)
				{
					MainHTMLPage = RestWebClient.Execute(requestmainpage);
				}
				var html = new HtmlDocument();
				html.LoadHtml(MainHTMLPage.Content);
				//var unreadmessages = html.GetElementbyId("_lnkInbox").ChildNodes[0].InnerText.Split(' ');
				//if (unreadmessages.Length == 3)
				//{
				//	var str = unreadmessages[2].Trim('(', ')');
				//	Int32.TryParse(str, out int tmp);
				//	ViewModelApplication.UnreadMessageCount = tmp;
				//}
				ViewModelApplication.NameAndNeptun = html.GetElementbyId("upTraining_topname")?.InnerText;
				var MainMenuEntries = html.GetElementbyId("Menu_neptun_neptun")?.ChildNodes[0].ChildNodes;
				//ViewModelApplication.ViewStateStr = html.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
				//ViewModelApplication.EventValidateStr = html.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");
				if (MainMenuEntries != null)
					foreach (var i in MainMenuEntries)
					{
						var tmp = new MainMenuItemViewModel(i)
						{
							Name = i.GetDirectInnerText()
						};
						Application.Current.Dispatcher.Invoke(() => Items.Add(tmp));
					}
				//Application.Current.Dispatcher.Invoke(() =>
				//{
				//Items = TempItems;
				//});
			});
		}
	}
}
