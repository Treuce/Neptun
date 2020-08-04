using HtmlAgilityPack;
using RestSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Neptun.Core.CoreDI;
using static Dna.FrameworkDI;
using Neptun.Core;
using System.Net;
using static Neptun.DI;
using Dna;

namespace Neptun
{
	public class MainMenuItemViewModel : BaseViewModel
	{

		public MainMenuItemViewModel(HtmlNode i = null)
		{
			// Act as a empty constructor call
			if (i == null) return;

			SubMenu = new List<MainMenuSubEntryViewModel>();
			var SubMenus = i.ChildNodes[1].ChildNodes;
			//HtmlDocument tmp = new HtmlDocument();
			foreach (var j in SubMenus)
			{
				if (j.Name != "div")
				{
					var asdasd = j.Attributes.First(s => s.Name == "targeturl");
					SubMenu.Add(new MainMenuSubEntryViewModel()
					{
						Name = j.GetDirectInnerText(),
						LinksTo = j.Attributes.First(s => s.Name == "targeturl").Value
					});
					
					//Debugger.Break();
					
				}
			}
		}

		public bool IsSelected { get; set; }

		public string Name { get; set; }

		public List<MainMenuSubEntryViewModel> SubMenu { get; set; }
	}
}