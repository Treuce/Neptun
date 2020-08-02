using System;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace Neptun.Core
{
	/// <summary>
	/// The data model for the login credentials of a client
	/// </summary>
	public class LoginCredentialsDataModel
	{
		/// <summary>
		/// The unique Id
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The users neptun code
		/// </summary>
		public string NeptunCode { get; set; }

		/// <summary>
		/// The users login password
		/// </summary>
		public string Password { get; set; }
	}
}
