using Dna;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Neptun.Core;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using static Neptun.DI;

namespace Neptun
{
	public class TakenSubjectsPageViewModel : BaseViewModel
	{
		#region Nested Classes
		public class TakenSubjectViewModel : BaseViewModel
		{
			public string code { get; set; }

			public string name { get; set; }

			public string credit { get; set; }

			public string count { get; set; }

			public bool varolista { get; set; }
		}

		#endregion

		#region Private Methods

		public void LoadSubjects()
		{
			try
			{

				Subjects = new ObservableCollection<TakenSubjectViewModel>();
				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0304", Method.GET);
				Task<IRestResponse> handle;
				var html = new HtmlDocument();
				lock (RestWebClient)
				{
					var tmp = RestWebClient.Execute(request).Content;
					string ViewStateStr, EventValidateStr;
					html.LoadHtml(tmp);
					ViewStateStr = html.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
					EventValidateStr = html.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0304", Method.POST);
					request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					request.AddParameter("__VIEWSTATE", ViewStateStr);
					request.AddParameter("upFilter$expandedsearchbutton", "Listázás");
					request.AddParameter("upFilter$cmb$m_cmb", SelectedSemester.Value);
					handle = RestWebClient.ExecuteAsync(request);
				}
				handle.Wait();
				var response = handle.Result;
				html.LoadHtml(response.Content);
				var table = html.GetElementbyId("h_addedsubjects_gridAddedSubjects_bodytable").ChildNodes[1].ChildNodes;
				foreach (var a in table)
				{
					try
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							Subjects.Add(new TakenSubjectViewModel()
							{
								code = a.ChildNodes[1].InnerText,
								name = a.ChildNodes[2].InnerText,
								credit = a.ChildNodes[3].InnerText,
								count = a.ChildNodes[4].InnerText,
								varolista = a.ChildNodes[5].ChildNodes.Count > 0,
							});
						});
					}
					catch (Exception e)
					{
						Logger.LogErrorSource(e.Message);
						Debugger.Break();
					}

				}
				Debugger.Break();
			}
			catch (Exception e)
			{
				Logger.LogErrorSource(e.Message + Environment.NewLine + "Retrying..");
				Debugger.Break();
				LoadSubjects();
			}
		}

		#endregion

		#region Constructor
		public TakenSubjectsPageViewModel()
		{
			Semesters = new ObservableCollection<TFViewModel.SemesterViewModel>();

			#region Semesters
			Task.Run(() =>
			{
				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0304", Method.GET);

				var tmp = RestWebClient.Execute(request).Content;
				string ViewStateStr, EventValidateStr;
				var tmphtml = new HtmlDocument();
				tmphtml.LoadHtml(tmp);
				ViewStateStr = tmphtml.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
				EventValidateStr = tmphtml.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

				if (Semesters.Count() == 0)
				{

					var asd = tmphtml.GetElementbyId("cmb_cmb").ChildNodes.Where(s => s.Name == "option");

					foreach (var a in asd)
					{
						if (a.GetAttributeValue("value", "") != "-1")
							Application.Current.Dispatcher.Invoke(() =>
							{
								Semesters.Add(new TFViewModel.SemesterViewModel()
								{
									Value = a.GetAttributeValue("value", ""),
									Semester = a.InnerText
								});
							});

					}
				}
			});
			#endregion
			Task.Run(LoadSubjects);
		}
		#endregion

		#region Public Properties

		public ObservableCollection<TFViewModel.SemesterViewModel> Semesters { get; set; }

		public ObservableCollection<TakenSubjectViewModel> Subjects { get; set; }

		public TFViewModel.SemesterViewModel SelectedSemester { get; set; }
		#endregion
	}
}
