using Neptun;

namespace Neptun
{
	public class MainMenuSubEntryViewModel : BaseViewModel
	{
		public string Name { get; set; }
		public string LinksTo { get; set; }

		//Dummy SubMenu object just to stop Binding error from the Menu and make the style work as it should ._.
		// TODO: Actually remove this and fix things
		public object SubMenu { get; private set; } = null;
	}
}