using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptun
{
	public class MainMenuDesignModel : MainNavMenuViewModel
	{
		public static MainMenuDesignModel Instance =>  new MainMenuDesignModel();

		public MainMenuDesignModel() : base()
		{
			Items = new ObservableCollection<MainMenuItemViewModel>
			{
				new MainMenuItemViewModel(null)
				{
					Name = "Saját adatok"
				},
				new MainMenuItemViewModel(null)
				{
					Name = "Tanulmányok"
				},
				new MainMenuItemViewModel(null)
				{
					Name = "Órarend"
				},
				new MainMenuItemViewModel(null)
				{
					Name = "Tárgyak"
					
				}
			};
		}
	}
}
