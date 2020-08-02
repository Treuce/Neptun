using Dna;
using Neptun.Core;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static Dna.FrameworkDI;
using static Neptun.DI;

namespace Neptun
{
	/// <summary>
	/// Extension methods for the <see cref="WebRequestResultExtensions"/> class
	/// </summary>
	public static class WebRequestResultExtensions
	{
		/// <summary>
		/// Checks the web request result for any errors, displaying them if there are any
		/// </summary>
		/// <typeparam name="T">The type of Api Response</typeparam>
		/// <param name="response">The response to check</param>
		/// <param name="title">The title of the error dialog if there is an error</param>
		/// <returns>Returns true if there was an error, or false if all was OK</returns>
	
		public static async Task<bool> HandleErrorIfFailedAsync(this IRestResponse response, string title)
		{
			// If there was no response, bad data, or a response with a error message...
			if (response != null)
			{
				// Default error message
				// TODO: Localize strings
				var message = "Unknown error from server call";
				string responsecontent = response.Content;
				var b = JsonConvert.DeserializeObject<Neptun_Response>(JsonConvert.DeserializeObject<BecauseResponses_come_with_prefix_d>(response.Content).d);
				message = b.errormessage + " / " + b.warningmessage;
				// If this is an unauthorized response...
				if (response?.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					// Log it
					Logger.LogInformationSource("Logging user out due to unauthorized response from server");

					return false;
				}
				else if (b.success && b.errormessage.Contains("Sikeres bejelentkezés"))
				{
					if (!String.IsNullOrEmpty(b.warningmessage))
					{
						try
						{

						await UI.ShowMessage(new MessageBoxDialogViewModel
						{
							Title = "Figyelmeztetés",
							Message = b.warningmessage
						});
						}
						catch (Exception e)
						{
							Logger.LogErrorSource(e.ToString());
							Debugger.Break();
						}
					}
					return true;
				}
				else
				{
					// Display error
					await UI.ShowMessage(new MessageBoxDialogViewModel
					{
						// TODO: Localize strings
						Title = title,
						Message = b.errormessage
					});

					return false; 
				}
			}

			// All was OK, so return false for no error
			return true;
		}
	}
}
