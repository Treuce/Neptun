using System.Security;

namespace Neptun.Core
{
	/// <summary>
	/// The result of a login request or get user profile details request via API
	/// </summary>
	public class UserProfileDetailsApiModel
	{
		#region Public Properties

		private string Password { get; set; }

		/// <summary>
		/// The users username
		/// </summary>
		public string Email { get; set; }

		public int Id { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public UserProfileDetailsApiModel()
		{

		}

		public UserProfileDetailsApiModel(string n, System.Security.SecureString a)
		{
			Email = n;
			Password = a.Unsecure();
		}
		public UserProfileDetailsApiModel(string n, string a)
		{
			Email = n;
			Password = a;
		}
		#endregion

		#region Public Helper Methods

		/// <summary>
		/// Creates a new <see cref="LoginCredentialsDataModel"/>
		/// from this model
		/// </summary>
		/// <returns></returns>
		public LoginCredentialsDataModel ToLoginCredentialsDataModel()
		{
			return new LoginCredentialsDataModel
			{
				//Id = (Id == null ? "" : Id) ,
				NeptunCode = Email,
				Password = Password
			};
		}

		#endregion
	}
}
