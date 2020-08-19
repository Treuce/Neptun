using System;
using System.Threading.Tasks;
using Neptun.Core;
using System.Windows;
using System.Diagnostics;
using Dna;

namespace Neptun
{
	/// <summary>
	/// The applications implementation of the <see cref="IUIManager"/>
	/// </summary>
	public class UIManager : IUIManager
	{
		/// <summary>
		/// Displays a single message box to the user
		/// </summary>
		/// <param name="viewModel">The view model</param>
		/// <returns></returns>
		public Task ShowMessage(MessageBoxDialogViewModel viewModel)
		{
			// Create a task completion source
			var tcs = new TaskCompletionSource<bool>();

			// Run on UI thread
			Application.Current.Dispatcher.Invoke(async () =>
			{
				try
				{
					// Show the dialog box
					await new DialogMessageBox().ShowDialog(viewModel);
				}
				finally
				{
					// Flag we are done
					tcs.SetResult(true);
				}
			});

			// Return the task once complete
			return tcs.Task;
		}
		public Task ShowMessage(MessageViewModel viewModel)
		{
			// Create a task completion source
			var tcs = new TaskCompletionSource<bool>();

			// Run on UI thread
			Application.Current.Dispatcher.Invoke(async () =>
			{
				try
				{
					// Show the dialog box
					await new MessageReadDialogBox().ShowDialog(viewModel);
				}
				finally
				{
					// Flag we are done
					tcs.SetResult(true);
				}
			});

			// Return the task once complete
			return tcs.Task;
		}
		public Task ShowSchedulePlanner()
		{
			// Create a task completion source
			var tcs = new TaskCompletionSource<bool>();

			// Run on UI thread
			Application.Current.Dispatcher.Invoke(async () =>
			{
				try
				{
					// Show the dialog box
					var window = new ScheduleWindow();
					window.Owner = Application.Current.MainWindow;
					window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
					window.Show();
				}
				finally
				{
					// Flag we are done
					tcs.SetResult(true);
				}
			});

			// Return the task once complete
			return tcs.Task;
		}
	}
}
