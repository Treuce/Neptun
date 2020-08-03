using Dna;
using GalaSoft.MvvmLight.Command;
using HtmlAgilityPack;
using RestSharp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using static Neptun.DI;

namespace Neptun
{
	public class TakeSubjectViewModel : BaseViewModel
	{
		#region Nested classes

		public class TFMenuItemViewModel : BaseViewModel
		{
			public string Name { get; set; }
			public override string ToString() => Name;
			public bool isEnabled { get; set; } = true;
		}

		public class AlapadatokViewModel : BaseViewModel
		{
			public string OriginalName { get; set; } = "Illés egy bohóc";
		}
		public AlapadatokViewModel AlapAdatok { get; set; } = new AlapadatokViewModel();
		public enum TFPage
		{
			Felvétel,
			Adatok,
			Téma,
			Notes,
			Hallgatók,
			Előkövetelmény
		}
		public class Course : BaseViewModel
		{
			public string CourseCode { get; set; }

			public string Type { get; set; }
			public string GroupName { get; set; }

			public string Teacher { get; set; }

			public string Language { get; set; }

			public string Note { get; set; }

			public string Limits { get; set; }
			public string Schedule { get; set; }
			public string rangsor { get; internal set; }
			public bool isEnabled { get; set; }
			public bool isSelected { get; set; }

			public string ID { get; internal set; }
			public bool SelectionChanged { get; set; } = false;

			public TFViewModel.SubjectType SubjectType { get; set; }
		}
		#endregion

		#region Constructors
		public TakeSubjectViewModel()
		{
			MenuItems = new ObservableCollection<TFMenuItemViewModel>();
			Courses = new ObservableCollection<Course>();
		}

