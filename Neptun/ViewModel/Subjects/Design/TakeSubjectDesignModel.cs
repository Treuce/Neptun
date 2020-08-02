using RestSharp;
using System.Collections.Generic;
using System.Windows.Input;
using static Neptun.Core.CoreDI;
using static Dna.FrameworkDI;
using HtmlAgilityPack;
using static Neptun.DI;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Net;
using System;

namespace Neptun
{
	public class TakeSubjectDesignModel : TakeSubjectViewModel
	{
		public static TakeSubjectDesignModel Instance { get; set; } = new TakeSubjectDesignModel();

		public TakeSubjectDesignModel()
		{
			MenuItems = new ObservableCollection<TFMenuItemViewModel>()
			{
				new TFMenuItemViewModel()
				{
					Name = "Kurzusok"
				},
				new TFMenuItemViewModel()
				{
					Name = "Tárgy adatok"
				}
			};
			Courses = new ObservableCollection<Course>()
			{
				new Course()
				{
					CourseCode = "18",
					Teacher = "KozsikHaj"
				},
				new Course()
				{
					CourseCode ="<3 11",
					Teacher = "Tukszár Ákos"
				}
			};
		}
	}
}
