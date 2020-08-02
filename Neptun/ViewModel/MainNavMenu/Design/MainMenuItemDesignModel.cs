using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptun
{
	public class MainMenuItemDesignModel : MainMenuItemViewModel
	{
		public static MainMenuItemDesignModel Instance => new MainMenuItemDesignModel();

		public MainMenuItemDesignModel()
		{
			Name = "Saját adatok";
		}
	}
}