		public TakeSubjectViewModel(string id, ref SubjectViewModel viewmodel, TFViewModel tFView, bool loadmenu = true)
		{
			//Logger.LogErrorSource(viewmodel.Code);
			//Debugger.Break();
			TFViewModel = tFView;
			ParentViewModel = viewmodel;
			myID = id;
			switchpage = new RelayCommand<TFMenuItemViewModel>(vm =>
			{
				foreach (var menuitem in MenuItems.Where(s => !s.isEnabled))
					menuitem.isEnabled = true;

				vm.isEnabled = false;
				switch (vm.Name)
				{
					case "Alapadatok":
						{
							currentPage = TFPage.Adatok;
							break;
						}
					case "Felvehető kurzusok":
						{
							currentPage = TFPage.Felvétel;
							break;
						}
					case "Témakör":
						{
							currentPage = TFPage.Téma;
							break;
						}
				}
			});

			#region Tárgy leadás
			ForgetSubject = new RelayCommand(() =>
			{
				//LoadCourseData(id, ParentViewModel.SubjectType);
				Task.Run(async () =>
				{
					HtmlNode node;
					string resultstr = "FUCK";
					var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
					IRestResponse response;
					string ViewStateStr;
					string EventValidateStr;
					lock (RestWebClient)
					{
						response = RestWebClient.Execute(request);

						var html = new HtmlDocument();
						html.LoadHtml(response.Content);
						ViewStateStr = html.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
						EventValidateStr = html.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

						#region TEST

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type.ToString());
						response = RestWebClient.Execute(request);

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddHeader("Cache-Control", "no-cache");
						request.AddHeader("X-Requested-With", "XMLHttpRequest");
						request.AddHeader("X-MicrosoftAjax", "Delta=true");
						request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
						request.AddHeader("Accept", "*");
						request.AddHeader("Sec-Fetch-Site", "same-origin");
						request.AddHeader("Sec-Fetch-Mode", "cors");
						request.AddHeader("Sec-Fetch-Dest", "empty");
						request.AddParameter("ActiveModalBehaviourID", "");
						request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upGrid$gridSubjects");
						request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={id};level=1");
						request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						request.AddParameter("hfCountDownTime", "600");
						request.AddParameter("hiddenEditLabel", "");
						request.AddParameter("progressalerttype", "progress");
						request.AddParameter("upFilter$cmbLanguage", "0");
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
						request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
						response = RestWebClient.Execute(request);

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type.ToString());
						response = RestWebClient.Execute(request);
						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						response = RestWebClient.Execute(request);

						#endregion

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
						request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
						request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":0,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
						request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab$ctl00$upAddSubjects$Addsubject_course1$upGrid$gridCourses");
						request.AddParameter("ToolkitScriptManager1_HiddenField", "");
						request.AddParameter("__ASYNCPOST", "true");
						request.AddParameter("__EVENTARGUMENT", "commandname=delete;commandsource=function");
						request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab$ctl00$upAddSubjects$Addsubject_course1$upGrid$gridCourses");
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__LASTFOCUS", "");
						request.AddParameter("__VIEWSTATE", ViewStateStr);
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
						request.AddParameter("upFilter$WTChooserFrom$cmbWTChooser_upFilter_WTChooserFrom", "Hétfő");
						request.AddParameter("upFilter$WTChooserFrom$maskEditT_upFilter_WTChooserFrom_ClientState", "");
						request.AddParameter("upFilter$WTChooserFrom$txbWTChooser_upFilter_WTChooserFrom", "");
						request.AddParameter("upFilter$WTChooserFrom$validCalloutExt_upFilter_WTChooserFrom_ClientState", "");
						request.AddParameter("upFilter$WTChooserTo$cmbWTChooser_upFilter_WTChooserTo", "Hétfő");
						request.AddParameter("upFilter$WTChooserTo$maskEditT_upFilter_WTChooserTo_ClientState", "");
						request.AddParameter("upFilter$WTChooserTo$txbWTChooser_upFilter_WTChooserTo", "");
						request.AddParameter("upFilter$WTChooserTo$validCalloutExt_upFilter_WTChooserTo_ClientState", "");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$cmbTerms", ParentViewModel.semester?.Value);
						request.AddParameter("upFilter$rbtnSubjectType", ParentViewModel.SubjectType == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
						request.AddParameter("upDetail$modaldetailAddsubject_course11782982854$upmodalDetail$_data", "Visible:false");
						//request.AddParameter("upFilter$txtOktato", "");
						//request.AddParameter("upFilter$txtTargyNev", "");
						//request.AddParameter("upFilter$txtTargykod", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
						request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
						//lock (RestWebClient)
						//{
						response = RestWebClient.Execute(request);
						//}
						html.LoadHtml(response.Content);

						node = html.GetElementbyId("_Label1");
					}
					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
					request.AddHeader("Cache-Control", "no-cache");
					request.AddHeader("X-Requested-With", "XMLHttpRequest");
					request.AddHeader("X-MicrosoftAjax", "Delta=true");
					request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
					request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":0,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
					request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab$ctl00$upAddSubjects$Addsubject_course1$upGrid$gridCourses");
					request.AddParameter("__EVENTARGUMENT", "commandname=update;commandsource=function");
					request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab$ctl00$upAddSubjects$Addsubject_course1$upGrid$gridCourses");
					request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					request.AddParameter("__VIEWSTATE", ViewStateStr);
					request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
					request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
					request.AddParameter("upFilter$rbtnSubjectType", ParentViewModel.SubjectType == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
					response = RestWebClient.Execute(request);

					//TODO: Find out why this crashes sometimes..
					var regex = new Regex(@"(<br />|<br/>|</ br>|</br>|<br>)");
					if (node != null)
						resultstr = regex.Replace(node.InnerHtml, Environment.NewLine);
					else Debugger.Break();
					foreach (var a in Courses)
					{
						a.isSelected = false;
					}
					if (resultstr != "FUCK")
					{
						ParentViewModel.InfoExpanded = false;
						ParentViewModel.taken = false;
					}
					await Task.Delay(TimeSpan.FromMinutes(TFViewModel.WaitTime));
					await UI.ShowMessage(new MessageBoxDialogViewModel()
					{
						Title = "Tárgyleadás: " + ParentViewModel.Code,
						Message = resultstr,
						OkText = "Fuck yeah"
					});
				});

			});

			#endregion

			#region Tárgyfelvétel
			TakeSubject = new RelayCommand(() =>
			{
				//LoadCourseData(id, ParentViewModel.SubjectType);
				Task.Run(async () =>
				{
					string resultstr;
					IRestResponse response;
					var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
					var html = new HtmlDocument();
					lock (RestWebClient)
					{
						var changedcourses = Courses.Where(s => s.SelectionChanged);
						response = RestWebClient.Execute(request);

						html.LoadHtml(response.Content);
						var ViewStateStr = html.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
						var EventValidateStr = html.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type.ToString());
						response = RestWebClient.Execute(request);

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddHeader("Cache-Control", "no-cache");
						request.AddHeader("X-Requested-With", "XMLHttpRequest");
						request.AddHeader("X-MicrosoftAjax", "Delta=true");
						request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
						request.AddHeader("Accept", "*");
						request.AddHeader("Sec-Fetch-Site", "same-origin");
						request.AddHeader("Sec-Fetch-Mode", "cors");
						request.AddHeader("Sec-Fetch-Dest", "empty");
						request.AddParameter("ActiveModalBehaviourID", "");
						request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upGrid$gridSubjects");
						request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={id};level=1");
						request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						request.AddParameter("hfCountDownTime", "600");
						request.AddParameter("hiddenEditLabel", "");
						request.AddParameter("progressalerttype", "progress");
						request.AddParameter("upFilter$cmbLanguage", "0");
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
						request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
						response = RestWebClient.Execute(request);

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", "All");
						request.AddParameter("upFilter$cmbTemplates", "883053304");
						request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type.ToString());
						response = RestWebClient.Execute(request);
						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						response = RestWebClient.Execute(request);

						//Debugger.Break();
						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "HandleRequest.ashx?RequestType=Update&GridID=Addsubject_course1_gridCourses&pageindex=1&pagesize=999999&sort1=&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=update&level=1", Method.POST);

						request.Timeout = -1;
						request.AddHeader("Content-Type", "text/plain;charset=UTF-8");
						string requestbodystr = $"{{\"Data\":[ ";
						foreach (var tmp in changedcourses)
							requestbodystr += $"{{\"ID\":\"{tmp.ID}\",\"chk\":\"%23{tmp.isSelected.ToString().ToLower()}\"}},";
						requestbodystr = requestbodystr.Remove(requestbodystr.Length - 1);
						requestbodystr += " ]}";
						request.AddParameter("text/plain;charset=UTF-8", requestbodystr, ParameterType.RequestBody);

						//lock (RestWebClient)
						//{
						response = RestWebClient.Execute(request);
						//}
						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddHeader("Cache-Control", "no-cache");
						request.AddHeader("X-Requested-With", "XMLHttpRequest");
						request.AddHeader("X-MicrosoftAjax", "Delta=true");
						request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
						request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":0,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
						request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab$ctl00$upAddSubjects$Addsubject_course1$upGrid$gridCourses");
						request.AddParameter("__EVENTARGUMENT", "commandname=update;commandsource=function");
						request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab$ctl00$upAddSubjects$Addsubject_course1$upGrid$gridCourses");
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
						request.AddParameter("upFilter$rbtnSubjectType", ParentViewModel.SubjectType == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
						response = RestWebClient.Execute(request);
					}
					html.LoadHtml(response.Content);
					var regex = new Regex(@"(<br />|<br/>|</ br>|</br>|<br>)");
					resultstr = regex.Replace(html.GetElementbyId("_Label1").InnerHtml, Environment.NewLine);
					//}
					if (resultstr.Contains("nem sikerült"))
						Debugger.Break();
					await Task.Delay(TimeSpan.FromMinutes(TFViewModel.WaitTime));
					await UI.ShowMessage(new MessageBoxDialogViewModel()
					{
						Title = "Tárgyfelvétel: " + ParentViewModel.Code,
						Message = resultstr,
						OkText = "Fuck yeah"
					});
					//if (resultstr.ToLower().Contains("siker"))
					//{
					ParentViewModel.taken = true;
					ParentViewModel.InfoExpanded = false;
					//}
				});
			});
			#endregion

