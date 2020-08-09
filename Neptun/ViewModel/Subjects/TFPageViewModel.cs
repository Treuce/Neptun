﻿using Dna;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static Dna.FrameworkDI;
using static Neptun.Core.CoreDI;
using static Neptun.DI;

namespace Neptun
{
	public class TFViewModel : BaseViewModel
	{
		#region Private Members

		private int SubjectCount = 0;

		#endregion

		#region Private Methods

		private async void LoadSubjects()
		{
			Subjects = new ObservableCollection<SubjectViewModel>();

			//await Task.Run(async () =>
			//{
			var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.GET);

			var tmp = RestWebClient.Execute(request).Content;
			string ViewStateStr, EventValidateStr;
			{
				var tmphtml = new HtmlDocument();
				tmphtml.LoadHtml(tmp);
				ViewStateStr = tmphtml.GetElementbyId("__VIEWSTATE").GetAttributeValue("value", "");
				EventValidateStr = tmphtml.GetElementbyId("__EVENTVALIDATION").GetAttributeValue("value", "");

				if (Languages.Count() == 0)
				{
					var asd = tmphtml.GetElementbyId("upFilter_cmbLanguage").ChildNodes.Where(s => s.Name == "option");
					foreach (var a in asd)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							Languages.Add(new LanguageViewModel()
							{
								Value = a.GetAttributeValue("value", ""),
								Language = a.InnerText
							});
						});
					}
				}
				if (Semesters.Count() == 0)
				{

					var asd = tmphtml.GetElementbyId("upFilter_cmbTerms").ChildNodes.Where(s => s.Name == "option");

					foreach (var a in asd)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							Semesters.Add(new SemesterViewModel()
							{
								Value = a.GetAttributeValue("value", ""),
								Semester = a.InnerText
							});
						}
						);

					}
				}
			}

			if (type == SubjectType.Mintatantervi)
			{


				var SubjectCodeFilters = SubjectCode.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				for (int i = 0; i < SubjectCodeFilters.Count; ++i)
				{
					SubjectCodeFilters[i] = SubjectCodeFilters[i].Trim().ToLower();
				}
				SubjectCodeFilters = SubjectCodeFilters.Distinct().ToList();
				var html = new HtmlDocument();
				if (SubjectCodeFilters.Count == 0)
					SubjectCodeFilters.Add("");
				foreach (var subjectcode in SubjectCodeFilters)
				{
					//if (SubjectCodeFilters.Count > 1 && subjectcode != "")
					//{

					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
					request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					request.AddParameter("__VIEWSTATE", ViewStateStr);
					//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
					request.AddParameter("upFilter$cmbLanguage", Languages[SelectedLanguageIndex].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", "All");
					request.AddParameter("upFilter$txtKurzuskod", CourseCode);
					request.AddParameter("upFilter$txtOktato", Teacher);
					request.AddParameter("upFilter$txtTargyNev", SubjectName);
					request.AddParameter("upFilter$txtTargykod", subjectcode);
					request.AddParameter("upFilter$cmbTemplates", "883053304");
					request.AddParameter("upFilter$cmbTerms", Semesters[SelectedSemesterIndex > 0 ? SelectedSemesterIndex : 0].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", "All");
					request.AddParameter("upFilter$cmbTemplates", "883053304");
					request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
					request.AddParameter("upFilter$rbtnSubjectType", type.ToString());

					IRestResponse response;
					lock (RestWebClient)
					{
						response = RestWebClient.Execute(request);
					}

					html.LoadHtml(response.Content);
					var AllSubjectsCounter = string.Join(String.Empty, html.GetElementbyId("h_addsubjects_gridSubjects_tablebottom").ChildNodes[0].InnerText.Split('/')[1].TakeWhile(s => Char.IsDigit(s)));
					Int32.TryParse(AllSubjectsCounter, out int test);
					SubjectCount = test;
					OnPropertyChanged("MaxPageNumber");

					var subjectscontainer = html.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
					foreach (var a in subjectscontainer)
					{
						try
						{
							if (a.InnerText == "Nincs találat")
							{
								await UI.ShowMessage(new MessageBoxDialogViewModel()
								{
									Title = "Tárgyak listázása",
									Message = $"Nincs találat : {subjectcode}",
									OkText = "Bigsad"
								});
								break;
							}
							var name = a.ChildNodes[1].InnerText;
							var code = a.ChildNodes[2].InnerText;
							var category = a.ChildNodes[3].InnerText;
							var ajanlottfelev = a.ChildNodes[5].InnerText;
							var kredit = a.ChildNodes[6].InnerText;
							var kotelezoe = a.ChildNodes[7].InnerText;
							var cmd = a.ChildNodes[13].ChildNodes[0].GetAttributeValue("onclick", "");
							var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
							var completed = Boolean.Parse(a.ChildNodes[11].GetAttributeValue("checked", ""));
							var taken = Boolean.Parse(a.ChildNodes[12].GetAttributeValue("checked", ""));
							//Debugger.Break();
							Application.Current.Dispatcher.Invoke(() =>
							Subjects.Add(new SubjectViewModel()
							{
								Name = name,
								Code = code,
								Credit = kredit,
								Category = category,
								onClick = cmd,
								id = id,
								completed = completed,
								taken = taken,
								type = kotelezoe,
								SubjectType = SubjectType.Mintatantervi
							}));
						}
						catch (Exception e)
						{
							Logger.LogErrorSource(e.Message);
							//Debugger.Break();
						}
					}
					//}
				}


				#region Oldcode
				//request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
				//request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				//request.AddParameter("__VIEWSTATE", ViewStateStr);
				////request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
				//request.AddParameter("upFilter$cmbLanguage", Languages[SelectedLanguageIndex].Value);
				//request.AddParameter("upFilter$cmbSubjectGroups", "All");
				//request.AddParameter("upFilter$txtKurzuskod", CourseCode);
				//request.AddParameter("upFilter$txtOktato", Teacher);
				//request.AddParameter("upFilter$txtTargyNev", SubjectName);
				//request.AddParameter("upFilter$cmbTemplates", "883053304");
				//request.AddParameter("upFilter$txtTargykod", SubjectCode);
				//request.AddParameter("upFilter$cmbTerms", Semesters[SelectedSemesterIndex > 0 ? SelectedSemesterIndex : 0].Value);
				//request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
				//request.AddParameter("upFilter$rbtnSubjectType", type.ToString());

				//IRestResponse response;
				//lock (RestWebClient)
				//{
				//	response = RestWebClient.Execute(request);
				//}
				//var html = new HtmlDocument();
				//html.LoadHtml(response.Content);
				//var subjectscontainer = html.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
				//var test = html.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].GetDirectInnerText();
				//foreach (var a in subjectscontainer)
				//{
				//	try
				//	{
				//		if (a.InnerText == "Nincs találat")
				//		{
				//			await UI.ShowMessage(new MessageBoxDialogViewModel()
				//			{
				//				Title = "Tárgyak listázása",
				//				Message = $"Nincs találat" + CourseCode,
				//				OkText = "Bigsad"
				//			});
				//			break;
				//		}
				//		var name = a.ChildNodes[1].InnerText;
				//		var code = a.ChildNodes[2].InnerText;
				//		var category = a.ChildNodes[3].InnerText;
				//		var ajanlottfelev = a.ChildNodes[5].InnerText;
				//		var kredit = a.ChildNodes[6].InnerText;
				//		var kotelezoe = a.ChildNodes[7].InnerText;
				//		var cmd = a.ChildNodes[13].ChildNodes[0].GetAttributeValue("onclick", "");
				//		var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
				//		var completed = Boolean.Parse(a.ChildNodes[11].GetAttributeValue("checked", ""));
				//		var taken = Boolean.Parse(a.ChildNodes[12].GetAttributeValue("checked", ""));
				//		//Debugger.Break();
				//		Application.Current.Dispatcher.Invoke(() =>
				//		Subjects.Add(new SubjectViewModel()
				//		{
				//			Name = name,
				//			Code = code,
				//			Credit = kredit,
				//			Category = category,
				//			onClick = cmd,
				//			id = id,
				//			completed = completed,
				//			taken = taken,
				//			type = kotelezoe,
				//			SubjectType = SubjectType.Mintatantervi
				//		}));
				//	}
				//	catch (Exception e)
				//	{
				//		Logger.LogErrorSource(e.Message);
				//		Debugger.Break();
				//	}
				//} 
				#endregion
			}

			else
			{
				var SubjectCodeFilters = SubjectCode.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				for (int i = 0; i < SubjectCodeFilters.Count; ++i)
				{
					SubjectCodeFilters[i] = SubjectCodeFilters[i].Trim().ToLower();
				}
				SubjectCodeFilters = SubjectCodeFilters.Distinct().ToList();
				var html = new HtmlDocument();
				if (SubjectCodeFilters.Count == 0)
					SubjectCodeFilters.Add("");
				foreach (var subjectcode in SubjectCodeFilters)
				{
					//if (SubjectCodeFilters.Count > 1 && subjectcode != "")
					//{

					request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
					request.AddParameter("__EVENTVALIDATION", EventValidateStr);
					request.AddParameter("__VIEWSTATE", ViewStateStr);
					//request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
					request.AddParameter("upFilter$cmbLanguage", Languages[SelectedLanguageIndex].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", "All");
					request.AddParameter("upFilter$txtKurzuskod", CourseCode);
					request.AddParameter("upFilter$txtOktato", Teacher);
					request.AddParameter("upFilter$txtTargyNev", SubjectName);
					request.AddParameter("upFilter$txtTargykod", subjectcode);
					request.AddParameter("upFilter$cmbTemplates", "883053304");
					request.AddParameter("upFilter$cmbTerms", Semesters[SelectedSemesterIndex > 0 ? SelectedSemesterIndex : 0].Value);
					request.AddParameter("upFilter$cmbSubjectGroups", "All");
					request.AddParameter("upFilter$cmbTemplates", "883053304");
					request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
					request.AddParameter("upFilter$rbtnSubjectType", type.ToString());

					IRestResponse response;
					lock (RestWebClient)
					{
						response = RestWebClient.Execute(request);
					}

					html.LoadHtml(response.Content);
					var AllSubjectsCounter = string.Join(String.Empty, html.GetElementbyId("h_addsubjects_gridSubjects_tablebottom").ChildNodes[0].InnerText.Split('/')[1].TakeWhile(s => Char.IsDigit(s)));
					Int32.TryParse(AllSubjectsCounter, out int test);
					SubjectCount = test;
					OnPropertyChanged("MaxPageNumber");

					var subjectscontainer = html.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
					foreach (var a in subjectscontainer)
					{
						if (Subjects.Count >= PageSize)
							break;
						try
						{
							if (a.InnerText == "Nincs találat")
							{
								await UI.ShowMessage(new MessageBoxDialogViewModel()
								{
									Title = "Tárgyak listázása",
									Message = $"Nincs találat : {subjectcode}",
									OkText = "Bigsad"
								});
								break;
							}
							var name = a.ChildNodes[1].InnerText;
							var code = a.ChildNodes[2].InnerText;
							if (!code.ToLower().Contains(subjectcode.ToLower()))
								continue;
							var kredit = a.ChildNodes[3].InnerText;
							// note: 4
							var cmd = a.ChildNodes[1].ChildNodes[0].GetAttributeValue("onclick", "");
							var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
							var completed = Boolean.Parse(a.ChildNodes[5].GetAttributeValue("checked", ""));
							var taken = Boolean.Parse(a.ChildNodes[6].GetAttributeValue("checked", ""));
							//Debugger.Break();
							Application.Current.Dispatcher.Invoke(() =>
							{
								Subjects.Add(new SubjectViewModel()
								{
									Name = name,
									Code = code,
									Credit = kredit,
									onClick = cmd,
									id = id,
									completed = completed,
									taken = taken,
									SubjectType = SubjectType.MindenIntezmenyi
								});
							});
						}
						catch (Exception e)
						{
							Logger.LogErrorSource(e.Message);
							//Debugger.Break();
						}
					}
					//}
				}
				//PageSize = Subjects.Count;
				#region RandomCode
				//request = new RestRequest(Configuration["NeptunServer:HostUrl"] + "main.aspx?ismenuclick=true&ctrl=0303", Method.POST);
				//request.AddParameter("__EVENTVALIDATION", EventValidateStr);
				//request.AddParameter("__VIEWSTATE", ViewStateStr);
				////request.AddParameter("__VIEWSTATEGENERATOR", "202EA31B");
				//request.AddParameter("upFilter$cmbLanguage", Languages[SelectedLanguageIndex].Value);
				//request.AddParameter("upFilter$cmbSubjectGroups", "All");
				//request.AddParameter("upFilter$txtKurzuskod", CourseCode);
				//request.AddParameter("upFilter$txtOktato", Teacher);
				//request.AddParameter("upFilter$txtTargyNev", SubjectName);
				//request.AddParameter("upFilter$txtTargykod", SubjectCode);
				//request.AddParameter("upFilter$cmbTemplates", "883053304");
				//request.AddParameter("upFilter$cmbTerms", Semesters[SelectedSemesterIndex > 0 ? SelectedSemesterIndex : 0].Value);
				//request.AddParameter("upFilter$cmbSubjectGroups", "All");
				//request.AddParameter("upFilter$cmbTemplates", "883053304");
				//request.AddParameter("upFilter$expandedsearchbutton", "Tárgyak listázása");
				//request.AddParameter("upFilter$rbtnSubjectType", type.ToString());

				//IRestResponse response;
				//lock (RestWebClient)
				//{
				//	response = RestWebClient.Execute(request);
				//}
				//var html = new HtmlDocument();

				//html.LoadHtml(response.Content);


				//var subjectscontainer = html.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
				//foreach (var a in subjectscontainer)
				//{
				//	if (Subjects.Count > PageSize)
				//		break;
				//	try
				//	{
				//		var b = a.ChildNodes;
				//		var name = a.ChildNodes[1].InnerText;
				//		var code = a.ChildNodes[2].InnerText;
				//		var kredit = a.ChildNodes[3].InnerText;
				//		// note: 4
				//		var cmd = a.ChildNodes[1].ChildNodes[0].GetAttributeValue("onclick", "");
				//		var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
				//		var completed = Boolean.Parse(a.ChildNodes[5].GetAttributeValue("checked", ""));
				//		var taken = Boolean.Parse(a.ChildNodes[6].GetAttributeValue("checked", ""));
				//		//Debugger.Break();
				//		Application.Current.Dispatcher.Invoke(() =>
				//		{
				//			Subjects.Add(new SubjectViewModel()
				//			{
				//				Name = name,
				//				Code = code,
				//				Credit = kredit,
				//				onClick = cmd,
				//				id = id,
				//				completed = completed,
				//				taken = taken,
				//				SubjectType = SubjectType.MindenIntezmenyi
				//			});
				//		});
				//	}
				//	catch (Exception e)
				//	{
				//		Logger.LogErrorSource(e.Message);
				//		//Debugger.Break();
				//	}
				//}
				//Application.Current.Dispatcher.Invoke(() =>
				//{
				//	Subjects = new ObservableCollection<SubjectViewModel>(AllSubjects.Take(PageSize));
				//});
				// Load elements from first request according to pagesize for the time being, then start loading all the other subjects
				// make the list unique once it's done

				//Task.Run(() =>
				//{
				//	var tmpmp = Math.Ceiling(SubjectCount / 500.0);
				//	var tmplist = new List<SubjectViewModel>();
				//	for (int i = 1; i <= Math.Ceiling(SubjectCount / 500.0); ++i)
				//	{

				//		string responsestr;
				//		{
				//			request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"HandleRequest.ashx?RequestType=GetData&GridID=h_addsubjects_gridSubjects&pageindex={i}&pagesize=500&sort1=&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);
				//			IRestResponse asd;
				//			lock (RestWebClient)
				//			{
				//				asd = RestWebClient.Execute(request);
				//			}
				//			responsestr = asd.Content;
				//		}
				//		responsestr = responsestr.Replace("{type:getdata}", String.Empty);
				//		var htmldocument = new HtmlDocument();
				//		htmldocument.LoadHtml(responsestr);
				//		var table = htmldocument.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
				//		foreach (var a in table)
				//		{
				//			try
				//			{
				//				var b = a.ChildNodes;
				//				var name = a.ChildNodes[1].InnerText;
				//				var code = a.ChildNodes[2].InnerText;
				//				var kredit = a.ChildNodes[3].InnerText;
				//				// note: 4
				//				var cmd = a.ChildNodes[1].ChildNodes[0].GetAttributeValue("onclick", "");
				//				var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
				//				var completed = Boolean.Parse(a.ChildNodes[5].GetAttributeValue("checked", ""));
				//				var taken = Boolean.Parse(a.ChildNodes[6].GetAttributeValue("checked", ""));
				//				//Debugger.Break();
				//				tmplist.Add(new SubjectViewModel()
				//				{
				//					Name = name,
				//					Code = code,
				//					Credit = kredit,
				//					onClick = cmd,
				//					id = id,
				//					completed = completed,
				//					taken = taken,
				//				});
				//			}
				//			catch (Exception e)
				//			{
				//				Logger.LogErrorSource(e.Message);
				//				Debugger.Break();
				//			}
				//		}
				//	}
				//	//Debugger.Break();
				//	AllSubjects = tmplist;
				//});

				#endregion
			}
			//});
		}

		#endregion

		#region Nested Classes

		public class LanguageViewModel
		{
			public string Language { get; set; }
			public string Value { get; set; }

			public override string ToString() => Language;
		}
		public class SemesterViewModel
		{
			public string Semester { get; set; }
			public string Value { get; set; }

			public override string ToString() => Semester;
		}
		public enum SubjectType
		{
			Mintatantervi,
			MindenIntezmenyi
		}

		#endregion

		#region Constructor
		public TFViewModel() : base()
		{
			//Debugger.Break();
			Semesters = new ObservableCollection<SemesterViewModel>();
			//Subjects = new ObservableCollection<SubjectViewModel>();
			Languages = new ObservableCollection<LanguageViewModel>();
			AllSubjects = new List<SubjectViewModel>();

			DeleteSelectedSavedSubjects = new RelayCommand(() =>
			{
				Task.Run(async () =>
				{
					var list = new List<string>();
					foreach (var a in Subjects.Where(s => s.isSelected))
					{
						list.Add(a.Code);
						Application.Current.Dispatcher.Invoke(() =>
					   {
						   Subjects.Remove(a);
					   });
					}
					string msg;
					if (list.Count == 0)
						msg = "Nincs kijelölve semmi.";
					else if (await ClientDataStore.DeleteSelectedSubjects(list))
						msg = "A kijelölt tárgyak törölve lettek.";
					else msg = "Valami hiba történt.";

					await UI.ShowMessage(new MessageBoxDialogViewModel()
					{
						Title = "K",
						Message = msg
					});
				});
			});

			DeleteSavedSubjects = new RelayCommand(() =>
			{
				Task.Run(async () =>
				{
					string msg;
					if (await ClientDataStore.DeleteSubjects())
						msg = "A mentett tárgyak törölve lettek.";
					else msg = "Valami hiba történt.";

					await UI.ShowMessage(new MessageBoxDialogViewModel()
					{
						Title = "K",
						Message = msg
					});
				});
			});
			LoadSavedSubjects = new RelayCommand(() =>
			{
				Task.Run(async () =>
				{
					var list = await ClientDataStore.LoadSavedSubjects();
					if (list == null)
					{
						await UI.ShowMessage(new MessageBoxDialogViewModel()
						{
							Message = "Valami hiba történt."
						});
						return;
					}
					else if (list.Count == 0)
					{
						await UI.ShowMessage(new MessageBoxDialogViewModel()
						{
							Title = "Káosz és anarchia",
							Message = "Nincs mentett tárgy."
						});
						return;
					}
					SubjectCode = String.Empty;
					foreach (var a in list.Where(s => s.type == type.ToString()))
						SubjectCode += a.code + ";";
					LoadSubjects();
					for (int i = 0; i < Subjects.Count && i < list.Count; ++i)
					{
						var a = Subjects[i];
						a.TakeViewModel = new TakeSubjectViewModel(a.id, a, this, true);
						a.TakeViewModel.Courses = new ObservableCollection<TakeSubjectViewModel.Course>();
						var asd = list[i].courses.Split(';');
						foreach (var dman in asd)
						{
							a.TakeViewModel.Courses.Add(new TakeSubjectViewModel.Course()
							{
								ID = dman,
								isSelected = true,
								SelectionChanged = true
							});
						}
					}
					MessageBoxResult result = MessageBox.Show("Felvegyem ezeket a tárgyakat most rögtön?", "Neptun", MessageBoxButton.YesNo);
					switch (result)
					{
						case MessageBoxResult.Yes:
							{
								foreach (var a in Subjects)
									a.TakeViewModel.TakeSubject.Execute(null);
								break;
							}
					}
				});
			});

			SaveListedSubjects = new RelayCommand(() =>
			{
				Task.Run(async () =>
				{
					if (Subjects.Where(s => s.isSelected && s.TakeViewModel != null).Count() == 0)
					{
						await UI.ShowMessage(new MessageBoxDialogViewModel()
						{
							OkText = "k boi",
							Message = "Nincs mentésre kijelölt tárgy.",
							Title = "Figyelmeztetés"
						});
						return;
					}
					List<SavedSubjectDataModel> savethese = new List<SavedSubjectDataModel>();
					foreach (var s in Subjects.Where(s => s.isSelected && s.TakeViewModel != null))
					{
						var datamodel = new SavedSubjectDataModel()
						{
							code = s.Code,
							type = s.SubjectType.ToString()
						};
						var courseIDs = String.Empty;
						foreach (var c in s.TakeViewModel.Courses.Where(c => c.isSelected))
							courseIDs += c.ID + ";";
						courseIDs = courseIDs.Trim(new char[] { ';' });
						datamodel.courses = courseIDs;
						savethese.Add(datamodel);
					}
					var asd = await ClientDataStore.SaveSubjects(savethese);
					await UI.ShowMessage(new MessageBoxDialogViewModel()
					{
						Message = asd ? "Sikeres mentés" : "Sikertelen mentés, fuck"
					});
				});
			});

			ForgetListedSubjects = new RelayCommand(() =>
			{
				Task.Run(async () =>
				{
					if (Subjects.Where(s => s.isSelected && s.taken).Count() == 0)
					{
						await UI.ShowMessage(new MessageBoxDialogViewModel()
						{
							OkText = "k boi",
							Message = "Nincs leadásra kijelölt tárgy.",
							Title = "Figyelmeztetés"
						});
						return;
					}
					for (int i = 0; i < Subjects.Count; ++i)
					{
						var subject = Subjects[i];
						if (subject.taken && subject.isSelected)
						{
							//Debugger.Break();
							if (subject.TakeViewModel == null)
							{
								subject.TakeViewModel = new TakeSubjectViewModel(subject.id, subject, this, false);
								subject.TakeViewModel.ForgetSubject.Execute(null);
								//Debugger.Break();
							}
							else subject.TakeViewModel.ForgetSubject.Execute(null);
						}
					}
				});
			});

			TakeListedSubjects = new RelayCommand(() =>
			{
				Task.Run(async () =>
				{

					if (Subjects.Where(s => s.TakeViewModel != null && s.isSelected).Count() == 0)
					{
						await UI.ShowMessage(new MessageBoxDialogViewModel()
						{
							Message = "Nem található kurzus amit fel lehetne venni. Adott tárgy kurzusait ki kell jelölni.",
							Title = "Figyelmeztetás",
							OkText = "K then.."
						});
						return;
					}
					foreach (var subject in Subjects)
					{
						// TODO: Rewrite this incase it's loaded from the local database
						if (subject.TakeViewModel != null && subject.isSelected)
							subject.TakeViewModel.TakeSubject.Execute(null);
					}

				});
			});

			#region Page Navigation

			FirstPage = new RelayCommand(() =>
	{
		CurrentPage = 1;
		//var tmplist = new List<SubjectViewModel>();
		Subjects = new ObservableCollection<SubjectViewModel>();
		Task.Run(() =>
		{
			string responsestr;
			{
				var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"HandleRequest.ashx?RequestType=GetData&GridID=h_addsubjects_gridSubjects&pageindex={CurrentPage}&pagesize={PageSize}&sort1=&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);
				IRestResponse asd;
				lock (RestWebClient)
				{
					asd = RestWebClient.Execute(request);
				}
				responsestr = asd.Content;
			}
			responsestr = responsestr.Replace("{type:getdata}", String.Empty);
			var htmldocument = new HtmlDocument();
			htmldocument.LoadHtml(responsestr);
			var table = htmldocument.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
			foreach (var a in table)
			{
				try
				{
					var b = a.ChildNodes;
					var name = a.ChildNodes[1].InnerText;
					var code = a.ChildNodes[2].InnerText;
					var kredit = a.ChildNodes[3].InnerText;
					// note: 4
					var cmd = a.ChildNodes[1].ChildNodes[0].GetAttributeValue("onclick", "");
					var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
					var completed = Boolean.Parse(a.ChildNodes[5].GetAttributeValue("checked", ""));
					var taken = Boolean.Parse(a.ChildNodes[6].GetAttributeValue("checked", ""));
					Application.Current.Dispatcher.Invoke(() =>
					{
						Subjects.Add(new SubjectViewModel()
						{
							Name = name,
							Code = code,
							Credit = kredit,
							onClick = cmd,
							id = id,
							completed = completed,
							taken = taken,
							SubjectType = SubjectType.MindenIntezmenyi,
							semester = Semesters[SelectedSemesterIndex]
						});
					});
				}
				catch (Exception e)
				{
					Logger.LogErrorSource(e.Message);
					Debugger.Break();
				}
			}
		});
	});

			LastPage = new RelayCommand(() =>
			{
				CurrentPage = MaxPageNumber;
				//Subjects = new ObservableCollection<SubjectViewModel>(AllSubjects.Skip(PageSize * MaxPageNumber).Take(PageSize));
				//Debugger.Break();
				Subjects = new ObservableCollection<SubjectViewModel>();
				Task.Run(() =>
				{
					string responsestr;
					{
						var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"HandleRequest.ashx?RequestType=GetData&GridID=h_addsubjects_gridSubjects&pageindex={MaxPageNumber}&pagesize={PageSize}&sort1=&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);
						IRestResponse asd;
						lock (RestWebClient)
						{
							asd = RestWebClient.Execute(request);
						}
						responsestr = asd.Content;
					}
					responsestr = responsestr.Replace("{type:getdata}", String.Empty);
					var htmldocument = new HtmlDocument();
					htmldocument.LoadHtml(responsestr);
					var table = htmldocument.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
					foreach (var a in table)
					{
						try
						{
							var b = a.ChildNodes;
							var name = a.ChildNodes[1].InnerText;
							var code = a.ChildNodes[2].InnerText;
							var kredit = a.ChildNodes[3].InnerText;
							// note: 4
							var cmd = a.ChildNodes[1].ChildNodes[0].GetAttributeValue("onclick", "");
							var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
							var completed = Boolean.Parse(a.ChildNodes[5].GetAttributeValue("checked", ""));
							var taken = Boolean.Parse(a.ChildNodes[6].GetAttributeValue("checked", ""));
							Application.Current.Dispatcher.Invoke(() =>
							{
								Subjects.Add(new SubjectViewModel()
								{
									Name = name,
									Code = code,
									Credit = kredit,
									onClick = cmd,
									id = id,
									completed = completed,
									taken = taken,
									SubjectType = SubjectType.MindenIntezmenyi,
									semester = Semesters[SelectedSemesterIndex]
								});
							});
						}
						catch (Exception e)
						{
							Logger.LogErrorSource(e.Message);
							Debugger.Break();
						}
					}
				});

			});

			NavigateToLeft = new RelayCommand(() =>
			{
				//Subjects = new ObservableCollection<SubjectViewModel>(AllSubjects.Skip(PageSize * (--CurrentPage - 1)).Take(PageSize));

				Subjects = new ObservableCollection<SubjectViewModel>();
				Task.Run(() =>
				{
					string responsestr;
					{
						var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"HandleRequest.ashx?RequestType=GetData&GridID=h_addsubjects_gridSubjects&pageindex={--CurrentPage}&pagesize={PageSize}&sort1=&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.GET);
						IRestResponse asd;
						lock (RestWebClient)
						{
							asd = RestWebClient.Execute(request);
						}
						responsestr = asd.Content;
					}
					responsestr = responsestr.Replace("{type:getdata}", String.Empty);
					var htmldocument = new HtmlDocument();
					htmldocument.LoadHtml(responsestr);
					var table = htmldocument.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
					foreach (var a in table)
					{
						try
						{
							var b = a.ChildNodes;
							var name = a.ChildNodes[1].InnerText;
							var code = a.ChildNodes[2].InnerText;
							var kredit = a.ChildNodes[3].InnerText;
							// note: 4
							var cmd = a.ChildNodes[1].ChildNodes[0].GetAttributeValue("onclick", "");
							var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
							var completed = Boolean.Parse(a.ChildNodes[5].GetAttributeValue("checked", ""));
							var taken = Boolean.Parse(a.ChildNodes[6].GetAttributeValue("checked", ""));
							Application.Current.Dispatcher.Invoke(() =>
							{
								Subjects.Add(new SubjectViewModel()
								{
									Name = name,
									Code = code,
									Credit = kredit,
									onClick = cmd,
									id = id,
									completed = completed,
									taken = taken,
									SubjectType = SubjectType.MindenIntezmenyi
								});
							});
						}
						catch (Exception e)
						{
							Logger.LogErrorSource(e.Message);
							Debugger.Break();
						}
					}
				});
			});

			NavigateToRight = new RelayCommand(() =>
			{
				Subjects = new ObservableCollection<SubjectViewModel>();
				Task.Run(() =>
				{
					// Get all subjects and use those..
					string responsestr;
					{
						var request = new RestRequest(Configuration["NeptunServer:HostUrl"] + $"HandleRequest.ashx?RequestType=GetData&GridID=h_addsubjects_gridSubjects&pageindex={++CurrentPage}&pagesize={PageSize}&sort1=&sort2=&fixedheader=false&searchcol=&searchtext=&searchexpanded=false&allsubrowsexpanded=False&selectedid=undefined&functionname=&level=", Method.POST);
						IRestResponse asd;
						lock (RestWebClient)
						{
							asd = RestWebClient.Execute(request);
						}
						responsestr = asd.Content;
					}
					responsestr = responsestr.Replace("{type:getdata}", String.Empty);
					var htmldocument = new HtmlDocument();
					htmldocument.LoadHtml(responsestr);
					var table = htmldocument.GetElementbyId("h_addsubjects_gridSubjects_bodytable").ChildNodes[1].ChildNodes;
					foreach (var a in table)
					{
						try
						{
							var b = a.ChildNodes;
							var name = a.ChildNodes[1].InnerText;
							var code = a.ChildNodes[2].InnerText;
							var kredit = a.ChildNodes[3].InnerText;
							// note: 4
							var cmd = a.ChildNodes[1].ChildNodes[0].GetAttributeValue("onclick", "");
							var id = cmd.Split('(')[1].Split(',')[0].Replace("\'", "");
							var completed = Boolean.Parse(a.ChildNodes[5].GetAttributeValue("checked", ""));
							var taken = Boolean.Parse(a.ChildNodes[6].GetAttributeValue("checked", ""));
							Application.Current.Dispatcher.Invoke(() =>
							{
								Subjects.Add(new SubjectViewModel()
								{
									Name = name,
									Code = code,
									Credit = kredit,
									onClick = cmd,
									id = id,
									completed = completed,
									taken = taken,
									SubjectType = SubjectType.MindenIntezmenyi
								});
							});
						}
						catch (Exception e)
						{
							Logger.LogErrorSource(e.Message);
							Debugger.Break();
						}
					}
				});
			});

			#endregion

			ListSubjects = new RelayCommand(() =>
			{
				MintaTantervView = type == SubjectType.Mintatantervi;
				var asd = SelectedSemesterIndex;
				//Debugger.Break();
				Task.Run(LoadSubjects);
				//isPageChanging = false;
			});
			Task.Run(LoadSubjects);
		}
		#endregion

		#region Public Properties

		public ObservableCollection<SemesterViewModel> Semesters { get; set; }
		public ObservableCollection<LanguageViewModel> Languages { get; set; }

		public int CurrentPage { get; set; } = 1;

		public int SelectedSemesterIndex { get; set; } = 0;
		public int SelectedLanguageIndex { get; set; } = 0;
		//public MessageFilterEnum filter { get; set; } = MessageFilterEnum.All;

		public ObservableCollection<SubjectViewModel> Subjects { get; set; }

		public List<SubjectViewModel> AllSubjects { get; set; }

		public SubjectType type { get; set; } = SubjectType.Mintatantervi;

		public string SubjectName { get; set; } = String.Empty;
		public string SubjectCode { get; set; } = String.Empty;
		public string Teacher { get; set; } = String.Empty;
		public string CourseCode { get; set; } = String.Empty;

		public int PageSize { get; set; } = 20; // update maxpagenumber on set
		public ObservableCollection<int> PageSizes { get; } = new ObservableCollection<int>() { 20, 100, 200, 500 };

		public int MaxPageNumber
		{
			get
			{
				var asd = Math.Ceiling((double)SubjectCount / PageSize);
				return (int)Math.Ceiling((double)SubjectCount / PageSize);
			}
		}

		public double WaitTime { get; set; } = 0;
		public bool MintaTantervView { get; set; } = true;

		public bool isFirstPageEnabled { get => CurrentPage != 1; }
		public bool isPrevPageEnabled { get => CurrentPage != 1; }
		public bool isNextPageEnabled { get => CurrentPage != MaxPageNumber; }
		public bool isLastPageEnabled { get => CurrentPage != MaxPageNumber; }

		#endregion

		#region Public Commands
		public ICommand ListSubjects { get; set; }
		public ICommand FirstPage { get; set; }
		public ICommand LastPage { get; set; }
		public ICommand NavigateToLeft { get; set; }
		public ICommand NavigateToRight { get; set; }

		public ICommand TakeListedSubjects { get; set; }
		public ICommand ForgetListedSubjects { get; set; }
		public ICommand SaveListedSubjects { get; set; }
		public ICommand DeleteSavedSubjects { get; set; }
		public ICommand LoadSavedSubjects { get; set; }
		#endregion
		public ICommand DeleteSelectedSavedSubjects { get; set; }
	}
}
