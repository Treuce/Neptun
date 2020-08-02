using HtmlAgilityPack;
using RestSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using static Dna.FrameworkDI;
using Dna;
using static Neptun.Core.CoreDI;
using System.Runtime.Hosting;
using System.Windows;
using System.Diagnostics;
using System;

namespace Neptun
{
	/// <summary>
	/// The View Model for the starting page, only the messages
	/// </summary>
	public class MessagesListViewModel : BaseViewModel
	{
		#region Private Members

		#endregion

		#region Constructor
		public MessagesListViewModel() : base()
		{
			AllMessages = new List<MessageEntry>();
			Messages = new ObservableCollection<MessageEntry>();
			FilterCommand = new RelayCommand(Filter);
			LoadMessages();
		}
		#endregion

		#region Private Methods
		private void Filter()
		{
			if (filter == MessageFilterEnum.Unique)
				Messages = new ObservableCollection<MessageEntry>(AllMessages.Where<MessageEntry>(s =>
				{
					switch (searchby)
					{
						case MessageSearchEnum.Sender:
							if (!String.IsNullOrWhiteSpace(SearchString))
								return s.Sender.ToLower().Contains(SearchString.ToLower()) && s.Sender != "Rendszerüzenet";
							else return s.Sender != "Rendszerüzenet";
						case MessageSearchEnum.Subject:
							if (!String.IsNullOrWhiteSpace(SearchString))
								return s.Subject.ToLower().Contains(SearchString.ToLower()) && s.Sender != "Rendszerüzenet";
							else return s.Subject != "Rendszerüzenet";
						case MessageSearchEnum.Time:
							{
								DateTime.TryParse(s.Time, out var tmp);
								return tmp.Date == selectedDate.Date && s.Sender != "Rendszerüzenet";
							}
						default:
							Debugger.Break(); break;
					}
					Debugger.Break();
					throw new NotImplementedException("Fuck..");
				}));

			else if (filter == MessageFilterEnum.Automatic)
				Messages = new ObservableCollection<MessageEntry>(AllMessages.Where<MessageEntry>(s =>
				{
					switch (searchby)
					{
						case MessageSearchEnum.Sender:
							if (!String.IsNullOrEmpty(SearchString))
								return s.Sender.ToLower().Contains(SearchString.ToLower()) && s.Sender == "Rendszerüzenet";
							else return s.Sender == "Rendszerüzenet";
						case MessageSearchEnum.Subject:
							if (!String.IsNullOrEmpty(SearchString))
								return s.Subject.ToLower().Contains(SearchString.ToLower()) && s.Sender == "Rendszerüzenet";
							else return s.Subject == "Rendszerüzenet";
						case MessageSearchEnum.Time:
							{
								DateTime.TryParse(s.Time, out var tmp);
								return selectedDate.Date == tmp.Date && s.Sender == "Rendszerüzenet";
							}
						default:
							Debugger.Break(); break;
					}
					Debugger.Break();
					throw new NotImplementedException("Fuck..");
				}));
			else
				Messages = new ObservableCollection<MessageEntry>(AllMessages.Where<MessageEntry>(s =>
				{
					switch (searchby)
					{
						case MessageSearchEnum.Sender:
							{
								if (!String.IsNullOrEmpty(SearchString))
									return s.Sender.ToLower().Contains(SearchString.ToLower());
								else return true;
							}
						case MessageSearchEnum.Subject:
							if (!String.IsNullOrEmpty(SearchString))
								return s.Subject.ToLower().Contains(SearchString.ToLower());
							else return true;
						case MessageSearchEnum.Time:
							{
								DateTime.TryParse(s.Time, out var tmp);
								return selectedDate.Date == tmp.Date;
							}
						default:
							Debugger.Break(); break;
					}
					Debugger.Break();
					throw new NotImplementedException("Fuck..");
				}));
		}

		private void LoadMessages()
		{
			Task.Run(() =>
			{
				Logger.LogDebugSource("Started loading messages");
				var Tmp = new ObservableCollection<MessageEntry>();
				var requestmainpage = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx", Method.GET);
				IRestResponse MainHTMLPage;
				lock (Neptun.Core.CoreDI.RestWebClient)
				{
					MainHTMLPage = RestWebClient.Execute(requestmainpage);
				}
				var html = new HtmlDocument();
				html.LoadHtml(MainHTMLPage.Content);
				var messages = html.GetElementbyId("c_messages_gridMessages_bodytable").ChildNodes[1].ChildNodes;
				var ViewStateStr = html.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
				var EventValidateStr = html.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");
				foreach (var a in messages)
				{
					var asd = a.ChildNodes;
					var sender = a.ChildNodes[4].InnerText;
					var subject = a.ChildNodes[6].InnerText;
					var elolvasott = a.ChildNodes[5].ChildNodes[0].GetAttributeValue("alt", "shouldn't happen") != "Olvasatlan üzenet";
					var onclick = a.ChildNodes[6].ChildNodes[0].GetAttributeValue("onclick", "FUCK");
					var date = a.ChildNodes[7].InnerText;
					var id = onclick.Split('(')[1].Split(',')[0].Replace("\'", "");
					//var tmp = new MessageEntry(EventValidateStr, ViewStateStr, onclick,sender,subject,date);
					var tmp = new MessageEntry()
					{
						ID = id,
						Sender = sender,
						Subject = subject,
						isRead = elolvasott,
						eventstr = EventValidateStr,
						viewstatestr = ViewStateStr,
						Time = date
					};
					AllMessages.Add(tmp);
					Application.Current.Dispatcher.Invoke(() => Messages.Add(tmp));
				}
				//Messages = new ObservableCollection<MessageEntry>(AllMessages);
			});
		}

		#endregion

		#region Public Properties

		public MessageFilterEnum filter { get; set; } = MessageFilterEnum.All;

		public MessageSearchEnum searchby { get; 
			set; } = MessageSearchEnum.Time;

		public ObservableCollection<MessageEntry> Messages { get; private set; }

		public List<MessageEntry> AllMessages { get; set; }

		public ICommand FilterCommand { get; set; }

		public string SearchString { get; set; }

		public bool isSenderFilterVisible { get => filter != MessageFilterEnum.Automatic; }

		public bool isDatePickerVisible { get => searchby == MessageSearchEnum.Time; }

		public DateTime selectedDate { get; set; } = DateTime.Now;
		#endregion
	}
}