			LoadCourseData(id, ParentViewModel.SubjectType, loadmenu);
		}

		private void LoadCourseData(string id, TFViewModel.SubjectType type, bool loadmenu)
		{
			MenuItems = new ObservableCollection<TFMenuItemViewModel>();
			Courses = new ObservableCollection<Course>();
			Task.Run(() =>
			{
				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
				string tmp;
				var html = new HtmlDocument();
				IRestResponse response;

				lock (RestWebClient)
				{
					//lock (RestWebClient)
					//{
					tmp = RestWebClient.Execute(request).Content;
					//}
					var tmphtml = new HtmlDocument();
					tmphtml.LoadHtml(tmp);
					string ViewStateStr = tmphtml.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
					string EventValidateStr = tmphtml.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");


					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
					request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					request.AddParameter("__VIEWSTATE", ViewStateStr);
					//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
					request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", "All");
					request.AddParameter("upFilter$txtKurzuskod", "");
					request.AddParameter("upFilter$txtOktato", "");
					request.AddParameter("upFilter$txtTargyNev", "");
					request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
					request.AddParameter("upFilter$cmbTemplates", "883053304");
					request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", "All");
					request.AddParameter("upFilter$cmbTemplates", "883053304");
					request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
					request.AddParameter("upFilter$rbtnSubjectType", type.ToString());
					//lock (RestWebClient)
					//{
					response = RestWebClient.Execute(request);
					//}
					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
					request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
					//lock (RestWebClient)
					//{
					response = RestWebClient.Execute(request);
					//}

					html.LoadHtml(response.Content);
					ViewStateStr = html.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
					EventValidateStr = html.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

					//request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
					////request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					////request.AddParameter("__VIEWSTATE", ViewStateStr);
					////request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
					//request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
					//request.AddParameter("upFilter$cmbSubjectGroups", "All");
					//request.AddParameter("upFilter$txtKurzuskod", "");
					//request.AddParameter("upFilter$txtOktato", "");
					//request.AddParameter("upFilter$txtTargyNev", "");
					//request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
					//request.AddParameter("upFilter$cmbTemplates", "883053304");
					//request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
					//request.AddParameter("upFilter$cmbSubjectGroups", "All");
					//request.AddParameter("upFilter$cmbTemplates", "883053304");
					//request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
					//request.AddParameter("upFilter$rbtnSubjectType", type.ToString());
					//lock (RestWebClient)
					//{
					//	response = RestWebClient.Execute(request);
					//}
					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
					request.AddHeader("Cache-Control", "no-cache");
					request.AddHeader("X-Requested-With", "XMLHttpRequest");
					request.AddHeader("X-MicrosoftAjax", "Delta=true");
					request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
					request.AddHeader("Accept", "*");
					request.AddHeader("Sec-Fetch-Site", "same-origin");
					request.AddHeader("Sec-Fetch-Mode", "cors");
					request.AddHeader("Sec-Fetch-Dest", "empty");
					request.AddParameter("ActiveModalBehaviourID", "");
					request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upGrid$gridSubjects");
					request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={id};level=1");
					request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
					request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					request.AddParameter("__VIEWSTATE", ViewStateStr);
					request.AddParameter("hfCountDownTime", "600");
					request.AddParameter("hiddenEditLabel", "");
					request.AddParameter("progressalerttype", "progress");
					request.AddParameter("upFilter$cmbLanguage", "0");
					request.AddParameter("upFilter$cmbSubjectGroups", "All");
					request.AddParameter("upFilter$cmbTemplates", "883053304");
					request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
					request.AddParameter("upFilter$rbtnSubjectType", type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
					request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
					//lock (RestWebClient)
					//{
					response = RestWebClient.Execute(request);
					//}
					html.LoadHtml(response.Content);

					if (loadmenu)
					{
						var menu = html.GetElementbyId("Subject_data_for_schedule_tab_header")?.ChildNodes.Where(s => s.Name == "span");
						if (menu == null)
							Debugger.Break();
						foreach (var a in menu)
						{
							Application.Current.Dispatcher.Invoke(() =>
							{
								MenuItems.Add(new TFMenuItemViewModel()
								{
									Name = a.InnerText
								});
							});
						}
						var list = html.GetElementbyId("Addsubject_course1_gridCourses_bodytable").ChildNodes.FindFirst("tbody");
						foreach (var a in list.ChildNodes)
						{
							try
							{

								var course = new Course();
								var b = a.ChildNodes;
								course.SubjectType = type;
								course.CourseCode = b[1].ChildNodes[0].InnerText;
								var tmpasd = b[1].ChildNodes[0].GetAttributeValue("onclick", "");
								course.ID = String.IsNullOrWhiteSpace(tmpasd) ? "Imnotsure" : tmpasd.Split('(')[1].Split(',')[0].Replace("\'", "");
								course.Note = b[1].ChildNodes.Count == 2 ? b[1].ChildNodes[1].InnerText : String.Empty;
								course.Type = b[2].ChildNodes[0].InnerText;
								course.GroupName = b[2].ChildNodes[0].InnerText + id;
								course.Limits = b[3].ChildNodes[0].InnerText;
								course.rangsor = b[6].GetDirectInnerText();
								course.Schedule = b[7].ChildNodes.Count == 2 ? b[7].ChildNodes[0].GetDirectInnerText() : String.Empty;
								course.isEnabled = !b[1].HasClass("link_disabled");
								//Debugger.Break();
								//TODO: órarendi infó, rohadt megjegyzés..
								course.Teacher = b[8].ChildNodes[0].InnerText;
								course.Language = b[9].GetDirectInnerText();
								//Debugger.Break();
								course.isSelected = b[b.Count - 1].ChildNodes.FindFirst("input").GetAttributeValue("checked", "") == "checked";
								Application.Current.Dispatcher.Invoke(() =>
								{
									Courses.Add(course);
								});
							}
							catch (Exception e)
							{
								Logger.LogErrorSource($"This shouldn't happen.. {e.Message}");
								Debugger.Break();
							}
						}
						//request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
						//response = RestWebClient.Execute(request);
						//tmphtml.LoadHtml(tmp);
						//ViewStateStr = tmphtml.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
						//EventValidateStr = tmphtml.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");
						//request = new RestRequest("https://hallgato.neptun.elte.hu/main.aspx?ismenuclick=true&ctrl=0303",Method.POST);
						//request.AddHeader("Cache-Control", "no-cache");
						//request.AddHeader("X-Requested-With", "XMLHttpRequest");
						//request.AddHeader("X-MicrosoftAjax", "Delta=true");
						//request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
						//request.AddHeader("Accept", "*/*");
						//request.AddHeader("Sec-Fetch-Site", "same-origin");
						//request.AddHeader("Sec-Fetch-Mode", "cors");
						//request.AddHeader("Sec-Fetch-Dest", "empty");
						//request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
						//request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
						//request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":1,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
						//request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
						//request.AddParameter("ToolkitScriptManager1_HiddenField", "");
						//request.AddParameter("__ASYNCPOST", "true");
						//request.AddParameter("__EVENTARGUMENT", "activeTabChanged:1");
						//request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
						//request.AddParameter("__EVENTVALIDATION",EventValidateStr);
						//request.AddParameter("__LASTFOCUS", "");
						//request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						//request.AddParameter("filedownload$hfDocumentId", "");
						//request.AddParameter("hfCountDownTime", "600");
						//request.AddParameter("hiddenEditLabel", "");
						//request.AddParameter("progressalerttype", "progress");
						//request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkAppointment", "on");
						//request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkExam", "on");
						//request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkKonzultacio", "on");
						//request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkRegisterList", "on");
						//request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkTask", "on");
						//request.AddParameter("upBoxes$upCalendar$gdgCalendar$ctl35$calendar$upPanel$chkTime", "on");
						//request.AddParameter("upFilter$WTChooserFrom$cmbWTChooser_upFilter_WTChooserFrom", "Hétfő");
						//request.AddParameter("upFilter$WTChooserFrom$maskEditT_upFilter_WTChooserFrom_ClientState", "");
						//request.AddParameter("upFilter$WTChooserFrom$txbWTChooser_upFilter_WTChooserFrom", "");
						//request.AddParameter("upFilter$WTChooserFrom$validCalloutExt_upFilter_WTChooserFrom_ClientState", "");
						//request.AddParameter("upFilter$WTChooserTo$cmbWTChooser_upFilter_WTChooserTo", "Hétfő");
						//request.AddParameter("upFilter$WTChooserTo$maskEditT_upFilter_WTChooserTo_ClientState", "");
						//request.AddParameter("upFilter$WTChooserTo$txbWTChooser_upFilter_WTChooserTo", "");
						//request.AddParameter("upFilter$WTChooserTo$validCalloutExt_upFilter_WTChooserTo_ClientState", "");
						//request.AddParameter("upFilter$cmbLanguage", "0");
						//request.AddParameter("upFilter$cmbSubjectGroups", "All");
						//request.AddParameter("upFilter$cmbTemplates", "All");
						//request.AddParameter("upFilter$cmbTerms", "70620");
						//request.AddParameter("upFilter$rbtnSubjectType", "Mintatantervi");
						//request.AddParameter("upFilter$txtKurzuskod", "");
						//request.AddParameter("upFilter$txtOktato", "");
						//request.AddParameter("upFilter$txtTargyNev", "");
						//request.AddParameter("upFilter$txtTargykod", "");
						//request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
						//request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
						//response = RestWebClient.Execute(request);
						//Debugger.Break();
					}
				}
			});

		}

		#endregion

		#region Public Properties

		public ObservableCollection<TFMenuItemViewModel> MenuItems { get; set; }

		public ObservableCollection<Course> Courses { get; set; }

		public SubjectViewModel ParentViewModel { get; set; }

		public string myID { get; private set; }

		public TFViewModel TFViewModel { get; set; }

		public bool CoursesPageVisible { get => currentPage == TFPage.Felvétel; }
		public bool StatsPageVisible { get => currentPage == TFPage.Adatok; }
		public bool ThemePageVisible { get => currentPage == TFPage.Téma; }
		public bool NotesPageVisible { get => currentPage == TFPage.Notes; }
		public bool StudentsPageVisible { get => currentPage == TFPage.Hallgatók; }
		public bool EloasdPageVisible { get => currentPage == TFPage.Előkövetelmény; }

		public TFPage currentPage { get; set; } = TFPage.Felvétel;
		#endregion

		#region Public Commands

		public ICommand switchpage { get; set; }

		public ICommand TakeSubject { get; set; }

		public ICommand ForgetSubject { get; set; }

		#endregion
	}
}
