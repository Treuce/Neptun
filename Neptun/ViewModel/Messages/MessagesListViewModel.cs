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
using static Neptun.DI;
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

		private MessageSearchEnum mSearch = MessageSearchEnum.Subject;

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

				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"/HandleRequest.ashx?RequestType=GetData&GridID=c_messages_gridMessages&pageindex=1&pagesize=10000&sort1=SendDate%20DESC&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);

				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");
				lock (RestWebClient)
				{
					RestWebClient.Execute(request);
				}

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
				var pagesize = Messages.Count;
				//
				//var tmppaginggridstuff = html.GetElementbyId("c_messages_gridMessages_gridmaindiv").ChildNodes[3].ChildNodes[0].ChildNodes[2].ChildNodes[0].ChildNodes[0].ChildNodes.Skip(1);
				//var test = tmppaginggridstuff.Take(tmppaginggridstuff.Count() - 2).ToList();
				//foreach (var a in test)
				//{
				//		 request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"/HandleRequest.ashx?RequestType=GetData&GridID=c_messages_gridMessages&pageindex={a.InnerText}&pagesize={pagesize}&sort1=SendDate%20DESC&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);

				//	request.AddHeader("Accept", "*/*");
				//	request.AddHeader("Sec-Fetch-Site", "same-origin");
				//	request.AddHeader("Sec-Fetch-Mode", "cors");
				//	request.AddHeader("Sec-Fetch-Dest", "empty");
				//	string result;
				//	lock (RestWebClient)
				//	{
				//		result = RestWebClient.Execute(request).Content;
				//	}
				//	result = result.Replace("{type:getdata}", String.Empty);
				//	html.LoadHtml(result);
				//	messages = html.GetElementbyId("c_messages_gridMessages_bodytable").ChildNodes[1].ChildNodes;
				//	foreach (var b in messages)
				//	{
				//		var asd = b.ChildNodes;
				//		var sender = b.ChildNodes[4].InnerText;
				//		var subject = b.ChildNodes[6].InnerText;
				//		var elolvasott = b.ChildNodes[5].ChildNodes[0].GetAttributeValue("alt", "shouldn't happen") != "Olvasatlan üzenet";
				//		var onclick = b.ChildNodes[6].ChildNodes[0].GetAttributeValue("onclick", "FUCK");
				//		var date = b.ChildNodes[7].InnerText;
				//		var id = onclick.Split('(')[1].Split(',')[0].Replace("\'", "");
				//		var tmp = new MessageEntry()
				//		{
				//			ID = id,
				//			Sender = sender,
				//			Subject = subject,
				//			isRead = elolvasott,
				//			eventstr = EventValidateStr,
				//			viewstatestr = ViewStateStr,
				//			Time = date
				//		};
				//		AllMessages.Add(tmp);
				//		Application.Current.Dispatcher.Invoke(() => Messages.Add(tmp));
				//	}
				//}
			});
		}

		#endregion

		#region Constructor
		public MessagesListViewModel() : base()
		{
			AllMessages = new List<MessageEntry>();
			Messages = new ObservableCollection<MessageEntry>();
			FilterCommand = new RelayCommand(Filter);
			MarkSelectedAsRead = new RelayCommand(() =>
			{
				List<MessageEntry> selected;
				if (AllSelected)
					selected = Messages.ToList();
				else
					selected = Messages.Where(s => s.isChecked).ToList();
				Task.Run(() =>
				{
					foreach (var msg in selected)
					{
						var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"/HandleRequest.ashx?RequestType=GetData&GridID=c_messages_gridMessages&pageindex=1&pagesize=10000&sort1=SendDate%20DESC&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);

						request.AddHeader("Accept", "*/*");
						request.AddHeader("Sec-Fetch-Site", "same-origin");
						request.AddHeader("Sec-Fetch-Mode", "cors");
						request.AddHeader("Sec-Fetch-Dest", "empty");
						lock (RestWebClient)
						{
							RestWebClient.Execute(request);
						}
						msg.isRead = true;
						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx", Method.POST);
						request.AddParameter("__EVENTVALIDATION", msg.eventstr);
						request.AddParameter("__VIEWSTATE", msg.viewstatestr);
						var id = msg.ID;
						request.AddParameter("__EVENTARGUMENT", $"commandname=Subject;commandsource=select;id={id};level=1");
						request.AddHeader("Cache-Control", "no-cache");
						request.AddHeader("X-Requested-With", "XMLHttpRequest");
						request.AddHeader("X-MicrosoftAjax", "Delta=true");
						request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
						request.AddHeader("Accept", "*/*");
						request.AddHeader("Sec-Fetch-Site", "same-origin");
						request.AddHeader("Sec-Fetch-Mode", "cors");
						request.AddHeader("Sec-Fetch-Dest", "empty");
						request.AddParameter("upFilter$rblMessageTypes", "Összes üzenet");
						request.AddParameter("__EVENTTARGET", "upFunction$c_messages$upMain$upGrid$gridMessages");
						IRestResponse response;
						lock (RestWebClient)
						{
							response = RestWebClient.Execute(request);
						}
					}
				});
			});

			DeleteSelectedMessages = new RelayCommand(() =>
			{
				Task.Run(async () =>
				{
					string responsemessage = String.Empty;
					for (int i = 0; i < Messages.Count; ++i)
					{
						if (Messages[i].isChecked)
						{
							IRestResponse response;

							var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"/HandleRequest.ashx?RequestType=GetData&GridID=c_messages_gridMessages&pageindex=1&pagesize=10000&sort1=SendDate%20DESC&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);

							request.AddHeader("Accept", "*/*");
							request.AddHeader("Sec-Fetch-Site", "same-origin");
							request.AddHeader("Sec-Fetch-Mode", "cors");
							request.AddHeader("Sec-Fetch-Dest", "empty");
							lock (RestWebClient)
							{
								RestWebClient.Execute(request);
							}

							request = new RestRequest("https://hallgato.neptun.elte.hu/HandleRequest.ashx?RequestType=Update&GridID=c_messages_gridMessages&pageindex=1&pagesize=10000&sort1=SendDate%20DESC&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=delete&level=1", Method.POST);
							request.AddHeader("Content-Type", "text/plain;charset=UTF-8");
							request.AddHeader("Accept", "*/*");
							request.AddHeader("Sec-Fetch-Site", "same-origin");
							request.AddHeader("Sec-Fetch-Mode", "cors");
							request.AddHeader("Sec-Fetch-Dest", "empty");
							request.AddParameter("text/plain;charset=UTF-8", $"{{\"Data\":[ {{\"PersonMessageID\":\"{Messages[i].ID}\",\"chk\":\"%23true\"}} ]}}", ParameterType.RequestBody);
							lock (RestWebClient)
							{
								response = RestWebClient.Execute(request);
							}
							request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx", Method.POST);
							request.AddHeader("Cache-Control", "no-cache");
							request.AddHeader("X-Requested-With", "XMLHttpRequest");
							request.AddHeader("X-MicrosoftAjax", "Delta=true");
							request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
							request.AddHeader("Accept", "*/*");
							request.AddHeader("Sec-Fetch-Site", "same-origin");
							request.AddHeader("Sec-Fetch-Mode", "cors");
							request.AddHeader("Sec-Fetch-Dest", "empty");
							request.AddParameter("ActiveModalBehaviourID", "");
							request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
							request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$c_messages$upMain$upGrid$gridMessages");
							request.AddParameter("ToolkitScriptManager1_HiddenField", "");
							request.AddParameter("__ASYNCPOST", "true");
							request.AddParameter("__EVENTARGUMENT", "commandname=delete;commandsource=function");
							request.AddParameter("__EVENTTARGET", "upFunction$c_messages$upMain$upGrid$gridMessages");
							request.AddParameter("__EVENTVALIDATION", Messages[i].eventstr);
							request.AddParameter("__LASTFOCUS", "");
							request.AddParameter("__VIEWSTATE", Messages[i].viewstatestr);
							request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
							request.AddParameter("filedownload$hfDocumentId", "");
							request.AddParameter("hfCountDownTime", "600");
							request.AddParameter("hiddenEditLabel", "");
							request.AddParameter("progressalerttype", "progress");
							request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkAppointment", "on");
							request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkExam", "on");
							request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkKonzultacio", "on");
							request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkRegisterList", "on");
							request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkTask", "on");
							request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkTime", "on");
							request.AddParameter("upFilter$rblMessageTypes", "Összes üzenet");
							request.AddParameter("upFunction$c_messages$upMain$hfDocumentId", "");
							request.AddParameter("upFunction$c_messages$upMain$upFilter$searchpanel$searchpanel_state", "expanded");
							lock (RestWebClient)
							{
								response = RestWebClient.Execute(request);
							}
							if (response.Content.Contains("sikerült"))
							{
								var html = new HtmlDocument();
								html.LoadHtml(response.Content);
								responsemessage = html.GetElementbyId("infomessage").InnerText;
								Application.Current.Dispatcher.Invoke(() => Messages.RemoveAt(i));
							}
						}
					}
					await UI.ShowMessage(new MessageBoxDialogViewModel()
					{
						Title = "Üzenetek törlése",
						Message = responsemessage,
						OkText = "Köszi"
					});
				});
			});
			SelectAll = new RelayCommand(() =>
			{
				//if (AllSelected)
				foreach (var msg in Messages)
					msg.isChecked = AllSelected;
				//else
				//	foreach (var msg in Messages)
				//		msg.isChecked = false;
			});
			LoadMessages();
		}
		#endregion


		#region Public Properties

		public MessageFilterEnum filter { get; set; } = MessageFilterEnum.All;

		public MessageSearchEnum searchby { get; set; } = MessageSearchEnum.Subject;

		public ObservableCollection<MessageEntry> Messages { get; private set; }

		public List<MessageEntry> AllMessages { get; set; }

		public string SearchString { get; set; }

		public bool isSenderFilterVisible { get => filter != MessageFilterEnum.Automatic; }

		public bool isDatePickerVisible { get => searchby == MessageSearchEnum.Time; }

		public DateTime selectedDate { get; set; } = DateTime.Now;

		public bool AllSelected { get; set; }
		#endregion

		#region Public Commands

		public ICommand FilterCommand { get; set; }
		public ICommand DeleteSelectedMessages { get; set; }
		public ICommand SelectAll { get; set; }
		public ICommand MarkSelectedAsRead { get; set; }

		#endregion
	}
}
