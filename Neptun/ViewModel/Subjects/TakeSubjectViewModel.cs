using Dna;
using GalaSoft.MvvmLight.Command;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Navigation;
using Xceed.Wpf.Toolkit;
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
		public enum TFPage
		{
			Default,
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
			public string ToolTip { get; set; }

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

		#region Private Methods

		private void LoadTheme()
		{
			lock (RestWebClient)
			{
				#region Load page on server too.. PS: a Neptun egy szar..

				IRestResponse response;
				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
				var html = new HtmlDocument();
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
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
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
				request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={ParentViewModel.id};level=1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
				request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				request.AddParameter("__VIEWSTATE", ViewStateStr);
				request.AddParameter("hfCountDownTime", "600");
				request.AddParameter("hiddenEditLabel", "");
				request.AddParameter("progressalerttype", "progress");
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				response = RestWebClient.Execute(request);

				#endregion

				request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
				request.AddHeader("Cache-Control", "no-cache");
				request.AddHeader("X-Requested-With", "XMLHttpRequest");
				request.AddHeader("X-MicrosoftAjax", "Delta=true");
				request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");
				request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
				request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
				request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":2,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
				request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("ToolkitScriptManager1_HiddenField", "");
				request.AddParameter("__ASYNCPOST", "true");
				request.AddParameter("__EVENTARGUMENT", "activeTabChanged:2");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("__EVENTVALIDATION", "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");
				request.AddParameter("__LASTFOCUS", "");
				request.AddParameter("__VIEWSTATE", "/wEPaA8FDzhkODM3OTZlZTY0NjBiMRgDBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WHQVQdXBGdW5jdGlvbiRoX2FkZHN1YmplY3RzJHVwT3JhcmVuZFRlcnZlem8kb3JhcmVuZHRlcnZlem8xJHVwVGltZVRhYmxlJFRpbWVUYWJsZTEFCWJ0bkxhbmdfMAUJYnRuTGFuZ18xBQlidG5MYW5nXzIFC2J0bnNraW5QaW5rBQtidG5za2luQmx1ZQUMYnRuc2tpbkdyZWVuBQ1idG5za2luT3JhbmdlBQ5idG5za2luVGVhY2hlcgUNYnRuc2tpblB1cnBsZQUeaW1nU2tpbkNob29zZXJQYXJ0aWFsbHlTaWdodGVkBR11cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0blJzcwUhdXBCb3hlcyR1cEJveGVzQnV0dG9ucyRidG5NZXNzYWdlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZhdm9yaXRlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkNhbGVuZGFyBR91cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZvcnVtBSZ1cEJveGVzJHVwUlNTJGdkZ1JTUyRnZGdSU1NfUmVmcmVzaEJ0bgUkdXBCb3hlcyR1cFJTUyRnZGdSU1MkZ2RnUlNTX0Nsb3NlQnRuBTJ1cEJveGVzJHVwTWVzc2FnZSRnZGdNZXNzYWdlJGdkZ01lc3NhZ2VfUmVmcmVzaEJ0bgUwdXBCb3hlcyR1cE1lc3NhZ2UkZ2RnTWVzc2FnZSRnZGdNZXNzYWdlX0Nsb3NlQnRuBTZ1cEJveGVzJHVwZmF2b3JpdGVzJGdkZ0Zhdm9yaXRlJGdkZ0Zhdm9yaXRlX1JlZnJlc2hCdG4FNHVwQm94ZXMkdXBmYXZvcml0ZXMkZ2RnRmF2b3JpdGUkZ2RnRmF2b3JpdGVfQ2xvc2VCdG4FNXVwQm94ZXMkdXBDYWxlbmRhciRnZGdDYWxlbmRhciRnZGdDYWxlbmRhcl9SZWZyZXNoQnRuBTN1cEJveGVzJHVwQ2FsZW5kYXIkZ2RnQ2FsZW5kYXIkZ2RnQ2FsZW5kYXJfQ2xvc2VCdG4FOnVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X1JlZnJlc2hCdG4FOHVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X0Nsb3NlQnRuBTp1cEZpbHRlciRXVENob29zZXJGcm9tJGNoa1dUQ2hvb3Nlcl91cEZpbHRlcl9XVENob29zZXJGcm9tBTZ1cEZpbHRlciRXVENob29zZXJUbyRjaGtXVENob29zZXJfdXBGaWx0ZXJfV1RDaG9vc2VyVG8FYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIFYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIPD2RmZAVXdXBCb3hlcyR1cEZvcnVtJGdkZ0ZvcnVtJHVwUGFyZW50JGdhZGdldCRjdGwzNSRWU19GYXZvdXJpdGVUb3BpY3NfZ2FkU21hbGwxJGx2RmF2VG9waWNzDzwrAA4DCGYMZg0C/////w9k/xFeA3s6riIQ9ZMoDYJ7fVkRXJvACO6MW7hxGZ/nUK8=");
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
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFilter$txtKurzuskod", "");
				request.AddParameter("upFilter$txtOktato", "");
				request.AddParameter("upFilter$txtTargyNev", "");
				request.AddParameter("upFilter$txtTargykod", "");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
				response = RestWebClient.Execute(request);
				html = new HtmlDocument();
				html.LoadHtml(response.Content);
				string theme = HtmlToXamlConverter.ConvertHtmlToXaml(html.GetElementbyId("Tematicsstudent1_gridThematics_bodytable").OuterHtml, false);
				//ThemeXAMLString = theme;

				var themesection = (Section)XamlReader.Parse(theme);
				var themerowgroups = themesection.Blocks.OfType<Table>().ToList()[0].RowGroups.ToList();
				var themerows = themerowgroups[0].Rows;
				foreach (var a in themerows)
				{
					foreach (var b in a.Cells)
					{
						if (Application.Current.Resources["FontSizeLarge"] is double d)
							b.FontSize = d;
						b.BorderThickness = new Thickness(0);
						var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
						inline.FontFamily = Application.Current.Resources["LatoBold"] as FontFamily;
					}
				}
				themerows = themerowgroups[1].Rows;
				foreach (var a in themerows)
				{
					foreach (var b in a.Cells)
					{
						if (Application.Current.Resources["FontSizeRegular"] is double d)
							b.FontSize = d;
						b.BorderThickness = new Thickness(0);
						var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
						inline.FontFamily = Application.Current.Resources["LatoRegular"] as FontFamily;
						inline.FontWeight = FontWeights.Regular;
					}
				}
				ThemeXAMLString = XamlWriter.Save(themesection);
			}
		}

		private void LoadDetails()
		{
			lock (RestWebClient)
			{
				#region Load page on server too.. PS: a Neptun egy szar..

				IRestResponse response;
				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
				var html = new HtmlDocument();
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
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
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
				request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={ParentViewModel.id};level=1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
				request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				request.AddParameter("__VIEWSTATE", ViewStateStr);
				request.AddParameter("hfCountDownTime", "600");
				request.AddParameter("hiddenEditLabel", "");
				request.AddParameter("progressalerttype", "progress");
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				response = RestWebClient.Execute(request);

				#endregion

				request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
				request.AddHeader("Cache-Control", "no-cache");
				request.AddHeader("X-Requested-With", "XMLHttpRequest");
				request.AddHeader("X-MicrosoftAjax", "Delta=true");
				request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");
				request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
				request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
				request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":1,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
				request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("ToolkitScriptManager1_HiddenField", "");
				request.AddParameter("__ASYNCPOST", "true");
				request.AddParameter("__EVENTARGUMENT", "activeTabChanged:1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("__EVENTVALIDATION", "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");
				request.AddParameter("__LASTFOCUS", "");
				request.AddParameter("__VIEWSTATE", "/wEPaA8FDzhkODM3OTZlZTY0NjBiMRgDBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WHQVQdXBGdW5jdGlvbiRoX2FkZHN1YmplY3RzJHVwT3JhcmVuZFRlcnZlem8kb3JhcmVuZHRlcnZlem8xJHVwVGltZVRhYmxlJFRpbWVUYWJsZTEFCWJ0bkxhbmdfMAUJYnRuTGFuZ18xBQlidG5MYW5nXzIFC2J0bnNraW5QaW5rBQtidG5za2luQmx1ZQUMYnRuc2tpbkdyZWVuBQ1idG5za2luT3JhbmdlBQ5idG5za2luVGVhY2hlcgUNYnRuc2tpblB1cnBsZQUeaW1nU2tpbkNob29zZXJQYXJ0aWFsbHlTaWdodGVkBR11cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0blJzcwUhdXBCb3hlcyR1cEJveGVzQnV0dG9ucyRidG5NZXNzYWdlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZhdm9yaXRlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkNhbGVuZGFyBR91cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZvcnVtBSZ1cEJveGVzJHVwUlNTJGdkZ1JTUyRnZGdSU1NfUmVmcmVzaEJ0bgUkdXBCb3hlcyR1cFJTUyRnZGdSU1MkZ2RnUlNTX0Nsb3NlQnRuBTJ1cEJveGVzJHVwTWVzc2FnZSRnZGdNZXNzYWdlJGdkZ01lc3NhZ2VfUmVmcmVzaEJ0bgUwdXBCb3hlcyR1cE1lc3NhZ2UkZ2RnTWVzc2FnZSRnZGdNZXNzYWdlX0Nsb3NlQnRuBTZ1cEJveGVzJHVwZmF2b3JpdGVzJGdkZ0Zhdm9yaXRlJGdkZ0Zhdm9yaXRlX1JlZnJlc2hCdG4FNHVwQm94ZXMkdXBmYXZvcml0ZXMkZ2RnRmF2b3JpdGUkZ2RnRmF2b3JpdGVfQ2xvc2VCdG4FNXVwQm94ZXMkdXBDYWxlbmRhciRnZGdDYWxlbmRhciRnZGdDYWxlbmRhcl9SZWZyZXNoQnRuBTN1cEJveGVzJHVwQ2FsZW5kYXIkZ2RnQ2FsZW5kYXIkZ2RnQ2FsZW5kYXJfQ2xvc2VCdG4FOnVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X1JlZnJlc2hCdG4FOHVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X0Nsb3NlQnRuBTp1cEZpbHRlciRXVENob29zZXJGcm9tJGNoa1dUQ2hvb3Nlcl91cEZpbHRlcl9XVENob29zZXJGcm9tBTZ1cEZpbHRlciRXVENob29zZXJUbyRjaGtXVENob29zZXJfdXBGaWx0ZXJfV1RDaG9vc2VyVG8FYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIFYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIPD2RmZAVXdXBCb3hlcyR1cEZvcnVtJGdkZ0ZvcnVtJHVwUGFyZW50JGdhZGdldCRjdGwzNSRWU19GYXZvdXJpdGVUb3BpY3NfZ2FkU21hbGwxJGx2RmF2VG9waWNzDzwrAA4DCGYMZg0C/////w9k/xFeA3s6riIQ9ZMoDYJ7fVkRXJvACO6MW7hxGZ/nUK8=");
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
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFilter$txtKurzuskod", "");
				request.AddParameter("upFilter$txtOktato", "");
				request.AddParameter("upFilter$txtTargyNev", "");
				request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
				response = RestWebClient.Execute(request);
				html = new HtmlDocument();
				html.LoadHtml(response.Content);
				var asdasd = html.GetElementbyId("dtbBaseData2_tableBodyLeft");
				var dasdasd = html.GetElementbyId("dtbBaseData2_tableBodyRight");
				string table1 = HtmlToXamlConverter.ConvertHtmlToXaml(html.GetElementbyId("dtbBaseData2_tableBodyLeft").OuterHtml, false);
				string table2 = HtmlToXamlConverter.ConvertHtmlToXaml(html.GetElementbyId("dtbBaseData2_tableBodyRight").OuterHtml, false);
				var section1 = (Section)XamlReader.Parse(table1);
				var section2 = (Section)XamlReader.Parse(table2);
				var rows1 = section1.Blocks.OfType<Table>().ToList()[0].RowGroups.ToList()[0].Rows;
				var rows2 = section2.Blocks.OfType<Table>().ToList()[0].RowGroups.ToList()[0].Rows;
				foreach (var a in rows1)
				{
					foreach (var b in a.Cells)
					{
						if (Application.Current.Resources["FontSizeLarge"] is double d)
							b.FontSize = d;
						b.BorderThickness = new Thickness(0);
						var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
						inline.FontFamily = Application.Current.Resources["LatoBold"] as FontFamily;
						inline.FontWeight = FontWeights.Bold;
					}
				}
				foreach (var a in rows2)
				{
					foreach (var b in a.Cells)
					{
						if (Application.Current.Resources["FontSizeLarge"] is double d)
							b.FontSize = d;
						b.BorderThickness = new Thickness(0);
						var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
						inline.FontFamily = Application.Current.Resources["LatoBold"] as FontFamily;
						inline.FontWeight = FontWeights.Bold;
					}
				}
				TempXAMLSTring = XamlWriter.Save(section1);
				TempXAMLSTring = TempXAMLSTring.Replace("Webcím::", "Webcím:");
				TempXAMLSTring2 = XamlWriter.Save(section2);
				TempXAMLSTring2 = TempXAMLSTring2.Replace("Illés Zoltán Dr.", "Illés egy bohóc");
			}
		}

		private void LoadNotes()
		{

			#region Load page on server too.. PS: a Neptun egy szar..

			IRestResponse response;
			var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
			var html = new HtmlDocument();
			lock (RestWebClient)
			{

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
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
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
				request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={ParentViewModel.id};level=1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
				request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				request.AddParameter("__VIEWSTATE", ViewStateStr);
				request.AddParameter("hfCountDownTime", "600");
				request.AddParameter("hiddenEditLabel", "");
				request.AddParameter("progressalerttype", "progress");
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				response = RestWebClient.Execute(request);

				#endregion

				request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
				request.AddHeader("Cache-Control", "no-cache");
				request.AddHeader("X-Requested-With", "XMLHttpRequest");
				request.AddHeader("X-MicrosoftAjax", "Delta=true");
				request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");
				request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
				request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
				request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":3,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
				request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("ToolkitScriptManager1_HiddenField", "");
				request.AddParameter("__ASYNCPOST", "true");
				request.AddParameter("__EVENTARGUMENT", "activeTabChanged:3");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("__EVENTVALIDATION", "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");
				request.AddParameter("__LASTFOCUS", "");
				request.AddParameter("__VIEWSTATE", "/wEPaA8FDzhkODM3OTZlZTY0NjBiMRgDBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WHQVQdXBGdW5jdGlvbiRoX2FkZHN1YmplY3RzJHVwT3JhcmVuZFRlcnZlem8kb3JhcmVuZHRlcnZlem8xJHVwVGltZVRhYmxlJFRpbWVUYWJsZTEFCWJ0bkxhbmdfMAUJYnRuTGFuZ18xBQlidG5MYW5nXzIFC2J0bnNraW5QaW5rBQtidG5za2luQmx1ZQUMYnRuc2tpbkdyZWVuBQ1idG5za2luT3JhbmdlBQ5idG5za2luVGVhY2hlcgUNYnRuc2tpblB1cnBsZQUeaW1nU2tpbkNob29zZXJQYXJ0aWFsbHlTaWdodGVkBR11cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0blJzcwUhdXBCb3hlcyR1cEJveGVzQnV0dG9ucyRidG5NZXNzYWdlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZhdm9yaXRlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkNhbGVuZGFyBR91cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZvcnVtBSZ1cEJveGVzJHVwUlNTJGdkZ1JTUyRnZGdSU1NfUmVmcmVzaEJ0bgUkdXBCb3hlcyR1cFJTUyRnZGdSU1MkZ2RnUlNTX0Nsb3NlQnRuBTJ1cEJveGVzJHVwTWVzc2FnZSRnZGdNZXNzYWdlJGdkZ01lc3NhZ2VfUmVmcmVzaEJ0bgUwdXBCb3hlcyR1cE1lc3NhZ2UkZ2RnTWVzc2FnZSRnZGdNZXNzYWdlX0Nsb3NlQnRuBTZ1cEJveGVzJHVwZmF2b3JpdGVzJGdkZ0Zhdm9yaXRlJGdkZ0Zhdm9yaXRlX1JlZnJlc2hCdG4FNHVwQm94ZXMkdXBmYXZvcml0ZXMkZ2RnRmF2b3JpdGUkZ2RnRmF2b3JpdGVfQ2xvc2VCdG4FNXVwQm94ZXMkdXBDYWxlbmRhciRnZGdDYWxlbmRhciRnZGdDYWxlbmRhcl9SZWZyZXNoQnRuBTN1cEJveGVzJHVwQ2FsZW5kYXIkZ2RnQ2FsZW5kYXIkZ2RnQ2FsZW5kYXJfQ2xvc2VCdG4FOnVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X1JlZnJlc2hCdG4FOHVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X0Nsb3NlQnRuBTp1cEZpbHRlciRXVENob29zZXJGcm9tJGNoa1dUQ2hvb3Nlcl91cEZpbHRlcl9XVENob29zZXJGcm9tBTZ1cEZpbHRlciRXVENob29zZXJUbyRjaGtXVENob29zZXJfdXBGaWx0ZXJfV1RDaG9vc2VyVG8FYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIFYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIPD2RmZAVXdXBCb3hlcyR1cEZvcnVtJGdkZ0ZvcnVtJHVwUGFyZW50JGdhZGdldCRjdGwzNSRWU19GYXZvdXJpdGVUb3BpY3NfZ2FkU21hbGwxJGx2RmF2VG9waWNzDzwrAA4DCGYMZg0C/////w9k/xFeA3s6riIQ9ZMoDYJ7fVkRXJvACO6MW7hxGZ/nUK8=");
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
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", "All");
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFilter$txtKurzuskod", "");
				request.AddParameter("upFilter$txtOktato", "");
				request.AddParameter("upFilter$txtTargyNev", "");
				request.AddParameter("upFilter$txtTargykod", "");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
				response = RestWebClient.Execute(request);
			}
			html.LoadHtml(response.Content);
			string notes = HtmlToXamlConverter.ConvertHtmlToXaml(html.GetElementbyId("Lecturenotes1_gridLectureNotes_bodytable").OuterHtml, false);

			var notessection = (Section)XamlReader.Parse(notes);
			var notesrowgroups = notessection.Blocks.OfType<Table>().ToList()[0].RowGroups.ToList();
			var notesrows = notesrowgroups[0].Rows;
			foreach (var a in notesrows)
			{
				foreach (var b in a.Cells)
				{
					if (Application.Current.Resources["FontSizeLarge"] is double d)
						b.FontSize = d;
					b.BorderThickness = new Thickness(0);
					var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
					inline.FontFamily = Application.Current.Resources["LatoBold"] as FontFamily;
				}
			}
			notesrows = notesrowgroups[1].Rows;
			foreach (var a in notesrows)
			{
				foreach (var b in a.Cells)
				{
					if (Application.Current.Resources["FontSizeRegular"] is double d)
						b.FontSize = d;
					b.BorderThickness = new Thickness(0);
					var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
					inline.FontFamily = Application.Current.Resources["LatoRegular"] as FontFamily;
					inline.FontWeight = FontWeights.Regular;
				}
			}
			NotesXAMLString = XamlWriter.Save(notessection);
		}

		private void LoadStudents()
		{
			#region Load page on server too.. PS: a Neptun egy szar..

			IRestResponse response;
			var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
			var html = new HtmlDocument();
			lock (RestWebClient)
			{
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
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
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
				request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={ParentViewModel.id};level=1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
				request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				request.AddParameter("__VIEWSTATE", ViewStateStr);
				request.AddParameter("hfCountDownTime", "600");
				request.AddParameter("hiddenEditLabel", "");
				request.AddParameter("progressalerttype", "progress");
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				response = RestWebClient.Execute(request);

				#endregion


				request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
				request.AddHeader("Cache-Control", "no-cache");
				request.AddHeader("X-Requested-With", "XMLHttpRequest");
				request.AddHeader("X-MicrosoftAjax", "Delta=true");
				request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");
				request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
				request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
				request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":4,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
				request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("ToolkitScriptManager1_HiddenField", "");
				request.AddParameter("__ASYNCPOST", "true");
				request.AddParameter("__EVENTARGUMENT", "activeTabChanged:4");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("__EVENTVALIDATION", "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");
				request.AddParameter("__LASTFOCUS", "");
				request.AddParameter("__VIEWSTATE", "/wEPaA8FDzhkODM3OTZlZTY0NjBiMRgDBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WHQVQdXBGdW5jdGlvbiRoX2FkZHN1YmplY3RzJHVwT3JhcmVuZFRlcnZlem8kb3JhcmVuZHRlcnZlem8xJHVwVGltZVRhYmxlJFRpbWVUYWJsZTEFCWJ0bkxhbmdfMAUJYnRuTGFuZ18xBQlidG5MYW5nXzIFC2J0bnNraW5QaW5rBQtidG5za2luQmx1ZQUMYnRuc2tpbkdyZWVuBQ1idG5za2luT3JhbmdlBQ5idG5za2luVGVhY2hlcgUNYnRuc2tpblB1cnBsZQUeaW1nU2tpbkNob29zZXJQYXJ0aWFsbHlTaWdodGVkBR11cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0blJzcwUhdXBCb3hlcyR1cEJveGVzQnV0dG9ucyRidG5NZXNzYWdlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZhdm9yaXRlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkNhbGVuZGFyBR91cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZvcnVtBSZ1cEJveGVzJHVwUlNTJGdkZ1JTUyRnZGdSU1NfUmVmcmVzaEJ0bgUkdXBCb3hlcyR1cFJTUyRnZGdSU1MkZ2RnUlNTX0Nsb3NlQnRuBTJ1cEJveGVzJHVwTWVzc2FnZSRnZGdNZXNzYWdlJGdkZ01lc3NhZ2VfUmVmcmVzaEJ0bgUwdXBCb3hlcyR1cE1lc3NhZ2UkZ2RnTWVzc2FnZSRnZGdNZXNzYWdlX0Nsb3NlQnRuBTZ1cEJveGVzJHVwZmF2b3JpdGVzJGdkZ0Zhdm9yaXRlJGdkZ0Zhdm9yaXRlX1JlZnJlc2hCdG4FNHVwQm94ZXMkdXBmYXZvcml0ZXMkZ2RnRmF2b3JpdGUkZ2RnRmF2b3JpdGVfQ2xvc2VCdG4FNXVwQm94ZXMkdXBDYWxlbmRhciRnZGdDYWxlbmRhciRnZGdDYWxlbmRhcl9SZWZyZXNoQnRuBTN1cEJveGVzJHVwQ2FsZW5kYXIkZ2RnQ2FsZW5kYXIkZ2RnQ2FsZW5kYXJfQ2xvc2VCdG4FOnVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X1JlZnJlc2hCdG4FOHVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X0Nsb3NlQnRuBTp1cEZpbHRlciRXVENob29zZXJGcm9tJGNoa1dUQ2hvb3Nlcl91cEZpbHRlcl9XVENob29zZXJGcm9tBTZ1cEZpbHRlciRXVENob29zZXJUbyRjaGtXVENob29zZXJfdXBGaWx0ZXJfV1RDaG9vc2VyVG8FYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIFYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIPD2RmZAVXdXBCb3hlcyR1cEZvcnVtJGdkZ0ZvcnVtJHVwUGFyZW50JGdhZGdldCRjdGwzNSRWU19GYXZvdXJpdGVUb3BpY3NfZ2FkU21hbGwxJGx2RmF2VG9waWNzDzwrAA4DCGYMZg0C/////w9k/xFeA3s6riIQ9ZMoDYJ7fVkRXJvACO6MW7hxGZ/nUK8=");
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
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", "All");
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFilter$txtKurzuskod", "");
				request.AddParameter("upFilter$txtOktato", "");
				request.AddParameter("upFilter$txtTargyNev", "");
				request.AddParameter("upFilter$txtTargykod", "");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");
				response = RestWebClient.Execute(request);
				request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "HandleRequest.ashx?RequestType=GetData&GridID=Students_on_subject1_gridStudents&pageindex=1&pagesize=10000&sort1=&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);
				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");

				response = RestWebClient.Execute(request);
			}


			html = new HtmlDocument();
			html.LoadHtml(response.Content);
			string notes = HtmlToXamlConverter.ConvertHtmlToXaml(html.GetElementbyId("Students_on_subject1_gridStudents_bodytable").OuterHtml, false);
			var count = html.GetElementbyId("Students_on_subject1_gridStudents_tablebottom").ChildNodes[0].ChildNodes[1].InnerText;
			//ThemeXAMLString = theme;
			count = count.Remove(count.LastIndexOf('('));
			var p = new Paragraph(new Run(count));
			if (Application.Current.Resources["FontSizeRegular"] is double fonts)
				p.FontSize = fonts;
			var notessection = (Section)XamlReader.Parse(notes);
			var notesrowgroups = notessection.Blocks.OfType<Table>().ToList()[0].RowGroups.ToList();
			notessection.Blocks.Add(p);
			var notesrows = notesrowgroups[0].Rows;
			foreach (var a in notesrows)
			{
				foreach (var b in a.Cells)
				{
					if (Application.Current.Resources["FontSizeLarge"] is double d)
						b.FontSize = d;
					b.BorderThickness = new Thickness(0);
					var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
					inline.FontFamily = Application.Current.Resources["LatoBold"] as FontFamily;
				}
			}

			notesrows = notesrowgroups[1].Rows;

			foreach (var a in notesrows)
			{
				foreach (var b in a.Cells)
				{
					if (Application.Current.Resources["FontSizeRegular"] is double d)
						b.FontSize = d;
					b.BorderThickness = new Thickness(0);
					var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
					inline.FontFamily = Application.Current.Resources["LatoRegular"] as FontFamily;
					inline.FontWeight = FontWeights.Regular;
				}
			}
			StudentsXAMLString = XamlWriter.Save(notessection);
		}

		private void LoadCourses()
		{
			Application.Current.Dispatcher.Invoke(() => Courses = new ObservableCollection<Course>());
			var html = new HtmlDocument();
			IRestResponse response;
			lock (RestWebClient)
			{
				#region Load page on server too.. PS: a Neptun egy szar..

				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
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
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
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
				request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={ParentViewModel.id};level=1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
				request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				request.AddParameter("__VIEWSTATE", ViewStateStr);
				request.AddParameter("hfCountDownTime", "600");
				request.AddParameter("hiddenEditLabel", "");
				request.AddParameter("progressalerttype", "progress");
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				response = RestWebClient.Execute(request);

				#endregion

				request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
				request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
				response = RestWebClient.Execute(request);

				html.LoadHtml(response.Content);
				ViewStateStr = html.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
				EventValidateStr = html.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

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
				request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={ParentViewModel.id};level=1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
				request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				request.AddParameter("__VIEWSTATE", ViewStateStr);
				request.AddParameter("hfCountDownTime", "600");
				request.AddParameter("hiddenEditLabel", "");
				request.AddParameter("progressalerttype", "progress");
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", ParentViewModel.SubjectType == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				response = RestWebClient.Execute(request);
			}
			html.LoadHtml(response.Content);
			var list = html.GetElementbyId("Addsubject_course1_gridCourses_bodytable").ChildNodes.FindFirst("tbody");
			foreach (var a in list.ChildNodes)
			{
				try
				{

					var course = new Course();
					var b = a.ChildNodes;
					course.SubjectType = ParentViewModel.SubjectType;
					course.CourseCode = b[1].ChildNodes[0].InnerText;
					var tmpasd = b[1].ChildNodes[0].GetAttributeValue("onclick", "");
					course.ID = String.IsNullOrWhiteSpace(tmpasd) ? "Imnotsure" : tmpasd.Split('(')[1].Split(',')[0].Replace("\'", "");
					course.ToolTip = b[1].ChildNodes.Count == 2 ? b[1].ChildNodes[1].InnerText : null;
					course.Type = b[2].ChildNodes[0].InnerText;
					course.GroupName = b[2].ChildNodes[0].InnerText + ParentViewModel.id;
					course.Limits = b[3].ChildNodes[0].InnerText;
					course.rangsor = b[6].GetDirectInnerText();
					course.Schedule = b[7].ChildNodes.Count == 2 ? b[7].ChildNodes[0].GetDirectInnerText() : String.Empty;
					course.isEnabled = !b[1].HasClass("link_disabled");
					//Debugger.Break();
					//TODO: órarendi infó, rohadt megjegyzés..
					course.Teacher = b[8].ChildNodes[0].InnerText;
					course.Language = b[9].GetDirectInnerText();
					course.Note = b[11].GetDirectInnerText();
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
			Task.Run(() =>
			{
				var courseschedulelist = new List<KeyValuePair<string, string>>();
				var client = new RestClient("http://gabeee.web.elte.hu/to_remake/");
				client.Timeout = -1;
				var request_to = new RestRequest(Method.GET);
				request_to.AddHeader("Upgrade-Insecure-Requests", "1");
				client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
				request_to.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
				IRestResponse response_to = client.Execute(request_to);
				request_to = new RestRequest("http://gabeee.web.elte.hu/to_remake/save.php", Method.POST);
				request_to.AddHeader("Accept", "*/*");
				request_to.AddHeader("X-Requested-With", "XMLHttpRequest");
				client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
				request_to.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request_to.AddParameter("width", "2240.02");
				response_to = client.Execute(request_to);

				request_to = new RestRequest("http://gabeee.web.elte.hu/to_remake/data.php", Method.POST);
				request_to.AddHeader("Accept", "*/*");
				request_to.AddHeader("X-Requested-With", "XMLHttpRequest");
				client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
				request_to.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request_to.AddParameter("felev", "2020-2021-1");
				request_to.AddParameter("limit", "1000");
				request_to.AddParameter("melyik", "kodalapjan");
				request_to.AddParameter("nar", "0");
				request_to.AddParameter("targykod", ParentViewModel.Code);
				request_to.AddParameter("width", "2240.02");
				response_to = client.Execute(request_to);
				var tohtml = new HtmlDocument();
				tohtml.LoadHtml(response_to.Content);
				var asdasd = tohtml.GetElementbyId("collapse1").ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes.Where(s => s.Name == "tr");
				foreach (var info in asdasd)
					courseschedulelist.Add(new KeyValuePair<string, string>(info.ChildNodes[1].InnerText, info.ChildNodes[8].InnerText));

				foreach (var course in Courses)
					if (String.IsNullOrEmpty(course.Schedule))
						course.Schedule = courseschedulelist.Find(s => s.Key == course.CourseCode).Value;
			});

		}

		private void LoadElokovetelmeny()
		{
			//subject_requirement_gridSubjectPre_bodytable
			// TODO Hard
			#region Load page on server too.. PS: a Neptun egy szar..
			IRestResponse response;
			var html = new HtmlDocument();
			var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
			lock (RestWebClient)
			{
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
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
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
				request.AddParameter("__EVENTARGUMENT", $"commandname=subjectdata;commandsource=select;id={ParentViewModel.id};level=1");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upGrid$gridSubjects");
				request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				request.AddParameter("__VIEWSTATE", ViewStateStr);
				request.AddParameter("hfCountDownTime", "600");
				request.AddParameter("hiddenEditLabel", "");
				request.AddParameter("progressalerttype", "progress");
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				response = RestWebClient.Execute(request);

				#endregion

				request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
				request.AddHeader("Cache-Control", "no-cache");
				request.AddHeader("X-Requested-With", "XMLHttpRequest");
				request.AddHeader("X-MicrosoftAjax", "Delta=true");
				request.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
				request.AddHeader("Accept", "*/*");
				request.AddHeader("Sec-Fetch-Site", "same-origin");
				request.AddHeader("Sec-Fetch-Mode", "cors");
				request.AddHeader("Sec-Fetch-Dest", "empty");
				request.AddParameter("ActiveModalBehaviourID", "behaviorupFunction_h_addsubjects_upModal_modal_subjectdata");
				request.AddParameter("NoMatchString", "A listában nincs ilyen elem!");
				request.AddParameter("Subject_data_for_schedule_tab_ClientState", "{\"ActiveTabIndex\":5,\"TabEnabledState\":[true,true,true,true,true,true],\"TabWasLoadedOnceState\":[true,false,false,false,false,false]}");
				request.AddParameter("ToolkitScriptManager1", "ToolkitScriptManager1|upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("ToolkitScriptManager1_HiddenField", "");
				request.AddParameter("__ASYNCPOST", "true");
				request.AddParameter("__EVENTARGUMENT", "activeTabChanged:5");
				request.AddParameter("__EVENTTARGET", "upFunction$h_addsubjects$upModal$upmodal_subjectdata$ctl02$Subject_data_for_schedule$upParent$tab");
				request.AddParameter("__EVENTVALIDATION", "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");
				request.AddParameter("__LASTFOCUS", "");
				request.AddParameter("__VIEWSTATE", "/wEPaA8FDzhkODM3OTZlZTY0NjBiMRgDBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WHQVQdXBGdW5jdGlvbiRoX2FkZHN1YmplY3RzJHVwT3JhcmVuZFRlcnZlem8kb3JhcmVuZHRlcnZlem8xJHVwVGltZVRhYmxlJFRpbWVUYWJsZTEFCWJ0bkxhbmdfMAUJYnRuTGFuZ18xBQlidG5MYW5nXzIFC2J0bnNraW5QaW5rBQtidG5za2luQmx1ZQUMYnRuc2tpbkdyZWVuBQ1idG5za2luT3JhbmdlBQ5idG5za2luVGVhY2hlcgUNYnRuc2tpblB1cnBsZQUeaW1nU2tpbkNob29zZXJQYXJ0aWFsbHlTaWdodGVkBR11cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0blJzcwUhdXBCb3hlcyR1cEJveGVzQnV0dG9ucyRidG5NZXNzYWdlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZhdm9yaXRlBSJ1cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkNhbGVuZGFyBR91cEJveGVzJHVwQm94ZXNCdXR0b25zJGJ0bkZvcnVtBSZ1cEJveGVzJHVwUlNTJGdkZ1JTUyRnZGdSU1NfUmVmcmVzaEJ0bgUkdXBCb3hlcyR1cFJTUyRnZGdSU1MkZ2RnUlNTX0Nsb3NlQnRuBTJ1cEJveGVzJHVwTWVzc2FnZSRnZGdNZXNzYWdlJGdkZ01lc3NhZ2VfUmVmcmVzaEJ0bgUwdXBCb3hlcyR1cE1lc3NhZ2UkZ2RnTWVzc2FnZSRnZGdNZXNzYWdlX0Nsb3NlQnRuBTZ1cEJveGVzJHVwZmF2b3JpdGVzJGdkZ0Zhdm9yaXRlJGdkZ0Zhdm9yaXRlX1JlZnJlc2hCdG4FNHVwQm94ZXMkdXBmYXZvcml0ZXMkZ2RnRmF2b3JpdGUkZ2RnRmF2b3JpdGVfQ2xvc2VCdG4FNXVwQm94ZXMkdXBDYWxlbmRhciRnZGdDYWxlbmRhciRnZGdDYWxlbmRhcl9SZWZyZXNoQnRuBTN1cEJveGVzJHVwQ2FsZW5kYXIkZ2RnQ2FsZW5kYXIkZ2RnQ2FsZW5kYXJfQ2xvc2VCdG4FOnVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X1JlZnJlc2hCdG4FOHVwQm94ZXMkdXBGb3J1bSRnZGdGb3J1bSR1cFBhcmVudCRnYWRnZXQkZ2FkZ2V0X0Nsb3NlQnRuBTp1cEZpbHRlciRXVENob29zZXJGcm9tJGNoa1dUQ2hvb3Nlcl91cEZpbHRlcl9XVENob29zZXJGcm9tBTZ1cEZpbHRlciRXVENob29zZXJUbyRjaGtXVENob29zZXJfdXBGaWx0ZXJfV1RDaG9vc2VyVG8FYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIFYXVwRnVuY3Rpb24kaF9hZGRzdWJqZWN0cyR1cE1vZGFsJHVwbW9kYWxfc3ViamVjdGRhdGEkY3RsMDIkU3ViamVjdF9kYXRhX2Zvcl9zY2hlZHVsZSR1cFBhcmVudCR0YWIPD2RmZAVXdXBCb3hlcyR1cEZvcnVtJGdkZ0ZvcnVtJHVwUGFyZW50JGdhZGdldCRjdGwzNSRWU19GYXZvdXJpdGVUb3BpY3NfZ2FkU21hbGwxJGx2RmF2VG9waWNzDzwrAA4DCGYMZg0C/////w9k/xFeA3s6riIQ9ZMoDYJ7fVkRXJvACO6MW7hxGZ/nUK8=");
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
				request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
				request.AddParameter("upFilter$cmbSubjectGroups", "All");
				request.AddParameter("upFilter$cmbTemplates", "All");
				request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
				request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
				request.AddParameter("upFilter$txtKurzuskod", "");
				request.AddParameter("upFilter$txtOktato", "");
				request.AddParameter("upFilter$txtTargyNev", "");
				request.AddParameter("upFilter$txtTargykod", "");
				request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
				request.AddParameter("upFunction$h_addsubjects$upModal$upmodal_subjectdata$_data", "Visible:true");

				response = RestWebClient.Execute(request);
			}

			html.LoadHtml(response.Content);
			var tableheader = html.GetElementbyId("subject_requirement_gridSubjectPre_bodytable").ChildNodes[0].InnerHtml; //header
			tableheader = tableheader.Insert(0, "<table>");
			tableheader += "</table>";
			var asd2 = html.GetElementbyId("subject_requirement_gridSubjectPre_bodytable").ChildNodes[1]; //Body
			for (int i = asd2.ChildNodes.Count - 1; i >= 0; --i)
			{
				if (i % 2 == 0)
					asd2.ChildNodes.RemoveAt(i);
			}
			var damn = asd2.InnerHtml;
			//asd2 = asd2.Insert(0, "<table>");
			//asd2 += "</table>";
			//var damn = HtmlToXamlConverter.ConvertHtmlToXaml(asd2,false);
			string elokovetelmenystr = HtmlToXamlConverter.ConvertHtmlToXaml(html.GetElementbyId("subject_requirement_gridSubjectPre_bodytable").OuterHtml, false);

			// TODO Fix this format to display better..
			var elokovetelmenysection = (Section)XamlReader.Parse(HtmlToXamlConverter.ConvertHtmlToXaml(tableheader, false));
			var elokovetelmenyrowgroups = elokovetelmenysection.Blocks.OfType<Table>().ToList()[0].RowGroups.ToList();
			foreach (var a in elokovetelmenyrowgroups[0].Rows)
			{
				foreach (var b in a.Cells)
				{
					if (Application.Current.Resources["FontSizeLarge"] is double d)
						b.FontSize = d;
					b.BorderThickness = new Thickness(0);
					var inline = (b.Blocks.ToList()[0] as Paragraph).Inlines.FirstInline;
					inline.FontFamily = Application.Current.Resources["LatoBold"] as FontFamily;
				}
			}
			ElokovetelmenyXAMLString = XamlWriter.Save(elokovetelmenysection);
			//ElokovetelmenyXAMLString = damn;
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
					#region Requests setup

					tmp = RestWebClient.Execute(request).Content;
					var tmphtml = new HtmlDocument();
					tmphtml.LoadHtml(tmp);
					string ViewStateStr = tmphtml.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
					string EventValidateStr = tmphtml.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");


					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
					request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					request.AddParameter("__VIEWSTATE", ViewStateStr);
					//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
					request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
					request.AddParameter("upFilter$txtKurzuskod", "");
					request.AddParameter("upFilter$txtOktato", "");
					request.AddParameter("upFilter$txtTargyNev", "");
					request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
					request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
					request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
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
					request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
					request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
					request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
					request.AddParameter("upFilter$rbtnSubjectType", type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
					request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");

					#endregion

					response = RestWebClient.Execute(request);
					html.LoadHtml(response.Content);

					if (loadmenu)
					{
						#region Menu
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
						MenuItems[0].isEnabled = false;

						#endregion

						#region Courses

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
								course.ToolTip = b[1].ChildNodes.Count == 2 ? b[1].ChildNodes[1].InnerText : null;
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
								course.Note = b[11].GetDirectInnerText();
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

						#region TO órarend
						Task.Run(() =>
				{
					try
					{
						var courseschedulelist = new List<KeyValuePair<string, string>>();
						var client = new RestClient("http://gabeee.web.elte.hu/to_remake/");
						client.Timeout = -1;
						var request_to = new RestRequest(Method.GET);
						request_to.AddHeader("Upgrade-Insecure-Requests", "1");
						client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
						request_to.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
						IRestResponse response_to = client.Execute(request_to);
						request_to = new RestRequest("http://gabeee.web.elte.hu/to_remake/save.php", Method.POST);
						request_to.AddHeader("Accept", "*/*");
						request_to.AddHeader("X-Requested-With", "XMLHttpRequest");
						client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
						request_to.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
						request_to.AddParameter("width", "2240.02");
						response_to = client.Execute(request_to);

						request_to = new RestRequest("http://gabeee.web.elte.hu/to_remake/data.php", Method.POST);
						request_to.AddHeader("Accept", "*/*");
						request_to.AddHeader("X-Requested-With", "XMLHttpRequest");
						client.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.105 Safari/537.36";
						request_to.AddHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
						request_to.AddParameter("felev", "2020-2021-1");
						request_to.AddParameter("limit", "1000");
						request_to.AddParameter("melyik", "kodalapjan");
						request_to.AddParameter("nar", "0");
						request_to.AddParameter("targykod", ParentViewModel.Code);
						request_to.AddParameter("width", "2240.02");
						response_to = client.Execute(request_to);
						var tohtml = new HtmlDocument();
						tohtml.LoadHtml(response_to.Content);
						var asdasd = tohtml.GetElementbyId("collapse1").ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes.Where(s => s.Name == "tr");
						foreach (var info in asdasd)
							courseschedulelist.Add(new KeyValuePair<string, string>(info.ChildNodes[1].InnerText, (info.ChildNodes[7].InnerText + Environment.NewLine + info.ChildNodes[8].InnerText)));

						foreach (var course in Courses)
							if (String.IsNullOrEmpty(course.Schedule))
								course.Schedule = courseschedulelist.Find(s => s.Key == course.CourseCode).Value;
					}
					catch (Exception e)
					{
						Logger.LogErrorSource(e.Message);
						Debugger.Break();
					}
				});
						#endregion

						#endregion

					}
				}
			});

		}

		#endregion

		#region Constructors
		public TakeSubjectViewModel()
		{
			MenuItems = new ObservableCollection<TFMenuItemViewModel>();
			Courses = new ObservableCollection<Course>();
		}

		public TakeSubjectViewModel(string id, SubjectViewModel viewmodel, TFViewModel tFView, bool loadmenu = true)
		{
			//Logger.LogErrorSource(viewmodel.Code);
			//Debugger.Break();
			#region Initialization and page navigation

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
							Task.Run(LoadDetails);
							break;
						}
					case "Felvehető kurzusok":
						{
							currentPage = TFPage.Felvétel;
							Task.Run(LoadCourses);
							break;
						}
					case "Témakör":
						{
							currentPage = TFPage.Téma;
							Task.Run(LoadTheme);
							break;
						}
					case "Jegyzetek":
						{
							currentPage = TFPage.Notes;
							Task.Run(LoadNotes);
							break;
						}
					case "Hallgatók":
						{
							currentPage = TFPage.Hallgatók;
							Task.Run(LoadStudents);
							break;
						}
					case "Táblázatos előkövetelmény":
						{
							currentPage = TFPage.Előkövetelmény;
							Task.Run(LoadElokovetelmeny);
							break;
						}
					default:
						{
							currentPage = TFPage.Default;
							break;
						}
				}
			});

			#endregion

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

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
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
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
						request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
						response = RestWebClient.Execute(request);

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type.ToString());
						response = RestWebClient.Execute(request);
						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						response = RestWebClient.Execute(request);

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
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
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
					string test = String.Empty;
					Task<IRestResponse> handle;
					var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);
					var html = new HtmlDocument();
					//System.Diagnostics.Stopwatch watch = new Stopwatch();
					lock (RestWebClient)
					{
						//watch.Start();
						//Logger.LogErrorSource($"{ParentViewModel.Code} : Entered locking block at {DateTime.Now.Millisecond}");
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
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
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
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex].Value);
						request.AddParameter("upFilter$rbtnSubjectType", TFViewModel.type == TFViewModel.SubjectType.Mintatantervi ? "Mintatantervi" : "MindenIntezmenyi");
						request.AddParameter("upFunction$h_addsubjects$upFilter$searchpanel$searchpanel_state", "expanded");
						response = RestWebClient.Execute(request);

						request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
						request.AddParameter("__EVENTVALIDATION", EventValidateStr);
						request.AddParameter("__VIEWSTATE", ViewStateStr);
						//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
						request.AddParameter("upFilter$cmbLanguage", TFViewModel.Languages[TFViewModel.SelectedLanguageIndex].Value);
						request.AddParameter("upFilter$cmbSubjectGroups", TFViewModel.SelectedSubjectGroup.value);
						request.AddParameter("upFilter$txtKurzuskod", "");
						request.AddParameter("upFilter$txtOktato", "");
						request.AddParameter("upFilter$txtTargyNev", "");
						request.AddParameter("upFilter$txtTargykod", ParentViewModel.Code);
						request.AddParameter("upFilter$cmbTemplates", TFViewModel.SelectedMintatanterv.value);
						request.AddParameter("upFilter$cmbTerms", TFViewModel.Semesters[TFViewModel.SelectedSemesterIndex > 0 ? TFViewModel.SelectedSemesterIndex : 0].Value);
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
						handle = RestWebClient.ExecuteAsync(request);
					}
					handle.Wait();
					response = handle.Result;
					//watch.Stop();
					//Logger.LogErrorSource($"{ParentViewModel.Code} : Left locking: {watch.ElapsedMilliseconds} at {DateTime.Now.Millisecond}");
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
					//ParentViewModel.InfoExpanded = false;
					//}
				});
			});
			#endregion

			LoadCourseData(id, ParentViewModel.SubjectType, loadmenu);
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
		public bool DefaultPage { get => currentPage == TFPage.Default; }


		public string TempXAMLSTring { get; set; }
		public string TempXAMLSTring2 { get; set; }
		public string ThemeXAMLString { get; set; }
		public string NotesXAMLString { get; set; }
		public string ElokovetelmenyXAMLString { get; set; }
		public string StudentsXAMLString { get; set; }
		public TFPage currentPage { get; set; } = TFPage.Felvétel;
		#endregion

		#region Public Commands

		public ICommand switchpage { get; set; }

		public ICommand TakeSubject { get; set; }

		public ICommand ForgetSubject { get; set; }

		#endregion
	}
}
