using Neptun.Core;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Neptun
{
	/// <summary>
	/// The View Model for the custom flat window
	/// </summary>
	public class DialogWindowViewModel : WindowViewModel
	{
		#region Public Properties

		/// <summary>
		/// The title of this dialog window
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The content to host inside the dialog
		/// </summary>
		public Control Content { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public DialogWindowViewModel(Window window) : base(window)
		{
			// Make minimum size smaller
			WindowMinimumWidth = 250;
			WindowMinimumHeight = 100;
			CloseCommand = new RelayCommand(() => mWindow.Close());
			// Make title bar smaller
			TitleHeight = 30;
		}

		#endregion
	}


	public class MessageWindowViewModel : WindowViewModel
	{
		#region Public Properties

		/// <summary>
		/// The title of this dialog window
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The content to host inside the dialog
		/// </summary>
		public Control Content { get; set; }

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public MessageWindowViewModel(Window window) : base(window)
		{
			// Make minimum size smaller
			WindowMinimumWidth = 250;
			WindowMinimumHeight = 100;
			CloseCommand = new RelayCommand(() => mWindow.Close());
			// Make title bar smaller
			TitleHeight = 30;
		}

		#endregion
	}
}
