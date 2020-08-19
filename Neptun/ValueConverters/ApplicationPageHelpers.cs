using Neptun.Core;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Neptun
{
	/// <summary>
	/// Converts the <see cref="ApplicationPage"/> to an actual view/page
	/// </summary>
	public static class ApplicationPageHelpers
	{
		/// <summary>
		/// Takes a <see cref="ApplicationPage"/> and a view model, if any, and creates the desired page
		/// </summary>
		/// <param name="page"></param>
		/// <param name="viewModel"></param>
		/// <returns></returns>
		public static BasePage ToBasePage(this ApplicationPage page, object viewModel = null)
		{
			// Find the appropriate page
			switch (page)
			{
				case ApplicationPage.Login:
					return new LoginPage(viewModel as LoginViewModel);

				case ApplicationPage.Messages:
					return new MessagePage(viewModel as MessagesListViewModel);

				case ApplicationPage.Tárgyfelvétel:
					return new TFPage(viewModel as TFViewModel);
				case ApplicationPage.FelvettTárgyak:
					return new TakenSubjectsPage(viewModel as TakenSubjectsPageViewModel);
				case ApplicationPage.Default:
					return new NoUIPage();
				default:
					Debugger.Break();
					return null;
			}
		}

		/// <summary>
		/// Converts a <see cref="BasePage"/> to the specific <see cref="ApplicationPage"/> that is for that type of page
		/// </summary>
		/// <param name="page"></param>
		/// <returns></returns>
		public static ApplicationPage ToApplicationPage(this BasePage page)
		{
			// Find application page that matches the base page
			if (page is MessagePage)
				return ApplicationPage.Messages;
			if (page is TFPage)
				return ApplicationPage.Tárgyfelvétel;
			if (page is TakenSubjectsPage)
				return ApplicationPage.FelvettTárgyak;
			if (page is LoginPage)
				return ApplicationPage.Login;
			if (page is NoUIPage)
				return ApplicationPage.Default;

			// Alert developer of issue
			Debugger.Break();
			return default(ApplicationPage);
		}
	}
}
