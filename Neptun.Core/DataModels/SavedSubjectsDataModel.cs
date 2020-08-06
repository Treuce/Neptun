using System;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Neptun.Core
{
	/// <summary>
	/// The data model for the login credentials of a client
	/// </summary>
	public class SavedSubjectDataModel
	{
		public string code { get; set; }
		public string courses { get; set; }

		public string type { get; set; }

	}
}
