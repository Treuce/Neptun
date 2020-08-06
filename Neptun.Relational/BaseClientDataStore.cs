﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dna;
using Neptun.Core;

namespace Neptun.Relational
{
	/// <summary>
	/// Stores and retrieves information about the client application 
	/// such as login credentials, messages, settings and so on
	/// in an SQLite database
	/// </summary>
	public class BaseClientDataStore : IClientDataStore
	{
		#region Protected Members

		/// <summary>
		/// The database context for the client data store
		/// </summary>
		protected ClientDataStoreDbContext mDbContext;

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="dbContext">The database to use</param>
		public BaseClientDataStore(ClientDataStoreDbContext dbContext)
		{
			// Set local member
			mDbContext = dbContext;
		}

		#endregion

		#region Interface Implementation

		/// <summary>
		/// Determines if the current user has logged in credentials
		/// </summary>
		public async Task<bool> HasCredentialsAsync()
		{
			//TODO: Encrpyt stored password
			var a = await GetLoginCredentialsAsync();
			return await GetLoginCredentialsAsync() != null;
		}

		/// <summary>
		/// Makes sure the client data store is correctly set up
		/// </summary>
		/// <returns>Returns a task that will finish once setup is complete</returns>
		public async Task EnsureDataStoreAsync()
		{
			try
			{
				// Make sure the database exists and is created
				await mDbContext.Database.EnsureCreatedAsync();
			}
			catch (Exception e)
			{
				Dna.FrameworkDI.Logger.LogDebugSource(e.Message);
			}
		}

		/// <summary>
		/// Gets the stored login credentials for this client
		/// </summary>
		/// <returns>Returns the login credentials if they exist, or null if none exist</returns>
		public Task<LoginCredentialsDataModel> GetLoginCredentialsAsync()
		{
			// Get the first column in the login credentials table, or null if none exist
			return Task.FromResult(mDbContext.LoginCredentials.FirstOrDefault());
		}

		/// <summary>
		/// Stores the given login credentials to the backing data store
		/// </summary>
		/// <param name="loginCredentials">The login credentials to save</param>
		/// <returns>Returns a task that will finish once the save is complete</returns>
		public async Task SaveLoginCredentialsAsync(LoginCredentialsDataModel loginCredentials)
		{
			// Clear all entries
			mDbContext.LoginCredentials.RemoveRange(mDbContext.LoginCredentials);

			// Add new one
			mDbContext.LoginCredentials.Add(loginCredentials);

			// Save changes
			await mDbContext.SaveChangesAsync();
		}

		/// <summary>
		/// Removes all login credentials stored in the data store
		/// </summary>
		/// <returns></returns>
		public async Task ClearAllLoginCredentialsAsync()
		{
			// Clear all entries
			mDbContext.LoginCredentials.RemoveRange(mDbContext.LoginCredentials);

			// Save changes
			await mDbContext.SaveChangesAsync();
		}

		public async Task<bool> SaveSubjects(List<SavedSubjectDataModel> l)
		{
			try
			{
				foreach (var s in l)
				{
					if (mDbContext.SavedSubjects.Any(c => c.code == s.code))
						mDbContext.SavedSubjects.FirstOrDefault(c => c.code == s.code).courses = s.courses;
					else
						mDbContext.SavedSubjects.Add(s);
				}
				await mDbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception e)
			{
				Dna.FrameworkDI.Logger.LogErrorSource(e.Message);
				Debugger.Break();
				return false;
			}
		}

		public async Task<bool> DeleteSubjects()
		{
			try
			{
				mDbContext.SavedSubjects.RemoveRange(mDbContext.SavedSubjects);
				await mDbContext.SaveChangesAsync();
				return true;
			}
			catch (Exception e)
			{
				Dna.FrameworkDI.Logger.LogErrorSource(e.Message);

				Debugger.Break();
				return false;
			}
		}
		public async Task<List<SavedSubjectDataModel>> LoadSavedSubjects()
		{
			return mDbContext.SavedSubjects.ToList();
		}

		#endregion
	}
}
